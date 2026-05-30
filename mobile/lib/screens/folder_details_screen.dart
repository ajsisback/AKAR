import 'package:flutter/foundation.dart';
import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';
import 'file_details_sheet.dart';

class FolderDetailsScreen extends StatefulWidget {
  final String projectId;
  final String folderId;
  final String folderName;
  const FolderDetailsScreen({
    super.key,
    required this.projectId,
    required this.folderId,
    required this.folderName,
  });

  @override
  State<FolderDetailsScreen> createState() => _FolderDetailsScreenState();
}

class _FolderDetailsScreenState extends State<FolderDetailsScreen> {
  List<Map<String, dynamic>> _files = [];
  bool _loading = true;
  bool _uploading = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() { _loading = true; _error = null; });
    final api = ApiService();
    await api.init();
    try {
      final list = await api.getFolderFiles(widget.projectId, widget.folderId);
      if (mounted) setState(() { _files = list; _loading = false; });
    } on ApiException catch (e) {
      if (mounted) setState(() { _error = e.code; _loading = false; });
    } catch (_) {
      if (mounted) setState(() { _error = 'ERR_GENERIC'; _loading = false; });
    }
  }

  String _mapError(String code, AppLocalizations l) {
    switch (code) {
      case 'FILE_TYPE_NOT_ALLOWED': return l.t('err_file_type_not_allowed');
      case 'FILE_TOO_LARGE': return l.t('err_file_too_large');
      case 'FILE_REQUIRED': return l.t('err_file_required');
      case 'FOLDER_NOT_FOUND': return l.t('err_folder_not_found');
      case 'FILE_NOT_FOUND': return l.t('err_file_not_found');
      case 'PROJECT_NOT_FOUND': return l.t('err_project_not_found');
      case 'STORAGE_SAVE_FAILED': return l.t('err_storage_save_failed');
      case 'FOLDER_SYSTEM_PROTECTED': return l.t('err_folder_system_protected');
      case 'UNAUTHORIZED': return l.t('err_generic');
      default: return l.t('err_generic');
    }
  }

  String _formatSize(dynamic bytes) {
    if (bytes == null) return '—';
    final b = bytes is int ? bytes : int.tryParse(bytes.toString()) ?? 0;
    if (b < 1024) return '$b B';
    if (b < 1024 * 1024) return '${(b / 1024).toStringAsFixed(1)} KB';
    return '${(b / (1024 * 1024)).toStringAsFixed(1)} MB';
  }

  String _categoryLabel(String? cat, AppLocalizations l) {
    switch (cat) {
      case 'Document': return l.t('cat_document');
      case 'Image': return l.t('cat_image');
      case 'Video': return l.t('cat_video');
      default: return l.t('cat_other');
    }
  }

  Color _categoryColor(String? cat) {
    switch (cat) {
      case 'Document': return AkarTheme.primary;
      case 'Image': return AkarTheme.accent;
      case 'Video': return AkarTheme.warning;
      default: return AkarTheme.textMuted;
    }
  }

  IconData _categoryIcon(String? cat) {
    switch (cat) {
      case 'Document': return Icons.description;
      case 'Image': return Icons.image;
      case 'Video': return Icons.videocam;
      default: return Icons.insert_drive_file;
    }
  }

  Future<void> _uploadFile() async {
    final l = AppLocalizations.of(context);

    try {
      final result = await FilePicker.platform.pickFiles(withData: true);
      if (result == null || result.files.isEmpty) return;

      final pickedFile = result.files.first;
      final Uint8List? bytes = pickedFile.bytes;
      final String fileName = pickedFile.name;

      if (bytes == null || bytes.isEmpty) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(l.t('err_file_required')), backgroundColor: AkarTheme.danger),
          );
        }
        return;
      }

      // Confirm upload
      if (!mounted) return;
      final confirmed = await showDialog<bool>(
        context: context,
        builder: (ctx) => AlertDialog(
          backgroundColor: AkarTheme.bgCard,
          title: Text(l.t('file_upload')),
          content: Column(mainAxisSize: MainAxisSize.min, crossAxisAlignment: CrossAxisAlignment.start, children: [
            Text('${l.t('file_selected')}:', style: const TextStyle(color: AkarTheme.textSecondary, fontSize: 13)),
            const SizedBox(height: 6),
            Row(children: [
              const Icon(Icons.insert_drive_file, size: 20, color: AkarTheme.accent),
              const SizedBox(width: 8),
              Expanded(child: Text(fileName, style: const TextStyle(fontWeight: FontWeight.w500), overflow: TextOverflow.ellipsis)),
            ]),
            const SizedBox(height: 4),
            Text(_formatSize(bytes.length), style: const TextStyle(color: AkarTheme.textMuted, fontSize: 12)),
          ]),
          actions: [
            TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(l.t('btn_cancel'))),
            TextButton(
              onPressed: () => Navigator.pop(ctx, true),
              child: Text(l.t('file_upload')),
            ),
          ],
        ),
      );

      if (confirmed != true) return;

      setState(() => _uploading = true);

      final api = ApiService();
      await api.init();
      await api.uploadFile(widget.projectId, widget.folderId, bytes, fileName);

      if (mounted) {
        setState(() => _uploading = false);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('file_upload_success')), backgroundColor: AkarTheme.success),
        );
        _load();
      }
    } on ApiException catch (e) {
      if (mounted) {
        setState(() => _uploading = false);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(_mapError(e.code, l)), backgroundColor: AkarTheme.danger),
        );
      }
    } catch (_) {
      if (mounted) {
        setState(() => _uploading = false);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('file_upload_failed')), backgroundColor: AkarTheme.danger),
        );
      }
    }
  }

  Future<void> _showFileActions(Map<String, dynamic> file) async {
    if (!mounted) return;
    await showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (ctx) => FileDetailsSheet(
        projectId: widget.projectId,
        file: file,
        onDeleted: () => _load(),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.folderName),
      ),
      body: _uploading
          ? Center(child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
              const CircularProgressIndicator(),
              const SizedBox(height: 16),
              Text(l.t('loading'), style: const TextStyle(color: AkarTheme.textSecondary)),
            ]))
          : _buildBody(l),
      floatingActionButton: _uploading ? null : FloatingActionButton.extended(
        backgroundColor: AkarTheme.primary,
        icon: const Icon(Icons.upload_file, color: Colors.white),
        label: Text(l.t('file_upload'), style: const TextStyle(color: Colors.white)),
        onPressed: _uploadFile,
      ),
    );
  }

  Widget _buildBody(AppLocalizations l) {
    if (_loading) {
      return const Center(child: CircularProgressIndicator());
    }
    if (_error != null) {
      return Center(child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(Icons.error_outline, size: 48, color: AkarTheme.danger),
          const SizedBox(height: 12),
          Text(_mapError(_error!, l), style: const TextStyle(color: AkarTheme.textSecondary)),
          const SizedBox(height: 16),
          TextButton.icon(onPressed: _load, icon: const Icon(Icons.refresh), label: Text(l.t('btn_view'))),
        ],
      ));
    }
    if (_files.isEmpty) {
      return Center(child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(Icons.folder_open, size: 56, color: AkarTheme.textMuted),
          const SizedBox(height: 12),
          Text(l.t('file_no_files'), style: const TextStyle(color: AkarTheme.textSecondary, fontSize: 15)),
        ],
      ));
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView.builder(
        padding: const EdgeInsets.fromLTRB(16, 16, 16, 80),
        itemCount: _files.length,
        itemBuilder: (ctx, i) => _buildFileCard(_files[i], l),
      ),
    );
  }

  Widget _buildFileCard(Map<String, dynamic> file, AppLocalizations l) {
    final name = file['originalFileName'] ?? '';
    final cat = file['fileCategory'] as String?;
    final size = _formatSize(file['fileSizeBytes']);

    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: () => _showFileActions(file),
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
          child: Row(children: [
            Container(
              width: 40, height: 40,
              decoration: BoxDecoration(
                color: _categoryColor(cat).withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Icon(_categoryIcon(cat), color: _categoryColor(cat), size: 20),
            ),
            const SizedBox(width: 12),
            Expanded(child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(name, style: const TextStyle(fontWeight: FontWeight.w500, fontSize: 13),
                  overflow: TextOverflow.ellipsis, maxLines: 1),
                const SizedBox(height: 3),
                Row(children: [
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 1),
                    decoration: BoxDecoration(
                      color: _categoryColor(cat).withValues(alpha: 0.10),
                      borderRadius: BorderRadius.circular(4),
                    ),
                    child: Text(_categoryLabel(cat, l),
                      style: TextStyle(color: _categoryColor(cat), fontSize: 10, fontWeight: FontWeight.w600)),
                  ),
                  const SizedBox(width: 8),
                  Text(size, style: const TextStyle(color: AkarTheme.textMuted, fontSize: 11)),
                ]),
              ],
            )),
            const Icon(Icons.chevron_right, size: 18, color: AkarTheme.textMuted),
          ]),
        ),
      ),
    );
  }
}

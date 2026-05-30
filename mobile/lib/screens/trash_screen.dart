import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';

class TrashScreen extends StatefulWidget {
  final String projectId;
  const TrashScreen({super.key, required this.projectId});

  @override
  State<TrashScreen> createState() => _TrashScreenState();
}

class _TrashScreenState extends State<TrashScreen> {
  List<Map<String, dynamic>> _deletedFiles = [];
  List<Map<String, dynamic>> _deletedFolders = [];
  bool _loading = true;
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
      final trash = await api.getTrash(widget.projectId);
      if (mounted) {
        setState(() {
          _deletedFiles = (trash['deletedFiles'] as List?)?.cast<Map<String, dynamic>>() ?? [];
          _deletedFolders = (trash['deletedFolders'] as List?)?.cast<Map<String, dynamic>>() ?? [];
          _loading = false;
        });
      }
    } on ApiException catch (e) {
      if (mounted) setState(() { _error = e.code; _loading = false; });
    } catch (_) {
      if (mounted) setState(() { _error = 'ERR_GENERIC'; _loading = false; });
    }
  }

  String _mapError(String code, AppLocalizations l) {
    switch (code) {
      case 'FILE_NOT_FOUND': return l.t('err_file_not_found');
      case 'PROJECT_NOT_FOUND': return l.t('err_project_not_found');
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

  String _formatDate(dynamic d) {
    if (d == null) return '—';
    try {
      final dt = DateTime.parse(d.toString());
      return '${dt.year}-${dt.month.toString().padLeft(2, '0')}-${dt.day.toString().padLeft(2, '0')}';
    } catch (_) {
      return d.toString();
    }
  }

  Future<void> _restoreFile(Map<String, dynamic> file) async {
    final l = AppLocalizations.of(context);
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AkarTheme.bgCard,
        title: Text(l.t('trash_restore')),
        content: Text(file['originalFileName'] ?? ''),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(l.t('btn_cancel'))),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: Text(l.t('btn_confirm')),
          ),
        ],
      ),
    );
    if (confirmed != true) return;

    try {
      final api = ApiService();
      await api.init();
      await api.restoreFile(widget.projectId, file['id']);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('trash_restore_success')), backgroundColor: AkarTheme.success),
        );
        _load();
      }
    } on ApiException catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(_mapError(e.code, l)), backgroundColor: AkarTheme.danger),
        );
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('trash_restore_failed')), backgroundColor: AkarTheme.danger),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Scaffold(
      appBar: AppBar(title: Text(l.t('trash_title'))),
      body: _buildBody(l),
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
    if (_deletedFiles.isEmpty && _deletedFolders.isEmpty) {
      return Center(child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(Icons.delete_outline, size: 56, color: AkarTheme.textMuted),
          const SizedBox(height: 12),
          Text(l.t('trash_empty'), style: const TextStyle(color: AkarTheme.textSecondary, fontSize: 15)),
        ],
      ));
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // Deleted files section
          if (_deletedFiles.isNotEmpty) ...[
            Padding(
              padding: const EdgeInsets.only(bottom: 8),
              child: Text(l.t('trash_deleted_files'),
                style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: AkarTheme.textSecondary)),
            ),
            ..._deletedFiles.map((f) => _buildDeletedFileCard(f, l)),
          ],
          // Deleted folders section
          if (_deletedFolders.isNotEmpty) ...[
            const SizedBox(height: 16),
            Padding(
              padding: const EdgeInsets.only(bottom: 8),
              child: Text(l.t('trash_deleted_folders'),
                style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: AkarTheme.textSecondary)),
            ),
            ..._deletedFolders.map((f) => _buildDeletedFolderCard(f, l)),
          ],
        ],
      ),
    );
  }

  Widget _buildDeletedFileCard(Map<String, dynamic> file, AppLocalizations l) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
        child: Row(children: [
          Container(
            width: 38, height: 38,
            decoration: BoxDecoration(
              color: AkarTheme.danger.withValues(alpha: 0.10),
              borderRadius: BorderRadius.circular(8),
            ),
            child: const Icon(Icons.insert_drive_file, color: AkarTheme.danger, size: 18),
          ),
          const SizedBox(width: 12),
          Expanded(child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(file['originalFileName'] ?? '', style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w500),
                overflow: TextOverflow.ellipsis, maxLines: 1),
              const SizedBox(height: 2),
              Text('${_formatSize(file['fileSizeBytes'])}  •  ${_formatDate(file['deletedAtUtc'])}',
                style: const TextStyle(color: AkarTheme.textMuted, fontSize: 11)),
            ],
          )),
          TextButton.icon(
            icon: const Icon(Icons.restore, size: 18),
            label: Text(l.t('trash_restore'), style: const TextStyle(fontSize: 12)),
            style: TextButton.styleFrom(foregroundColor: AkarTheme.success),
            onPressed: () => _restoreFile(file),
          ),
        ]),
      ),
    );
  }

  Widget _buildDeletedFolderCard(Map<String, dynamic> folder, AppLocalizations l) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
        child: Row(children: [
          Container(
            width: 38, height: 38,
            decoration: BoxDecoration(
              color: AkarTheme.warning.withValues(alpha: 0.10),
              borderRadius: BorderRadius.circular(8),
            ),
            child: const Icon(Icons.folder_off, color: AkarTheme.warning, size: 18),
          ),
          const SizedBox(width: 12),
          Expanded(child: Text(folder['folderName'] ?? '',
            style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w500))),
          // Read-only — backend folder restore not supported
        ]),
      ),
    );
  }
}

import 'package:flutter/material.dart';
import '../core/download_helper.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';

class FileDetailsSheet extends StatelessWidget {
  final String projectId;
  final Map<String, dynamic> file;
  final VoidCallback? onDeleted;

  const FileDetailsSheet({
    super.key,
    required this.projectId,
    required this.file,
    this.onDeleted,
  });

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

  String _categoryLabel(String? cat, AppLocalizations l) {
    switch (cat) {
      case 'Document': return l.t('cat_document');
      case 'Image': return l.t('cat_image');
      case 'Video': return l.t('cat_video');
      default: return l.t('cat_other');
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

  Future<void> _downloadFile(BuildContext context) async {
    final l = AppLocalizations.of(context);
    final fileId = file['id'] as String;
    final fileName = file['originalFileName'] as String? ?? 'download';

    try {
      final api = ApiService();
      await api.init();
      final bytes = await api.downloadFileBytes(projectId, fileId);

      downloadFileBytes(bytes, fileName);
    } on ApiException catch (e) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(_mapError(e.code, l)), backgroundColor: AkarTheme.danger),
        );
      }
    } catch (_) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('err_generic')), backgroundColor: AkarTheme.danger),
        );
      }
    }
  }

  Future<void> _deleteFile(BuildContext context) async {
    final l = AppLocalizations.of(context);
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AkarTheme.bgCard,
        title: Text(l.t('file_move_trash')),
        content: Text(l.t('btn_delete_confirm')),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(l.t('btn_cancel'))),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: Text(l.t('btn_confirm'), style: const TextStyle(color: AkarTheme.danger)),
          ),
        ],
      ),
    );

    if (confirmed != true) return;

    try {
      final api = ApiService();
      await api.init();
      await api.deleteFile(projectId, file['id']);
      if (context.mounted) {
        Navigator.pop(context);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('file_move_trash')), backgroundColor: AkarTheme.success),
        );
        onDeleted?.call();
      }
    } on ApiException catch (e) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(_mapError(e.code, l)), backgroundColor: AkarTheme.danger),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Container(
      decoration: const BoxDecoration(
        color: AkarTheme.bgCard,
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
      child: SafeArea(
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          // Handle
          Container(width: 40, height: 4, decoration: BoxDecoration(
            color: AkarTheme.textMuted, borderRadius: BorderRadius.circular(2))),
          const SizedBox(height: 16),

          // Title
          Text(l.t('file_details'), style: const TextStyle(fontSize: 17, fontWeight: FontWeight.bold)),
          const SizedBox(height: 16),

          // Metadata rows
          _MetaRow(label: l.t('file_name'), value: file['originalFileName'] ?? '—'),
          _MetaRow(label: l.t('file_type'), value: file['contentType'] ?? '—'),
          _MetaRow(label: l.t('file_size'), value: _formatSize(file['fileSizeBytes'])),
          _MetaRow(label: l.t('file_category'), value: _categoryLabel(file['fileCategory'], l)),
          _MetaRow(label: l.t('file_created_at'), value: _formatDate(file['createdAtUtc'])),

          const SizedBox(height: 20),

          // Actions
          Row(children: [
            Expanded(
              child: OutlinedButton.icon(
                icon: const Icon(Icons.download, size: 18),
                label: Text(l.t('file_download')),
                style: OutlinedButton.styleFrom(
                  foregroundColor: AkarTheme.accent,
                  side: const BorderSide(color: AkarTheme.accent),
                  padding: const EdgeInsets.symmetric(vertical: 12),
                ),
                onPressed: () => _downloadFile(context),
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: OutlinedButton.icon(
                icon: const Icon(Icons.delete_outline, size: 18),
                label: Text(l.t('file_move_trash')),
                style: OutlinedButton.styleFrom(
                  foregroundColor: AkarTheme.danger,
                  side: const BorderSide(color: AkarTheme.danger),
                  padding: const EdgeInsets.symmetric(vertical: 12),
                ),
                onPressed: () => _deleteFile(context),
              ),
            ),
          ]),
        ]),
      ),
    );
  }
}

class _MetaRow extends StatelessWidget {
  final String label;
  final String value;
  const _MetaRow({required this.label, required this.value});

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 5),
    child: Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(width: 110, child: Text(label,
          style: const TextStyle(color: AkarTheme.textMuted, fontSize: 13))),
        const SizedBox(width: 8),
        Expanded(child: Text(value,
          style: const TextStyle(fontSize: 13), overflow: TextOverflow.ellipsis, maxLines: 2)),
      ],
    ),
  );
}

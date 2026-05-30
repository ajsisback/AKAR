import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';
import 'folder_details_screen.dart';
import 'trash_screen.dart';

class DocumentVaultScreen extends StatefulWidget {
  final String projectId;
  final String projectName;
  const DocumentVaultScreen({super.key, required this.projectId, required this.projectName});

  @override
  State<DocumentVaultScreen> createState() => _DocumentVaultScreenState();
}

class _DocumentVaultScreenState extends State<DocumentVaultScreen> {
  List<Map<String, dynamic>> _folders = [];
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
      final list = await api.getProjectFolders(widget.projectId);
      if (mounted) setState(() { _folders = list; _loading = false; });
    } on ApiException catch (e) {
      if (mounted) setState(() { _error = e.code; _loading = false; });
    } catch (_) {
      if (mounted) setState(() { _error = 'ERR_GENERIC'; _loading = false; });
    }
  }

  IconData _folderIcon(String folderType) {
    switch (folderType) {
      case 'License': return Icons.description;
      case 'ProjectLocation': return Icons.location_on;
      case 'Contracts': return Icons.handshake;
      case 'Drawings': return Icons.architecture;
      case 'Photos': return Icons.photo_library;
      case 'Videos': return Icons.videocam;
      case 'Invoices': return Icons.receipt_long;
      case 'Warranties': return Icons.verified_user;
      case 'FollowersInbox': return Icons.inbox;
      default: return Icons.folder;
    }
  }

  String _folderLabel(String folderType, String folderName, AppLocalizations l) {
    switch (folderType) {
      case 'License': return l.t('folder_type_license');
      case 'ProjectLocation': return l.t('folder_type_project_location');
      case 'Contracts': return l.t('folder_type_contracts');
      case 'Drawings': return l.t('folder_type_drawings');
      case 'Photos': return l.t('folder_type_photos');
      case 'Videos': return l.t('folder_type_videos');
      case 'Invoices': return l.t('folder_type_invoices');
      case 'Warranties': return l.t('folder_type_warranties');
      case 'FollowersInbox': return l.t('folder_type_followers_inbox');
      default: return folderName;
    }
  }

  String _mapError(String code, AppLocalizations l) {
    switch (code) {
      case 'PROJECT_NOT_FOUND': return l.t('err_project_not_found');
      case 'FOLDER_NOT_FOUND': return l.t('err_folder_not_found');
      case 'FOLDER_SYSTEM_PROTECTED': return l.t('err_folder_system_protected');
      case 'UNAUTHORIZED': return l.t('err_generic');
      default: return l.t('err_generic');
    }
  }

  Future<void> _showCreateFolderDialog() async {
    final l = AppLocalizations.of(context);
    final controller = TextEditingController();
    final result = await showDialog<String>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AkarTheme.bgCard,
        title: Text(l.t('vault_create_folder')),
        content: TextField(
          controller: controller,
          autofocus: true,
          decoration: InputDecoration(hintText: l.t('folder_name')),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: Text(l.t('btn_cancel'))),
          TextButton(
            onPressed: () => Navigator.pop(ctx, controller.text.trim()),
            child: Text(l.t('btn_create')),
          ),
        ],
      ),
    );
    if (result != null && result.isNotEmpty) {
      final api = ApiService();
      await api.init();
      try {
        await api.createFolder(widget.projectId, result);
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(l.t('folder_created')), backgroundColor: AkarTheme.success),
          );
          _load();
        }
      } on ApiException catch (e) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(_mapError(e.code, l)), backgroundColor: AkarTheme.danger),
          );
        }
      }
    }
  }

  Future<void> _showFolderActions(Map<String, dynamic> folder) async {
    final l = AppLocalizations.of(context);
    final isSystem = folder['isSystemFolder'] == true;

    if (isSystem) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(l.t('folder_system_no_rename')), backgroundColor: AkarTheme.warning),
      );
      return;
    }

    final action = await showModalBottomSheet<String>(
      context: context,
      backgroundColor: AkarTheme.bgCard,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
      ),
      builder: (ctx) => SafeArea(
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          const SizedBox(height: 12),
          Container(width: 40, height: 4, decoration: BoxDecoration(
            color: AkarTheme.textMuted, borderRadius: BorderRadius.circular(2))),
          const SizedBox(height: 16),
          ListTile(
            leading: const Icon(Icons.edit, color: AkarTheme.accent),
            title: Text(l.t('folder_rename')),
            onTap: () => Navigator.pop(ctx, 'rename'),
          ),
          ListTile(
            leading: const Icon(Icons.delete_outline, color: AkarTheme.danger),
            title: Text(l.t('folder_delete'), style: const TextStyle(color: AkarTheme.danger)),
            onTap: () => Navigator.pop(ctx, 'delete'),
          ),
          const SizedBox(height: 8),
        ]),
      ),
    );

    if (action == 'rename') {
      await _renameFolderDialog(folder);
    } else if (action == 'delete') {
      await _deleteFolderConfirm(folder);
    }
  }

  Future<void> _renameFolderDialog(Map<String, dynamic> folder) async {
    final l = AppLocalizations.of(context);
    final controller = TextEditingController(text: folder['folderName'] ?? '');
    final result = await showDialog<String>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AkarTheme.bgCard,
        title: Text(l.t('folder_rename')),
        content: TextField(
          controller: controller,
          autofocus: true,
          decoration: InputDecoration(hintText: l.t('folder_new_name')),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: Text(l.t('btn_cancel'))),
          TextButton(
            onPressed: () => Navigator.pop(ctx, controller.text.trim()),
            child: Text(l.t('btn_save')),
          ),
        ],
      ),
    );
    if (result != null && result.isNotEmpty) {
      final api = ApiService();
      await api.init();
      try {
        await api.renameFolder(widget.projectId, folder['id'], result);
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(l.t('folder_renamed')), backgroundColor: AkarTheme.success),
          );
          _load();
        }
      } on ApiException catch (e) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(_mapError(e.code, l)), backgroundColor: AkarTheme.danger),
          );
        }
      }
    }
  }

  Future<void> _deleteFolderConfirm(Map<String, dynamic> folder) async {
    final l = AppLocalizations.of(context);
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AkarTheme.bgCard,
        title: Text(l.t('folder_delete')),
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
    if (confirmed == true) {
      final api = ApiService();
      await api.init();
      try {
        await api.deleteFolder(widget.projectId, folder['id']);
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(l.t('folder_deleted')), backgroundColor: AkarTheme.success),
          );
          _load();
        }
      } on ApiException catch (e) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(_mapError(e.code, l)), backgroundColor: AkarTheme.danger),
          );
        }
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Scaffold(
      appBar: AppBar(
        title: Text(l.t('vault_title')),
      ),
      body: _buildBody(l),
      floatingActionButton: FloatingActionButton.extended(
        backgroundColor: AkarTheme.primary,
        icon: const Icon(Icons.create_new_folder, color: Colors.white),
        label: Text(l.t('vault_create_folder'), style: const TextStyle(color: Colors.white)),
        onPressed: _showCreateFolderDialog,
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
          TextButton.icon(
            onPressed: _load,
            icon: const Icon(Icons.refresh),
            label: Text(l.t('btn_view')),
          ),
        ],
      ));
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.fromLTRB(16, 16, 16, 80),
        children: [
          // Folder grid
          ..._folders.map((folder) => _buildFolderCard(folder, l)),

          const SizedBox(height: 8),
          // Trash entry
          Card(
            child: ListTile(
              leading: Container(
                width: 42, height: 42,
                decoration: BoxDecoration(
                  color: AkarTheme.danger.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(Icons.delete_outline, color: AkarTheme.danger, size: 22),
              ),
              title: Text(l.t('trash_title'), style: const TextStyle(fontWeight: FontWeight.w500)),
              trailing: const Icon(Icons.chevron_right, color: AkarTheme.textMuted),
              onTap: () => Navigator.push(context, MaterialPageRoute(
                builder: (_) => TrashScreen(projectId: widget.projectId),
              )),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFolderCard(Map<String, dynamic> folder, AppLocalizations l) {
    final isSystem = folder['isSystemFolder'] == true;
    final folderType = folder['folderType'] ?? 'Custom';
    final fileCount = folder['fileCount'] ?? 0;
    final name = _folderLabel(folderType, folder['folderName'] ?? '', l);

    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: () => Navigator.push(context, MaterialPageRoute(
          builder: (_) => FolderDetailsScreen(
            projectId: widget.projectId,
            folderId: folder['id'],
            folderName: name,
          ),
        )).then((_) => _load()),
        onLongPress: () => _showFolderActions(folder),
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
          child: Row(children: [
            Container(
              width: 42, height: 42,
              decoration: BoxDecoration(
                color: (isSystem ? AkarTheme.primary : AkarTheme.accent).withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Icon(_folderIcon(folderType),
                color: isSystem ? AkarTheme.primaryLight : AkarTheme.accent, size: 22),
            ),
            const SizedBox(width: 14),
            Expanded(child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(name, style: const TextStyle(fontWeight: FontWeight.w500, fontSize: 14)),
                const SizedBox(height: 2),
                Text('$fileCount ${l.t('files_count')}',
                  style: const TextStyle(color: AkarTheme.textMuted, fontSize: 12)),
              ],
            )),
            if (isSystem)
              const Icon(Icons.lock_outline, size: 16, color: AkarTheme.textMuted),
            if (!isSystem)
              const Icon(Icons.chevron_right, size: 18, color: AkarTheme.textMuted),
          ]),
        ),
      ),
    );
  }
}

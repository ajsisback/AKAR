import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';
import 'add_edit_follower_screen.dart';
import 'folder_details_screen.dart';

class FollowerDetailsScreen extends StatefulWidget {
  final String projectId;
  final String projectName;
  final String followerId;
  const FollowerDetailsScreen({
    super.key,
    required this.projectId,
    required this.projectName,
    required this.followerId,
  });
  @override
  State<FollowerDetailsScreen> createState() => _FollowerDetailsScreenState();
}

class _FollowerDetailsScreenState extends State<FollowerDetailsScreen> {
  final _api = ApiService();
  Map<String, dynamic>? _follower;
  List<Map<String, dynamic>> _links = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    await _api.init();
    try {
      final f = await _api.getProjectFollower(widget.projectId, widget.followerId);
      final links = await _api.getFollowerUploadLinks(widget.projectId, widget.followerId);
      if (mounted) setState(() { _follower = f; _links = links; _loading = false; _error = null; });
    } on ApiException catch (e) {
      if (mounted) setState(() { _error = e.code; _loading = false; });
    } catch (_) {
      if (mounted) setState(() { _error = 'err_generic'; _loading = false; });
    }
  }

  String _typeLabel(String? type, AppLocalizations l) {
    switch (type) {
      case 'Supervisor': return l.t('follower_type_supervisor');
      case 'Relative': return l.t('follower_type_relative');
      case 'Contractor': return l.t('follower_type_contractor');
      case 'Designer': return l.t('follower_type_designer');
      case 'EngineeringOffice': return l.t('follower_type_engineering_office');
      case 'Other': return l.t('follower_type_other');
      default: return type ?? '';
    }
  }

  String _mapError(String code, AppLocalizations l) {
    switch (code) {
      case 'FOLLOWER_NOT_FOUND': return l.t('err_follower_not_found');
      case 'UPLOAD_LINK_NOT_FOUND': return l.t('err_upload_link_not_found');
      case 'UPLOAD_LINK_REVOKED': return l.t('err_upload_link_revoked');
      case 'UPLOAD_LINK_EXPIRED': return l.t('err_upload_link_expired');
      case 'FOLLOWER_INACTIVE': return l.t('err_follower_inactive');
      default: return l.t('err_generic');
    }
  }

  String _formatDate(dynamic d) {
    if (d == null) return '—';
    try {
      final dt = DateTime.parse(d.toString());
      return '${dt.year}-${dt.month.toString().padLeft(2, '0')}-${dt.day.toString().padLeft(2, '0')} ${dt.hour.toString().padLeft(2, '0')}:${dt.minute.toString().padLeft(2, '0')}';
    } catch (_) { return d.toString(); }
  }

  Future<void> _deleteFollower() async {
    final l = AppLocalizations.of(context);
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AkarTheme.bgCard,
        title: Text(l.t('follower_delete_title')),
        content: Text(l.t('follower_delete_warning')),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(l.t('btn_cancel'))),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: Text(l.t('btn_confirm'), style: const TextStyle(color: AkarTheme.danger)),
          ),
        ],
      ),
    );
    if (confirmed != true || !mounted) return;
    try {
      await _api.deleteProjectFollower(widget.projectId, widget.followerId);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('follower_deleted')), backgroundColor: AkarTheme.success),
        );
        Navigator.pop(context, true);
      }
    } on ApiException catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(_mapError(e.code, l)), backgroundColor: AkarTheme.danger),
        );
      }
    }
  }

  Future<void> _generateLink() async {
    final l = AppLocalizations.of(context);
    try {
      final result = await _api.generateFollowerUploadLink(widget.projectId, widget.followerId);
      final rawToken = result['uploadToken'] ?? '';
      final uploadUrl = 'http://localhost:8888/#/follower-upload/$rawToken';
      if (mounted) {
        await showDialog(
          context: context,
          barrierDismissible: false,
          builder: (ctx) => AlertDialog(
            backgroundColor: AkarTheme.bgCard,
            title: Row(children: [
              const Icon(Icons.link, color: AkarTheme.success),
              const SizedBox(width: 8),
              Text(l.t('upload_link_generated')),
            ]),
            content: Column(mainAxisSize: MainAxisSize.min, crossAxisAlignment: CrossAxisAlignment.start, children: [
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: AkarTheme.bgInput,
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: AkarTheme.accent.withValues(alpha: 0.3)),
                ),
                child: SelectableText(uploadUrl, style: const TextStyle(fontSize: 12, color: AkarTheme.accent)),
              ),
              const SizedBox(height: 12),
              Row(children: [
                const Icon(Icons.warning_amber, color: AkarTheme.warning, size: 16),
                const SizedBox(width: 6),
                Expanded(child: Text(l.t('upload_link_save_warning'),
                    style: const TextStyle(color: AkarTheme.warning, fontSize: 12))),
              ]),
            ]),
            actions: [
              TextButton.icon(
                icon: const Icon(Icons.copy, size: 16),
                label: Text(l.t('upload_link_copy')),
                onPressed: () {
                  Clipboard.setData(ClipboardData(text: uploadUrl));
                  ScaffoldMessenger.of(ctx).showSnackBar(
                    SnackBar(content: Text(l.t('upload_link_copied')), backgroundColor: AkarTheme.success),
                  );
                },
              ),
              TextButton(
                onPressed: () => Navigator.pop(ctx),
                child: Text(l.t('btn_confirm')),
              ),
            ],
          ),
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

  Future<void> _revokeLink(String linkId) async {
    final l = AppLocalizations.of(context);
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AkarTheme.bgCard,
        title: Text(l.t('upload_link_revoke')),
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
    if (confirmed != true || !mounted) return;
    try {
      await _api.revokeFollowerUploadLink(widget.projectId, widget.followerId, linkId);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('upload_link_revoked')), backgroundColor: AkarTheme.success),
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

  String _linkStatus(Map<String, dynamic> link, AppLocalizations l) {
    if (link['isRevoked'] == true) return l.t('upload_link_status_revoked');
    if (link['expiresAtUtc'] != null) {
      try {
        final exp = DateTime.parse(link['expiresAtUtc']);
        if (exp.isBefore(DateTime.now().toUtc())) return l.t('upload_link_status_expired');
      } catch (_) {}
    }
    if (link['isActive'] == true) return l.t('upload_link_status_active');
    return l.t('follower_inactive');
  }

  Color _linkStatusColor(Map<String, dynamic> link) {
    if (link['isRevoked'] == true) return AkarTheme.danger;
    if (link['expiresAtUtc'] != null) {
      try {
        final exp = DateTime.parse(link['expiresAtUtc']);
        if (exp.isBefore(DateTime.now().toUtc())) return AkarTheme.warning;
      } catch (_) {}
    }
    if (link['isActive'] == true) return AkarTheme.success;
    return AkarTheme.textMuted;
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    if (_loading) return Scaffold(appBar: AppBar(title: Text(l.t('follower_details'))), body: const Center(child: CircularProgressIndicator()));
    if (_error != null || _follower == null) {
      return Scaffold(
        appBar: AppBar(title: Text(l.t('follower_details'))),
        body: Center(child: Text(l.t(_error ?? 'err_generic'), style: const TextStyle(color: AkarTheme.danger))),
      );
    }

    final f = _follower!;
    final isActive = f['isActive'] == true;
    final isDeleted = f['isDeleted'] == true;

    return Scaffold(
      appBar: AppBar(
        title: Text(l.t('follower_details')),
        actions: [
          IconButton(
            icon: const Icon(Icons.edit),
            onPressed: () async {
              final result = await Navigator.push(context, MaterialPageRoute(
                builder: (_) => AddEditFollowerScreen(projectId: widget.projectId, follower: f),
              ));
              if (result == true) _load();
            },
          ),
          IconButton(icon: const Icon(Icons.delete, color: AkarTheme.danger), onPressed: _deleteFollower),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _load,
        child: ListView(padding: const EdgeInsets.all(16), children: [
          // ─── Follower Info Card ───
          Card(child: Padding(padding: const EdgeInsets.all(16), child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Row(children: [
              Container(
                width: 48, height: 48,
                decoration: BoxDecoration(
                  color: isActive && !isDeleted ? AkarTheme.primary.withValues(alpha: 0.15) : AkarTheme.textMuted.withValues(alpha: 0.15),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Icon(Icons.person, color: isActive && !isDeleted ? AkarTheme.primaryLight : AkarTheme.textMuted, size: 26),
              ),
              const SizedBox(width: 14),
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text(f['fullName'] ?? '', style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
                const SizedBox(height: 2),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                  decoration: BoxDecoration(
                    color: isActive && !isDeleted ? AkarTheme.success.withValues(alpha: 0.15) : AkarTheme.danger.withValues(alpha: 0.15),
                    borderRadius: BorderRadius.circular(4),
                  ),
                  child: Text(
                    isActive && !isDeleted ? l.t('follower_active') : l.t('follower_inactive'),
                    style: TextStyle(color: isActive && !isDeleted ? AkarTheme.success : AkarTheme.danger, fontSize: 11, fontWeight: FontWeight.w600),
                  ),
                ),
              ])),
            ]),
            const Divider(height: 24),
            _DetailRow(label: l.t('follower_phone'), value: f['phone'] ?? '—'),
            _DetailRow(label: l.t('follower_type'), value: _typeLabel(f['followerType'], l)),
            _DetailRow(label: l.t('follower_notes'), value: (f['notes'] ?? '').toString().isEmpty ? '—' : f['notes']),
            _DetailRow(label: l.t('follower_created_at'), value: _formatDate(f['createdAtUtc'])),
          ]))),

          const SizedBox(height: 8),

          // ─── Open Inbox Button ───
          Card(child: InkWell(
            borderRadius: BorderRadius.circular(12),
            onTap: () {
              final inboxId = f['inboxFolderId'];
              if (inboxId == null) return;
              Navigator.push(context, MaterialPageRoute(
                builder: (_) => FolderDetailsScreen(
                  projectId: widget.projectId,
                  folderId: inboxId,
                  folderName: '${f['fullName']} — ${l.t('follower_open_inbox')}',
                ),
              ));
            },
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
              child: Row(children: [
                Container(
                  width: 40, height: 40,
                  decoration: BoxDecoration(
                    color: AkarTheme.accent.withValues(alpha: 0.12),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: const Icon(Icons.inbox, color: AkarTheme.accent, size: 20),
                ),
                const SizedBox(width: 12),
                Expanded(child: Text(l.t('follower_open_inbox'), style: const TextStyle(fontWeight: FontWeight.w600))),
                const Icon(Icons.chevron_right, color: AkarTheme.textMuted),
              ]),
            ),
          )),

          const SizedBox(height: 16),

          // ─── Upload Links Section ───
          Row(children: [
            Text(l.t('upload_links_title'), style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
            const Spacer(),
            TextButton.icon(
              icon: const Icon(Icons.add_link, size: 18),
              label: Text(l.t('upload_link_generate')),
              onPressed: isActive && !isDeleted ? _generateLink : null,
            ),
          ]),
          const SizedBox(height: 8),

          if (_links.isEmpty)
            Card(child: Padding(
              padding: const EdgeInsets.all(24),
              child: Center(child: Text(l.t('upload_links_empty'), style: const TextStyle(color: AkarTheme.textMuted))),
            ))
          else
            ..._links.map((link) => Card(
              margin: const EdgeInsets.only(bottom: 8),
              child: Padding(padding: const EdgeInsets.all(12), child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Row(children: [
                  Icon(Icons.link, size: 16, color: _linkStatusColor(link)),
                  const SizedBox(width: 6),
                  Text(link['tokenPreview'] ?? '•••', style: TextStyle(fontWeight: FontWeight.w600, color: _linkStatusColor(link))),
                  const Spacer(),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                    decoration: BoxDecoration(
                      color: _linkStatusColor(link).withValues(alpha: 0.15),
                      borderRadius: BorderRadius.circular(4),
                    ),
                    child: Text(_linkStatus(link, l),
                        style: TextStyle(color: _linkStatusColor(link), fontSize: 10, fontWeight: FontWeight.w600)),
                  ),
                ]),
                const SizedBox(height: 8),
                if (link['expiresAtUtc'] != null)
                  _DetailRow(label: l.t('upload_link_expires_at'), value: _formatDate(link['expiresAtUtc'])),
                _DetailRow(label: l.t('upload_link_last_used'), value: _formatDate(link['lastUsedAtUtc'])),
                _DetailRow(label: l.t('follower_created_at'), value: _formatDate(link['createdAtUtc'])),
                if (link['isActive'] == true && link['isRevoked'] != true)
                  Align(alignment: AlignmentDirectional.centerEnd, child: TextButton.icon(
                    icon: const Icon(Icons.block, size: 16, color: AkarTheme.danger),
                    label: Text(l.t('upload_link_revoke'), style: const TextStyle(color: AkarTheme.danger)),
                    onPressed: () => _revokeLink(link['id']),
                  )),
              ])),
            )),
        ]),
      ),
    );
  }
}

class _DetailRow extends StatelessWidget {
  final String label; final String value;
  const _DetailRow({required this.label, required this.value});
  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 4),
    child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
      SizedBox(width: 110, child: Text(label, style: const TextStyle(color: AkarTheme.textSecondary, fontSize: 13))),
      const SizedBox(width: 8),
      Expanded(child: Text(value, style: const TextStyle(fontSize: 14))),
    ]),
  );
}

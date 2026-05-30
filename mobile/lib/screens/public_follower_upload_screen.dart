import 'package:flutter/foundation.dart';
import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';

/// Public follower upload page — no login required.
/// Accessed via: /#/follower-upload/{token}
class PublicFollowerUploadScreen extends StatefulWidget {
  final String token;
  const PublicFollowerUploadScreen({super.key, required this.token});
  @override
  State<PublicFollowerUploadScreen> createState() => _PublicFollowerUploadScreenState();
}

enum _PageState { loading, valid, invalid, expired, revoked }

class _PublicFollowerUploadScreenState extends State<PublicFollowerUploadScreen> {
  final _api = ApiService();
  _PageState _state = _PageState.loading;
  Map<String, dynamic>? _info;

  String? _selectedFileName;
  Uint8List? _selectedFileBytes;
  bool _uploading = false;
  bool _uploaded = false;

  @override
  void initState() { super.initState(); _loadInfo(); }

  Future<void> _loadInfo() async {
    setState(() => _state = _PageState.loading);
    try {
      final info = await _api.getFollowerUploadInfo(widget.token);
      if (mounted) setState(() { _info = info; _state = _PageState.valid; });
    } on ApiException catch (e) {
      if (mounted) {
        if (e.code.contains('EXPIRED')) {
          setState(() => _state = _PageState.expired);
        } else if (e.code.contains('REVOKED')) {
          setState(() => _state = _PageState.revoked);
        } else {
          setState(() => _state = _PageState.invalid);
        }
      }
    } catch (_) {
      if (mounted) setState(() => _state = _PageState.invalid);
    }
  }

  Future<void> _pickFile() async {
    final result = await FilePicker.platform.pickFiles(withData: true);
    if (result != null && result.files.isNotEmpty) {
      final f = result.files.first;
      if (mounted) {
        setState(() {
          _selectedFileName = f.name;
          _selectedFileBytes = f.bytes;
          _uploaded = false;
        });
      }
    }
  }

  String _mapError(String code, AppLocalizations l) {
    if (code.contains('FILE_TYPE_NOT_ALLOWED')) return l.t('err_file_type_not_allowed');
    if (code.contains('FILE_TOO_LARGE')) return l.t('err_file_too_large');
    if (code.contains('FILE_REQUIRED')) return l.t('err_file_required');
    if (code.contains('EXPIRED')) return l.t('public_upload_expired_link');
    if (code.contains('REVOKED')) return l.t('public_upload_revoked_link');
    return l.t('public_upload_failed');
  }

  Future<void> _upload() async {
    if (_selectedFileBytes == null || _selectedFileName == null) return;
    setState(() => _uploading = true);
    final l = AppLocalizations.of(context);

    try {
      await _api.uploadFollowerFile(widget.token, _selectedFileBytes!, _selectedFileName!);
      if (mounted) {
        setState(() { _uploading = false; _uploaded = true; _selectedFileBytes = null; _selectedFileName = null; });
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('public_upload_success')), backgroundColor: AkarTheme.success),
        );
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
          SnackBar(content: Text(l.t('public_upload_failed')), backgroundColor: AkarTheme.danger),
        );
      }
    }
  }

  String _formatDate(dynamic d) {
    if (d == null) return '';
    try {
      final dt = DateTime.parse(d.toString());
      return '${dt.year}-${dt.month.toString().padLeft(2, '0')}-${dt.day.toString().padLeft(2, '0')} ${dt.hour.toString().padLeft(2, '0')}:${dt.minute.toString().padLeft(2, '0')}';
    } catch (_) { return d.toString(); }
  }

  String _formatSize(dynamic bytes) {
    if (bytes == null) return '';
    final b = bytes is int ? bytes : int.tryParse(bytes.toString()) ?? 0;
    if (b >= 1048576) return '${(b / 1048576).toStringAsFixed(0)} MB';
    if (b >= 1024) return '${(b / 1024).toStringAsFixed(0)} KB';
    return '$b B';
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    final lp = context.read<LocaleProvider>();

    return Scaffold(
      backgroundColor: AkarTheme.bgDark,
      appBar: AppBar(
        title: Text(l.t('public_upload_title')),
        automaticallyImplyLeading: false,
        actions: [
          IconButton(
            icon: const Icon(Icons.language),
            tooltip: l.t('lang_switch'),
            onPressed: () => lp.toggle(),
          ),
        ],
      ),
      body: _buildBody(l),
    );
  }

  Widget _buildBody(AppLocalizations l) {
    switch (_state) {
      case _PageState.loading:
        return const Center(child: CircularProgressIndicator());

      case _PageState.invalid:
        return _ErrorView(
          icon: Icons.link_off,
          message: l.t('public_upload_invalid_link'),
          color: AkarTheme.danger,
        );

      case _PageState.expired:
        return _ErrorView(
          icon: Icons.timer_off,
          message: l.t('public_upload_expired_link'),
          color: AkarTheme.warning,
        );

      case _PageState.revoked:
        return _ErrorView(
          icon: Icons.block,
          message: l.t('public_upload_revoked_link'),
          color: AkarTheme.danger,
        );

      case _PageState.valid:
        return _buildUploadView(l);
    }
  }

  Widget _buildUploadView(AppLocalizations l) {
    final info = _info!;
    final extensions = info['allowedFileExtensions'];
    final extStr = extensions is List ? extensions.join(', ') : extensions?.toString() ?? '';

    return SingleChildScrollView(
      padding: const EdgeInsets.all(20),
      child: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 500),
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            // ─── Branding ───
            Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  colors: [AkarTheme.primary.withValues(alpha: 0.3), AkarTheme.bgCard],
                  begin: Alignment.topCenter,
                  end: Alignment.bottomCenter,
                ),
                borderRadius: BorderRadius.circular(16),
                border: Border.all(color: AkarTheme.border),
              ),
              child: Column(children: [
                const Icon(Icons.cloud_upload, size: 48, color: AkarTheme.accent),
                const SizedBox(height: 8),
                Text(l.t('public_upload_for_project'),
                    style: const TextStyle(fontSize: 16, color: AkarTheme.textSecondary)),
                const SizedBox(height: 4),
                Text(info['projectName'] ?? '',
                    style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold, color: AkarTheme.textPrimary)),
              ]),
            ),

            const SizedBox(height: 16),

            // ─── Info Card ───
            Card(child: Padding(padding: const EdgeInsets.all(16), child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              _InfoRow(icon: Icons.person, label: l.t('public_upload_follower_name'), value: info['followerName'] ?? ''),
              const Divider(height: 16),
              _InfoRow(icon: Icons.folder, label: l.t('public_upload_project_name'), value: info['projectName'] ?? ''),
              const Divider(height: 16),
              _InfoRow(icon: Icons.description, label: l.t('public_upload_allowed_types'), value: extStr),
              const Divider(height: 16),
              _InfoRow(icon: Icons.storage, label: l.t('public_upload_max_size'), value: _formatSize(info['maxFileSizeBytes'])),
              const Divider(height: 16),
              _InfoRow(
                icon: Icons.timer,
                label: l.t('public_upload_expires_at'),
                value: info['expiresAtUtc'] != null ? _formatDate(info['expiresAtUtc']) : l.t('public_upload_no_expires'),
              ),
            ]))),

            const SizedBox(height: 20),

            // ─── File Picker ───
            if (_selectedFileName != null) ...[
              Card(child: Padding(
                padding: const EdgeInsets.all(12),
                child: Row(children: [
                  const Icon(Icons.insert_drive_file, color: AkarTheme.accent),
                  const SizedBox(width: 10),
                  Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    Text(l.t('public_upload_selected_file'), style: const TextStyle(fontSize: 11, color: AkarTheme.textMuted)),
                    Text(_selectedFileName!, style: const TextStyle(fontWeight: FontWeight.w600)),
                  ])),
                  IconButton(
                    icon: const Icon(Icons.close, size: 18, color: AkarTheme.textMuted),
                    onPressed: () => setState(() { _selectedFileName = null; _selectedFileBytes = null; }),
                  ),
                ]),
              )),
              const SizedBox(height: 12),
            ],

            // ─── Action Buttons ───
            if (!_uploaded || _selectedFileName != null)
              OutlinedButton.icon(
                icon: const Icon(Icons.attach_file),
                label: Text(l.t('public_upload_select_file')),
                style: OutlinedButton.styleFrom(
                  foregroundColor: AkarTheme.accent,
                  side: const BorderSide(color: AkarTheme.accent),
                  minimumSize: const Size(double.infinity, 48),
                ),
                onPressed: _uploading ? null : _pickFile,
              ),

            const SizedBox(height: 12),

            if (_selectedFileName != null)
              ElevatedButton.icon(
                icon: _uploading
                    ? const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2, color: AkarTheme.textPrimary))
                    : const Icon(Icons.cloud_upload),
                label: Text(l.t('public_upload_button')),
                onPressed: _uploading ? null : _upload,
              ),

            if (_uploaded && _selectedFileName == null) ...[
              const SizedBox(height: 16),
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: AkarTheme.success.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(color: AkarTheme.success.withValues(alpha: 0.3)),
                ),
                child: Row(children: [
                  const Icon(Icons.check_circle, color: AkarTheme.success, size: 28),
                  const SizedBox(width: 12),
                  Expanded(child: Text(l.t('public_upload_success'),
                      style: const TextStyle(color: AkarTheme.success, fontWeight: FontWeight.w600))),
                ]),
              ),
              const SizedBox(height: 12),
              TextButton.icon(
                icon: const Icon(Icons.upload_file),
                label: Text(l.t('public_upload_another')),
                onPressed: () => setState(() => _uploaded = false),
              ),
            ],

            const SizedBox(height: 32),

            // ─── Footer ───
            Center(child: Text(l.t('public_upload_powered_by'),
                style: const TextStyle(color: AkarTheme.textMuted, fontSize: 12))),
          ]),
        ),
      ),
    );
  }
}

class _ErrorView extends StatelessWidget {
  final IconData icon;
  final String message;
  final Color color;
  const _ErrorView({required this.icon, required this.message, required this.color});

  @override
  Widget build(BuildContext context) => Center(
    child: Padding(
      padding: const EdgeInsets.all(32),
      child: Column(mainAxisSize: MainAxisSize.min, children: [
        Icon(icon, size: 64, color: color),
        const SizedBox(height: 16),
        Text(message, textAlign: TextAlign.center,
            style: TextStyle(fontSize: 18, color: color, fontWeight: FontWeight.w600)),
      ]),
    ),
  );
}

class _InfoRow extends StatelessWidget {
  final IconData icon;
  final String label;
  final String value;
  const _InfoRow({required this.icon, required this.label, required this.value});

  @override
  Widget build(BuildContext context) => Row(children: [
    Icon(icon, size: 18, color: AkarTheme.textMuted),
    const SizedBox(width: 10),
    Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Text(label, style: const TextStyle(fontSize: 11, color: AkarTheme.textMuted)),
      const SizedBox(height: 2),
      Text(value, style: const TextStyle(fontSize: 14)),
    ])),
  ]);
}

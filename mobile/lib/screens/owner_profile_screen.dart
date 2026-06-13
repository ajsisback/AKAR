import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';
import '../core/widgets.dart';
import 'change_password_screen.dart';

class OwnerProfileScreen extends StatefulWidget {
  const OwnerProfileScreen({super.key});
  @override
  State<OwnerProfileScreen> createState() => _OwnerProfileScreenState();
}

class _OwnerProfileScreenState extends State<OwnerProfileScreen> {
  final _api = ApiService();
  Map<String, dynamic>? _profile;
  bool _loading = true;
  String? _loadError;
  bool _editing = false;
  bool _saving = false;

  final _nameCtrl = TextEditingController();
  final _phoneCtrl = TextEditingController();
  final _formKey = GlobalKey<FormState>();

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _nameCtrl.dispose();
    _phoneCtrl.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    setState(() { _loading = true; _loadError = null; });
    await _api.init();
    try {
      final p = await _api.getOwnerProfile();
      if (mounted) {
        setState(() {
          _profile = p;
          _nameCtrl.text = p['fullName'] ?? '';
          _phoneCtrl.text = p['phone'] ?? '';
          _loading = false;
        });
      }
    } on ApiException catch (e) {
      if (mounted) setState(() { _loadError = e.code; _loading = false; });
    } catch (_) {
      if (mounted) setState(() { _loadError = 'err_generic'; _loading = false; });
    }
  }

  void _showError(String code) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(localizeError(context, code)), backgroundColor: AkarTheme.danger),
    );
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _saving = true);
    final l = AppLocalizations.of(context);
    try {
      final updated = await _api.updateOwnerProfile(
        fullName: _nameCtrl.text.trim(),
        phone: _phoneCtrl.text.trim(),
      );
      if (mounted) {
        setState(() {
          _profile = updated;
          _editing = false;
          _saving = false;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('profile_updated')), backgroundColor: AkarTheme.success),
        );
      }
    } on ApiException catch (e) {
      if (mounted) {
        setState(() => _saving = false);
        _showError(e.code);
      }
    } catch (_) {
      if (mounted) {
        setState(() => _saving = false);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('profile_failed')), backgroundColor: AkarTheme.danger),
        );
      }
    }
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

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Scaffold(
      appBar: AppBar(
        title: Text(l.t('profile_title')),
        actions: [
          if (!_loading && _profile != null && !_editing)
            IconButton(
              icon: const Icon(Icons.edit),
              tooltip: l.t('profile_edit'),
              onPressed: () => setState(() => _editing = true),
            ),
        ],
      ),
      body: _loading
          ? const AkarLoadingState()
          : _loadError != null
              ? AkarErrorState(
                  message: localizeError(context, _loadError!),
                  onRetry: _load,
                )
              : _profile == null
                  ? AkarErrorState(message: l.t('err_generic'), onRetry: _load)
                  : _buildContent(l),
    );
  }

  Widget _buildContent(AppLocalizations l) {
    return Form(
      key: _formKey,
      child: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          // Avatar / header
          Center(
            child: Container(
              width: 80,
              height: 80,
              decoration: BoxDecoration(
                color: AkarTheme.accent.withValues(alpha: 0.15),
                shape: BoxShape.circle,
              ),
              child: const Icon(Icons.person, size: 40, color: AkarTheme.accent),
            ),
          ),
          const SizedBox(height: 8),
          Center(
            child: Text(
              _profile!['fullName'] ?? '',
              style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
            ),
          ),
          Center(
            child: Text(
              _profile!['email'] ?? '',
              style: const TextStyle(color: AkarTheme.textMuted, fontSize: 13),
            ),
          ),
          const SizedBox(height: 28),

          // Full Name
          TextFormField(
            controller: _nameCtrl,
            enabled: _editing,
            decoration: InputDecoration(
              labelText: l.t('profile_full_name'),
              prefixIcon: const Icon(Icons.person_outline),
            ),
            validator: (v) => (v == null || v.trim().isEmpty) ? l.t('val_required') : null,
          ),
          const SizedBox(height: 16),

          // Email — read-only always
          TextFormField(
            initialValue: _profile!['email'] ?? '',
            enabled: false,
            decoration: InputDecoration(
              labelText: l.t('profile_email'),
              prefixIcon: const Icon(Icons.email_outlined),
              helperText: l.t('profile_email_readonly'),
            ),
          ),
          const SizedBox(height: 16),

          // Phone
          TextFormField(
            controller: _phoneCtrl,
            enabled: _editing,
            decoration: InputDecoration(
              labelText: l.t('profile_phone'),
              prefixIcon: const Icon(Icons.phone_outlined),
            ),
            keyboardType: TextInputType.phone,
          ),
          const SizedBox(height: 16),

          // Created at
          _InfoRow(
            icon: Icons.calendar_today_outlined,
            label: l.t('profile_created_at'),
            value: _formatDate(_profile!['createdAtUtc']),
          ),
          const SizedBox(height: 8),

          // Updated at
          _InfoRow(
            icon: Icons.update_outlined,
            label: l.t('profile_updated_at'),
            value: _formatDate(_profile!['updatedAtUtc']),
          ),
          const SizedBox(height: 20),

          // ── Subscription Status (pilot placeholder — local-only) ──
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
            decoration: BoxDecoration(
              color: AkarTheme.success.withValues(alpha: 0.08),
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: AkarTheme.success.withValues(alpha: 0.25)),
            ),
            child: Row(children: [
              Container(
                width: 40,
                height: 40,
                decoration: BoxDecoration(
                  color: AkarTheme.success.withValues(alpha: 0.15),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(Icons.verified_user_outlined, color: AkarTheme.success, size: 22),
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(l.t('subscription_status'),
                        style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14)),
                    const SizedBox(height: 2),
                    Text(l.t('subscription_pilot_trial'),
                        style: const TextStyle(color: AkarTheme.success, fontSize: 12, fontWeight: FontWeight.w500)),
                  ],
                ),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(
                  color: AkarTheme.success.withValues(alpha: 0.15),
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Text(l.t('subscription_active'),
                    style: const TextStyle(color: AkarTheme.success, fontSize: 11, fontWeight: FontWeight.bold)),
              ),
            ]),
          ),
          const SizedBox(height: 28),

          // Save button (editing mode)
          if (_editing)
            FilledButton.icon(
              onPressed: _saving ? null : _save,
              icon: _saving
                  ? const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                  : const Icon(Icons.save),
              label: Text(l.t('profile_save')),
              style: FilledButton.styleFrom(
                minimumSize: const Size.fromHeight(48),
              ),
            ),

          if (_editing) const SizedBox(height: 12),

          if (_editing)
            OutlinedButton(
              onPressed: () {
                _nameCtrl.text = _profile!['fullName'] ?? '';
                _phoneCtrl.text = _profile!['phone'] ?? '';
                setState(() => _editing = false);
              },
              style: OutlinedButton.styleFrom(minimumSize: const Size.fromHeight(48)),
              child: Text(l.t('btn_cancel')),
            ),

          const SizedBox(height: 24),

          // Change Password card
          Card(
            child: InkWell(
              borderRadius: BorderRadius.circular(12),
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const ChangePasswordScreen()),
              ),
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                child: Row(children: [
                  Container(
                    width: 44,
                    height: 44,
                    decoration: BoxDecoration(
                      color: AkarTheme.warning.withValues(alpha: 0.12),
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: const Icon(Icons.lock_outline, color: AkarTheme.warning, size: 24),
                  ),
                  const SizedBox(width: 14),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(l.t('change_password_title'),
                            style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 15)),
                        const SizedBox(height: 2),
                        Text(l.t('profile_account_settings'),
                            style: const TextStyle(color: AkarTheme.textMuted, fontSize: 12)),
                      ],
                    ),
                  ),
                  const Icon(Icons.chevron_right, color: AkarTheme.textMuted),
                ]),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _InfoRow extends StatelessWidget {
  final IconData icon;
  final String label;
  final String value;
  const _InfoRow({required this.icon, required this.label, required this.value});

  @override
  Widget build(BuildContext context) => Padding(
        padding: const EdgeInsets.symmetric(vertical: 4),
        child: Row(children: [
          Icon(icon, size: 18, color: AkarTheme.textMuted),
          const SizedBox(width: 10),
          Text(label, style: const TextStyle(color: AkarTheme.textMuted, fontSize: 13)),
          const Spacer(),
          Text(value, style: const TextStyle(fontSize: 13)),
        ]),
      );
}

import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';
import '../core/widgets.dart';

class ChangePasswordScreen extends StatefulWidget {
  const ChangePasswordScreen({super.key});
  @override
  State<ChangePasswordScreen> createState() => _ChangePasswordScreenState();
}

class _ChangePasswordScreenState extends State<ChangePasswordScreen> {
  final _api = ApiService();
  final _formKey = GlobalKey<FormState>();
  final _currentCtrl = TextEditingController();
  final _newCtrl = TextEditingController();
  final _confirmCtrl = TextEditingController();

  bool _saving = false;
  bool _showCurrent = false;
  bool _showNew = false;
  bool _showConfirm = false;

  @override
  void initState() {
    super.initState();
    _api.init();
  }

  @override
  void dispose() {
    _currentCtrl.dispose();
    _newCtrl.dispose();
    _confirmCtrl.dispose();
    super.dispose();
  }



  String? _validateNewPassword(String? v, AppLocalizations l) {
    if (v == null || v.isEmpty) return l.t('val_required');
    if (v.length < 8) return l.t('val_password_min');
    if (!v.contains(RegExp(r'[A-Z]'))) return l.t('val_password_uppercase');
    if (!v.contains(RegExp(r'[a-z]'))) return l.t('val_password_lowercase');
    if (!v.contains(RegExp(r'[0-9]'))) return l.t('val_password_number');
    if (!v.contains(RegExp(r'[!@#$%^&*(),.?":{}|<>_\-+=\[\]\\\/~`]'))) {
      return l.t('val_password_symbol');
    }
    return null;
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _saving = true);
    final l = AppLocalizations.of(context);

    try {
      await _api.changeOwnerPassword(
        currentPassword: _currentCtrl.text,
        newPassword: _newCtrl.text,
        confirmNewPassword: _confirmCtrl.text,
      );
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('change_password_success')), backgroundColor: AkarTheme.success),
        );
        _currentCtrl.clear();
        _newCtrl.clear();
        _confirmCtrl.clear();
        Navigator.pop(context);
      }
    } on ApiException catch (e) {
      if (mounted) {
        setState(() => _saving = false);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(localizeError(context, e.code)), backgroundColor: AkarTheme.danger),
        );
      }
    } catch (_) {
      if (mounted) {
        setState(() => _saving = false);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('change_password_failed')), backgroundColor: AkarTheme.danger),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Scaffold(
      appBar: AppBar(title: Text(l.t('change_password_title'))),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(20),
          children: [
            // Header icon
            Center(
              child: Container(
                width: 72,
                height: 72,
                decoration: BoxDecoration(
                  color: AkarTheme.warning.withValues(alpha: 0.15),
                  shape: BoxShape.circle,
                ),
                child: const Icon(Icons.lock_outline, size: 36, color: AkarTheme.warning),
              ),
            ),
            const SizedBox(height: 24),

            // Current Password
            TextFormField(
              controller: _currentCtrl,
              obscureText: !_showCurrent,
              decoration: InputDecoration(
                labelText: l.t('change_password_current'),
                prefixIcon: const Icon(Icons.lock_outline),
                suffixIcon: IconButton(
                  icon: Icon(_showCurrent ? Icons.visibility_off : Icons.visibility),
                  tooltip: _showCurrent ? l.t('hide_password') : l.t('show_password'),
                  onPressed: () => setState(() => _showCurrent = !_showCurrent),
                ),
              ),
              validator: (v) => (v == null || v.isEmpty) ? l.t('val_required') : null,
            ),
            const SizedBox(height: 16),

            // New Password
            TextFormField(
              controller: _newCtrl,
              obscureText: !_showNew,
              decoration: InputDecoration(
                labelText: l.t('change_password_new'),
                prefixIcon: const Icon(Icons.lock_reset),
                suffixIcon: IconButton(
                  icon: Icon(_showNew ? Icons.visibility_off : Icons.visibility),
                  tooltip: _showNew ? l.t('hide_password') : l.t('show_password'),
                  onPressed: () => setState(() => _showNew = !_showNew),
                ),
              ),
              validator: (v) => _validateNewPassword(v, l),
            ),
            const SizedBox(height: 16),

            // Confirm New Password
            TextFormField(
              controller: _confirmCtrl,
              obscureText: !_showConfirm,
              decoration: InputDecoration(
                labelText: l.t('change_password_confirm'),
                prefixIcon: const Icon(Icons.lock_reset),
                suffixIcon: IconButton(
                  icon: Icon(_showConfirm ? Icons.visibility_off : Icons.visibility),
                  tooltip: _showConfirm ? l.t('hide_password') : l.t('show_password'),
                  onPressed: () => setState(() => _showConfirm = !_showConfirm),
                ),
              ),
              validator: (v) {
                if (v == null || v.isEmpty) return l.t('val_required');
                if (v != _newCtrl.text) return l.t('val_password_match');
                return null;
              },
            ),
            const SizedBox(height: 28),

            // Submit
            FilledButton.icon(
              onPressed: _saving ? null : _submit,
              icon: _saving
                  ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                    )
                  : const Icon(Icons.check),
              label: Text(l.t('change_password_button')),
              style: FilledButton.styleFrom(minimumSize: const Size.fromHeight(48)),
            ),
          ],
        ),
      ),
    );
  }
}

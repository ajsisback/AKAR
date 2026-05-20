import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';

class LoginScreen extends StatefulWidget {
  final VoidCallback onLoginSuccess;
  final VoidCallback onGoRegister;
  const LoginScreen({super.key, required this.onLoginSuccess, required this.onGoRegister});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _email = TextEditingController();
  final _password = TextEditingController();
  bool _loading = false;
  String? _error;

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() { _loading = true; _error = null; });
    try {
      final api = ApiService();
      await api.init();
      await api.login(email: _email.text.trim(), password: _password.text);
      if (mounted) widget.onLoginSuccess();
    } on ApiException catch (e) {
      if (!mounted) return;
      final l = AppLocalizations.of(context);
      setState(() => _error = e.code == 'AUTH_INVALID_CREDENTIALS'
          ? l.t('err_invalid_credentials') : l.t('err_generic'));
    } catch (_) {
      if (!mounted) return;
      setState(() => _error = AppLocalizations.of(context).t('err_network'));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Scaffold(
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 400),
              child: Form(
                key: _formKey,
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(Icons.construction, size: 56, color: Theme.of(context).colorScheme.primary),
                    const SizedBox(height: 12),
                    Text(l.t('app_title'), style: const TextStyle(fontSize: 28, fontWeight: FontWeight.bold, color: Color(0xFFD4A843))),
                    const SizedBox(height: 4),
                    Text(l.t('app_subtitle'), style: const TextStyle(color: Color(0xFF8B95A5), fontSize: 13)),
                    const SizedBox(height: 32),
                    Text(l.t('login_title'), style: const TextStyle(fontSize: 22, fontWeight: FontWeight.w600)),
                    const SizedBox(height: 24),
                    if (_error != null) ...[
                      Container(
                        width: double.infinity,
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(color: Colors.red.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(8), border: Border.all(color: Colors.red.withValues(alpha: 0.3))),
                        child: Text(_error!, style: const TextStyle(color: Colors.red, fontSize: 13)),
                      ),
                      const SizedBox(height: 16),
                    ],
                    TextFormField(
                      controller: _email,
                      decoration: InputDecoration(labelText: l.t('login_email'), prefixIcon: const Icon(Icons.email_outlined)),
                      keyboardType: TextInputType.emailAddress,
                      validator: (v) => v == null || v.isEmpty ? l.t('val_required') : (!v.contains('@') ? l.t('val_email_invalid') : null),
                    ),
                    const SizedBox(height: 16),
                    TextFormField(
                      controller: _password,
                      decoration: InputDecoration(labelText: l.t('login_password'), prefixIcon: const Icon(Icons.lock_outline)),
                      obscureText: true,
                      validator: (v) => v == null || v.isEmpty ? l.t('val_required') : null,
                    ),
                    const SizedBox(height: 24),
                    ElevatedButton(
                      onPressed: _loading ? null : _submit,
                      child: _loading ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white)) : Text(l.t('login_button')),
                    ),
                    const SizedBox(height: 16),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Text(l.t('login_no_account'), style: const TextStyle(color: Color(0xFF8B95A5), fontSize: 13)),
                        TextButton(onPressed: widget.onGoRegister, child: Text(l.t('login_register_link'))),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  @override
  void dispose() { _email.dispose(); _password.dispose(); super.dispose(); }
}

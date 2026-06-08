import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/widgets.dart';

class RegisterScreen extends StatefulWidget {
  final VoidCallback onRegisterSuccess;
  final VoidCallback onGoLogin;
  const RegisterScreen({super.key, required this.onRegisterSuccess, required this.onGoLogin});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _formKey = GlobalKey<FormState>();
  final _name = TextEditingController();
  final _email = TextEditingController();
  final _phone = TextEditingController();
  final _password = TextEditingController();
  final _company = TextEditingController();
  bool _loading = false;
  String? _error;

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() { _loading = true; _error = null; });
    try {
      final api = ApiService();
      await api.init();
      await api.register(
        fullName: _name.text.trim(),
        email: _email.text.trim(),
        phone: _phone.text.trim(),
        password: _password.text,
        companyName: _company.text.trim(),
      );
      if (mounted) widget.onRegisterSuccess();
    } on ApiException catch (e) {
      if (!mounted) return;
      final l = AppLocalizations.of(context);
      setState(() => _error = e.code == 'AUTH_EMAIL_EXISTS'
          ? l.t('err_email_exists') : localizeError(context, e.code));
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
                    Text(l.t('app_title'), style: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold, color: Color(0xFFD4A843))),
                    const SizedBox(height: 4),
                    Text(l.t('app_subtitle'), style: const TextStyle(color: Color(0xFF8B95A5), fontSize: 13)),
                    const SizedBox(height: 24),
                    Text(l.t('register_title'), style: const TextStyle(fontSize: 22, fontWeight: FontWeight.w600)),
                    const SizedBox(height: 20),
                    if (_error != null) ...[
                      Container(
                        width: double.infinity,
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(color: Colors.red.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(8), border: Border.all(color: Colors.red.withValues(alpha: 0.3))),
                        child: Text(_error!, style: const TextStyle(color: Colors.red, fontSize: 13)),
                      ),
                      const SizedBox(height: 12),
                    ],
                    TextFormField(
                      controller: _name,
                      decoration: InputDecoration(labelText: l.t('register_fullname'), prefixIcon: const Icon(Icons.person_outline)),
                      validator: (v) => v == null || v.isEmpty ? l.t('val_required') : null,
                    ),
                    const SizedBox(height: 14),
                    TextFormField(
                      controller: _email,
                      decoration: InputDecoration(labelText: l.t('register_email'), prefixIcon: const Icon(Icons.email_outlined)),
                      keyboardType: TextInputType.emailAddress,
                      validator: (v) => v == null || v.isEmpty ? l.t('val_required') : (!v.contains('@') ? l.t('val_email_invalid') : null),
                    ),
                    const SizedBox(height: 14),
                    TextFormField(
                      controller: _phone,
                      decoration: InputDecoration(labelText: l.t('register_phone'), prefixIcon: const Icon(Icons.phone_outlined)),
                      keyboardType: TextInputType.phone,
                      validator: (v) => v == null || v.isEmpty ? l.t('val_required') : null,
                    ),
                    const SizedBox(height: 14),
                    TextFormField(
                      controller: _password,
                      decoration: InputDecoration(labelText: l.t('register_password'), prefixIcon: const Icon(Icons.lock_outline)),
                      obscureText: true,
                      validator: (v) => v == null || v.isEmpty ? l.t('val_required') : (v.length < 8 ? l.t('val_password_min') : null),
                    ),
                    const SizedBox(height: 14),
                    TextFormField(
                      controller: _company,
                      decoration: InputDecoration(labelText: l.t('register_company'), prefixIcon: const Icon(Icons.business_outlined)),
                    ),
                    const SizedBox(height: 24),
                    AkarPrimaryButton(label: l.t('register_button'), loading: _loading, onPressed: _submit),
                    const SizedBox(height: 12),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Text(l.t('register_has_account'), style: const TextStyle(color: Color(0xFF8B95A5), fontSize: 13)),
                        TextButton(onPressed: widget.onGoLogin, child: Text(l.t('register_login_link'))),
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
  void dispose() { _name.dispose(); _email.dispose(); _phone.dispose(); _password.dispose(); _company.dispose(); super.dispose(); }
}

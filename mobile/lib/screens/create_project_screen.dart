import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/widgets.dart';

class CreateProjectScreen extends StatefulWidget {
  final VoidCallback onCreated;
  const CreateProjectScreen({super.key, required this.onCreated});
  @override
  State<CreateProjectScreen> createState() => _CreateProjectScreenState();
}

class _CreateProjectScreenState extends State<CreateProjectScreen> {
  final _formKey = GlobalKey<FormState>();
  final _name = TextEditingController();
  final _city = TextEditingController();
  final _location = TextEditingController();
  final _mapLink = TextEditingController();
  String _type = '';
  String _stage = 'NotStarted';
  bool _loading = false;
  String? _error;

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() { _loading = true; _error = null; });
    try {
      final api = ApiService();
      await api.init();
      await api.createProject({
        'projectName': _name.text.trim(),
        'projectType': _type,
        'city': _city.text.trim(),
        'locationText': _location.text.trim(),
        'mapLink': _mapLink.text.trim(),
        'currentStage': _stage,
      });
      if (mounted) widget.onCreated();
    } on ApiException catch (e) {
      setState(() => _error = localizeError(context, e.code));
    } catch (_) {
      setState(() => _error = AppLocalizations.of(context).t('err_generic'));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Scaffold(
      appBar: AppBar(title: Text(l.t('projects_new'))),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Form(
          key: _formKey,
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            if (_error != null) ...[
              Container(padding: const EdgeInsets.all(12), decoration: BoxDecoration(color: Colors.red.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(8)),
                child: Text(_error!, style: const TextStyle(color: Colors.red, fontSize: 13))),
              const SizedBox(height: 16),
            ],
            TextFormField(controller: _name, decoration: InputDecoration(labelText: '${l.t('project_name')} *'),
              validator: (v) => v == null || v.isEmpty ? l.t('val_required') : null),
            const SizedBox(height: 16),
            DropdownButtonFormField<String>(initialValue: _type.isEmpty ? null : _type, decoration: InputDecoration(labelText: '${l.t('project_type')} *'),
              items: [
                DropdownMenuItem(value: 'Villa', child: Text(l.t('type_villa'))),
                DropdownMenuItem(value: 'Duplex', child: Text(l.t('type_duplex'))),
                DropdownMenuItem(value: 'SmallBuilding', child: Text(l.t('type_small_building'))),
              ], onChanged: (v) => setState(() => _type = v ?? ''),
              validator: (v) => v == null || v.isEmpty ? l.t('val_required') : null),
            const SizedBox(height: 16),
            TextFormField(controller: _city, decoration: InputDecoration(labelText: l.t('project_city'))),
            const SizedBox(height: 16),
            TextFormField(controller: _location, decoration: InputDecoration(labelText: l.t('project_location'))),
            const SizedBox(height: 16),
            TextFormField(controller: _mapLink, decoration: InputDecoration(labelText: l.t('project_map_link'))),
            const SizedBox(height: 16),
            DropdownButtonFormField<String>(initialValue: _stage, decoration: InputDecoration(labelText: l.t('project_stage')),
              items: [
                DropdownMenuItem(value: 'NotStarted', child: Text(l.t('stage_not_started'))),
                DropdownMenuItem(value: 'Structural', child: Text(l.t('stage_structural'))),
                DropdownMenuItem(value: 'Finishing', child: Text(l.t('stage_finishing'))),
                DropdownMenuItem(value: 'Completed', child: Text(l.t('stage_completed'))),
              ], onChanged: (v) => setState(() => _stage = v ?? 'NotStarted')),
            const SizedBox(height: 28),
            AkarPrimaryButton(label: l.t('btn_create'), loading: _loading, onPressed: _submit),
          ]),
        ),
      ),
    );
  }

  @override
  void dispose() { _name.dispose(); _city.dispose(); _location.dispose(); _mapLink.dispose(); super.dispose(); }
}

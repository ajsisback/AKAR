import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';
import '../core/widgets.dart';

class ProjectSettingsScreen extends StatefulWidget {
  final String projectId;
  const ProjectSettingsScreen({super.key, required this.projectId});
  @override
  State<ProjectSettingsScreen> createState() => _ProjectSettingsScreenState();
}

class _ProjectSettingsScreenState extends State<ProjectSettingsScreen> {
  final _api = ApiService();
  final _formKey = GlobalKey<FormState>();
  Map<String, dynamic>? _settings;
  bool _loading = true;
  String? _loadError;
  bool _saving = false;

  final _nameCtrl = TextEditingController();
  final _cityCtrl = TextEditingController();
  final _locationCtrl = TextEditingController();
  final _mapLinkCtrl = TextEditingController();
  String _projectType = 'Villa';

  static const _projectTypes = ['Villa', 'Duplex', 'SmallBuilding'];

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _nameCtrl.dispose();
    _cityCtrl.dispose();
    _locationCtrl.dispose();
    _mapLinkCtrl.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    setState(() { _loading = true; _loadError = null; });
    await _api.init();
    try {
      final s = await _api.getProjectSettings(widget.projectId);
      if (mounted) {
        setState(() {
          _settings = s;
          _nameCtrl.text = s['projectName'] ?? '';
          _projectType = _projectTypes.contains(s['projectType']) ? s['projectType'] : 'Villa';
          _cityCtrl.text = s['city'] ?? '';
          _locationCtrl.text = s['locationText'] ?? '';
          _mapLinkCtrl.text = s['mapLink'] ?? '';
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

  String _typeLabel(String t, AppLocalizations l) {
    switch (t) {
      case 'Villa':
        return l.t('type_villa');
      case 'Duplex':
        return l.t('type_duplex');
      case 'SmallBuilding':
        return l.t('type_small_building');
      default:
        return t;
    }
  }

  String _stageLabel(String s, AppLocalizations l) {
    switch (s) {
      case 'NotStarted':
        return l.t('stage_not_started');
      case 'Structural':
        return l.t('stage_structural');
      case 'Finishing':
        return l.t('stage_finishing');
      case 'Completed':
        return l.t('stage_completed');
      default:
        return s;
    }
  }

  String? _validateMapUrl(String? v, AppLocalizations l) {
    if (v == null || v.trim().isEmpty) return null; // optional field
    final trimmed = v.trim();
    if (!trimmed.startsWith('http://') && !trimmed.startsWith('https://')) {
      return l.t('val_invalid_map_url');
    }
    return null;
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _saving = true);
    final l = AppLocalizations.of(context);
    try {
      final payload = <String, dynamic>{
        'projectName': _nameCtrl.text.trim(),
        'projectType': _projectType,
        'city': _cityCtrl.text.trim(),
        'locationText': _locationCtrl.text.trim(),
        'mapLink': _mapLinkCtrl.text.trim(),
      };
      final updated = await _api.updateProjectSettings(widget.projectId, payload);
      if (mounted) {
        setState(() {
          _settings = updated;
          _saving = false;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('project_settings_updated')), backgroundColor: AkarTheme.success),
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
          SnackBar(content: Text(l.t('project_settings_failed')), backgroundColor: AkarTheme.danger),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Scaffold(
      appBar: AppBar(title: Text(l.t('project_settings_title'))),
      body: _loading
          ? const AkarLoadingState()
          : _loadError != null
              ? AkarErrorState(
                  message: localizeError(context, _loadError!),
                  onRetry: _load,
                )
              : _settings == null
                  ? AkarErrorState(message: l.t('err_generic'), onRetry: _load)
                  : _buildForm(l),
    );
  }

  Widget _buildForm(AppLocalizations l) {
    return Form(
      key: _formKey,
      child: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          // Current Stage — read-only
          Card(
            color: AkarTheme.accent.withValues(alpha: 0.08),
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(children: [
                    const Icon(Icons.flag_outlined, color: AkarTheme.accent, size: 20),
                    const SizedBox(width: 8),
                    Text(l.t('project_stage'),
                        style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14)),
                    const Spacer(),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                      decoration: BoxDecoration(
                        color: AkarTheme.accent.withValues(alpha: 0.15),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Text(
                        _stageLabel(_settings!['currentStage'] ?? '', l),
                        style: const TextStyle(
                          color: AkarTheme.accent,
                          fontWeight: FontWeight.w600,
                          fontSize: 13,
                        ),
                      ),
                    ),
                  ]),
                  const SizedBox(height: 8),
                  Text(
                    l.t('project_stage_managed'),
                    style: const TextStyle(color: AkarTheme.textMuted, fontSize: 12),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 24),

          // Project Name
          TextFormField(
            controller: _nameCtrl,
            decoration: InputDecoration(
              labelText: l.t('project_name'),
              prefixIcon: const Icon(Icons.business_outlined),
            ),
            validator: (v) => (v == null || v.trim().isEmpty) ? l.t('val_required') : null,
          ),
          const SizedBox(height: 16),

          // Project Type dropdown
          DropdownButtonFormField<String>(
            initialValue: _projectType,
            decoration: InputDecoration(
              labelText: l.t('project_type'),
              prefixIcon: const Icon(Icons.category_outlined),
            ),
            items: _projectTypes
                .map((t) => DropdownMenuItem(value: t, child: Text(_typeLabel(t, l))))
                .toList(),
            onChanged: (v) {
              if (v != null) setState(() => _projectType = v);
            },
          ),
          const SizedBox(height: 16),

          // City
          TextFormField(
            controller: _cityCtrl,
            decoration: InputDecoration(
              labelText: l.t('project_city'),
              prefixIcon: const Icon(Icons.location_city_outlined),
            ),
          ),
          const SizedBox(height: 16),

          // Location Text
          TextFormField(
            controller: _locationCtrl,
            decoration: InputDecoration(
              labelText: l.t('project_location'),
              prefixIcon: const Icon(Icons.place_outlined),
            ),
          ),
          const SizedBox(height: 16),

          // Map Link
          TextFormField(
            controller: _mapLinkCtrl,
            decoration: InputDecoration(
              labelText: l.t('project_map_link'),
              prefixIcon: const Icon(Icons.map_outlined),
            ),
            keyboardType: TextInputType.url,
            validator: (v) => _validateMapUrl(v, l),
          ),
          const SizedBox(height: 28),

          // Save button
          FilledButton.icon(
            onPressed: _saving ? null : _save,
            icon: _saving
                ? const SizedBox(
                    width: 18,
                    height: 18,
                    child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                  )
                : const Icon(Icons.save),
            label: Text(l.t('project_settings_save')),
            style: FilledButton.styleFrom(minimumSize: const Size.fromHeight(48)),
          ),
        ],
      ),
    );
  }
}

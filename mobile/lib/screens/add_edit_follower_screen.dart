import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';

/// Add or Edit follower. Pass [follower] map to edit, or null to add.
class AddEditFollowerScreen extends StatefulWidget {
  final String projectId;
  final Map<String, dynamic>? follower;
  const AddEditFollowerScreen({super.key, required this.projectId, this.follower});

  bool get isEdit => follower != null;

  @override
  State<AddEditFollowerScreen> createState() => _AddEditFollowerScreenState();
}

class _AddEditFollowerScreenState extends State<AddEditFollowerScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nameCtrl = TextEditingController();
  final _phoneCtrl = TextEditingController();
  final _notesCtrl = TextEditingController();
  String _type = 'Supervisor';
  bool _isActive = true;
  bool _saving = false;

  static const _types = [
    'Supervisor', 'Relative', 'Contractor', 'Designer', 'EngineeringOffice', 'Other',
  ];

  @override
  void initState() {
    super.initState();
    if (widget.isEdit) {
      final f = widget.follower!;
      _nameCtrl.text = f['fullName'] ?? '';
      _phoneCtrl.text = f['phone'] ?? '';
      _notesCtrl.text = f['notes'] ?? '';
      _type = f['followerType'] ?? 'Supervisor';
      _isActive = f['isActive'] == true;
    }
  }

  @override
  void dispose() { _nameCtrl.dispose(); _phoneCtrl.dispose(); _notesCtrl.dispose(); super.dispose(); }

  String _typeLabelFor(String type, AppLocalizations l) {
    switch (type) {
      case 'Supervisor': return l.t('follower_type_supervisor');
      case 'Relative': return l.t('follower_type_relative');
      case 'Contractor': return l.t('follower_type_contractor');
      case 'Designer': return l.t('follower_type_designer');
      case 'EngineeringOffice': return l.t('follower_type_engineering_office');
      case 'Other': return l.t('follower_type_other');
      default: return type;
    }
  }

  String _mapError(String code, AppLocalizations l) {
    switch (code) {
      case 'FOLLOWER_PHONE_ALREADY_EXISTS': return l.t('err_follower_phone_exists');
      case 'FOLLOWER_NOT_FOUND': return l.t('err_follower_not_found');
      case 'PROJECT_NOT_FOUND': return l.t('err_project_not_found');
      default: return l.t('err_generic');
    }
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _saving = true);
    final l = AppLocalizations.of(context);

    try {
      final api = ApiService();
      await api.init();

      final Map<String, dynamic> data = {
        'fullName': _nameCtrl.text.trim(),
        'phone': _phoneCtrl.text.trim(),
        'followerType': _type,
        'notes': _notesCtrl.text.trim(),
      };

      if (widget.isEdit) {
        data['isActive'] = _isActive;
        await api.updateProjectFollower(widget.projectId, widget.follower!['id'], data);
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(l.t('follower_updated')), backgroundColor: AkarTheme.success),
          );
          Navigator.pop(context, true);
        }
      } else {
        await api.createProjectFollower(widget.projectId, data);
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(l.t('follower_created')), backgroundColor: AkarTheme.success),
          );
          Navigator.pop(context, true);
        }
      }
    } on ApiException catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(_mapError(e.code, l)), backgroundColor: AkarTheme.danger),
        );
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('err_generic')), backgroundColor: AkarTheme.danger),
        );
      }
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Scaffold(
      appBar: AppBar(title: Text(widget.isEdit ? l.t('follower_edit') : l.t('follower_add'))),
      body: Form(
        key: _formKey,
        child: ListView(padding: const EdgeInsets.all(20), children: [
          TextFormField(
            controller: _nameCtrl,
            decoration: InputDecoration(labelText: l.t('follower_full_name')),
            validator: (v) => v == null || v.trim().isEmpty ? l.t('val_required') : null,
          ),
          const SizedBox(height: 16),
          TextFormField(
            controller: _phoneCtrl,
            decoration: InputDecoration(labelText: l.t('follower_phone')),
            keyboardType: TextInputType.phone,
            validator: (v) => v == null || v.trim().isEmpty ? l.t('val_required') : null,
          ),
          const SizedBox(height: 16),
          DropdownButtonFormField<String>(
            initialValue: _type,
            decoration: InputDecoration(labelText: l.t('follower_type')),
            dropdownColor: AkarTheme.bgCard,
            items: _types.map((t) => DropdownMenuItem(value: t, child: Text(_typeLabelFor(t, l)))).toList(),
            onChanged: (v) { if (v != null) setState(() => _type = v); },
          ),
          const SizedBox(height: 16),
          TextFormField(
            controller: _notesCtrl,
            decoration: InputDecoration(labelText: l.t('follower_notes')),
            maxLines: 3,
          ),
          if (widget.isEdit) ...[
            const SizedBox(height: 16),
            SwitchListTile(
              title: Text(l.t('follower_active')),
              value: _isActive,
              activeTrackColor: AkarTheme.success,
              onChanged: (v) => setState(() => _isActive = v),
              contentPadding: EdgeInsets.zero,
            ),
          ],
          const SizedBox(height: 24),
          ElevatedButton(
            onPressed: _saving ? null : _save,
            child: _saving
                ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2, color: AkarTheme.textPrimary))
                : Text(widget.isEdit ? l.t('btn_save') : l.t('btn_create')),
          ),
        ]),
      ),
    );
  }
}

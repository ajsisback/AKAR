import 'dart:convert';
import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';

/// Create or Edit a project contract.
class ContractFormScreen extends StatefulWidget {
  final String projectId;
  final Map<String, dynamic>? existingContract; // null = create mode
  const ContractFormScreen({super.key, required this.projectId, this.existingContract});
  @override
  State<ContractFormScreen> createState() => _ContractFormScreenState();
}

class _ContractFormScreenState extends State<ContractFormScreen> {
  final _formKey = GlobalKey<FormState>();
  bool _loading = false;
  bool _templatesLoading = true;
  List<Map<String, dynamic>> _templates = [];
  String? _selectedTemplateId;

  // Fields
  final _titleCtrl = TextEditingController();
  final _partyNameCtrl = TextEditingController();
  final _partyPhoneCtrl = TextEditingController();
  final _partyNationalIdCtrl = TextEditingController();
  final _valueCtrl = TextEditingController();
  final _scopeCtrl = TextEditingController();
  final _paymentCtrl = TextEditingController();
  final _ownerObligCtrl = TextEditingController();
  final _contractorObligCtrl = TextEditingController();
  final _notesCtrl = TextEditingController();
  DateTime? _startDate;
  DateTime? _endDate;

  bool get _isEdit => widget.existingContract != null;

  @override
  void initState() {
    super.initState();
    if (_isEdit) {
      _populateFromExisting();
    }
    _loadTemplates();
  }

  void _populateFromExisting() {
    final c = widget.existingContract!;
    _titleCtrl.text = c['contractTitle'] ?? '';
    _partyNameCtrl.text = c['partyName'] ?? '';
    _partyPhoneCtrl.text = c['partyPhone'] ?? '';
    _partyNationalIdCtrl.text = c['partyNationalId'] ?? '';
    _valueCtrl.text = c['contractValue']?.toString() ?? '';
    _selectedTemplateId = c['contractTemplateId'];
    if (c['startDate'] != null) {
      try { _startDate = DateTime.parse(c['startDate']); } catch (_) {}
    }
    if (c['endDate'] != null) {
      try { _endDate = DateTime.parse(c['endDate']); } catch (_) {}
    }
    // Parse contractDataJson
    if (c['contractDataJson'] != null && c['contractDataJson'].toString().isNotEmpty) {
      try {
        final data = json.decode(c['contractDataJson']) as Map<String, dynamic>;
        _scopeCtrl.text = data['scopeOfWork'] ?? '';
        _paymentCtrl.text = data['paymentTerms'] ?? '';
        _ownerObligCtrl.text = data['ownerObligations'] ?? '';
        _contractorObligCtrl.text = data['contractorObligations'] ?? '';
        _notesCtrl.text = data['notes'] ?? '';
      } catch (_) {}
    }
  }

  Future<void> _loadTemplates() async {
    final api = ApiService();
    await api.init();
    try {
      final list = await api.getContractTemplates();
      if (mounted) setState(() { _templates = list; _templatesLoading = false; });
    } catch (_) {
      if (mounted) setState(() => _templatesLoading = false);
    }
  }

  String _mapError(String code, AppLocalizations l) {
    switch (code) {
      case 'CONTRACT_TEMPLATE_NOT_FOUND': return l.t('err_contract_template_not_found');
      case 'CONTRACT_NOT_FOUND': return l.t('err_contract_not_found');
      case 'CONTRACT_NOT_DRAFT': return l.t('err_contract_not_draft');
      case 'INVALID_CONTRACT_DATES': return l.t('err_invalid_contract_dates');
      case 'CONTRACT_TITLE_REQUIRED': return l.t('err_contract_title_required');
      case 'CONTRACT_PARTY_REQUIRED': return l.t('err_contract_party_required');
      case 'PROJECT_NOT_FOUND': return l.t('err_project_not_found');
      default: return l.t('err_generic');
    }
  }

  String _formatDate(DateTime? d) {
    if (d == null) return '';
    return '${d.year}-${d.month.toString().padLeft(2,'0')}-${d.day.toString().padLeft(2,'0')}';
  }

  Future<void> _pickDate(bool isStart) async {
    final now = DateTime.now();
    final picked = await showDatePicker(
      context: context,
      initialDate: (isStart ? _startDate : _endDate) ?? now,
      firstDate: DateTime(2020),
      lastDate: DateTime(2050),
    );
    if (picked != null) {
      setState(() {
        if (isStart) {
          _startDate = picked;
        } else {
          _endDate = picked;
        }
      });
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (!_isEdit && _selectedTemplateId == null) return;

    final l = AppLocalizations.of(context);

    // Validate dates
    if (_startDate != null && _endDate != null && _endDate!.isBefore(_startDate!)) {
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(l.t('val_end_after_start')), backgroundColor: AkarTheme.danger));
      return;
    }

    setState(() => _loading = true);

    final contractDataJson = json.encode({
      'scopeOfWork': _scopeCtrl.text,
      'paymentTerms': _paymentCtrl.text,
      'ownerObligations': _ownerObligCtrl.text,
      'contractorObligations': _contractorObligCtrl.text,
      'notes': _notesCtrl.text,
    });

    final payload = <String, dynamic>{
      'contractTitle': _titleCtrl.text.trim(),
      'partyName': _partyNameCtrl.text.trim(),
      'partyPhone': _partyPhoneCtrl.text.trim(),
      'partyNationalId': _partyNationalIdCtrl.text.trim(),
      'contractDataJson': contractDataJson,
    };

    if (_valueCtrl.text.isNotEmpty) {
      payload['contractValue'] = double.tryParse(_valueCtrl.text) ?? 0;
    }
    if (_startDate != null) payload['startDate'] = _formatDate(_startDate);
    if (_endDate != null) payload['endDate'] = _formatDate(_endDate);
    if (!_isEdit) payload['contractTemplateId'] = _selectedTemplateId;

    final api = ApiService();
    await api.init();

    try {
      if (_isEdit) {
        await api.updateProjectContract(widget.projectId, widget.existingContract!['id'], payload);
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(l.t('contract_updated')), backgroundColor: AkarTheme.success));
          Navigator.pop(context, true);
        }
      } else {
        await api.createProjectContract(widget.projectId, payload);
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(l.t('contract_created')), backgroundColor: AkarTheme.success));
          Navigator.pop(context, true);
        }
      }
    } on ApiException catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(_mapError(e.code, l)), backgroundColor: AkarTheme.danger));
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(l.t('err_generic')), backgroundColor: AkarTheme.danger));
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  void dispose() {
    _titleCtrl.dispose(); _partyNameCtrl.dispose(); _partyPhoneCtrl.dispose();
    _partyNationalIdCtrl.dispose(); _valueCtrl.dispose(); _scopeCtrl.dispose();
    _paymentCtrl.dispose(); _ownerObligCtrl.dispose(); _contractorObligCtrl.dispose();
    _notesCtrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Scaffold(
      appBar: AppBar(title: Text(_isEdit ? l.t('contracts_edit') : l.t('contracts_create'))),
      body: _templatesLoading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.all(20),
              child: Form(
                key: _formKey,
                child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                  // Disclaimer
                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: AkarTheme.warning.withValues(alpha: 0.08),
                      borderRadius: BorderRadius.circular(8),
                      border: Border.all(color: AkarTheme.warning.withValues(alpha: 0.3)),
                    ),
                    child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                      Row(children: [
                        const Icon(Icons.info_outline, size: 16, color: AkarTheme.warning),
                        const SizedBox(width: 6),
                        Text(l.t('contract_disclaimer'), style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13, color: AkarTheme.warning)),
                      ]),
                      const SizedBox(height: 6),
                      Text(l.t('contract_disclaimer_text'), style: const TextStyle(fontSize: 12, color: AkarTheme.textSecondary)),
                      const SizedBox(height: 2),
                      Text(l.t('contract_disclaimer_text_en'), style: const TextStyle(fontSize: 11, color: AkarTheme.textMuted)),
                    ]),
                  ),
                  const SizedBox(height: 20),

                  // Template selector (create only)
                  if (!_isEdit) ...[
                    Text(l.t('contracts_select_template'), style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14)),
                    const SizedBox(height: 8),
                    DropdownButtonFormField<String>(
                      initialValue: _selectedTemplateId,
                      decoration: InputDecoration(labelText: l.t('contract_template_name')),
                      validator: (v) => v == null ? l.t('val_required') : null,
                      items: _templates.map((t) => DropdownMenuItem<String>(
                        value: t['id'] as String,
                        child: Text(t['templateNameAr'] ?? t['templateNameEn'] ?? '', overflow: TextOverflow.ellipsis),
                      )).toList(),
                      onChanged: (v) => setState(() => _selectedTemplateId = v),
                    ),
                    const SizedBox(height: 16),
                  ],

                  TextFormField(
                    controller: _titleCtrl,
                    decoration: InputDecoration(labelText: l.t('contract_title')),
                    validator: (v) => v == null || v.trim().isEmpty ? l.t('val_required') : null,
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _partyNameCtrl,
                    decoration: InputDecoration(labelText: l.t('contract_party_name')),
                    validator: (v) => v == null || v.trim().isEmpty ? l.t('val_required') : null,
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _partyPhoneCtrl,
                    decoration: InputDecoration(labelText: l.t('contract_party_phone')),
                    keyboardType: TextInputType.phone,
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _partyNationalIdCtrl,
                    decoration: InputDecoration(labelText: l.t('contract_party_national_id')),
                    keyboardType: TextInputType.number,
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _valueCtrl,
                    decoration: InputDecoration(labelText: l.t('contract_value'), suffixText: l.t('contract_sar')),
                    keyboardType: TextInputType.number,
                    validator: (v) {
                      if (v != null && v.isNotEmpty) {
                        final n = double.tryParse(v);
                        if (n == null || n < 0) return l.t('val_value_positive');
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 12),

                  // Date pickers
                  Row(children: [
                    Expanded(child: InkWell(
                      onTap: () => _pickDate(true),
                      child: InputDecorator(
                        decoration: InputDecoration(labelText: l.t('contract_start_date')),
                        child: Text(_startDate != null ? _formatDate(_startDate) : '—', style: const TextStyle(fontSize: 14)),
                      ),
                    )),
                    const SizedBox(width: 12),
                    Expanded(child: InkWell(
                      onTap: () => _pickDate(false),
                      child: InputDecorator(
                        decoration: InputDecoration(labelText: l.t('contract_end_date')),
                        child: Text(_endDate != null ? _formatDate(_endDate) : '—', style: const TextStyle(fontSize: 14)),
                      ),
                    )),
                  ]),
                  const SizedBox(height: 16),

                  // Contract data fields
                  Text(l.t('contract_scope_of_work'), style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                  const SizedBox(height: 6),
                  TextFormField(controller: _scopeCtrl, maxLines: 3, decoration: const InputDecoration(border: OutlineInputBorder())),
                  const SizedBox(height: 12),
                  Text(l.t('contract_payment_terms'), style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                  const SizedBox(height: 6),
                  TextFormField(controller: _paymentCtrl, maxLines: 2, decoration: const InputDecoration(border: OutlineInputBorder())),
                  const SizedBox(height: 12),
                  Text(l.t('contract_owner_obligations'), style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                  const SizedBox(height: 6),
                  TextFormField(controller: _ownerObligCtrl, maxLines: 2, decoration: const InputDecoration(border: OutlineInputBorder())),
                  const SizedBox(height: 12),
                  Text(l.t('contract_contractor_obligations'), style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                  const SizedBox(height: 6),
                  TextFormField(controller: _contractorObligCtrl, maxLines: 2, decoration: const InputDecoration(border: OutlineInputBorder())),
                  const SizedBox(height: 12),
                  Text(l.t('contract_notes'), style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                  const SizedBox(height: 6),
                  TextFormField(controller: _notesCtrl, maxLines: 2, decoration: const InputDecoration(border: OutlineInputBorder())),
                  const SizedBox(height: 24),

                  ElevatedButton(
                    onPressed: _loading ? null : _submit,
                    child: _loading
                        ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(strokeWidth: 2))
                        : Text(_isEdit ? l.t('btn_save') : l.t('btn_create')),
                  ),
                  const SizedBox(height: 24),
                ]),
              ),
            ),
    );
  }
}

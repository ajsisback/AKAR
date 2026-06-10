import 'dart:convert';
import 'dart:typed_data';
import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import '../core/download_helper.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';
import 'contract_form_screen.dart';

class ContractDetailsScreen extends StatefulWidget {
  final String projectId;
  final String contractId;
  const ContractDetailsScreen({super.key, required this.projectId, required this.contractId});
  @override
  State<ContractDetailsScreen> createState() => _ContractDetailsScreenState();
}

class _ContractDetailsScreenState extends State<ContractDetailsScreen> {
  Map<String, dynamic>? _contract;
  bool _loading = true;
  bool _pdfGenerating = false;
  bool _signedUploading = false;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    setState(() => _loading = true);
    final api = ApiService();
    await api.init();
    try {
      final c = await api.getProjectContract(widget.projectId, widget.contractId);
      if (mounted) setState(() { _contract = c; _loading = false; });
    } catch (_) {
      if (mounted) setState(() => _loading = false);
    }
  }

  String _statusLabel(String? status, AppLocalizations l) {
    switch (status) {
      case 'Draft': return l.t('status_draft');
      case 'ReadyForPdf': return l.t('status_ready_for_pdf');
      case 'PdfGenerated': return l.t('status_pdf_generated');
      case 'SignedUploaded': return l.t('status_signed_uploaded');
      case 'Cancelled': return l.t('status_cancelled');
      default: return status ?? '';
    }
  }

  Color _statusColor(String? status) {
    switch (status) {
      case 'Draft': return AkarTheme.warning;
      case 'ReadyForPdf': return AkarTheme.primaryLight;
      case 'PdfGenerated': return AkarTheme.success;
      case 'SignedUploaded': return AkarTheme.accent;
      case 'Cancelled': return AkarTheme.danger;
      default: return AkarTheme.textMuted;
    }
  }

  String _formatDate(dynamic d) {
    if (d == null) return '—';
    try { final dt = DateTime.parse(d.toString()); return '${dt.year}-${dt.month.toString().padLeft(2,'0')}-${dt.day.toString().padLeft(2,'0')}'; } catch (_) { return d.toString(); }
  }

  String _mapError(String code, AppLocalizations l) {
    switch (code) {
      case 'CONTRACT_NOT_FOUND': return l.t('err_contract_not_found');
      case 'CONTRACT_NOT_DRAFT': return l.t('err_contract_not_draft');
      case 'CONTRACT_NOT_ELIGIBLE_FOR_PDF': return l.t('err_contract_not_eligible_for_pdf');
      case 'CONTRACT_CANCELLED': return l.t('err_contract_cancelled');
      case 'CONTRACTS_FOLDER_NOT_FOUND': return l.t('err_contracts_folder_not_found');
      case 'PDF_GENERATION_FAILED': return l.t('err_pdf_generation_failed');
      case 'STORAGE_SAVE_FAILED': return l.t('err_storage_save_failed');
      case 'PDF_FILE_METADATA_FAILED': return l.t('err_pdf_file_metadata_failed');
      case 'PROJECT_NOT_FOUND': return l.t('err_project_not_found');
      // Sprint 6 — Signed contract upload error codes
      case 'SIGNED_FILE_REQUIRED': return l.t('err_signed_file_required');
      case 'SIGNED_FILE_MUST_BE_PDF': return l.t('err_signed_file_must_be_pdf');
      case 'SIGNED_FILE_TOO_LARGE': return l.t('err_signed_file_too_large');
      case 'CONTRACT_ALREADY_SIGNED': return l.t('err_contract_already_signed');
      case 'CONTRACT_NOT_PDF_GENERATED':
        // Guard-ordering note: backend may return this when status is already SignedUploaded
        if (_contract?['status'] == 'SignedUploaded') {
          return l.t('err_signed_status_mismatch');
        }
        return l.t('err_contract_not_pdf_generated');
      case 'SIGNED_FILE_METADATA_FAILED': return l.t('err_storage_save_failed');
      default: return l.t('err_generic');
    }
  }

  bool get _isDraft => _contract?['status'] == 'Draft';
  bool get _canGeneratePdf => _contract?['status'] == 'Draft' || _contract?['status'] == 'ReadyForPdf';
  bool get _hasPdf => _contract?['pdfFileId'] != null && _contract!['pdfFileId'].toString().isNotEmpty;
  bool get _hasSignedFile => _contract?['signedFileId'] != null && _contract!['signedFileId'].toString().isNotEmpty;

  Future<void> _editContract() async {
    final result = await Navigator.push<bool>(context, MaterialPageRoute(
      builder: (_) => ContractFormScreen(
        projectId: widget.projectId,
        existingContract: _contract,
      ),
    ));
    if (result == true) _load();
  }

  Future<void> _deleteContract() async {
    final l = AppLocalizations.of(context);
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AkarTheme.bgCard,
        title: Text(l.t('contract_delete_title')),
        content: Text(l.t('contract_delete_warning')),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(l.t('btn_cancel'))),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: Text(l.t('btn_confirm'), style: const TextStyle(color: AkarTheme.danger)),
          ),
        ],
      ),
    );
    if (confirmed != true) return;

    try {
      final api = ApiService();
      await api.init();
      await api.deleteProjectContract(widget.projectId, widget.contractId);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(l.t('contract_deleted')), backgroundColor: AkarTheme.success));
        Navigator.pop(context, true);
      }
    } on ApiException catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(_mapError(e.code, l)), backgroundColor: AkarTheme.danger));
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(l.t('err_generic')), backgroundColor: AkarTheme.danger));
      }
    }
  }

  Future<void> _generatePdf() async {
    final l = AppLocalizations.of(context);
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AkarTheme.bgCard,
        title: Text(l.t('contract_generate_pdf')),
        content: Text(l.t('contract_generate_pdf_confirm')),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(l.t('btn_cancel'))),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: Text(l.t('btn_confirm'), style: const TextStyle(color: AkarTheme.success)),
          ),
        ],
      ),
    );
    if (confirmed != true) return;

    setState(() => _pdfGenerating = true);
    try {
      final api = ApiService();
      await api.init();
      await api.generateContractPdf(widget.projectId, widget.contractId);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(l.t('contract_pdf_generated')), backgroundColor: AkarTheme.success));
        _load(); // Refresh to get new status + pdfFileId
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
      if (mounted) setState(() => _pdfGenerating = false);
    }
  }

  Future<void> _downloadPdf() async {
    final l = AppLocalizations.of(context);
    final pdfFileId = _contract!['pdfFileId'] as String;
    final fileName = '${_contract!['contractTitle'] ?? 'contract'}.pdf';

    try {
      final api = ApiService();
      await api.init();
      final bytes = await api.downloadFileBytes(widget.projectId, pdfFileId);

      downloadFileBytes(bytes, fileName);
    } on ApiException catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(_mapError(e.code, l)), backgroundColor: AkarTheme.danger));
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(l.t('err_generic')), backgroundColor: AkarTheme.danger));
      }
    }
  }

  // ═══════════════════════════════════════════════════════════════
  // Sprint 6 — Signed Contract Upload
  // ═══════════════════════════════════════════════════════════════

  Future<void> _uploadSignedContract() async {
    final l = AppLocalizations.of(context);

    // Step 1: Show explanation dialog
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AkarTheme.bgCard,
        title: Row(children: [
          const Icon(Icons.upload_file, color: AkarTheme.accent, size: 22),
          const SizedBox(width: 8),
          Expanded(child: Text(l.t('signed_upload_confirm'), style: const TextStyle(fontSize: 16))),
        ]),
        content: Text(l.t('signed_upload_explanation'), style: const TextStyle(fontSize: 14, height: 1.5)),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(l.t('btn_cancel'))),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: Text(l.t('btn_confirm'), style: const TextStyle(color: AkarTheme.accent)),
          ),
        ],
      ),
    );
    if (confirmed != true || !mounted) return;

    // Step 2: Pick file using file_picker
    final result = await FilePicker.platform.pickFiles(
      withData: true,
      type: FileType.custom,
      allowedExtensions: ['pdf'],
    );

    if (result == null || result.files.isEmpty || result.files.first.bytes == null) return;

    final pickedFile = result.files.first;
    final fileName = pickedFile.name;
    final fileBytes = pickedFile.bytes!;

    // Step 3: Client-side extension validation
    if (!fileName.toLowerCase().endsWith('.pdf')) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('err_signed_file_must_be_pdf')), backgroundColor: AkarTheme.danger),
        );
      }
      return;
    }

    // Step 4: Show selected file info and get final confirmation
    if (!mounted) return;
    final uploadConfirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AkarTheme.bgCard,
        title: Text(l.t('signed_pdf_selected'), style: const TextStyle(fontSize: 16)),
        content: Column(mainAxisSize: MainAxisSize.min, crossAxisAlignment: CrossAxisAlignment.start, children: [
          Row(children: [
            const Icon(Icons.picture_as_pdf, color: AkarTheme.danger, size: 32),
            const SizedBox(width: 12),
            Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text(fileName, style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14)),
              const SizedBox(height: 4),
              Text('${(fileBytes.length / 1024).toStringAsFixed(1)} KB', style: const TextStyle(color: AkarTheme.textMuted, fontSize: 12)),
            ])),
          ]),
          const SizedBox(height: 16),
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: AkarTheme.warning.withValues(alpha: 0.08),
              borderRadius: BorderRadius.circular(6),
              border: Border.all(color: AkarTheme.warning.withValues(alpha: 0.3)),
            ),
            child: Row(children: [
              const Icon(Icons.info_outline, size: 16, color: AkarTheme.warning),
              const SizedBox(width: 8),
              Expanded(child: Text(l.t('signed_cannot_replace'), style: const TextStyle(fontSize: 12, color: AkarTheme.warning))),
            ]),
          ),
        ]),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(l.t('btn_cancel'))),
          ElevatedButton(
            onPressed: () => Navigator.pop(ctx, true),
            style: ElevatedButton.styleFrom(backgroundColor: AkarTheme.accent, foregroundColor: AkarTheme.bgDark),
            child: Text(l.t('signed_upload_button')),
          ),
        ],
      ),
    );
    if (uploadConfirmed != true || !mounted) return;

    // Step 5: Upload
    setState(() => _signedUploading = true);
    try {
      final api = ApiService();
      await api.init();
      await api.uploadSignedContractFile(
        widget.projectId,
        widget.contractId,
        Uint8List.fromList(fileBytes),
        fileName,
      );
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('signed_upload_success')), backgroundColor: AkarTheme.success),
        );
        _load(); // Refresh to get new status + signedFileId
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
      if (mounted) setState(() => _signedUploading = false);
    }
  }

  Future<void> _downloadSignedPdf() async {
    final l = AppLocalizations.of(context);
    final signedFileId = _contract!['signedFileId'] as String;
    final fileName = '${_contract!['contractTitle'] ?? 'contract'}_signed.pdf';

    try {
      final api = ApiService();
      await api.init();
      final bytes = await api.downloadFileBytes(widget.projectId, signedFileId);

      downloadFileBytes(bytes, fileName);
    } on ApiException catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(_mapError(e.code, l)), backgroundColor: AkarTheme.danger));
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(l.t('err_generic')), backgroundColor: AkarTheme.danger));
      }
    }
  }

  // ═══════════════════════════════════════════════════════════════
  // Build — Signed Contract Section Widget
  // ═══════════════════════════════════════════════════════════════

  Widget _buildSignedContractSection(AppLocalizations l) {
    final status = _contract?['status'] as String?;

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AkarTheme.bgCard,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AkarTheme.accent.withValues(alpha: 0.25)),
      ),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        // Section header
        Row(children: [
          Icon(Icons.draw, color: AkarTheme.accent, size: 20),
          const SizedBox(width: 8),
          Text(l.t('signed_contract'), style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15, color: AkarTheme.accent)),
        ]),
        const SizedBox(height: 12),

        // Status indicator
        Row(children: [
          Text(l.t('signed_current_status'), style: const TextStyle(color: AkarTheme.textMuted, fontSize: 12)),
          const SizedBox(width: 8),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
            decoration: BoxDecoration(
              color: _statusColor(status).withValues(alpha: 0.15),
              borderRadius: BorderRadius.circular(6),
            ),
            child: Text(
              _statusLabel(status, l),
              style: TextStyle(fontWeight: FontWeight.w600, fontSize: 11, color: _statusColor(status)),
            ),
          ),
        ]),

        // SignedFileId indicator
        if (_hasSignedFile) ...[
          const SizedBox(height: 6),
          Row(children: [
            Text(l.t('signed_file_id'), style: const TextStyle(color: AkarTheme.textMuted, fontSize: 12)),
            const SizedBox(width: 8),
            Expanded(
              child: Text(
                '${_contract!['signedFileId'].toString().substring(0, 8)}...',
                style: const TextStyle(fontSize: 11, color: AkarTheme.textSecondary, fontFamily: 'monospace'),
              ),
            ),
          ]),
        ],

        const SizedBox(height: 14),

        // Content based on status
        if (status == 'Draft' || status == 'ReadyForPdf') ...[
          // Cannot upload yet — show helper message
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: AkarTheme.warning.withValues(alpha: 0.08),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
              const Icon(Icons.info_outline, size: 16, color: AkarTheme.warning),
              const SizedBox(width: 8),
              Expanded(child: Text(l.t('signed_generate_pdf_first'), style: const TextStyle(fontSize: 12, color: AkarTheme.textSecondary))),
            ]),
          ),
        ] else if (status == 'PdfGenerated') ...[
          // Can upload signed contract
          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              icon: _signedUploading
                  ? const SizedBox(height: 18, width: 18, child: CircularProgressIndicator(strokeWidth: 2, color: AkarTheme.bgDark))
                  : const Icon(Icons.upload_file),
              label: Text(l.t('signed_upload_button')),
              onPressed: _signedUploading ? null : _uploadSignedContract,
              style: ElevatedButton.styleFrom(
                backgroundColor: AkarTheme.accent,
                foregroundColor: AkarTheme.bgDark,
                padding: const EdgeInsets.symmetric(vertical: 12),
              ),
            ),
          ),
        ] else if (status == 'SignedUploaded') ...[
          // Signed file uploaded — show success and download
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: AkarTheme.success.withValues(alpha: 0.08),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Row(children: [
              const Icon(Icons.check_circle, size: 18, color: AkarTheme.success),
              const SizedBox(width: 8),
              Expanded(child: Text(l.t('signed_version_uploaded'), style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600, color: AkarTheme.success))),
            ]),
          ),
          if (_hasSignedFile) ...[
            const SizedBox(height: 10),
            SizedBox(
              width: double.infinity,
              child: OutlinedButton.icon(
                icon: const Icon(Icons.download, size: 18),
                label: Text(l.t('signed_download_button')),
                onPressed: _downloadSignedPdf,
                style: OutlinedButton.styleFrom(
                  foregroundColor: AkarTheme.accent,
                  side: BorderSide(color: AkarTheme.accent.withValues(alpha: 0.5)),
                  padding: const EdgeInsets.symmetric(vertical: 10),
                ),
              ),
            ),
          ],
        ] else if (status == 'Cancelled') ...[
          // Read-only, no upload
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: AkarTheme.danger.withValues(alpha: 0.08),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Row(children: [
              const Icon(Icons.block, size: 16, color: AkarTheme.danger),
              const SizedBox(width: 8),
              Expanded(child: Text(l.t('err_contract_cancelled'), style: const TextStyle(fontSize: 12, color: AkarTheme.textSecondary))),
            ]),
          ),
        ],
      ]),
    );
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    if (_loading) {
      return Scaffold(
        appBar: AppBar(title: Text(l.t('contracts_details'))),
        body: const Center(child: CircularProgressIndicator()),
      );
    }
    if (_contract == null) {
      return Scaffold(
        appBar: AppBar(title: Text(l.t('contracts_details'))),
        body: Center(child: Text(l.t('err_contract_not_found'))),
      );
    }

    final c = _contract!;
    Map<String, dynamic>? dataMap;
    if (c['contractDataJson'] != null && c['contractDataJson'].toString().isNotEmpty) {
      try { dataMap = json.decode(c['contractDataJson']) as Map<String, dynamic>; } catch (_) {}
    }

    return Scaffold(
      appBar: AppBar(
        title: Text(l.t('contracts_details')),
        actions: [
          if (_isDraft) IconButton(icon: const Icon(Icons.edit), onPressed: _editContract, tooltip: l.t('contracts_edit')),
          if (_isDraft) IconButton(icon: const Icon(Icons.delete, color: AkarTheme.danger), onPressed: _deleteContract, tooltip: l.t('contract_delete_title')),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _load,
        child: ListView(padding: const EdgeInsets.all(20), children: [
          // Title + status badge
          Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Expanded(child: Text(c['contractTitle'] ?? '', style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold))),
            const SizedBox(width: 8),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
              decoration: BoxDecoration(
                color: _statusColor(c['status']).withValues(alpha: 0.15),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Text(
                _statusLabel(c['status'], l),
                style: TextStyle(fontWeight: FontWeight.w600, fontSize: 12, color: _statusColor(c['status'])),
              ),
            ),
          ]),
          const SizedBox(height: 16),

          _DetailRow(label: l.t('contract_template_name'), value: c['templateNameAr'] ?? c['templateNameEn'] ?? '—'),
          _DetailRow(label: l.t('contract_type'), value: c['contractType'] ?? '—'),
          const Divider(height: 24),

          // Party info
          _DetailRow(label: l.t('contract_party_name'), value: c['partyName'] ?? '—'),
          _DetailRow(label: l.t('contract_party_phone'), value: c['partyPhone'] ?? '—'),
          _DetailRow(label: l.t('contract_party_national_id'), value: c['partyNationalId'] ?? '—'),
          const Divider(height: 24),

          // Value & dates
          if (c['contractValue'] != null)
            _DetailRow(label: l.t('contract_value'), value: '${c['contractValue']} ${l.t('contract_sar')}'),
          _DetailRow(label: l.t('contract_start_date'), value: _formatDate(c['startDate'])),
          _DetailRow(label: l.t('contract_end_date'), value: _formatDate(c['endDate'])),
          const Divider(height: 24),

          // Contract data
          if (dataMap != null) ...[
            if (dataMap['scopeOfWork'] != null && dataMap['scopeOfWork'].toString().isNotEmpty) ...[
              _SectionHeader(l.t('contract_scope_of_work')),
              Text(dataMap['scopeOfWork'], style: const TextStyle(fontSize: 13, color: AkarTheme.textSecondary)),
              const SizedBox(height: 12),
            ],
            if (dataMap['paymentTerms'] != null && dataMap['paymentTerms'].toString().isNotEmpty) ...[
              _SectionHeader(l.t('contract_payment_terms')),
              Text(dataMap['paymentTerms'], style: const TextStyle(fontSize: 13, color: AkarTheme.textSecondary)),
              const SizedBox(height: 12),
            ],
            if (dataMap['ownerObligations'] != null && dataMap['ownerObligations'].toString().isNotEmpty) ...[
              _SectionHeader(l.t('contract_owner_obligations')),
              Text(dataMap['ownerObligations'], style: const TextStyle(fontSize: 13, color: AkarTheme.textSecondary)),
              const SizedBox(height: 12),
            ],
            if (dataMap['contractorObligations'] != null && dataMap['contractorObligations'].toString().isNotEmpty) ...[
              _SectionHeader(l.t('contract_contractor_obligations')),
              Text(dataMap['contractorObligations'], style: const TextStyle(fontSize: 13, color: AkarTheme.textSecondary)),
              const SizedBox(height: 12),
            ],
            if (dataMap['notes'] != null && dataMap['notes'].toString().isNotEmpty) ...[
              _SectionHeader(l.t('contract_notes')),
              Text(dataMap['notes'], style: const TextStyle(fontSize: 13, color: AkarTheme.textSecondary)),
              const SizedBox(height: 12),
            ],
            const Divider(height: 24),
          ],

          // Created at
          _DetailRow(label: l.t('project_created_at'), value: _formatDate(c['createdAtUtc'])),
          const SizedBox(height: 16),

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

          // Action buttons — Generate PDF & Download generated PDF
          if (_canGeneratePdf)
            ElevatedButton.icon(
              icon: _pdfGenerating
                  ? const SizedBox(height: 18, width: 18, child: CircularProgressIndicator(strokeWidth: 2, color: AkarTheme.textPrimary))
                  : const Icon(Icons.picture_as_pdf),
              label: Text(l.t('contract_generate_pdf')),
              onPressed: _pdfGenerating ? null : _generatePdf,
              style: ElevatedButton.styleFrom(backgroundColor: AkarTheme.primaryLight),
            ),

          if (_hasPdf) ...[
            const SizedBox(height: 10),
            ElevatedButton.icon(
              icon: const Icon(Icons.download),
              label: Text(l.t('contract_download_pdf')),
              onPressed: _downloadPdf,
              style: ElevatedButton.styleFrom(backgroundColor: AkarTheme.accent, foregroundColor: AkarTheme.bgDark),
            ),
          ],

          // ═══════════════════════════════════════════════════
          // Sprint 6 — Signed Contract Section
          // ═══════════════════════════════════════════════════
          const SizedBox(height: 20),
          _buildSignedContractSection(l),

          const SizedBox(height: 30),
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
    padding: const EdgeInsets.symmetric(vertical: 6),
    child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
      SizedBox(width: 130, child: Text(label, style: const TextStyle(color: AkarTheme.textMuted, fontSize: 13))),
      const SizedBox(width: 10),
      Expanded(child: Text(value, style: const TextStyle(fontSize: 14))),
    ]),
  );
}

class _SectionHeader extends StatelessWidget {
  final String text;
  const _SectionHeader(this.text);
  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.only(bottom: 4),
    child: Text(text, style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13, color: AkarTheme.accent)),
  );
}

import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';
import 'contract_form_screen.dart';
import 'contract_details_screen.dart';

class ContractsScreen extends StatefulWidget {
  final String projectId;
  final String projectName;
  const ContractsScreen({super.key, required this.projectId, required this.projectName});
  @override
  State<ContractsScreen> createState() => _ContractsScreenState();
}

class _ContractsScreenState extends State<ContractsScreen> {
  List<Map<String, dynamic>> _contracts = [];
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    setState(() => _loading = true);
    final api = ApiService();
    await api.init();
    try {
      final list = await api.getProjectContracts(widget.projectId);
      if (mounted) setState(() { _contracts = list; _loading = false; });
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

  Future<void> _addContract() async {
    final result = await Navigator.push<bool>(context, MaterialPageRoute(
      builder: (_) => ContractFormScreen(projectId: widget.projectId),
    ));
    if (result == true) _load();
  }

  Future<void> _openContract(Map<String, dynamic> c) async {
    final result = await Navigator.push<bool>(context, MaterialPageRoute(
      builder: (_) => ContractDetailsScreen(
        projectId: widget.projectId,
        contractId: c['id'] as String,
      ),
    ));
    if (result == true) _load();
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Scaffold(
      appBar: AppBar(title: Text(l.t('contracts_title'))),
      floatingActionButton: FloatingActionButton(
        backgroundColor: AkarTheme.accent,
        onPressed: _addContract,
        child: const Icon(Icons.add, color: AkarTheme.bgDark),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _contracts.isEmpty
              ? Center(child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(Icons.description_outlined, size: 64, color: AkarTheme.textMuted),
                    const SizedBox(height: 12),
                    Text(l.t('contracts_empty'), style: const TextStyle(fontSize: 16, color: AkarTheme.textSecondary)),
                    const SizedBox(height: 4),
                    Text(l.t('contracts_empty_sub'), style: const TextStyle(fontSize: 13, color: AkarTheme.textMuted)),
                  ],
                ))
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.separated(
                    padding: const EdgeInsets.all(16),
                    itemCount: _contracts.length,
                    separatorBuilder: (_, __) => const SizedBox(height: 10),
                    itemBuilder: (context, i) {
                      final c = _contracts[i];
                      final hasPdf = c['pdfFileId'] != null && c['pdfFileId'].toString().isNotEmpty;
                      return Card(
                        child: InkWell(
                          borderRadius: BorderRadius.circular(12),
                          onTap: () => _openContract(c),
                          child: Padding(
                            padding: const EdgeInsets.all(14),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Row(children: [
                                  Expanded(child: Text(
                                    c['contractTitle'] ?? '',
                                    style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 15),
                                    maxLines: 2,
                                    overflow: TextOverflow.ellipsis,
                                  )),
                                  Container(
                                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                                    decoration: BoxDecoration(
                                      color: _statusColor(c['status']).withValues(alpha: 0.15),
                                      borderRadius: BorderRadius.circular(6),
                                    ),
                                    child: Text(
                                      _statusLabel(c['status'], l),
                                      style: TextStyle(fontSize: 11, fontWeight: FontWeight.w600, color: _statusColor(c['status'])),
                                    ),
                                  ),
                                ]),
                                const SizedBox(height: 8),
                                if (c['partyName'] != null && c['partyName'].toString().isNotEmpty)
                                  _InfoChip(icon: Icons.person_outline, text: c['partyName']),
                                if (c['contractValue'] != null)
                                  _InfoChip(icon: Icons.payments_outlined, text: '${c['contractValue']} ${l.t('contract_sar')}'),
                                Row(children: [
                                  _InfoChip(icon: Icons.calendar_today_outlined, text: _formatDate(c['createdAtUtc'])),
                                  const Spacer(),
                                  if (hasPdf)
                                    Icon(Icons.picture_as_pdf, size: 18, color: AkarTheme.success),
                                ]),
                              ],
                            ),
                          ),
                        ),
                      );
                    },
                  ),
                ),
    );
  }
}

class _InfoChip extends StatelessWidget {
  final IconData icon;
  final String text;
  const _InfoChip({required this.icon, required this.text});
  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.only(bottom: 4),
    child: Row(mainAxisSize: MainAxisSize.min, children: [
      Icon(icon, size: 14, color: AkarTheme.textMuted),
      const SizedBox(width: 6),
      Flexible(child: Text(text, style: const TextStyle(fontSize: 12, color: AkarTheme.textSecondary), overflow: TextOverflow.ellipsis)),
    ]),
  );
}

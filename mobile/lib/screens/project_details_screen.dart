import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';
import 'document_vault_screen.dart';

class ProjectDetailsScreen extends StatefulWidget {
  final String projectId;
  final VoidCallback? onBack;
  const ProjectDetailsScreen({super.key, required this.projectId, this.onBack});
  @override
  State<ProjectDetailsScreen> createState() => _ProjectDetailsScreenState();
}

class _ProjectDetailsScreenState extends State<ProjectDetailsScreen> {
  Map<String, dynamic>? _project;
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    final api = ApiService();
    await api.init();
    try {
      final p = await api.getProject(widget.projectId);
      if (mounted) setState(() { _project = p; _loading = false; });
    } catch (_) {
      if (mounted) setState(() => _loading = false);
    }
  }

  String _stageLabel(String s, AppLocalizations l) {
    switch (s) { case 'NotStarted': return l.t('stage_not_started'); case 'Structural': return l.t('stage_structural');
      case 'Finishing': return l.t('stage_finishing'); case 'Completed': return l.t('stage_completed'); default: return s; }
  }

  String _typeLabel(String t, AppLocalizations l) {
    switch (t) { case 'Villa': return l.t('type_villa'); case 'Duplex': return l.t('type_duplex');
      case 'SmallBuilding': return l.t('type_small_building'); default: return t; }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Scaffold(
      appBar: AppBar(
        leading: widget.onBack != null ? IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: widget.onBack,
        ) : null,
        title: Text(l.t('project_details_title')),
      ),
      body: _loading ? const Center(child: CircularProgressIndicator())
          : _project == null ? Center(child: Text(l.t('err_generic')))
          : ListView(padding: const EdgeInsets.all(20), children: [
              Text(_project!['projectName'] ?? '', style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold)),
              const SizedBox(height: 20),
              _Row(label: l.t('project_type'), value: _typeLabel(_project!['projectType'] ?? '', l)),
              _Row(label: l.t('project_stage'), value: _stageLabel(_project!['currentStage'] ?? '', l)),
              _Row(label: l.t('project_city'), value: _project!['city'] ?? '—'),
              _Row(label: l.t('project_location'), value: _project!['locationText'] ?? '—'),
              _Row(label: l.t('project_map_link'), value: _project!['mapLink'] ?? '—'),
              _Row(label: l.t('project_created_at'), value: _formatDate(_project!['createdAtUtc'])),
              _Row(label: l.t('project_updated_at'), value: _formatDate(_project!['updatedAtUtc'])),

              // Document Vault entry
              const SizedBox(height: 24),
              Card(
                child: InkWell(
                  borderRadius: BorderRadius.circular(12),
                  onTap: () => Navigator.push(context, MaterialPageRoute(
                    builder: (_) => DocumentVaultScreen(
                      projectId: widget.projectId,
                      projectName: _project!['projectName'] ?? '',
                    ),
                  )),
                  child: Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                    child: Row(children: [
                      Container(
                        width: 44, height: 44,
                        decoration: BoxDecoration(
                          color: AkarTheme.accent.withValues(alpha: 0.12),
                          borderRadius: BorderRadius.circular(10),
                        ),
                        child: const Icon(Icons.folder_special, color: AkarTheme.accent, size: 24),
                      ),
                      const SizedBox(width: 14),
                      Expanded(child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(l.t('vault_title'), style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 15)),
                          const SizedBox(height: 2),
                          Text(l.t('vault_folders'), style: const TextStyle(color: AkarTheme.textMuted, fontSize: 12)),
                        ],
                      )),
                      const Icon(Icons.chevron_right, color: AkarTheme.textMuted),
                    ]),
                  ),
                ),
              ),
            ]),
    );
  }

  String _formatDate(dynamic d) {
    if (d == null) return '—';
    try { final dt = DateTime.parse(d.toString()); return '${dt.year}-${dt.month.toString().padLeft(2,'0')}-${dt.day.toString().padLeft(2,'0')}'; } catch (_) { return d.toString(); }
  }
}

class _Row extends StatelessWidget {
  final String label; final String value;
  const _Row({required this.label, required this.value});
  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 8),
    child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
      SizedBox(width: 120, child: Text(label, style: const TextStyle(color: Color(0xFF8B95A5), fontSize: 13))),
      const SizedBox(width: 12),
      Expanded(child: Text(value, style: const TextStyle(fontSize: 14))),
    ]),
  );
}

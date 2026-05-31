import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';

class TimelineScreen extends StatefulWidget {
  final String projectId;
  final String projectName;
  const TimelineScreen({super.key, required this.projectId, required this.projectName});
  @override
  State<TimelineScreen> createState() => _TimelineScreenState();
}

class _TimelineScreenState extends State<TimelineScreen> {
  final _api = ApiService();
  List<Map<String, dynamic>> _events = [];
  String _currentStage = 'NotStarted';
  bool _loading = true;
  String? _filterStage;
  String? _filterType;

  static const _stages = ['NotStarted', 'Structural', 'Finishing', 'Completed'];
  static const _eventTypes = [
    'StageChanged', 'ManualNote', 'FileUploaded', 'ContractCreated',
    'ContractPdfGenerated', 'FollowerAdded', 'FollowerFileUploaded',
  ];

  @override
  void initState() { super.initState(); _init(); }

  Future<void> _init() async {
    await _api.init();
    await _loadAll();
  }

  Future<void> _loadAll() async {
    if (!mounted) return;
    setState(() => _loading = true);
    try {
      final project = await _api.getProject(widget.projectId);
      final events = await _api.getProjectTimeline(
        widget.projectId,
        stage: _filterStage,
        eventType: _filterType,
      );
      if (mounted) {
        setState(() {
          _currentStage = project['currentStage'] ?? 'NotStarted';
          _events = events;
          _loading = false;
        });
      }
    } on ApiException catch (e) {
      if (mounted) {
        setState(() => _loading = false);
        _showError(e.code);
      }
    } catch (_) {
      if (mounted) setState(() => _loading = false);
    }
  }

  String _stageLabel(String s, AppLocalizations l) {
    switch (s) {
      case 'NotStarted': return l.t('stage_not_started');
      case 'Structural': return l.t('stage_structural');
      case 'Finishing': return l.t('stage_finishing');
      case 'Completed': return l.t('stage_completed');
      default: return s;
    }
  }

  String _eventTypeLabel(String t, AppLocalizations l) {
    switch (t) {
      case 'StageChanged': return l.t('timeline_stage_changed');
      case 'ManualNote': return l.t('timeline_manual_note');
      case 'FileUploaded': return l.t('timeline_file_uploaded');
      case 'ContractCreated': return l.t('timeline_contract_created');
      case 'ContractPdfGenerated': return l.t('timeline_contract_pdf_generated');
      case 'FollowerAdded': return l.t('timeline_follower_added');
      case 'FollowerFileUploaded': return l.t('timeline_follower_file_uploaded');
      default: return t;
    }
  }

  IconData _eventIcon(String t) {
    switch (t) {
      case 'StageChanged': return Icons.flag_rounded;
      case 'ManualNote': return Icons.edit_note;
      case 'FileUploaded': return Icons.upload_file;
      case 'ContractCreated': return Icons.description;
      case 'ContractPdfGenerated': return Icons.picture_as_pdf;
      case 'FollowerAdded': return Icons.person_add;
      case 'FollowerFileUploaded': return Icons.cloud_upload;
      default: return Icons.event;
    }
  }

  Color _eventColor(String t) {
    switch (t) {
      case 'StageChanged': return AkarTheme.accent;
      case 'ManualNote': return AkarTheme.primaryLight;
      case 'FileUploaded': return const Color(0xFF5AC8FA);
      case 'ContractCreated': return AkarTheme.success;
      case 'ContractPdfGenerated': return const Color(0xFFFF6B6B);
      case 'FollowerAdded': return const Color(0xFFAF52DE);
      case 'FollowerFileUploaded': return const Color(0xFF64D2FF);
      default: return AkarTheme.textMuted;
    }
  }

  int _stageIndex(String s) => _stages.indexOf(s);

  void _showError(String code) {
    final l = AppLocalizations.of(context);
    final errKey = 'err_${code.toLowerCase().replaceAll(RegExp(r'[^a-z0-9]'), '_')}';
    final msg = l.t(errKey) != errKey ? l.t(errKey) : l.t('err_generic');
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(msg), backgroundColor: AkarTheme.danger));
  }

  void _showSuccess(String key) {
    final l = AppLocalizations.of(context);
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(l.t(key)), backgroundColor: AkarTheme.success),
    );
  }

  // ═══ Change Stage Dialog ═══
  Future<void> _showChangeStageDialog() async {
    final l = AppLocalizations.of(context);
    String selected = _currentStage;
    final noteCtrl = TextEditingController();
    final result = await showDialog<bool>(
      context: context,
      builder: (ctx) => StatefulBuilder(builder: (ctx, setDState) => AlertDialog(
        title: Text(l.t('timeline_change_stage')),
        content: Column(mainAxisSize: MainAxisSize.min, children: [
          DropdownButtonFormField<String>(
            initialValue: selected,
            decoration: InputDecoration(labelText: l.t('timeline_select_stage')),
            items: _stages.map((s) => DropdownMenuItem(value: s, child: Text(_stageLabel(s, l)))).toList(),
            onChanged: (v) => setDState(() => selected = v!),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: noteCtrl,
            decoration: InputDecoration(labelText: l.t('timeline_change_note')),
            maxLines: 2,
          ),
        ]),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(l.t('btn_cancel'))),
          ElevatedButton(onPressed: () => Navigator.pop(ctx, true), child: Text(l.t('btn_confirm'))),
        ],
      )),
    );
    if (result != true) return;
    try {
      await _api.updateProjectStage(widget.projectId, selected, note: noteCtrl.text);
      _showSuccess('timeline_stage_updated');
      await _loadAll();
    } on ApiException catch (e) {
      _showError(e.code);
    }
  }

  // ═══ Add Note Dialog ═══
  Future<void> _showAddNoteDialog() async {
    final l = AppLocalizations.of(context);
    String stage = _currentStage;
    final titleCtrl = TextEditingController();
    final descCtrl = TextEditingController();
    final formKey = GlobalKey<FormState>();
    final result = await showDialog<bool>(
      context: context,
      builder: (ctx) => StatefulBuilder(builder: (ctx, setDState) => AlertDialog(
        title: Text(l.t('timeline_add_note')),
        content: Form(
          key: formKey,
          child: SingleChildScrollView(child: Column(mainAxisSize: MainAxisSize.min, children: [
            DropdownButtonFormField<String>(
              initialValue: stage,
              decoration: InputDecoration(labelText: l.t('timeline_select_stage')),
              items: _stages.map((s) => DropdownMenuItem(value: s, child: Text(_stageLabel(s, l)))).toList(),
              onChanged: (v) => setDState(() => stage = v!),
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: titleCtrl,
              decoration: InputDecoration(labelText: l.t('timeline_note_title')),
              validator: (v) => v == null || v.trim().isEmpty ? l.t('val_required') : null,
            ),
            const SizedBox(height: 12),
            TextField(
              controller: descCtrl,
              decoration: InputDecoration(labelText: l.t('timeline_note_description')),
              maxLines: 3,
            ),
          ])),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(l.t('btn_cancel'))),
          ElevatedButton(
            onPressed: () { if (formKey.currentState!.validate()) Navigator.pop(ctx, true); },
            child: Text(l.t('btn_create')),
          ),
        ],
      )),
    );
    if (result != true) return;
    try {
      await _api.addProjectTimelineNote(
        widget.projectId,
        stage: stage,
        title: titleCtrl.text.trim(),
        description: descCtrl.text.trim().isEmpty ? null : descCtrl.text.trim(),
      );
      _showSuccess('timeline_note_added');
      await _loadAll();
    } on ApiException catch (e) {
      _showError(e.code);
    }
  }

  // ═══ Delete Note ═══
  Future<void> _deleteNote(Map<String, dynamic> event) async {
    final l = AppLocalizations.of(context);
    if (event['isSystemGenerated'] == true) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(l.t('timeline_system_no_delete')), backgroundColor: AkarTheme.warning),
      );
      return;
    }
    final confirm = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(l.t('timeline_delete_note')),
        content: Text(l.t('timeline_delete_confirm')),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(l.t('btn_cancel'))),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AkarTheme.danger),
            onPressed: () => Navigator.pop(ctx, true),
            child: Text(l.t('btn_confirm')),
          ),
        ],
      ),
    );
    if (confirm != true) return;
    try {
      await _api.deleteProjectTimelineEvent(widget.projectId, event['id']);
      _showSuccess('timeline_note_deleted');
      await _loadAll();
    } on ApiException catch (e) {
      _showError(e.code);
    }
  }

  String _formatDate(dynamic d) {
    if (d == null) return '—';
    try {
      final dt = DateTime.parse(d.toString()).toLocal();
      return '${dt.year}-${dt.month.toString().padLeft(2, '0')}-${dt.day.toString().padLeft(2, '0')} '
          '${dt.hour.toString().padLeft(2, '0')}:${dt.minute.toString().padLeft(2, '0')}';
    } catch (_) { return d.toString(); }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Scaffold(
      appBar: AppBar(title: Text(l.t('timeline_title'))),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _showAddNoteDialog,
        icon: const Icon(Icons.add),
        label: Text(l.t('timeline_add_note')),
        backgroundColor: AkarTheme.primary,
        foregroundColor: AkarTheme.textPrimary,
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _loadAll,
              child: ListView(padding: const EdgeInsets.all(16), children: [
                // ── Stage Card ──
                _buildStageCard(l),
                const SizedBox(height: 16),
                // ── Filters ──
                _buildFilters(l),
                const SizedBox(height: 12),
                // ── Events Header ──
                Padding(
                  padding: const EdgeInsets.symmetric(vertical: 8),
                  child: Row(children: [
                    Text(l.t('timeline_events'),
                        style: const TextStyle(fontSize: 17, fontWeight: FontWeight.w600)),
                    const Spacer(),
                    Text('${_events.length}',
                        style: const TextStyle(color: AkarTheme.textMuted, fontSize: 14)),
                  ]),
                ),
                // ── Events List ──
                if (_events.isEmpty) _buildEmptyState(l),
                ..._events.map((e) => _buildEventCard(e, l)),
                const SizedBox(height: 80), // FAB clearance
              ]),
            ),
    );
  }

  Widget _buildStageCard(AppLocalizations l) {
    final idx = _stageIndex(_currentStage);
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Row(children: [
            Container(
              width: 44, height: 44,
              decoration: BoxDecoration(
                color: AkarTheme.accent.withValues(alpha: 0.15),
                borderRadius: BorderRadius.circular(10),
              ),
              child: const Icon(Icons.flag_rounded, color: AkarTheme.accent, size: 24),
            ),
            const SizedBox(width: 14),
            Expanded(child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(l.t('timeline_current_stage'),
                    style: const TextStyle(color: AkarTheme.textMuted, fontSize: 12)),
                const SizedBox(height: 2),
                Text(_stageLabel(_currentStage, l),
                    style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
              ],
            )),
            OutlinedButton.icon(
              onPressed: _showChangeStageDialog,
              icon: const Icon(Icons.swap_horiz, size: 18),
              label: Text(l.t('timeline_change_stage'), style: const TextStyle(fontSize: 12)),
              style: OutlinedButton.styleFrom(
                foregroundColor: AkarTheme.accent,
                side: const BorderSide(color: AkarTheme.accent),
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
              ),
            ),
          ]),
          const SizedBox(height: 16),
          // Stage progress bar
          Row(children: List.generate(4, (i) {
            final isActive = i <= idx;
            final isCurrent = i == idx;
            return Expanded(child: Padding(
              padding: EdgeInsets.only(right: i < 3 ? 4 : 0),
              child: Column(children: [
                Container(
                  height: 6,
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(3),
                    color: isActive ? AkarTheme.accent : AkarTheme.border,
                    boxShadow: isCurrent ? [BoxShadow(color: AkarTheme.accent.withValues(alpha: 0.4), blurRadius: 6)] : null,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  _stageLabel(_stages[i], l),
                  style: TextStyle(
                    fontSize: 9,
                    fontWeight: isCurrent ? FontWeight.w700 : FontWeight.normal,
                    color: isActive ? AkarTheme.textPrimary : AkarTheme.textMuted,
                  ),
                  textAlign: TextAlign.center,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ]),
            ));
          })),
        ]),
      ),
    );
  }

  Widget _buildFilters(AppLocalizations l) {
    return Row(children: [
      Expanded(
        child: DropdownButtonFormField<String?>(
          initialValue: _filterStage,
          isExpanded: true,
          decoration: InputDecoration(
            labelText: l.t('timeline_filter_stage'),
            contentPadding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
            isDense: true,
          ),
          items: [
            DropdownMenuItem(value: null, child: Text(l.t('timeline_all_stages'), style: const TextStyle(fontSize: 13))),
            ..._stages.map((s) => DropdownMenuItem(value: s, child: Text(_stageLabel(s, l), style: const TextStyle(fontSize: 13)))),
          ],
          onChanged: (v) { setState(() => _filterStage = v); _loadAll(); },
        ),
      ),
      const SizedBox(width: 8),
      Expanded(
        child: DropdownButtonFormField<String?>(
          initialValue: _filterType,
          isExpanded: true,
          decoration: InputDecoration(
            labelText: l.t('timeline_filter_type'),
            contentPadding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
            isDense: true,
          ),
          items: [
            DropdownMenuItem(value: null, child: Text(l.t('timeline_all_types'), style: const TextStyle(fontSize: 13))),
            ..._eventTypes.map((t) => DropdownMenuItem(value: t, child: Text(_eventTypeLabel(t, l), style: const TextStyle(fontSize: 13)))),
          ],
          onChanged: (v) { setState(() => _filterType = v); _loadAll(); },
        ),
      ),
    ]);
  }

  Widget _buildEmptyState(AppLocalizations l) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 48),
      child: Column(children: [
        Icon(Icons.timeline, size: 64, color: AkarTheme.textMuted.withValues(alpha: 0.3)),
        const SizedBox(height: 16),
        Text(l.t('timeline_no_events'), style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w500)),
        const SizedBox(height: 4),
        Text(l.t('timeline_no_events_sub'), style: const TextStyle(color: AkarTheme.textMuted, fontSize: 13)),
      ]),
    );
  }

  Widget _buildEventCard(Map<String, dynamic> event, AppLocalizations l) {
    final type = event['eventType'] ?? '';
    final isSystem = event['isSystemGenerated'] == true;
    final color = _eventColor(type);
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Card(
        child: InkWell(
          borderRadius: BorderRadius.circular(12),
          onLongPress: isSystem ? null : () => _deleteNote(event),
          child: Padding(
            padding: const EdgeInsets.all(14),
            child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
              // Icon
              Container(
                width: 38, height: 38,
                decoration: BoxDecoration(
                  color: color.withValues(alpha: 0.15),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Icon(_eventIcon(type), color: color, size: 20),
              ),
              const SizedBox(width: 12),
              // Content
              Expanded(child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(children: [
                    Expanded(child: Text(
                      event['title'] ?? _eventTypeLabel(type, l),
                      style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14),
                      maxLines: 2, overflow: TextOverflow.ellipsis,
                    )),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                      decoration: BoxDecoration(
                        color: isSystem
                            ? AkarTheme.textMuted.withValues(alpha: 0.2)
                            : AkarTheme.primaryLight.withValues(alpha: 0.2),
                        borderRadius: BorderRadius.circular(4),
                      ),
                      child: Text(
                        isSystem ? l.t('timeline_system_event') : l.t('timeline_manual_event'),
                        style: TextStyle(
                          fontSize: 9,
                          fontWeight: FontWeight.w500,
                          color: isSystem ? AkarTheme.textMuted : AkarTheme.primaryLight,
                        ),
                      ),
                    ),
                  ]),
                  if (event['description'] != null && (event['description'] as String).isNotEmpty) ...[
                    const SizedBox(height: 4),
                    Text(event['description'], style: const TextStyle(color: AkarTheme.textSecondary, fontSize: 12),
                        maxLines: 2, overflow: TextOverflow.ellipsis),
                  ],
                  const SizedBox(height: 6),
                  Row(children: [
                    // Stage badge
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                      decoration: BoxDecoration(
                        color: AkarTheme.accent.withValues(alpha: 0.12),
                        borderRadius: BorderRadius.circular(4),
                      ),
                      child: Text(
                        _stageLabel(event['stage'] ?? '', l),
                        style: const TextStyle(fontSize: 10, color: AkarTheme.accent, fontWeight: FontWeight.w500),
                      ),
                    ),
                    const SizedBox(width: 8),
                    Icon(Icons.access_time, size: 12, color: AkarTheme.textMuted),
                    const SizedBox(width: 3),
                    Text(_formatDate(event['eventDateUtc']),
                        style: const TextStyle(fontSize: 11, color: AkarTheme.textMuted)),
                    const Spacer(),
                    // Delete for manual only
                    if (!isSystem)
                      InkWell(
                        onTap: () => _deleteNote(event),
                        borderRadius: BorderRadius.circular(4),
                        child: const Padding(
                          padding: EdgeInsets.all(4),
                          child: Icon(Icons.delete_outline, size: 16, color: AkarTheme.danger),
                        ),
                      ),
                  ]),
                ],
              )),
            ]),
          ),
        ),
      ),
    );
  }
}

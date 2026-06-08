import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/widgets.dart';

class ProjectsScreen extends StatefulWidget {
  final void Function(String id) onViewProject;
  const ProjectsScreen({super.key, required this.onViewProject});

  @override
  State<ProjectsScreen> createState() => _ProjectsScreenState();
}

class _ProjectsScreenState extends State<ProjectsScreen> {
  List<Map<String, dynamic>> _projects = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() { _loading = true; _error = null; });
    final api = ApiService();
    await api.init();
    try {
      final list = await api.getProjects();
      if (mounted) setState(() { _projects = list; _loading = false; });
    } on ApiException catch (e) {
      if (mounted) setState(() { _error = e.code; _loading = false; });
    } catch (_) {
      if (mounted) setState(() { _error = 'err_generic'; _loading = false; });
    }
  }

  String _stageLabel(String stage, AppLocalizations l) {
    switch (stage) {
      case 'NotStarted': return l.t('stage_not_started');
      case 'Structural': return l.t('stage_structural');
      case 'Finishing': return l.t('stage_finishing');
      case 'Completed': return l.t('stage_completed');
      default: return stage;
    }
  }

  String _typeLabel(String type, AppLocalizations l) {
    switch (type) {
      case 'Villa': return l.t('type_villa');
      case 'Duplex': return l.t('type_duplex');
      case 'SmallBuilding': return l.t('type_small_building');
      default: return type;
    }
  }

  Color _stageColor(String stage) {
    switch (stage) {
      case 'NotStarted': return const Color(0xFF8B95A5);
      case 'Structural': return const Color(0xFFFF9500);
      case 'Finishing': return const Color(0xFF2A7A5E);
      case 'Completed': return const Color(0xFF34C759);
      default: return const Color(0xFF8B95A5);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);

    if (_loading) return const AkarLoadingState();

    if (_error != null) {
      return AkarErrorState(
        message: localizeError(context, _error!),
        onRetry: _load,
      );
    }

    if (_projects.isEmpty) {
      return AkarEmptyState(
        icon: Icons.folder_open,
        message: l.t('projects_empty'),
        subtitle: l.t('projects_empty_sub'),
      );
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: _projects.length,
        itemBuilder: (ctx, i) {
          final p = _projects[i];
          final stage = p['currentStage'] ?? 'NotStarted';
          return Card(
            margin: const EdgeInsets.only(bottom: 12),
            child: InkWell(
              borderRadius: BorderRadius.circular(12),
              onTap: () => widget.onViewProject(p['id']),
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(p['projectName'] ?? '', style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
                    const SizedBox(height: 8),
                    Row(children: [
                      Icon(Icons.location_city, size: 14, color: const Color(0xFF8B95A5)),
                      const SizedBox(width: 4),
                      Text(p['city'] ?? '—', style: const TextStyle(color: Color(0xFF8B95A5), fontSize: 13)),
                      const SizedBox(width: 16),
                      Icon(Icons.home, size: 14, color: const Color(0xFF8B95A5)),
                      const SizedBox(width: 4),
                      Text(_typeLabel(p['projectType'] ?? '', l), style: const TextStyle(color: Color(0xFF8B95A5), fontSize: 13)),
                    ]),
                    const SizedBox(height: 8),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                      decoration: BoxDecoration(
                        color: _stageColor(stage).withValues(alpha: 0.15),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Text(_stageLabel(stage, l), style: TextStyle(color: _stageColor(stage), fontSize: 12, fontWeight: FontWeight.w600)),
                    ),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}

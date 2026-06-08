import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';
import '../core/widgets.dart';
import 'follower_details_screen.dart';
import 'add_edit_follower_screen.dart';

class FollowersScreen extends StatefulWidget {
  final String projectId;
  final String projectName;
  const FollowersScreen({super.key, required this.projectId, required this.projectName});
  @override
  State<FollowersScreen> createState() => _FollowersScreenState();
}

class _FollowersScreenState extends State<FollowersScreen> {
  final _api = ApiService();
  List<Map<String, dynamic>> _followers = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    await _api.init();
    try {
      final list = await _api.getProjectFollowers(widget.projectId);
      if (mounted) setState(() { _followers = list; _loading = false; _error = null; });
    } on ApiException catch (e) {
      if (mounted) setState(() { _error = e.code; _loading = false; });
    } catch (_) {
      if (mounted) setState(() { _error = 'err_generic'; _loading = false; });
    }
  }

  String _typeLabel(String? type, AppLocalizations l) {
    switch (type) {
      case 'Supervisor': return l.t('follower_type_supervisor');
      case 'Relative': return l.t('follower_type_relative');
      case 'Contractor': return l.t('follower_type_contractor');
      case 'Designer': return l.t('follower_type_designer');
      case 'EngineeringOffice': return l.t('follower_type_engineering_office');
      case 'Other': return l.t('follower_type_other');
      default: return type ?? '';
    }
  }

  IconData _typeIcon(String? type) {
    switch (type) {
      case 'Supervisor': return Icons.engineering;
      case 'Relative': return Icons.family_restroom;
      case 'Contractor': return Icons.construction;
      case 'Designer': return Icons.design_services;
      case 'EngineeringOffice': return Icons.business;
      default: return Icons.person;
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Scaffold(
      appBar: AppBar(title: Text(l.t('followers_title'))),
      floatingActionButton: FloatingActionButton(
        backgroundColor: AkarTheme.primary,
        onPressed: () async {
          final result = await Navigator.push(context, MaterialPageRoute(
            builder: (_) => AddEditFollowerScreen(projectId: widget.projectId),
          ));
          if (result == true) _load();
        },
        child: const Icon(Icons.person_add, color: AkarTheme.textPrimary),
      ),
      body: _loading
          ? const AkarLoadingState()
          : _error != null
              ? AkarErrorState(
                  message: localizeError(context, _error!),
                  onRetry: () { setState(() => _loading = true); _load(); },
                )
              : _followers.isEmpty
                  ? AkarEmptyState(
                      icon: Icons.people_outline,
                      message: l.t('followers_empty'),
                    )
                  : RefreshIndicator(
                      onRefresh: _load,
                      child: ListView.builder(
                        padding: const EdgeInsets.all(16),
                        itemCount: _followers.length,
                        itemBuilder: (ctx, i) => _FollowerCard(
                          follower: _followers[i],
                          l: l,
                          typeLabel: _typeLabel,
                          typeIcon: _typeIcon,
                          onTap: () async {
                            await Navigator.push(context, MaterialPageRoute(
                              builder: (_) => FollowerDetailsScreen(
                                projectId: widget.projectId,
                                projectName: widget.projectName,
                                followerId: _followers[i]['id'],
                              ),
                            ));
                            _load();
                          },
                        ),
                      ),
                    ),
    );
  }
}

class _FollowerCard extends StatelessWidget {
  final Map<String, dynamic> follower;
  final AppLocalizations l;
  final String Function(String?, AppLocalizations) typeLabel;
  final IconData Function(String?) typeIcon;
  final VoidCallback onTap;

  const _FollowerCard({
    required this.follower,
    required this.l,
    required this.typeLabel,
    required this.typeIcon,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final isActive = follower['isActive'] == true;
    final isDeleted = follower['isDeleted'] == true;
    return Card(
      margin: const EdgeInsets.only(bottom: 10),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Row(children: [
            Container(
              width: 44, height: 44,
              decoration: BoxDecoration(
                color: isActive && !isDeleted
                    ? AkarTheme.primary.withValues(alpha: 0.15)
                    : AkarTheme.textMuted.withValues(alpha: 0.15),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Icon(typeIcon(follower['followerType']),
                  color: isActive && !isDeleted ? AkarTheme.primaryLight : AkarTheme.textMuted, size: 22),
            ),
            const SizedBox(width: 14),
            Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text(follower['fullName'] ?? '', style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 15)),
              const SizedBox(height: 2),
              Row(children: [
                Text(typeLabel(follower['followerType'], l),
                    style: const TextStyle(color: AkarTheme.textSecondary, fontSize: 12)),
                const SizedBox(width: 8),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 1),
                  decoration: BoxDecoration(
                    color: isActive && !isDeleted
                        ? AkarTheme.success.withValues(alpha: 0.15)
                        : AkarTheme.danger.withValues(alpha: 0.15),
                    borderRadius: BorderRadius.circular(4),
                  ),
                  child: Text(
                    isActive && !isDeleted ? l.t('follower_active') : l.t('follower_inactive'),
                    style: TextStyle(
                      color: isActive && !isDeleted ? AkarTheme.success : AkarTheme.danger,
                      fontSize: 10, fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
              ]),
            ])),
            const Icon(Icons.chevron_right, color: AkarTheme.textMuted),
          ]),
        ),
      ),
    );
  }
}

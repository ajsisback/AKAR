import 'package:flutter/material.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/widgets.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  Map<String, dynamic>? _data;
  String? _ownerName;
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
    _ownerName = api.owner?['fullName'] ?? '';
    try {
      final dash = await api.getDashboard();
      if (mounted) setState(() { _data = dash; _loading = false; });
    } on ApiException catch (e) {
      if (mounted) setState(() { _error = e.code; _loading = false; });
    } catch (_) {
      if (mounted) setState(() { _error = 'err_generic'; _loading = false; });
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

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Text('${l.t('dashboard_welcome')}, ${_ownerName ?? ''} 👋',
              style: const TextStyle(fontSize: 18, color: Color(0xFF8B95A5))),
          const SizedBox(height: 20),
          if (_data != null) ...[
            _StatCard(label: l.t('dashboard_total'), value: '${_data!['totalProjects'] ?? 0}', color: const Color(0xFFD4A843)),
            const SizedBox(height: 12),
            Row(children: [
              Expanded(child: _StatCard(label: l.t('dashboard_not_started'), value: '${_data!['notStartedCount'] ?? 0}', color: const Color(0xFF8B95A5))),
              const SizedBox(width: 12),
              Expanded(child: _StatCard(label: l.t('dashboard_structural'), value: '${_data!['structuralCount'] ?? 0}', color: const Color(0xFFFF9500))),
            ]),
            const SizedBox(height: 12),
            Row(children: [
              Expanded(child: _StatCard(label: l.t('dashboard_finishing'), value: '${_data!['finishingCount'] ?? 0}', color: const Color(0xFF2A7A5E))),
              const SizedBox(width: 12),
              Expanded(child: _StatCard(label: l.t('dashboard_completed'), value: '${_data!['completedCount'] ?? 0}', color: const Color(0xFF34C759))),
            ]),
          ],
          if (_data != null && (_data!['totalProjects'] ?? 0) == 0) ...[
            const SizedBox(height: 40),
            AkarEmptyState(
              icon: Icons.construction,
              message: l.t('dashboard_no_projects'),
              subtitle: l.t('dashboard_create_first'),
            ),
          ],
        ],
      ),
    );
  }
}

class _StatCard extends StatelessWidget {
  final String label;
  final String value;
  final Color color;
  const _StatCard({required this.label, required this.value, required this.color});

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 20, horizontal: 16),
        child: Column(
          children: [
            Text(value, style: TextStyle(fontSize: 28, fontWeight: FontWeight.bold, color: color)),
            const SizedBox(height: 4),
            Text(label, style: const TextStyle(fontSize: 12, color: Color(0xFF8B95A5))),
          ],
        ),
      ),
    );
  }
}


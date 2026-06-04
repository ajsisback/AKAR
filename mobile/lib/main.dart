import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:provider/provider.dart';
import 'core/l10n.dart';
import 'core/theme.dart';
import 'core/api_service.dart';
import 'screens/login_screen.dart';
import 'screens/register_screen.dart';
import 'screens/dashboard_screen.dart';
import 'screens/projects_screen.dart';
import 'screens/create_project_screen.dart';
import 'screens/project_details_screen.dart';
import 'screens/public_follower_upload_screen.dart';
import 'screens/owner_profile_screen.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  final localeProvider = LocaleProvider();
  await localeProvider.init();
  runApp(
    ChangeNotifierProvider.value(
      value: localeProvider,
      child: const AkarApp(),
    ),
  );
}

class AkarApp extends StatelessWidget {
  const AkarApp({super.key});

  /// Extract token from URL fragment: /#/follower-upload/{token}
  static String? _extractUploadToken() {
    try {
      final uri = Uri.base;
      final fragment = uri.fragment; // e.g. "/follower-upload/abc123"
      if (fragment.startsWith('/follower-upload/')) {
        final token = fragment.substring('/follower-upload/'.length);
        if (token.isNotEmpty) return token;
      }
    } catch (_) {}
    return null;
  }

  @override
  Widget build(BuildContext context) {
    final lp = context.watch<LocaleProvider>();
    final uploadToken = _extractUploadToken();

    return MaterialApp(
      debugShowCheckedModeBanner: false,
      title: 'AKAR',
      theme: AkarTheme.dark,
      locale: lp.locale,
      supportedLocales: const [Locale('ar'), Locale('en')],
      localizationsDelegates: const [
        AppLocalizationsDelegate(),
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      home: uploadToken != null
          ? PublicFollowerUploadScreen(token: uploadToken)
          : const AppShell(),
    );
  }
}

class AppShell extends StatefulWidget {
  const AppShell({super.key});
  @override
  State<AppShell> createState() => _AppShellState();
}

class _AppShellState extends State<AppShell> {
  bool _isAuth = false;
  bool _showRegister = false;
  bool _checkingAuth = true;

  @override
  void initState() {
    super.initState();
    _checkAuth();
  }

  Future<void> _checkAuth() async {
    final api = ApiService();
    await api.init();
    setState(() {
      _isAuth = api.isAuthenticated;
      _checkingAuth = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    if (_checkingAuth) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    if (!_isAuth) {
      if (_showRegister) {
        return RegisterScreen(
          onRegisterSuccess: () => setState(() => _isAuth = true),
          onGoLogin: () => setState(() => _showRegister = false),
        );
      }
      return LoginScreen(
        onLoginSuccess: () => setState(() => _isAuth = true),
        onGoRegister: () => setState(() => _showRegister = true),
      );
    }

    return const MainNav();
  }
}

class MainNav extends StatefulWidget {
  const MainNav({super.key});
  @override
  State<MainNav> createState() => _MainNavState();
}

class _MainNavState extends State<MainNav> {
  int _idx = 0;
  String? _detailsId;
  bool _showCreate = false;

  void _reset() => setState(() { _detailsId = null; _showCreate = false; });

  Future<void> _logout() async {
    final api = ApiService();
    await api.init();
    await api.logout();
    if (mounted) {
      Navigator.of(context).pushAndRemoveUntil(
        MaterialPageRoute(builder: (_) => const AppShell()),
        (_) => false,
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    final lp = context.read<LocaleProvider>();

    if (_detailsId != null) {
      return ProjectDetailsScreen(
        projectId: _detailsId!,
        onBack: () => setState(() => _detailsId = null),
      );
    }
    if (_showCreate) {
      return CreateProjectScreen(onCreated: () {
        setState(() { _showCreate = false; _idx = 1; });
      });
    }

    return Scaffold(
      appBar: AppBar(
        title: Text(l.t('app_title'), style: const TextStyle(
          fontWeight: FontWeight.bold, color: Color(0xFFD4A843))),
        actions: [
          IconButton(
            icon: const Icon(Icons.language),
            tooltip: l.t('lang_switch'),
            onPressed: () => lp.toggle(),
          ),
          IconButton(
            icon: const Icon(Icons.person_outline),
            tooltip: l.t('profile_title'),
            onPressed: () => Navigator.push(
              context,
              MaterialPageRoute(builder: (_) => const OwnerProfileScreen()),
            ),
          ),
          IconButton(
            icon: const Icon(Icons.logout),
            tooltip: l.t('btn_logout'),
            onPressed: _logout,
          ),
        ],
      ),
      body: IndexedStack(
        index: _idx,
        children: [
          const DashboardScreen(),
          ProjectsScreen(onViewProject: (id) => setState(() => _detailsId = id)),
        ],
      ),
      floatingActionButton: _idx == 1
          ? FloatingActionButton(
              backgroundColor: const Color(0xFF1B4D3E),
              onPressed: () => setState(() => _showCreate = true),
              child: const Icon(Icons.add, color: Colors.white),
            )
          : null,
      bottomNavigationBar: NavigationBar(
        selectedIndex: _idx,
        onDestinationSelected: (i) { _reset(); setState(() => _idx = i); },
        destinations: [
          NavigationDestination(icon: const Icon(Icons.dashboard_outlined),
            selectedIcon: const Icon(Icons.dashboard), label: l.t('dashboard_title')),
          NavigationDestination(icon: const Icon(Icons.folder_outlined),
            selectedIcon: const Icon(Icons.folder), label: l.t('projects_title')),
        ],
      ),
    );
  }
}

import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:shared_preferences/shared_preferences.dart';

class AppLocalizations {
  final Locale locale;
  late Map<String, String> _strings;

  AppLocalizations(this.locale);

  static AppLocalizations of(BuildContext context) {
    return Localizations.of<AppLocalizations>(context, AppLocalizations)!;
  }

  Future<void> load() async {
    final jsonStr = await rootBundle.loadString('assets/i18n/${locale.languageCode}.json');
    final Map<String, dynamic> map = json.decode(jsonStr);
    _strings = map.map((k, v) => MapEntry(k, v.toString()));
  }

  String t(String key) => _strings[key] ?? key;

  bool get isArabic => locale.languageCode == 'ar';
}

class AppLocalizationsDelegate extends LocalizationsDelegate<AppLocalizations> {
  const AppLocalizationsDelegate();

  @override
  bool isSupported(Locale locale) => ['ar', 'en'].contains(locale.languageCode);

  @override
  Future<AppLocalizations> load(Locale locale) async {
    final loc = AppLocalizations(locale);
    await loc.load();
    return loc;
  }

  @override
  bool shouldReload(AppLocalizationsDelegate old) => false;
}

class LocaleProvider extends ChangeNotifier {
  static const _key = 'akar_lang';
  Locale _locale = const Locale('ar');

  Locale get locale => _locale;
  bool get isArabic => _locale.languageCode == 'ar';

  Future<void> init() async {
    final prefs = await SharedPreferences.getInstance();
    final lang = prefs.getString(_key) ?? 'ar';
    _locale = Locale(lang);
    notifyListeners();
  }

  Future<void> toggle() async {
    _locale = _locale.languageCode == 'ar' ? const Locale('en') : const Locale('ar');
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_key, _locale.languageCode);
    notifyListeners();
  }
}

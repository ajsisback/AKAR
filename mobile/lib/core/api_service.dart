import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

class ApiService {
  // Use 10.0.2.2 for Android emulator, localhost for web/desktop
  static String get baseUrl {
    if (kIsWeb) return 'http://localhost:5000';
    return 'http://10.0.2.2:5000';
  }

  static const _tokenKey = 'akar_jwt';
  static const _ownerKey = 'akar_owner';

  String? _token;
  Map<String, dynamic>? _owner;

  String? get token => _token;
  Map<String, dynamic>? get owner => _owner;
  bool get isAuthenticated => _token != null && _token!.isNotEmpty;

  Future<void> init() async {
    final prefs = await SharedPreferences.getInstance();
    _token = prefs.getString(_tokenKey);
    final ownerJson = prefs.getString(_ownerKey);
    if (ownerJson != null) _owner = json.decode(ownerJson);
  }

  Future<void> _saveAuth(Map<String, dynamic> data) async {
    _token = data['token'] as String?;
    _owner = data['owner'] as Map<String, dynamic>?;
    final prefs = await SharedPreferences.getInstance();
    if (_token != null) await prefs.setString(_tokenKey, _token!);
    if (_owner != null) await prefs.setString(_ownerKey, json.encode(_owner));
  }

  Future<void> logout() async {
    _token = null;
    _owner = null;
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_tokenKey);
    await prefs.remove(_ownerKey);
  }

  Map<String, String> get _headers => {
    'Content-Type': 'application/json',
    if (_token != null) 'Authorization': 'Bearer $_token',
  };

  Future<Map<String, dynamic>> register({
    required String fullName,
    required String email,
    required String phone,
    required String password,
    String? companyName,
  }) async {
    final resp = await http.post(
      Uri.parse('$baseUrl/api/auth/register'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode({
        'fullName': fullName,
        'email': email,
        'phone': phone,
        'password': password,
        if (companyName != null && companyName.isNotEmpty) 'companyName': companyName,
      }),
    );
    if (resp.statusCode == 201 || resp.statusCode == 200) {
      final data = json.decode(resp.body) as Map<String, dynamic>;
      await _saveAuth(data);
      return data;
    }
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> login({
    required String email,
    required String password,
  }) async {
    final resp = await http.post(
      Uri.parse('$baseUrl/api/auth/login'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode({'email': email, 'password': password}),
    );
    if (resp.statusCode == 200) {
      final data = json.decode(resp.body) as Map<String, dynamic>;
      await _saveAuth(data);
      return data;
    }
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> getDashboard() async {
    final resp = await http.get(Uri.parse('$baseUrl/api/dashboard'), headers: _headers);
    if (resp.statusCode == 200) return json.decode(resp.body);
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<List<Map<String, dynamic>>> getProjects() async {
    final resp = await http.get(Uri.parse('$baseUrl/api/projects'), headers: _headers);
    if (resp.statusCode == 200) {
      return (json.decode(resp.body) as List).cast<Map<String, dynamic>>();
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> getProject(String id) async {
    final resp = await http.get(Uri.parse('$baseUrl/api/projects/$id'), headers: _headers);
    if (resp.statusCode == 200) return json.decode(resp.body);
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> createProject(Map<String, dynamic> data) async {
    final resp = await http.post(
      Uri.parse('$baseUrl/api/projects'),
      headers: _headers,
      body: json.encode(data),
    );
    if (resp.statusCode == 201 || resp.statusCode == 200) {
      return json.decode(resp.body);
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  String _parseError(http.Response resp) {
    try {
      final body = json.decode(resp.body);
      return body['title'] ?? body['detail'] ?? 'ERR_${resp.statusCode}';
    } catch (_) {
      return 'ERR_${resp.statusCode}';
    }
  }
}

class ApiException implements Exception {
  final String code;
  ApiException(this.code);
  @override
  String toString() => code;
}

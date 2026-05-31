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

  Map<String, String> get _authOnly => {
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

  // ═══════════════════════════════════════════════════════════════
  // Document Vault — Folder APIs
  // ═══════════════════════════════════════════════════════════════

  Future<List<Map<String, dynamic>>> getProjectFolders(String projectId) async {
    final resp = await http.get(
      Uri.parse('$baseUrl/api/projects/$projectId/folders'),
      headers: _headers,
    );
    if (resp.statusCode == 200) {
      return (json.decode(resp.body) as List).cast<Map<String, dynamic>>();
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> createFolder(String projectId, String folderName, {String? parentFolderId}) async {
    final resp = await http.post(
      Uri.parse('$baseUrl/api/projects/$projectId/folders'),
      headers: _headers,
      body: json.encode({
        'folderName': folderName,
        'parentFolderId': parentFolderId,
      }),
    );
    if (resp.statusCode == 201 || resp.statusCode == 200) {
      return json.decode(resp.body) as Map<String, dynamic>;
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> renameFolder(String projectId, String folderId, String newName) async {
    final resp = await http.put(
      Uri.parse('$baseUrl/api/projects/$projectId/folders/$folderId'),
      headers: _headers,
      body: json.encode({'newName': newName}),
    );
    if (resp.statusCode == 200) {
      return json.decode(resp.body) as Map<String, dynamic>;
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<void> deleteFolder(String projectId, String folderId) async {
    final resp = await http.delete(
      Uri.parse('$baseUrl/api/projects/$projectId/folders/$folderId'),
      headers: _headers,
    );
    if (resp.statusCode == 204 || resp.statusCode == 200) return;
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  // ═══════════════════════════════════════════════════════════════
  // Document Vault — File APIs
  // ═══════════════════════════════════════════════════════════════

  Future<List<Map<String, dynamic>>> getFolderFiles(String projectId, String folderId) async {
    final resp = await http.get(
      Uri.parse('$baseUrl/api/projects/$projectId/folders/$folderId/files'),
      headers: _headers,
    );
    if (resp.statusCode == 200) {
      return (json.decode(resp.body) as List).cast<Map<String, dynamic>>();
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> uploadFile(String projectId, String folderId, Uint8List fileBytes, String fileName) async {
    final uri = Uri.parse('$baseUrl/api/projects/$projectId/folders/$folderId/files');
    final request = http.MultipartRequest('POST', uri);
    request.headers.addAll(_authOnly);
    request.files.add(http.MultipartFile.fromBytes('file', fileBytes, filename: fileName));

    final streamed = await request.send();
    final resp = await http.Response.fromStream(streamed);

    if (resp.statusCode == 201 || resp.statusCode == 200) {
      return json.decode(resp.body) as Map<String, dynamic>;
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> getFileMetadata(String projectId, String fileId) async {
    final resp = await http.get(
      Uri.parse('$baseUrl/api/projects/$projectId/files/$fileId'),
      headers: _headers,
    );
    if (resp.statusCode == 200) {
      return json.decode(resp.body) as Map<String, dynamic>;
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  /// Downloads file bytes using authenticated header — never passes JWT in URL.
  Future<Uint8List> downloadFileBytes(String projectId, String fileId) async {
    final resp = await http.get(
      Uri.parse('$baseUrl/api/projects/$projectId/files/$fileId/download'),
      headers: _authOnly,
    );
    if (resp.statusCode == 200) return resp.bodyBytes;
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<void> deleteFile(String projectId, String fileId) async {
    final resp = await http.delete(
      Uri.parse('$baseUrl/api/projects/$projectId/files/$fileId'),
      headers: _headers,
    );
    if (resp.statusCode == 204 || resp.statusCode == 200) return;
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<void> restoreFile(String projectId, String fileId) async {
    final resp = await http.post(
      Uri.parse('$baseUrl/api/projects/$projectId/files/$fileId/restore'),
      headers: _headers,
    );
    if (resp.statusCode == 200) return;
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  // ═══════════════════════════════════════════════════════════════
  // Document Vault — Trash API
  // ═══════════════════════════════════════════════════════════════

  Future<Map<String, dynamic>> getTrash(String projectId) async {
    final resp = await http.get(
      Uri.parse('$baseUrl/api/projects/$projectId/trash'),
      headers: _headers,
    );
    if (resp.statusCode == 200) {
      return json.decode(resp.body) as Map<String, dynamic>;
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  // ═══════════════════════════════════════════════════════════════
  // Sprint 3 — Followers API
  // ═══════════════════════════════════════════════════════════════

  Future<List<Map<String, dynamic>>> getProjectFollowers(String projectId) async {
    final resp = await http.get(
      Uri.parse('$baseUrl/api/projects/$projectId/followers'),
      headers: _headers,
    );
    if (resp.statusCode == 200) {
      return (json.decode(resp.body) as List).cast<Map<String, dynamic>>();
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> createProjectFollower(String projectId, Map<String, dynamic> data) async {
    final resp = await http.post(
      Uri.parse('$baseUrl/api/projects/$projectId/followers'),
      headers: _headers,
      body: json.encode(data),
    );
    if (resp.statusCode == 201 || resp.statusCode == 200) {
      return json.decode(resp.body) as Map<String, dynamic>;
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> getProjectFollower(String projectId, String followerId) async {
    final resp = await http.get(
      Uri.parse('$baseUrl/api/projects/$projectId/followers/$followerId'),
      headers: _headers,
    );
    if (resp.statusCode == 200) return json.decode(resp.body);
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> updateProjectFollower(String projectId, String followerId, Map<String, dynamic> data) async {
    final resp = await http.put(
      Uri.parse('$baseUrl/api/projects/$projectId/followers/$followerId'),
      headers: _headers,
      body: json.encode(data),
    );
    if (resp.statusCode == 200) return json.decode(resp.body);
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<void> deleteProjectFollower(String projectId, String followerId) async {
    final resp = await http.delete(
      Uri.parse('$baseUrl/api/projects/$projectId/followers/$followerId'),
      headers: _headers,
    );
    if (resp.statusCode == 204 || resp.statusCode == 200) return;
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  // ═══════════════════════════════════════════════════════════════
  // Sprint 3 — Follower Upload Links API
  // ═══════════════════════════════════════════════════════════════

  Future<Map<String, dynamic>> generateFollowerUploadLink(String projectId, String followerId, {String? expiresAtUtc}) async {
    final resp = await http.post(
      Uri.parse('$baseUrl/api/projects/$projectId/followers/$followerId/upload-link'),
      headers: _headers,
      body: json.encode({
        if (expiresAtUtc != null) 'expiresAtUtc': expiresAtUtc,
      }),
    );
    if (resp.statusCode == 201 || resp.statusCode == 200) {
      return json.decode(resp.body) as Map<String, dynamic>;
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<List<Map<String, dynamic>>> getFollowerUploadLinks(String projectId, String followerId) async {
    final resp = await http.get(
      Uri.parse('$baseUrl/api/projects/$projectId/followers/$followerId/upload-links'),
      headers: _headers,
    );
    if (resp.statusCode == 200) {
      return (json.decode(resp.body) as List).cast<Map<String, dynamic>>();
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<void> revokeFollowerUploadLink(String projectId, String followerId, String linkId) async {
    final resp = await http.post(
      Uri.parse('$baseUrl/api/projects/$projectId/followers/$followerId/upload-links/$linkId/revoke'),
      headers: _headers,
    );
    if (resp.statusCode == 204 || resp.statusCode == 200) return;
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  // ═══════════════════════════════════════════════════════════════
  // Sprint 3D — Public Follower Upload API (no JWT)
  // ═══════════════════════════════════════════════════════════════

  Future<Map<String, dynamic>> getFollowerUploadInfo(String token) async {
    final resp = await http.get(
      Uri.parse('$baseUrl/api/public/follower-upload/$token/info'),
    );
    if (resp.statusCode == 200) return json.decode(resp.body);
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> uploadFollowerFile(String token, Uint8List fileBytes, String fileName) async {
    final uri = Uri.parse('$baseUrl/api/public/follower-upload/$token/files');
    final request = http.MultipartRequest('POST', uri);
    // No Authorization header — public endpoint
    request.files.add(http.MultipartFile.fromBytes('file', fileBytes, filename: fileName));

    final streamed = await request.send();
    final resp = await http.Response.fromStream(streamed);

    if (resp.statusCode == 201 || resp.statusCode == 200) {
      return json.decode(resp.body) as Map<String, dynamic>;
    }
    throw ApiException(_parseError(resp));
  }

  // ═══════════════════════════════════════════════════════════════
  // Sprint 4 — Ready Contracts API
  // ═══════════════════════════════════════════════════════════════

  Future<List<Map<String, dynamic>>> getContractTemplates() async {
    final resp = await http.get(
      Uri.parse('$baseUrl/api/contract-templates'),
      headers: _headers,
    );
    if (resp.statusCode == 200) {
      return (json.decode(resp.body) as List).cast<Map<String, dynamic>>();
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> getContractTemplate(String templateId) async {
    final resp = await http.get(
      Uri.parse('$baseUrl/api/contract-templates/$templateId'),
      headers: _headers,
    );
    if (resp.statusCode == 200) return json.decode(resp.body);
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<List<Map<String, dynamic>>> getProjectContracts(String projectId) async {
    final resp = await http.get(
      Uri.parse('$baseUrl/api/projects/$projectId/contracts'),
      headers: _headers,
    );
    if (resp.statusCode == 200) {
      return (json.decode(resp.body) as List).cast<Map<String, dynamic>>();
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> createProjectContract(String projectId, Map<String, dynamic> data) async {
    final resp = await http.post(
      Uri.parse('$baseUrl/api/projects/$projectId/contracts'),
      headers: _headers,
      body: json.encode(data),
    );
    if (resp.statusCode == 201 || resp.statusCode == 200) {
      return json.decode(resp.body) as Map<String, dynamic>;
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> getProjectContract(String projectId, String contractId) async {
    final resp = await http.get(
      Uri.parse('$baseUrl/api/projects/$projectId/contracts/$contractId'),
      headers: _headers,
    );
    if (resp.statusCode == 200) return json.decode(resp.body);
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> updateProjectContract(String projectId, String contractId, Map<String, dynamic> data) async {
    final resp = await http.put(
      Uri.parse('$baseUrl/api/projects/$projectId/contracts/$contractId'),
      headers: _headers,
      body: json.encode(data),
    );
    if (resp.statusCode == 200) return json.decode(resp.body);
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<void> deleteProjectContract(String projectId, String contractId) async {
    final resp = await http.delete(
      Uri.parse('$baseUrl/api/projects/$projectId/contracts/$contractId'),
      headers: _headers,
    );
    if (resp.statusCode == 204 || resp.statusCode == 200) return;
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> generateContractPdf(String projectId, String contractId) async {
    final resp = await http.post(
      Uri.parse('$baseUrl/api/projects/$projectId/contracts/$contractId/generate-pdf'),
      headers: _headers,
    );
    if (resp.statusCode == 200) return json.decode(resp.body);
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  // ═══════════════════════════════════════════════════════════════
  // Sprint 5 — Project Timeline & Stage API
  // ═══════════════════════════════════════════════════════════════

  Future<Map<String, dynamic>> getProjectStage(String projectId) async {
    final resp = await http.get(
      Uri.parse('$baseUrl/api/projects/$projectId'),
      headers: _headers,
    );
    if (resp.statusCode == 200) return json.decode(resp.body);
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> updateProjectStage(String projectId, String currentStage, {String? note}) async {
    final resp = await http.put(
      Uri.parse('$baseUrl/api/projects/$projectId/stage'),
      headers: _headers,
      body: json.encode({
        'currentStage': currentStage,
        if (note != null && note.isNotEmpty) 'note': note,
      }),
    );
    if (resp.statusCode == 200) return json.decode(resp.body);
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<List<Map<String, dynamic>>> getProjectTimeline(String projectId, {String? stage, String? eventType}) async {
    final uri = Uri.parse('$baseUrl/api/projects/$projectId/timeline').replace(queryParameters: {
      if (stage != null) 'stage': stage,
      if (eventType != null) 'eventType': eventType,
    });
    final resp = await http.get(uri, headers: _headers);
    if (resp.statusCode == 200) {
      return (json.decode(resp.body) as List).cast<Map<String, dynamic>>();
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<Map<String, dynamic>> addProjectTimelineNote(String projectId, {
    required String stage,
    required String title,
    String? description,
    String? eventDateUtc,
  }) async {
    final resp = await http.post(
      Uri.parse('$baseUrl/api/projects/$projectId/timeline/notes'),
      headers: _headers,
      body: json.encode({
        'stage': stage,
        'title': title,
        if (description != null && description.isNotEmpty) 'description': description,
        if (eventDateUtc != null) 'eventDateUtc': eventDateUtc,
      }),
    );
    if (resp.statusCode == 201 || resp.statusCode == 200) {
      return json.decode(resp.body) as Map<String, dynamic>;
    }
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  Future<void> deleteProjectTimelineEvent(String projectId, String eventId) async {
    final resp = await http.delete(
      Uri.parse('$baseUrl/api/projects/$projectId/timeline/$eventId'),
      headers: _headers,
    );
    if (resp.statusCode == 204 || resp.statusCode == 200) return;
    if (resp.statusCode == 401) throw ApiException('UNAUTHORIZED');
    throw ApiException(_parseError(resp));
  }

  // ═══════════════════════════════════════════════════════════════

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

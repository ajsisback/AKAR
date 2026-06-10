import 'package:flutter/material.dart';
import '../core/download_helper.dart';
import '../core/api_service.dart';
import '../core/l10n.dart';
import '../core/theme.dart';

class FileSearchScreen extends StatefulWidget {
  final String projectId;
  const FileSearchScreen({super.key, required this.projectId});

  @override
  State<FileSearchScreen> createState() => _FileSearchScreenState();
}

class _FileSearchScreenState extends State<FileSearchScreen> {
  final _searchController = TextEditingController();
  final _extController = TextEditingController();

  // State
  List<Map<String, dynamic>> _results = [];
  bool _loading = false;
  bool _loadingMore = false;
  String? _error;
  int _page = 1;
  int _totalCount = 0;
  int _totalPages = 0;
  bool _hasSearched = false;

  // Filters
  String? _selectedCategory;
  String _sortBy = 'createdAtUtc';
  String _sortDirection = 'desc';
  bool _includeDeleted = false;
  bool _filtersExpanded = false;

  // Image preview state
  bool _previewLoading = false;

  static const _categories = ['Document', 'Image', 'Spreadsheet', 'Presentation', 'Archive', 'Other'];
  static const _sortByOptions = ['createdAtUtc', 'originalFileName', 'fileSizeBytes', 'fileExtension'];

  @override
  void dispose() {
    _searchController.dispose();
    _extController.dispose();
    super.dispose();
  }

  String _mapError(String code, AppLocalizations l) {
    switch (code) {
      case 'PROJECT_NOT_FOUND': return l.t('err_project_not_found');
      case 'INVALID_PAGE':
      case 'INVALID_PAGE_SIZE':
      case 'INVALID_SORT_BY':
      case 'INVALID_SORT_DIRECTION':
      case 'INVALID_DATE_RANGE':
      case 'INVALID_FILE_CATEGORY':
        return l.t('err_invalid_search');
      case 'UNAUTHORIZED': return l.t('err_generic');
      default: return l.t('err_search_failed');
    }
  }

  String _categoryLabel(String? cat, AppLocalizations l) {
    switch (cat) {
      case 'Document': return l.t('cat_document');
      case 'Image': return l.t('cat_image');
      case 'Video': return l.t('cat_video');
      case 'Spreadsheet': return l.t('cat_spreadsheet');
      case 'Presentation': return l.t('cat_presentation');
      case 'Archive': return l.t('cat_archive');
      case 'Other': return l.t('cat_other');
      default: return l.t('cat_other');
    }
  }

  String _sortByLabel(String key, AppLocalizations l) {
    switch (key) {
      case 'createdAtUtc': return l.t('search_sort_date');
      case 'originalFileName': return l.t('search_sort_name');
      case 'fileSizeBytes': return l.t('search_sort_size');
      case 'fileExtension': return l.t('search_sort_ext');
      default: return key;
    }
  }

  String _formatSize(dynamic bytes) {
    if (bytes == null) return '—';
    final b = bytes is int ? bytes : int.tryParse(bytes.toString()) ?? 0;
    if (b < 1024) return '$b B';
    if (b < 1024 * 1024) return '${(b / 1024).toStringAsFixed(1)} KB';
    return '${(b / (1024 * 1024)).toStringAsFixed(1)} MB';
  }

  String _formatDate(dynamic d) {
    if (d == null) return '—';
    try {
      final dt = DateTime.parse(d.toString());
      return '${dt.year}-${dt.month.toString().padLeft(2, '0')}-${dt.day.toString().padLeft(2, '0')}';
    } catch (_) {
      return d.toString();
    }
  }

  IconData _categoryIcon(String? cat) {
    switch (cat) {
      case 'Document': return Icons.description;
      case 'Image': return Icons.image;
      case 'Video': return Icons.videocam;
      case 'Spreadsheet': return Icons.table_chart;
      case 'Presentation': return Icons.slideshow;
      case 'Archive': return Icons.archive;
      default: return Icons.insert_drive_file;
    }
  }

  Color _categoryColor(String? cat) {
    switch (cat) {
      case 'Document': return AkarTheme.accent;
      case 'Image': return AkarTheme.success;
      case 'Video': return AkarTheme.primaryLight;
      case 'Spreadsheet': return const Color(0xFF34C759);
      case 'Presentation': return const Color(0xFFFF9500);
      case 'Archive': return AkarTheme.textMuted;
      default: return AkarTheme.textSecondary;
    }
  }

  Future<void> _search({bool loadMore = false}) async {
    if (_loading || _loadingMore) return;

    setState(() {
      if (loadMore) {
        _loadingMore = true;
      } else {
        _loading = true;
        _error = null;
        _page = 1;
        _results = [];
      }
      _hasSearched = true;
    });

    final api = ApiService();
    await api.init();

    try {
      final data = await api.searchProjectFiles(
        widget.projectId,
        q: _searchController.text.trim().isEmpty ? null : _searchController.text.trim(),
        fileCategory: _selectedCategory,
        extension: _extController.text.trim().isEmpty ? null : _extController.text.trim(),
        sortBy: _sortBy,
        sortDirection: _sortDirection,
        includeDeleted: _includeDeleted ? true : null,
        page: loadMore ? _page + 1 : 1,
        pageSize: 20,
      );

      if (mounted) {
        final items = (data['items'] as List?)?.cast<Map<String, dynamic>>() ?? [];
        setState(() {
          if (loadMore) {
            _results.addAll(items);
            _page = data['page'] as int? ?? _page + 1;
          } else {
            _results = items;
            _page = data['page'] as int? ?? 1;
          }
          _totalCount = data['totalCount'] as int? ?? 0;
          _totalPages = data['totalPages'] as int? ?? 0;
          _loading = false;
          _loadingMore = false;
        });
      }
    } on ApiException catch (e) {
      if (mounted) {
        setState(() {
          _error = e.code;
          _loading = false;
          _loadingMore = false;
        });
      }
    } catch (_) {
      if (mounted) {
        setState(() {
          _error = 'ERR_GENERIC';
          _loading = false;
          _loadingMore = false;
        });
      }
    }
  }

  void _clearFilters() {
    setState(() {
      _selectedCategory = null;
      _extController.clear();
      _sortBy = 'createdAtUtc';
      _sortDirection = 'desc';
      _includeDeleted = false;
    });
  }

  Future<void> _downloadFile(Map<String, dynamic> file) async {
    final l = AppLocalizations.of(context);
    final fileId = file['id'] as String;
    final fileName = file['originalFileName'] as String? ?? 'download';

    try {
      final api = ApiService();
      await api.init();
      final bytes = await api.downloadFileBytes(widget.projectId, fileId);

      downloadFileBytes(bytes, fileName);
    } on ApiException catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(_mapError(e.code, l)), backgroundColor: AkarTheme.danger),
        );
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('err_generic')), backgroundColor: AkarTheme.danger),
        );
      }
    }
  }

  Future<void> _previewImage(Map<String, dynamic> file) async {
    final l = AppLocalizations.of(context);
    final fileId = file['id'] as String;
    final fileName = file['originalFileName'] as String? ?? '';

    setState(() => _previewLoading = true);

    try {
      final api = ApiService();
      await api.init();
      final bytes = await api.downloadFileBytes(widget.projectId, fileId);

      if (!mounted) return;
      setState(() => _previewLoading = false);

      showDialog(
        context: context,
        builder: (ctx) => Dialog(
          backgroundColor: Colors.transparent,
          insetPadding: const EdgeInsets.all(16),
          child: Stack(
            children: [
              // Image
              Center(
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(12),
                  child: Container(
                    constraints: BoxConstraints(
                      maxWidth: MediaQuery.of(ctx).size.width * 0.9,
                      maxHeight: MediaQuery.of(ctx).size.height * 0.8,
                    ),
                    decoration: BoxDecoration(
                      color: AkarTheme.bgCard,
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        // Header
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                          decoration: const BoxDecoration(
                            color: AkarTheme.bgCard,
                            borderRadius: BorderRadius.vertical(top: Radius.circular(12)),
                          ),
                          child: Row(
                            children: [
                              Expanded(
                                child: Text(
                                  fileName,
                                  style: const TextStyle(color: AkarTheme.textPrimary, fontSize: 14, fontWeight: FontWeight.w500),
                                  overflow: TextOverflow.ellipsis,
                                ),
                              ),
                              IconButton(
                                icon: const Icon(Icons.download, color: AkarTheme.accent, size: 20),
                                onPressed: () {
                                  Navigator.pop(ctx);
                                  _downloadFile(file);
                                },
                                tooltip: l.t('search_download'),
                              ),
                              IconButton(
                                icon: const Icon(Icons.close, color: AkarTheme.textMuted, size: 20),
                                onPressed: () => Navigator.pop(ctx),
                              ),
                            ],
                          ),
                        ),
                        // Image content
                        Flexible(
                          child: InteractiveViewer(
                            minScale: 0.5,
                            maxScale: 4.0,
                            child: Image.memory(
                              bytes,
                              fit: BoxFit.contain,
                              errorBuilder: (_, __, ___) => const Padding(
                                padding: EdgeInsets.all(40),
                                child: Icon(Icons.broken_image, size: 64, color: AkarTheme.textMuted),
                              ),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      );
    } on ApiException catch (e) {
      if (mounted) {
        setState(() => _previewLoading = false);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(_mapError(e.code, l)), backgroundColor: AkarTheme.danger),
        );
      }
    } catch (_) {
      if (mounted) {
        setState(() => _previewLoading = false);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l.t('err_generic')), backgroundColor: AkarTheme.danger),
        );
      }
    }
  }

  bool _isImage(Map<String, dynamic> file) {
    final cat = file['fileCategory'] as String?;
    final ct = file['contentType'] as String? ?? '';
    return cat == 'Image' || ct.startsWith('image/');
  }

  bool _isPdf(Map<String, dynamic> file) {
    final ext = (file['fileExtension'] as String? ?? '').toLowerCase();
    final ct = file['contentType'] as String? ?? '';
    return ext == '.pdf' || ct == 'application/pdf';
  }

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Scaffold(
      appBar: AppBar(
        title: Text(l.t('search_title')),
      ),
      body: Column(
        children: [
          // Search bar
          _buildSearchBar(l),
          // Filters
          _buildFilters(l),
          // Results count
          if (_hasSearched && !_loading && _error == null)
            _buildResultsCount(l),
          // Results
          Expanded(child: _buildBody(l)),
        ],
      ),
    );
  }

  Widget _buildSearchBar(AppLocalizations l) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 12, 16, 0),
      child: Row(
        children: [
          Expanded(
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: l.t('search_hint'),
                prefixIcon: const Icon(Icons.search, color: AkarTheme.textMuted, size: 20),
                suffixIcon: _searchController.text.isNotEmpty
                    ? IconButton(
                        icon: const Icon(Icons.clear, size: 18, color: AkarTheme.textMuted),
                        onPressed: () {
                          _searchController.clear();
                          setState(() {});
                        },
                      )
                    : null,
                contentPadding: const EdgeInsets.symmetric(vertical: 12, horizontal: 16),
                isDense: true,
              ),
              onChanged: (_) => setState(() {}),
              onSubmitted: (_) => _search(),
              textInputAction: TextInputAction.search,
            ),
          ),
          const SizedBox(width: 8),
          SizedBox(
            height: 46,
            child: ElevatedButton.icon(
              onPressed: _search,
              icon: const Icon(Icons.search, size: 18),
              label: Text(l.t('search_button')),
              style: ElevatedButton.styleFrom(
                padding: const EdgeInsets.symmetric(horizontal: 16),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFilters(AppLocalizations l) {
    return Card(
      margin: const EdgeInsets.fromLTRB(16, 10, 16, 0),
      child: Theme(
        data: Theme.of(context).copyWith(dividerColor: Colors.transparent),
        child: ExpansionTile(
          title: Row(
            children: [
              const Icon(Icons.tune, size: 18, color: AkarTheme.accent),
              const SizedBox(width: 8),
              Text(l.t('search_filters'),
                  style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500)),
              if (_selectedCategory != null || _extController.text.isNotEmpty || _includeDeleted) ...[
                const SizedBox(width: 8),
                Container(
                  width: 8, height: 8,
                  decoration: const BoxDecoration(
                    color: AkarTheme.accent,
                    shape: BoxShape.circle,
                  ),
                ),
              ],
            ],
          ),
          initiallyExpanded: _filtersExpanded,
          onExpansionChanged: (v) => _filtersExpanded = v,
          children: [
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Category dropdown
                  Text(l.t('file_category'),
                      style: const TextStyle(color: AkarTheme.textMuted, fontSize: 12)),
                  const SizedBox(height: 4),
                  DropdownButtonFormField<String?>(
                    initialValue: _selectedCategory,
                    isExpanded: true,
                    decoration: const InputDecoration(
                      contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                      isDense: true,
                    ),
                    items: [
                      DropdownMenuItem<String?>(
                        value: null,
                        child: Text(l.t('search_all_categories'),
                            style: const TextStyle(fontSize: 13)),
                      ),
                      ..._categories.map((c) => DropdownMenuItem<String?>(
                            value: c,
                            child: Text(_categoryLabel(c, l),
                                style: const TextStyle(fontSize: 13)),
                          )),
                    ],
                    onChanged: (v) => setState(() => _selectedCategory = v),
                  ),

                  const SizedBox(height: 12),

                  // Extension
                  Text(l.t('search_extension'),
                      style: const TextStyle(color: AkarTheme.textMuted, fontSize: 12)),
                  const SizedBox(height: 4),
                  TextField(
                    controller: _extController,
                    decoration: InputDecoration(
                      hintText: 'pdf, png, docx...',
                      contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                      isDense: true,
                      hintStyle: TextStyle(color: AkarTheme.textMuted.withValues(alpha: 0.6), fontSize: 13),
                    ),
                    style: const TextStyle(fontSize: 13),
                  ),

                  const SizedBox(height: 12),

                  // Sort by + direction in a row
                  Row(
                    children: [
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(l.t('search_sort_by'),
                                style: const TextStyle(color: AkarTheme.textMuted, fontSize: 12)),
                            const SizedBox(height: 4),
                            DropdownButtonFormField<String>(
                              initialValue: _sortBy,
                              isExpanded: true,
                              decoration: const InputDecoration(
                                contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                                isDense: true,
                              ),
                              items: _sortByOptions.map((s) => DropdownMenuItem(
                                    value: s,
                                    child: Text(_sortByLabel(s, l),
                                        style: const TextStyle(fontSize: 13)),
                                  )).toList(),
                              onChanged: (v) => setState(() => _sortBy = v ?? 'createdAtUtc'),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(l.t('search_sort_direction'),
                                style: const TextStyle(color: AkarTheme.textMuted, fontSize: 12)),
                            const SizedBox(height: 4),
                            DropdownButtonFormField<String>(
                              initialValue: _sortDirection,
                              isExpanded: true,
                              decoration: const InputDecoration(
                                contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                                isDense: true,
                              ),
                              items: [
                                DropdownMenuItem(value: 'desc',
                                    child: Text(l.t('search_sort_desc'), style: const TextStyle(fontSize: 13))),
                                DropdownMenuItem(value: 'asc',
                                    child: Text(l.t('search_sort_asc'), style: const TextStyle(fontSize: 13))),
                              ],
                              onChanged: (v) => setState(() => _sortDirection = v ?? 'desc'),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),

                  const SizedBox(height: 12),

                  // Include deleted switch
                  Row(
                    children: [
                      Text(l.t('search_include_deleted'),
                          style: const TextStyle(fontSize: 13)),
                      const Spacer(),
                      Switch(
                        value: _includeDeleted,
                        onChanged: (v) => setState(() => _includeDeleted = v),
                        activeThumbColor: AkarTheme.accent,
                      ),
                    ],
                  ),

                  const SizedBox(height: 8),

                  // Action buttons
                  Row(
                    children: [
                      Expanded(
                        child: OutlinedButton(
                          onPressed: _clearFilters,
                          style: OutlinedButton.styleFrom(
                            foregroundColor: AkarTheme.textSecondary,
                            side: const BorderSide(color: AkarTheme.border),
                            padding: const EdgeInsets.symmetric(vertical: 10),
                          ),
                          child: Text(l.t('search_clear_filters'), style: const TextStyle(fontSize: 13)),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: ElevatedButton(
                          onPressed: _search,
                          style: ElevatedButton.styleFrom(
                            padding: const EdgeInsets.symmetric(vertical: 10),
                          ),
                          child: Text(l.t('search_apply_filters'), style: const TextStyle(fontSize: 13)),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildResultsCount(AppLocalizations l) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(20, 10, 20, 0),
      child: Row(
        children: [
          Text(
            '$_totalCount ${l.t('search_results_count')}',
            style: const TextStyle(color: AkarTheme.textMuted, fontSize: 13),
          ),
        ],
      ),
    );
  }

  Widget _buildBody(AppLocalizations l) {
    if (_loading || _previewLoading) {
      return const Center(child: CircularProgressIndicator());
    }

    if (_error != null) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.error_outline, size: 48, color: AkarTheme.danger),
            const SizedBox(height: 12),
            Text(_mapError(_error!, l),
                style: const TextStyle(color: AkarTheme.textSecondary)),
            const SizedBox(height: 16),
            TextButton.icon(
              onPressed: _search,
              icon: const Icon(Icons.refresh),
              label: Text(l.t('search_button')),
            ),
          ],
        ),
      );
    }

    if (!_hasSearched) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.search, size: 64, color: AkarTheme.textMuted.withValues(alpha: 0.4)),
            const SizedBox(height: 16),
            Text(l.t('search_subtitle'),
                style: const TextStyle(color: AkarTheme.textMuted, fontSize: 15)),
          ],
        ),
      );
    }

    if (_results.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.search_off, size: 56, color: AkarTheme.textMuted.withValues(alpha: 0.4)),
            const SizedBox(height: 12),
            Text(l.t('search_no_results'),
                style: const TextStyle(color: AkarTheme.textSecondary, fontSize: 15)),
            const SizedBox(height: 4),
            Text(l.t('search_no_results_sub'),
                style: const TextStyle(color: AkarTheme.textMuted, fontSize: 13)),
          ],
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: () => _search(),
      child: ListView.builder(
        padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
        itemCount: _results.length + (_page < _totalPages ? 1 : 0),
        itemBuilder: (ctx, idx) {
          if (idx == _results.length) {
            return _buildLoadMoreButton(l);
          }
          return _buildResultCard(_results[idx], l);
        },
      ),
    );
  }

  Widget _buildResultCard(Map<String, dynamic> file, AppLocalizations l) {
    final name = file['originalFileName'] as String? ?? '—';
    final folder = file['folderName'] as String? ?? '';
    final cat = file['fileCategory'] as String?;
    final ext = file['fileExtension'] as String? ?? '';
    final size = _formatSize(file['fileSizeBytes']);
    final date = _formatDate(file['createdAtUtc']);
    final isDeleted = file['isDeleted'] == true;
    final isImg = _isImage(file);
    final isPdf = _isPdf(file);

    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: isImg ? () => _previewImage(file) : () => _downloadFile(file),
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Top row: icon + name + badges
              Row(
                children: [
                  // Category icon
                  Container(
                    width: 38, height: 38,
                    decoration: BoxDecoration(
                      color: _categoryColor(cat).withValues(alpha: 0.12),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Icon(_categoryIcon(cat), color: _categoryColor(cat), size: 20),
                  ),
                  const SizedBox(width: 12),
                  // File name + folder
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          name,
                          style: const TextStyle(fontWeight: FontWeight.w500, fontSize: 14),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                        const SizedBox(height: 2),
                        Text(
                          '${l.t('search_folder')}: $folder',
                          style: const TextStyle(color: AkarTheme.textMuted, fontSize: 12),
                        ),
                      ],
                    ),
                  ),
                  // Extension badge
                  if (ext.isNotEmpty)
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                      decoration: BoxDecoration(
                        color: AkarTheme.bgInput,
                        borderRadius: BorderRadius.circular(4),
                        border: Border.all(color: AkarTheme.border),
                      ),
                      child: Text(
                        ext.replaceFirst('.', '').toUpperCase(),
                        style: const TextStyle(fontSize: 10, color: AkarTheme.textMuted, fontWeight: FontWeight.w600),
                      ),
                    ),
                ],
              ),

              const SizedBox(height: 10),

              // Metadata row: category, size, date
              Row(
                children: [
                  Icon(Icons.category, size: 13, color: AkarTheme.textMuted.withValues(alpha: 0.6)),
                  const SizedBox(width: 4),
                  Text(_categoryLabel(cat, l),
                      style: const TextStyle(color: AkarTheme.textMuted, fontSize: 11)),
                  const SizedBox(width: 12),
                  Icon(Icons.data_usage, size: 13, color: AkarTheme.textMuted.withValues(alpha: 0.6)),
                  const SizedBox(width: 4),
                  Text(size,
                      style: const TextStyle(color: AkarTheme.textMuted, fontSize: 11)),
                  const SizedBox(width: 12),
                  Icon(Icons.calendar_today, size: 13, color: AkarTheme.textMuted.withValues(alpha: 0.6)),
                  const SizedBox(width: 4),
                  Text(date,
                      style: const TextStyle(color: AkarTheme.textMuted, fontSize: 11)),
                  const Spacer(),
                  // Deleted badge
                  if (isDeleted)
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                      decoration: BoxDecoration(
                        color: AkarTheme.danger.withValues(alpha: 0.12),
                        borderRadius: BorderRadius.circular(4),
                      ),
                      child: Text(
                        l.t('search_deleted_badge'),
                        style: const TextStyle(color: AkarTheme.danger, fontSize: 10, fontWeight: FontWeight.w600),
                      ),
                    ),
                ],
              ),

              const SizedBox(height: 10),
              const Divider(height: 1, color: AkarTheme.border),
              const SizedBox(height: 8),

              // Action buttons
              Row(
                mainAxisAlignment: MainAxisAlignment.end,
                children: [
                  if (isImg)
                    TextButton.icon(
                      onPressed: () => _previewImage(file),
                      icon: const Icon(Icons.visibility, size: 16),
                      label: Text(l.t('search_preview'), style: const TextStyle(fontSize: 12)),
                      style: TextButton.styleFrom(
                        foregroundColor: AkarTheme.accent,
                        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                        minimumSize: Size.zero,
                        tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                      ),
                    ),
                  if (isImg) const SizedBox(width: 8),
                  TextButton.icon(
                    onPressed: () => _downloadFile(file),
                    icon: const Icon(Icons.download, size: 16),
                    label: Text(
                      isPdf ? l.t('search_download_pdf') : l.t('search_download'),
                      style: const TextStyle(fontSize: 12),
                    ),
                    style: TextButton.styleFrom(
                      foregroundColor: AkarTheme.primaryLight,
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                      minimumSize: Size.zero,
                      tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildLoadMoreButton(AppLocalizations l) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Center(
        child: _loadingMore
            ? const SizedBox(
                width: 24, height: 24,
                child: CircularProgressIndicator(strokeWidth: 2))
            : OutlinedButton.icon(
                onPressed: () => _search(loadMore: true),
                icon: const Icon(Icons.expand_more, size: 18),
                label: Text(l.t('search_load_more')),
                style: OutlinedButton.styleFrom(
                  foregroundColor: AkarTheme.accent,
                  side: const BorderSide(color: AkarTheme.accent),
                  padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 10),
                ),
              ),
      ),
    );
  }
}

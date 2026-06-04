# Sprint 7: File Search & Preview

Sprint 7 adds File Search & Preview capabilities to AKAR, allowing project owners to quickly find, filter, preview, and securely download files across the Document Vault. 

The sprint was delivered in three stages to maintain Owner-first focus:

## Sprint 7A (Backend)
- Backend file search API (`GET /api/projects/{projectId}/files/search`).
- Search by partial file name (`q`).
- Advanced filters: category, extension, content type, folder, dates.
- Sorting (`sortBy`, `sortDirection`) and pagination (`page`, `pageSize`).
- Database search indexes optimized.
- Strict owner isolation and omission of internal storage internals (`storagePath`, `storedFileName`) in API responses.

## Sprint 7B (Flutter Owner UI)
- Flutter file search UI accessed from Project Details.
- Dynamic filters and "Load More" pagination.
- Secure image preview using authenticated in-memory bytes.
- PDF and other document downloads via temporary Blob URLs.
- Comprehensive Arabic/English localization.

## Sprint 7C (Angular Support View)
- Angular read-only file search support view in the Admin Portal.
- Safe read-only file metadata inspection.
- Secure file download mechanism matching Flutter's implementation.
- Documentation updates and PR readiness.

## Deferred Items
The following advanced search capabilities are deferred to future sprints to keep the scope focused:
- OCR (Optical Character Recognition).
- AI/Semantic search.
- Full-text file content search.
- File tagging.
- Image thumbnails.
- Advanced preview (e.g. PDF/DOCX rendering inside the browser).
- External search engine integrations.

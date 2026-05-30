# Sprint 2 Scope — Document Vault

## Objective

Add a full **Document Vault** to every project, allowing owners to organise construction documents in folders, upload / download files, soft-delete with trash, and restore items — all from the Flutter mobile app.

## Channel Ownership

| Channel | Role |
|---------|------|
| **Flutter Mobile App** | PRIMARY owner application (Sprint 2 features delivered here) |
| **Angular Admin Portal** | Internal / admin / support portal only (no Sprint 2 changes) |
| **Backend (.NET API)** | Shared API layer |

> Flutter is the only owner-facing channel for Sprint 2 features.

## Sprint 2 Status — Complete

All Sprint 2 features are implemented and verified in code across three sub-sprints.

---

### Sprint 2A — Folder Foundation ✅

Domain entities and API endpoints for project folders.

- **ProjectFolder entity** — system + custom folders, soft-delete, rename
- **FolderType enum** — License, ProjectLocation, Contracts, Drawings, Photos, Videos, Invoices, Warranties, FollowersInbox, Trash, Custom
- **10 default system folders** auto-created per project
- **Folder CRUD** — create custom folder, rename (custom only), soft-delete (custom only), list active folders
- **EF Core migration** — `AddDocumentVault` migration for `project_folders` + `project_files` tables
- **Owner-scoped data isolation** — all folder queries filtered by OwnerId from JWT

### Sprint 2B — File Storage APIs ✅

File upload, download, metadata, and trash management via local storage.

- **ProjectFile entity** — file metadata, soft-delete, restore
- **FileCategory enum** — Document, Image, Video, Other (auto-detected from extension)
- **StorageProvider enum** — Local (future: AzureBlob, S3)
- **IFileStorageService abstraction** — cloud-portable interface (Save, OpenRead, Delete)
- **LocalFileStorageService** — local disk storage under `storage/owners/{ownerId}/{projectId}/{folderId}/`
- **Upload API** — multipart file upload with 100 MB limit, extension whitelist, automatic category detection
- **Download API** — authenticated file streaming with original filename
- **File metadata API** — returns file details without downloading
- **Soft-delete API** — marks file as deleted, preserves physical file
- **Restore API** — restores soft-deleted file
- **Trash API** — lists deleted files and deleted custom folders per project
- **Upload validator** — FluentValidation for file size limit and allowed extensions

### Sprint 2C — Flutter Document Vault Screens ✅

Full Flutter UI for the document vault feature.

| Screen | File | Status |
|--------|------|--------|
| Document Vault | `lib/screens/document_vault_screen.dart` | ✅ Implemented |
| Folder Details | `lib/screens/folder_details_screen.dart` | ✅ Implemented |
| File Details Sheet | `lib/screens/file_details_sheet.dart` | ✅ Implemented |
| Trash | `lib/screens/trash_screen.dart` | ✅ Implemented |

Additional Flutter capabilities (Sprint 2C):
- API integration for all document vault endpoints (9 new API methods)
- File upload via `file_picker` package with confirmation dialog
- File download via Blob URL (web) with authenticated headers
- File metadata details in bottom sheet
- Soft-delete with move-to-trash flow
- Restore from trash with confirmation
- Folder management (create, rename, delete custom folders)
- System folder protection (no rename/delete)
- Folder type icons and localized labels
- File category badges (Document, Image, Video, Other)
- File size formatting
- Arabic/English localization (131 keys each, up from 62)
- Error handling with mapped backend error codes
- Pull-to-refresh on all list screens
- Entry point from Project Details screen via Document Vault card

## Verification

| Component | Status | Verification |
|-----------|--------|-------------|
| Backend API | ✅ Complete | `dotnet build` — 0 warnings, 0 errors |
| Flutter Mobile App | ✅ Complete | `flutter analyze` — no issues |
| EF Core Migrations | ✅ Complete | InitialCreate + AddDocumentVault |
| Arabic/English i18n | ✅ Complete | 131 keys each (AR + EN) |
| Local File Storage | ✅ Complete | `storage/owners/` directory structure |

## API Endpoints Added (Sprint 2)

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/projects/{id}/folders` | List project folders |
| POST | `/api/projects/{id}/folders` | Create custom folder |
| PUT | `/api/projects/{id}/folders/{folderId}` | Rename custom folder |
| DELETE | `/api/projects/{id}/folders/{folderId}` | Soft-delete custom folder |
| POST | `/api/projects/{id}/folders/{folderId}/files` | Upload file |
| GET | `/api/projects/{id}/folders/{folderId}/files` | List folder files |
| GET | `/api/projects/{id}/files/{fileId}` | Get file metadata |
| GET | `/api/projects/{id}/files/{fileId}/download` | Download file |
| DELETE | `/api/projects/{id}/files/{fileId}` | Soft-delete file |
| POST | `/api/projects/{id}/files/{fileId}/restore` | Restore file |
| GET | `/api/projects/{id}/trash` | List deleted items |

## Features NOT Included (Deferred to Sprint 3+)

- Project edit / update
- Project delete
- Cloud file storage (Azure Blob / S3)
- File previews (image thumbnails, PDF viewer)
- Folder drag-and-drop reordering
- Multi-file upload
- Permanent delete (purge from trash)
- File sharing / external links
- Notifications
- AI features
- Payment integration
- Multi-role access control (admin roles)
- Contract management workflows
- Engineering workflows
- Timeline tracking
- Marketplace / contractor ERP

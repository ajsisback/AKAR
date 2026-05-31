# AKAR — أكار

**Owner-first residential construction project vault for Saudi project owners.**

Project Vault + Construction Helper + Lightweight Directory

---

## Repository Structure

```
App/
├── backend/          .NET 10 Web API (Clean Architecture)
├── admin-portal/     Angular 19+ Admin Portal (AR/EN bilingual, timeline support view)
├── mobile/           Flutter Mobile App (AR/EN bilingual, Sprint 5 complete)
├── docker/           Docker Compose (PostgreSQL)
└── docs/             Architecture decisions & documentation
```

## Tech Stack

| Layer | Technology |
|-------|-----------| 
| Backend API | .NET 10, ASP.NET Core |
| Database | PostgreSQL 17 |
| Admin Portal | Angular 19+, TypeScript |
| Mobile App | Flutter 3.44+ / Dart 3.12+ |
| Auth | Self-hosted JWT |
| Containerization | Docker Compose |

## Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 20+
- Docker & Docker Compose
- Flutter SDK 3.44+ (installed and verified)

### 1. Start PostgreSQL
```bash
cd docker
docker compose up -d
```

### 2. Run Backend API
```bash
cd backend/src/Akar.Api
dotnet run
```
API will be available at `https://localhost:5001` with Swagger UI.

### 3. Run Admin Portal
```bash
cd admin-portal
npm install
npm start
```
Portal will be available at `http://localhost:4200`.

### 4. Run Flutter Mobile App
```bash
cd mobile
flutter pub get
flutter run -d chrome --web-port 8888
```
Mobile app will be available at `http://localhost:8888` (web mode) or on a connected device/emulator.

## Sprint 1 Status — Complete

### Backend API ✅
- Owner registration & login (JWT)
- Owner dashboard (project stats by stage)
- Create project, list projects, view project details
- Owner data isolation (all queries scoped by OwnerId from JWT)
- BCrypt password hashing
- Swagger UI with JWT bearer auth
- Health check endpoint

### Angular Admin Portal ✅
- Login, register, dashboard, projects list, create project, project details
- Arabic/English bilingual (128 translation keys each)
- RTL/LTR layout switching with language persistence
- JWT interceptor, auth guard, environment config

### Flutter Mobile App ✅
- Login, register, dashboard, projects list, create project, project details
- Backend API integration (all endpoints)
- JWT token storage via SharedPreferences
- Arabic/English bilingual (62 translation keys each)
- Arabic default language
- RTL/LTR support (locale-driven)
- Language switcher with persistence via SharedPreferences
- Dark theme matching admin portal
- `flutter analyze` passes with 0 issues

### Infrastructure ✅
- PostgreSQL 17 via Docker Compose (port 5433)
- EF Core code-first migration (owners + projects tables)
- Git baseline: `sprint-1-closed` tag

---

## Sprint 2 Status — Complete ✅ (Document Vault) — Merged

### Backend API — Document Vault ✅
- ProjectFolder & ProjectFile entities (DDD, soft-delete)
- 10 default system folders auto-created per project
- Folder CRUD (create custom, rename, soft-delete)
- File upload (multipart, 100 MB limit, extension whitelist)
- File download (authenticated streaming)
- File metadata, soft-delete, restore
- Trash API (list deleted files + folders)
- LocalFileStorageService (cloud-portable IFileStorageService)
- EF Core migration: `AddDocumentVault`
- 11 new API endpoints

### Flutter Mobile App — Document Vault ✅
- Document vault screen (folder list with system/custom indicators)
- Folder details screen (file list with upload)
- File details bottom sheet (metadata, download, delete)
- Trash screen (deleted files restore, deleted folders display)
- File upload via `file_picker` with confirmation dialog
- File download via Blob URL (web) with authenticated headers
- Arabic/English bilingual (123 translation keys each, up from 62)
- `flutter analyze` passes with 0 issues

### Angular Admin Portal — Document Vault Support View ✅
- Read-only vault section in project details (folder list, file metadata, trash summary)
- Authenticated file download via Blob URL (JWT in Authorization header)
- DocumentVaultService with 4 methods (getProjectFolders, getFolderFiles, getProjectTrash, downloadFile)
- Arabic/English bilingual (128 translation keys each, +21 vault keys)
- No upload, no delete, no restore — admin support view only

### Infrastructure ✅
- Local file storage under `storage/owners/` (cloud-portable via `IFileStorageService`)
- EF Core migrations: InitialCreate + AddDocumentVault
- Tag: `sprint-2-closed`

---

## Sprint 3 Status — Complete ✅ (Followers & Incoming Files) — Merged

### Sprint 3A — Followers Foundation ✅
- ProjectFollower entity with FollowerType enum (Supervisor, Relative, Contractor, Designer, EngineeringOffice, Other)
- Follower CRUD APIs (GET, POST, PUT, DELETE soft-delete)
- Automatic FollowerInbox folder creation per follower
- Owner isolation enforced via JWT

### Sprint 3B — Follower Upload Links ✅
- FollowerUploadLink entity with SHA256 token hashing
- Owner APIs: Generate, List (preview only), Revoke
- Public APIs: Get Info (minimal safe data), Upload File (multipart/form-data)
- Raw token returned once at generation only
- File validation (PDF, DOC, DOCX, XLS, XLSX, JPG, JPEG, PNG, WEBP, MP4, MOV)
- Dangerous files rejected (EXE, BAT, CMD, PS1, SH, JS, MSI, DLL, ZIP)

### Sprint 3C — Flutter Owner Followers UI ✅
- Followers list screen with type icons and status badges
- Add/Edit follower form with type dropdown
- Follower details with inbox shortcut and upload links section
- Generate upload link with save-now warning dialog
- Copy link, revoke link, list links (preview only)
- Open follower inbox shortcut to Document Vault folder
- 49 AR/EN localization keys

### Sprint 3D — Public Follower Upload Page ✅
- Public route: `/#/follower-upload/{token}`
- Token-based info loading (no JWT required)
- File picker + upload (no JWT required)
- Invalid/expired/revoked token error screens with translated messages
- Language switcher on public page
- 20 AR/EN localization keys

### Security Model ✅
- Raw token returned once only at generation
- Token hash stored in database (SHA256)
- No owner JWT in upload link or public upload requests
- Public upload page is upload-only (no browse, download, delete, or file listing)
- Storage paths are not exposed in API responses
- All error messages mapped to localized translations

### Deferred
- Angular follower admin UI
- Follower login / auth
- Notifications
- Rate limiting on public upload
- File download via token
- Contracts, Saudi building code, directory

---

## Sprint 4 Status — Complete ✅ (Ready Contracts) — Merged

### Backend API — Ready Contracts ✅
- ContractTemplate entity with 7 seeded Saudi construction templates
- ProjectContract entity with CRUD + status workflow (Draft → ReadyForPdf → PdfGenerated → SignedUploaded / Cancelled)
- PDF generation via QuestPDF with Arabic font rendering (Noto Sans Arabic)
- Contract data stored as JSON (scope of work, payment terms, obligations)
- 9 new API endpoints (7 owner + 2 template)

### Flutter Mobile App — Contracts UI ✅
- Contract templates list with Arabic/English names
- Contract creation form with template fields
- Contract details with status badges
- PDF generation and download
- 260 AR/EN localization keys each

### Angular Admin Portal — Contracts Support View ✅
- Read-only contracts section in project details
- Contract list table with status badges
- Contract detail panel with all fields + PDF download
- 166 AR/EN localization keys each

---

## Sprint 5 Status — Complete ✅ (Project Timeline)

### Sprint 5A — Timeline Foundation ✅
- ProjectTimelineEvent entity with DDD patterns
- TimelineEventType and TimelineSourceType enums
- Stage APIs (PUT update stage) + Timeline APIs (GET list, POST note, DELETE note)
- System StageChanged events auto-created on stage update
- EF Core migration: AddProjectTimeline

### Sprint 5B — Automatic Timeline Events ✅
- Automatic events for: file upload, contract created, contract PDF, follower added, follower file uploaded
- Duplicate prevention using SourceId + SourceType
- System events protected from deletion

### Sprint 5C — Flutter Timeline UI ✅
- Timeline screen with stage progress card, change stage dialog, manual note CRUD
- Event list with type icons, color coding, system/manual badges
- Stage and event type filters, pull-to-refresh
- 40 AR/EN localization keys each (300 total per language)

### Sprint 5D — Angular Support View & Documentation ✅
- Read-only timeline support section in project details
- Stage progress bar, timeline events table with filters
- TimelineService with getProjectTimeline method
- 30 AR/EN localization keys each (196 total per language)
- Sprint 5 documentation and PR readiness

### Deferred
- Sub-stages
- Stage approvals
- Task management
- Gantt charts
- Advanced scheduling
- Timeline attachments
- Notifications

## Language Support
- **Arabic (العربية)** — Default, RTL layout
- **English** — LTR layout
- Language switcher available on all screens in both Angular and Flutter

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

## Release Documentation

For pilot release preparation, deployment guides, and Android build details, please see:
- [Pilot Runbook](docs/release/pilot-runbook.md) — Step-by-step local execution and release guidance.
- [Android Build Readiness](docs/release/android-build-readiness.md) — Android configuration, build commands, and signing readiness.
- [Pilot Release Readiness Checklist](docs/release/pilot-release-readiness.md) — Final checks before pilot deployment.

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
docker compose ps          # confirm "healthy"
```

### 2. Run Backend API
```bash
cd backend/src/Akar.Api
dotnet run
```
- API: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger` (dev only)
- Health check: `http://localhost:5000/health`

### 3. Run Admin Portal (Angular)
```bash
cd admin-portal
npm install
npm start
```
Portal: `http://localhost:4200`

### 4. Run Flutter Mobile App
```bash
cd mobile
flutter pub get
flutter run -d chrome --web-port 8888
```
Mobile app: `http://localhost:8888` (web mode)

### Environment Configuration

All backend configuration is driven by `appsettings.{Environment}.json` or environment variables.
See `appsettings.Example.json` for a reference of all configurable values.

For staging/pilot, key settings to update:
- `Jwt:Key` — **Must** be changed from the dev placeholder
- `ConnectionStrings:DefaultConnection` — Production database
- `Cors:AllowedOrigins` — Deployed frontend URLs
- `Storage:LocalRootPath` — Persistent server path

Flutter API URL can be overridden at build time:
```bash
flutter run --dart-define=AKAR_API_URL=https://api.yourdomain.com
```

### Troubleshooting

| Issue | Fix |
|---|---|
| Port 5000 locked | `Stop-Process -Name Akar.Api -Force` or kill the process using port 5000 |
| PostgreSQL not healthy | `docker compose down -v && docker compose up -d` (resets data) |
| Flutter web OOM | Close other Chrome tabs, increase `--old-gen-heap-size` |
| Angular build fails | Delete `node_modules`, run `npm install` again |
| CORS errors | Verify `Cors:AllowedOrigins` in appsettings includes your frontend URL |

### Useful URLs (Local Development)

| Service | URL |
|---|---|
| Backend API | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |
| Health Check | http://localhost:5000/health |
| Angular Admin | http://localhost:4200 |
| Flutter Web | http://localhost:8888 |
| PostgreSQL | localhost:5433 (via Docker) |


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

---

## Sprint 6 Status — Complete ✅ (Signed Contract Upload)

### Backend API — Signed Contracts ✅
- Added `SignedFileId` to `ProjectContract`.
- New `POST /api/projects/{projectId}/contracts/{contractId}/upload-signed` endpoint.
- File upload integration with `IFileStorageService` (10MB limit, PDF only).
- Contract status transitions to `SignedUploaded` automatically.

### Flutter Mobile App — Contracts UI Updates ✅
- Owner-facing UI to upload a manually signed PDF when contract is `PdfGenerated`.
- Added "Download Signed Version" button when contract is `SignedUploaded`.
- Integrated `file_picker` for PDF selection with size and extension validation.
- AR/EN localization for new statuses and buttons.

### Angular Admin Portal — Support View Updates ✅
- Read-only visibility of `SignedUploaded` status.
- Added "Signed" column in contract list and "Download Signed PDF" button in contract details.
- Ensured backend file download endpoints are used securely via Blob URLs (no JWTs in URLs).

---

## Sprint 7 Status — Complete ✅ (File Search & Preview)

### Backend API — File Search ✅
- Added `GET /api/projects/{projectId}/files/search` endpoint.
- File search with filters (query, category, extension) and pagination.
- Secure owner isolation and omitted internal storage paths.

### Flutter Mobile App — Owner Search UI ✅
- File search UI accessed from Project Details.
- Dynamic filters and "Load More" pagination.
- Secure image preview and PDF/document downloads via Blob URLs.

### Angular Admin Portal — Support Search View ✅
- Read-only file search support view in the Admin Portal.
- Search filters (query, category, extension, sort).
- Safe read-only file metadata inspection and secure downloads.

## Sprint 8 Status — Complete ✅ (Owner Profile & Project Settings)

### Backend API — Owner Settings ✅
- Owner Profile API (GET, PUT)
- Change Password API
- Project Settings API (GET, PUT) with MapLink field
- All endpoints owner-isolated via JWT

### Flutter Mobile App — Settings UI ✅
- Owner profile screen (view/edit)
- Change password screen
- Project settings screen (view/edit, MapLink, stage read-only)
- AR/EN localization parity

### Angular Admin Portal — Support Views ✅
- Read-only owner profile support view on dashboard
- Read-only project settings support view in project details
- Stage display-only with "managed from timeline" note

---

## Sprint 9 Status — Complete ✅ (Pilot Readiness & Quality Hardening)

### Sprint 9A — Backend Hardening ✅
- Validation pipeline cleanup — all commands return Result objects
- Zero bare IRequest commands remaining
- ExceptionHandlingMiddleware hardened — no raw exceptions leak

### Sprint 9B — Flutter UX Polish ✅
- Shared state widgets: AkarLoadingState, AkarEmptyState, AkarErrorState, AkarPrimaryButton
- Centralized error mapping (localizeError) for 40+ API error codes
- 20/20 runtime smoke checks passed
- AR/EN localization parity: 391 keys each

### Sprint 9C — Angular Support Polish ✅
- Loading/empty/error/retry states added to all support views
- Read-only badges and clarity improvements
- MapLink safety: `target="_blank" rel="noopener noreferrer"`
- Common i18n keys added

---

## Sprint 10 Status — Complete ✅ (Release Readiness)

### Pilot Release Documentation ✅
- Pilot runbook with full local startup sequence and deployment guidance
- Android build readiness guide (debug APK, release APK, app bundle)
- Pilot release readiness checklist with security review
- Environment configuration examples for backend, Flutter, and Angular

### Infrastructure ✅
- `appsettings.Example.json` template for all configurable values
- CORS, JWT, storage, and database configuration documented
- Android `network_security_config.xml` for cleartext HTTP restriction
- Flutter `--dart-define=AKAR_API_URL` build-time configuration

---

## Sprint 11 Status — Complete ✅ (Pilot UAT Setup)

> ⚠️ Sprint 11 is **read-only admin monitoring** only. It does not include admin write/update/delete actions, owner impersonation, full RBAC, audit logs, or production seed strategy.

### Sprint 11A — Backend Admin Baseline ✅
- `AdminUser` entity and `AdminRole` enum (`SuperAdmin`, `SupportAdmin`)
- EF Core migration: `AddAdminUsers` (admin_users table)
- Admin authentication: `POST /api/admin/auth/login` (JWT with `userType=Admin`)
- Admin read-only APIs (all require `AdminOnly` policy):
  - `GET /api/admin/owners` — List all owners with project counts
  - `GET /api/admin/owners/{ownerId}` — Owner detail with project summaries
  - `GET /api/admin/projects` — List all projects system-wide
  - `GET /api/admin/projects/{projectId}` — Project detail with entity counts
- Authorization policies: `AdminOnly` (userType=Admin), `SuperAdminOnly` (userType=Admin + role=SuperAdmin)
- Pilot seed endpoint: `POST /api/dev/seed/pilot` (Development-only, returns 404 in other environments)
- Seed creates: 2 admin accounts, 1 pilot owner, project, folders, followers, timeline, contract, upload link

### Sprint 11B — Angular Admin Portal Read-Only Views ✅
- Admin login (`/admin/login`) with isolated `akar_admin_token`
- Admin dashboard (`/admin/dashboard`) with total owners/projects counts
- Owners list (`/admin/owners`) and details (`/admin/owners/:id`)
- Projects list (`/admin/projects`) and details (`/admin/projects/:id`)
- AdminAuthService, AdminApiService, AdminGuard, AuthInterceptor (dual-token URL routing)
- All admin views display "Admin View — Read Only" badge
- Arabic/English bilingual (38 admin i18n keys each)

### Security Model ✅
- Admin and Owner are completely separate user contexts (different JWTs, different APIs, different localStorage keys)
- Owner token cannot access admin APIs (AdminOnly policy check)
- Admin token does not affect owner APIs (different sub claim)
- All admin endpoints are GET-only (read-only)
- Seed endpoint is gated by `IsDevelopment()` — returns 404 in production
- Passwords hashed with BCrypt, no raw passwords or hashes exposed

### Deferred
- Admin write/update/delete actions
- Owner impersonation
- Production seed strategy
- Audit logs
- Full RBAC matrix
- Password reset for admin
- Billing/subscriptions

---

## Sprint 12 Status — Complete ✅ (Pilot UAT Execution & MVP Gap Closure)

### Sprint 12A — End-to-End UAT Smoke Test ✅
- Owner flow: 23/23 API tests passed (register, login, dashboard, profile, password, project, folders, search, followers, upload links, contracts, timeline, settings, trash)
- Admin flow: 5/5 API tests passed (admin login, owners, owner details, projects, project details)
- Security: 4/4 tests passed (invalid login 401, unauthenticated 401, owner→admin blocked 403, admin isolation)

### Sprint 12B — MVP Gap Closure ✅
- Subscription placeholder: "Pilot — Trial" badge added to owner profile screen (local-only, no backend changes)
- i18n parity verified: 394 keys each (AR/EN), 100% match
- Error handling: All 30+ mapped error codes verified present in both AR/EN i18n files

### Sprint 12C — Pilot Release Readiness ✅
- All release checklist items verified (health, seed, CORS, HTTPS, env vars, auth guards)
- Angular production build: 0 errors
- Backend build: 0 warnings, 0 errors
- Flutter analyze: 0 issues

### Sprint 12D — Content Readiness Assessment ✅
- Building guide, Saudi Code, directory: documented as deferred to Sprint 13

### Deferred
- Step-by-step building guide (Sprint 13 — static JSON)
- Saudi Code simplified content (Sprint 13 — static JSON)
- Lightweight directory (Sprint 13+ — static categories)
- Custom app icon (before Play Store)
- Custom splash screen (before Play Store)
- Android applicationId rename to `com.meyaar.akar` (before Play Store)

## Language Support
- **Arabic (العربية)** — Default, RTL layout
- **English** — LTR layout
- Language switcher available on all screens in both Angular and Flutter


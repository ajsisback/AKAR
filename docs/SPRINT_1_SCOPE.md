# Sprint 1 Scope

## Channel Ownership

| Channel | Role |
|---------|------|
| **Flutter Mobile App** | PRIMARY owner application |
| **Angular Admin Portal** | Internal / admin / support portal only |
| **Backend (.NET API)** | Shared API layer |

> From Sprint 2 onward, all new owner-facing features must be in Flutter.

## Sprint 1 Status — Complete

All Sprint 1 features are implemented and verified in code.

### 1. Owner Authentication ✅
- Register (email, phone, password, optional company name)
- Login (email + password)
- JWT token generation (HMAC-SHA256)
- BCrypt password hashing
- Implemented in: Backend API, Angular admin portal, Flutter mobile app

### 2. Owner Dashboard ✅
- Total project count
- Breakdown by construction stage (Not Started, Structural, Finishing, Completed)
- Implemented in: Backend API, Angular admin portal, Flutter mobile app

### 3. Project CRUD ✅
- Create project (name, type, city, location, map link, stage, optional image URL)
- List owner's projects (scoped by OwnerId)
- View project details
- Owner data isolation enforced at Application layer
- Implemented in: Backend API, Angular admin portal, Flutter mobile app

### 4. Bilingual Support (Arabic/English) ✅
- Arabic is the default language
- Dynamic RTL/LTR switching
- All UI text in translation JSON files
- Backend returns stable error codes for frontend translation
- Language persistence via localStorage (Angular) and SharedPreferences (Flutter)
- Implemented in: Angular admin portal, Flutter mobile app

### 5. Technical Foundation ✅
- .NET 10 Clean Architecture backend (5 projects + test project)
- Angular 19+ admin portal (6 pages, builds with 0 errors)
- Flutter mobile app (6 screens, `flutter analyze` passes with 0 issues)
- PostgreSQL 17 via Docker Compose (port 5433)
- EF Core code-first migration (owners + projects tables)
- Swagger UI with JWT bearer auth
- CORS configured for Angular (4200) and Flutter web (8888)

### 6. Flutter Mobile App — Sprint 1 Complete ✅

The Flutter mobile app is fully implemented for Sprint 1 and includes:

| Screen | File | Status |
|--------|------|--------|
| Login | `lib/screens/login_screen.dart` | ✅ Implemented |
| Register | `lib/screens/register_screen.dart` | ✅ Implemented |
| Dashboard | `lib/screens/dashboard_screen.dart` | ✅ Implemented |
| Projects List | `lib/screens/projects_screen.dart` | ✅ Implemented |
| Create Project | `lib/screens/create_project_screen.dart` | ✅ Implemented |
| Project Details | `lib/screens/project_details_screen.dart` | ✅ Implemented |

Additional Flutter capabilities:
- Backend API integration (all endpoints)
- JWT token storage via SharedPreferences
- Arabic/English localization (62 keys each, AR default)
- RTL/LTR support (locale-driven via MaterialApp)
- Language switcher in AppBar
- Language persistence via SharedPreferences
- Dark theme matching admin portal color scheme
- Bottom navigation (Dashboard + Projects)
- Auth flow with token check on startup

## Features NOT Included (Deferred to Sprint 2+)

- File upload / document storage
- Contract management
- Marketplace / contractor ERP
- Notifications
- AI features
- Payment integration
- Multi-role access control (admin roles)
- Engineering workflows
- Timeline tracking
- Project edit/update
- Project delete

## Sprint 2

> Sprint 2 (Document Vault) is complete. See [SPRINT_2_SCOPE.md](SPRINT_2_SCOPE.md) for details.

# AKAR - Architecture Notes

## Overview

AKAR (أكار) is an **owner-first residential construction project vault** for Saudi homeowners.

The MVP focuses on:
- Owner authentication (self-hosted JWT)
- Owner dashboard (project status overview)
- Project CRUD (create, list, view)
- Bilingual support (Arabic default / English)

## Product Channel Ownership

> ⚠️ **THIS IS A MANDATORY ARCHITECTURAL RULE**

| Channel | Role | Target User |
|---------|------|-------------|
| **Flutter Mobile App** | **PRIMARY** owner application | Saudi homeowners |
| **Angular Admin Portal** | Internal / admin / support portal | Internal staff |
| **Backend (.NET API)** | Shared API layer | Both channels |

### Rules

1. **Flutter is the MAIN owner application.** All owner-facing features must be planned and implemented primarily in Flutter.
2. **Angular is NOT the owner application.** It serves only as an internal admin/support portal.
3. **Backend is channel-agnostic.** The API serves both Flutter and Angular equally, with no channel-specific logic.
4. From Sprint 2 onward, all owner-facing features must be implemented in Flutter first.
5. Angular should only receive admin/support features (e.g., user management, system monitoring).

### Flutter SDK Status

> ✅ **Flutter SDK is installed and verified.**
>
> - Flutter 3.44.0 (stable channel)
> - Dart 3.12.0
> - `flutter analyze` passes with 0 issues
> - Available targets: Windows (desktop), Chrome (web), Edge (web)
>
> Flutter mobile app is fully implemented through Sprint 5 with timeline screens,
> API integration, JWT storage, AR/EN localization (300 keys), and RTL/LTR support.

## Architecture

### Backend (.NET 10 - Clean Architecture)

```
Akar.Domain         → Entities, Enums, Value Objects (zero dependencies)
Akar.Shared         → Result<T> monad, abstractions (IFileStorageService)
Akar.Application    → CQRS (MediatR), DTOs, Validators, Behaviors
Akar.Infrastructure → EF Core, PostgreSQL, JWT, BCrypt
Akar.Api            → Controllers, Middleware, Program.cs
```

### Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| **Self-hosted JWT** | No dependency on external identity providers. Cloud-portable. |
| **PostgreSQL** | Docker-hosted. No vendor lock-in. |
| **Stable error codes** | Backend returns codes like `AUTH_INVALID_CREDENTIALS`. Frontend translates to Arabic/English. |
| **Owner-scoped queries** | All project queries filtered by `OwnerId` from JWT claims. No cross-owner data leakage. |
| **Result\<T\> monad** | No exceptions for business logic. Explicit success/failure flow. |
| **Swashbuckle 6.9** | Used instead of built-in OpenAPI due to .NET 10 Microsoft.OpenApi v2 breaking changes. |
| **IFileStorageService** | Cloud-portable abstraction. LocalFileStorageService for dev, future swap to Azure/S3. |
| **Soft-delete** | Files and folders use `IsDeleted` flag. Physical files preserved for trash/restore. |

### Localization Strategy

- Backend: **stateless** — returns stable error codes only
- Flutter (primary): **stateful** — `flutter_localizations` + `intl` + JSON translation files (300 keys AR/EN)
- Angular (admin): **stateful** — `ngx-translate` + JSON translation files (196 keys AR/EN)
- Arabic is the default language in both channels
- RTL/LTR toggling handled natively in Flutter via `Directionality` (locale-driven)
- RTL/LTR toggling in Angular via `LanguageService` setting `dir` attribute on `<html>`
- Language persistence: SharedPreferences (Flutter), localStorage (Angular)

### Database

- PostgreSQL 17 via Docker (port **5433** to avoid local PG conflict)
- EF Core with code-first migrations
- Snake_case column naming convention
- Enums stored as strings for readability

### Security

- BCrypt password hashing
- JWT Bearer authentication
- Owner-scoped data isolation at the Application layer
- CORS restricted to dev servers in development

## Project Structure

```
C:\Users\JSSurface2\Desktop\AKAR\App
├── backend/          .NET 10 Web API (Clean Architecture) — Shared API
├── mobile/           Flutter Mobile App (AR/EN bilingual, Sprint 5 complete)
├── admin-portal/     Angular admin portal — INTERNAL USE ONLY (Sprint 5D timeline support view)
├── storage/          Local file storage (document vault files)
├── docker/           Docker Compose (PostgreSQL)
└── docs/             Architecture notes
```

## Running Locally

### Prerequisites
- .NET 10 SDK
- Flutter SDK 3.44+ (installed ✅)
- Node.js 20+ (for admin portal only)
- Docker Desktop

### Steps
1. Start PostgreSQL: `cd docker && docker compose up -d`
2. Apply migrations: `cd backend && dotnet ef database update --project src/Akar.Infrastructure --startup-project src/Akar.Api`
3. Run API: `cd backend/src/Akar.Api && dotnet run`
4. Run Flutter App: `cd mobile && flutter run -d chrome --web-port 8888`
5. Run Admin Portal: `cd admin-portal && npm start`

## Sprint 1 Completion

All Sprint 1 features are implemented and verified across all three layers:

| Component | Status | Build/Analyze |
|-----------|--------|---------------|
| Backend API | ✅ Complete | `dotnet build` — 0 warnings, 0 errors |
| Angular Admin Portal | ✅ Complete | `npm run build` — success |
| Flutter Mobile App | ✅ Complete | `flutter analyze` — no issues |
| PostgreSQL Docker | ✅ Implemented | Requires Docker Desktop running |
| EF Core Migration | ✅ Implemented | InitialCreate (owners + projects) |
| Arabic/English i18n | ✅ Complete | Both Angular and Flutter |
| RTL/LTR Support | ✅ Complete | Both Angular and Flutter |
| Git Baseline | ✅ Tagged | `sprint-1-closed` |

## Sprint 2 Completion — Document Vault (Merged ✅)

All Sprint 2 features are implemented, verified, and merged to main:

| Component | Status | Build/Analyze |
|-----------|--------|---------------|
| Backend API (Vault) | ✅ Complete | `dotnet build` — 0 warnings, 0 errors |
| Flutter (Vault UI) | ✅ Complete | `flutter analyze` — no issues |
| Angular (Vault Support) | ✅ Complete | `npm run build` — success |
| EF Core Migration | ✅ Complete | AddDocumentVault (project_folders + project_files) |
| Local File Storage | ✅ Complete | `storage/owners/` directory structure |
| Tag | ✅ | `sprint-2-closed` |

## Sprint 3 Completion — Followers & Incoming Files (Pending PR Review)

All Sprint 3 features are implemented and verified:

| Component | Status | Build/Analyze |
|-----------|--------|---------------|
| Backend (Followers) | ✅ Complete | `dotnet build` — success, `dotnet test` — passed |
| Backend (Upload Links) | ✅ Complete | SHA256 token hashing, public upload APIs |
| Flutter (Owner Followers UI) | ✅ Complete | `flutter analyze` — no issues |
| Flutter (Public Upload Page) | ✅ Complete | `flutter build web` — success |
| Angular | N/A | No changes in Sprint 3 |
| EF Core Migration | ✅ Complete | AddFollowersAndUploadLinks |
| Security | ✅ Verified | Token hash stored only, public APIs no JWT |
| Arabic/English i18n (Flutter) | ✅ Complete | 201 keys each (AR + EN) |
| API Endpoints | ✅ Complete | 10 new endpoints (8 owner + 2 public) |
| Branch | ✅ Pushed | `feature/sprint-3-followers-inbox` (merged to main) |
| Tag | ✅ | `sprint-3-closed` |

## Sprint 4 Completion — Ready Contracts

All Sprint 4 features are implemented and verified:

| Component | Status | Build/Analyze |
|-----------|--------|---------------|
| Backend (Templates + Contracts) | ✅ Complete | `dotnet build` — success, `dotnet test` — passed |
| Backend (PDF Generation) | ✅ Complete | QuestPDF + Arabic font rendering verified |
| Flutter (Contracts UI) | ✅ Complete | `flutter analyze` — no issues |
| Angular (Contracts Support) | ✅ Complete | `npm run build` — success |
| EF Core Migration | ✅ Complete | AddReadyContracts (contract_templates + project_contracts) |
| Seed Data | ✅ Complete | 7 contract templates seeded |
| PDF Storage | ✅ Complete | PDFs saved to project Contracts folder |
| Security | ✅ Verified | Owner isolation, no JWT in URL, Blob download |
| Arabic/English i18n (Flutter) | ✅ Complete | 260 keys each (AR + EN) |
| Arabic/English i18n (Angular) | ✅ Complete | 166 keys each (AR + EN) |
| API Endpoints | ✅ Complete | 9 new endpoints (7 owner + 2 templates) |
| Branch | ✅ Pushed | `feature/sprint-4-ready-contracts` |

## Sprint 5 Completion — Project Timeline

All Sprint 5 features are implemented and verified:

| Component | Status | Build/Analyze |
|-----------|--------|---------------|
| Backend (Timeline Foundation) | ✅ Complete | `dotnet build` — success, `dotnet test` — passed |
| Backend (Automatic Events) | ✅ Complete | Duplicate prevention, system event protection |
| Flutter (Timeline UI) | ✅ Complete | `flutter analyze` — no issues |
| Angular (Timeline Support) | ✅ Complete | `npm run build` — success |
| EF Core Migration | ✅ Complete | AddProjectTimeline |
| Security | ✅ Verified | Owner isolation, no JWT in URL, system events protected |
| Arabic/English i18n (Flutter) | ✅ Complete | 300 keys each (AR + EN) |
| Arabic/English i18n (Angular) | ✅ Complete | 196 keys each (AR + EN) |
| Integration Tests | ✅ Complete | 27/27 passed |
| Branch | ✅ Pushed | `feature/sprint-5-project-timeline` |

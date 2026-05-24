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
> Flutter mobile app is fully implemented for Sprint 1 with 6 screens,
> API integration, JWT storage, AR/EN localization, and RTL/LTR support.

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

### Localization Strategy

- Backend: **stateless** — returns stable error codes only
- Flutter (primary): **stateful** — `flutter_localizations` + `intl` + JSON translation files (62 keys AR/EN)
- Angular (admin): **stateful** — `ngx-translate` + JSON translation files (107 keys AR/EN)
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
├── mobile/           Flutter mobile app — PRIMARY OWNER APP (Sprint 1 complete)
├── admin-portal/     Angular admin portal — INTERNAL USE ONLY (Sprint 1 complete)
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

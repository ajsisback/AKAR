# AKAR — أكار

**Owner-first residential construction project vault for Saudi project owners.**

Project Vault + Construction Helper + Lightweight Directory

---

## Repository Structure

```
App/
├── backend/          .NET 10 Web API (Clean Architecture)
├── admin-portal/     Angular 19+ Admin Portal (AR/EN bilingual)
├── mobile/           Flutter Mobile App (AR/EN bilingual, Sprint 1 complete)
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
- Arabic/English bilingual (107 translation keys each)
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

## Language Support
- **Arabic (العربية)** — Default, RTL layout
- **English** — LTR layout
- Language switcher available on all screens in both Angular and Flutter

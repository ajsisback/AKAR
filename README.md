# AKAR — أكار

**Owner-first residential construction project vault for Saudi project owners.**

Project Vault + Construction Helper + Lightweight Directory

---

## Repository Structure

```
App/
├── backend/          .NET 10 Web API (Clean Architecture)
├── admin-portal/     Angular 19+ Admin Portal (AR/EN bilingual)
├── mobile/           Flutter Mobile App (skeleton)
├── docker/           Docker Compose (PostgreSQL)
└── docs/             Architecture decisions & documentation
```

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend API | .NET 10, ASP.NET Core |
| Database | PostgreSQL 17 |
| Admin Portal | Angular 19+, TypeScript |
| Mobile App | Flutter / Dart |
| Auth | Self-hosted JWT |
| Containerization | Docker Compose |

## Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 20+
- Docker & Docker Compose
- Flutter SDK (for mobile development)

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

## Sprint 1 Scope
- ✅ Owner registration & login (JWT)
- ✅ Owner dashboard
- ✅ Create project
- ✅ List owner projects
- ✅ View project details
- ✅ Arabic/English bilingual support (Arabic default)
- ✅ RTL/LTR layout switching

## Language Support
- **Arabic (العربية)** — Default, RTL layout
- **English** — LTR layout
- Language switcher available on all screens

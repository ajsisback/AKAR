# AKAR Pilot Release Readiness Checklist

## 1. Local Startup Checklist

- [ ] Docker Desktop is running
- [ ] `cd docker && docker compose up -d` starts PostgreSQL (healthy)
- [ ] `cd backend/src/Akar.Api && dotnet run` starts API on http://localhost:5000
- [ ] `cd mobile && flutter run -d chrome --web-port 8888` starts Flutter on http://localhost:8888
- [ ] `cd admin-portal && npm start` starts Angular on http://localhost:4200
- [ ] http://localhost:5000/health returns `Healthy`
- [ ] http://localhost:5000/swagger shows Swagger UI

## 2. Backend Configuration Checklist

| Setting | Config Key | Default (Dev) | Release Action |
|---|---|---|---|
| Database | `ConnectionStrings:DefaultConnection` | localhost:5433 akar_db | Set production connection string |
| JWT Secret | `Jwt:Key` | Dev-only placeholder | **MUST change** to a unique 32+ char secret |
| JWT Issuer | `Jwt:Issuer` | AkarApi | Review if custom domain needed |
| JWT Audience | `Jwt:Audience` | AkarClients | Review if custom domain needed |
| JWT Expiry | `Jwt:ExpiryHours` | 24 | Adjust per security policy |
| CORS Origins | `Cors:AllowedOrigins` | localhost:4200,localhost:8888 | Set to deployed frontend URLs |
| File Storage | `Storage:LocalRootPath` | Relative `./storage` | Set absolute path on server |
| Swagger | Enabled in Development | `IsDevelopment()` guard | Disabled in Production by default |
| Logging | Serilog config | Console, Information | Add file/cloud sink for production |

### Configuration Methods (priority order):
1. Environment variables (recommended for secrets)
2. `appsettings.{Environment}.json`
3. `appsettings.json` (base defaults)

### Example environment variable overrides:
```bash
export ConnectionStrings__DefaultConnection="Host=prod-db;Port=5432;Database=akar_db;Username=akar_prod;Password=SECURE_PASS"
export Jwt__Key="YourProductionSecretKeyAtLeast32CharsLong!!"
export Cors__AllowedOrigins="https://app.yourdomain.com,https://admin.yourdomain.com"
export Storage__LocalRootPath="/var/akar/storage"
```

## 3. Flutter Configuration Checklist

- [ ] API URL centralized in `mobile/lib/core/api_service.dart`
- [ ] Default: `http://localhost:5000` (web), `http://10.0.2.2:5000` (Android emulator)
- [ ] Override via build-time define: `flutter run --dart-define=AKAR_API_URL=https://api.yourdomain.com`
- [ ] No hardcoded localhost references outside `api_service.dart` (except follower upload link which uses window.location)
- [ ] `flutter analyze` passes with 0 issues

## 4. Angular Admin Portal Configuration Checklist

- [ ] Dev API URL: `admin-portal/src/environments/environment.ts` → `http://localhost:5000/api`
- [ ] Prod API URL: `admin-portal/src/environments/environment.prod.ts` → `/api` (reverse proxy)
- [ ] For pilot with separate API host, update `environment.prod.ts` to `https://api.yourdomain.com/api`
- [ ] `npm run build` produces production bundle in `dist/admin-portal`
- [ ] No hardcoded localhost references in service files (all use `environment.apiUrl`)

## 5. Docker / PostgreSQL Checklist

- [ ] `docker-compose.yml` uses env vars with safe dev defaults
- [ ] Health check defined: `pg_isready` with 10s interval, 5 retries
- [ ] Port mapping: `5433:5432` (dev default, configurable via `POSTGRES_PORT`)
- [ ] Volume: `akar_pgdata` persists data across restarts
- [ ] Database credentials are dev-only defaults (`akar_user` / `akar_pass_2026`)
- [ ] **Release action**: Use `.env` file or real env vars with production credentials

### Docker environment variables:
| Variable | Default | Description |
|---|---|---|
| `POSTGRES_DB` | akar_db | Database name |
| `POSTGRES_USER` | akar_user | Database user |
| `POSTGRES_PASSWORD` | akar_pass_2026 | Database password |
| `POSTGRES_PORT` | 5433 | Host port mapping |

## 6. Secrets Checklist

- [ ] No production JWT secret in source code
- [ ] No production database password in source code
- [ ] `appsettings.Development.json` contains dev-only values
- [ ] `appsettings.Example.json` has `CHANGE_ME` placeholder for JWT key
- [ ] `.gitignore` covers `.env`, `.env.local`, `.env.*.local`
- [ ] `docker/.env` is git-ignored
- [ ] `storage/` directory is git-ignored
- [ ] `**/Properties/launchSettings.json` is git-ignored
- [ ] No uploaded files are tracked in git

## 7. CORS Checklist

- [ ] CORS origins are now config-driven (`Cors:AllowedOrigins` in appsettings)
- [ ] Dev defaults: `http://localhost:4200`, `http://localhost:8888`
- [ ] Fallback hard-coded if config is missing (safe dev defaults)
- [ ] **Release action**: Set `Cors:AllowedOrigins` to deployed frontend URLs
- [ ] `AllowAnyOrigin` is NOT used — origins are explicitly whitelisted
- [ ] `AllowCredentials` is enabled (required for JWT cookie support)

## 8. File Storage Checklist

- [ ] Upload storage path: `Storage:LocalRootPath` in appsettings
- [ ] Dev default: absolute path to workspace `storage/` directory
- [ ] `storage/` is in `.gitignore` — uploaded files are never committed
- [ ] Storage paths are not exposed in API responses
- [ ] File IDs (GUIDs) are used in API responses, not file paths
- [ ] **Release action**: Set `LocalRootPath` to a persistent server directory

## 9. Build Checklist

- [ ] `dotnet build` — 0 errors, 0 warnings
- [ ] `dotnet test` — all tests pass
- [ ] `flutter pub get` — dependencies resolved
- [ ] `flutter analyze` — no issues found
- [ ] `npm install` (admin-portal) — dependencies installed
- [ ] `npm run build` (admin-portal) — production bundle generated

## 10. Security Checklist

- [ ] Swagger UI disabled in production (gated by `IsDevelopment()`)
- [ ] JWT authentication on all owner API endpoints
- [ ] Owner data isolation via OwnerId from JWT claims
- [ ] Password hashing via BCrypt
- [ ] No raw exceptions exposed to clients (ExceptionHandlingMiddleware)
- [ ] No password hashes in API responses
- [ ] No internal file paths in API responses
- [ ] Token hashes stored (SHA256), raw tokens returned once only
- [ ] Public upload endpoints: upload-only, no browse/download/delete
- [ ] File extension whitelist enforced on upload
- [ ] Dangerous file types rejected (exe, bat, cmd, ps1, sh, js, etc.)

## 11. Known Deferred Items

| Item | Target Sprint |
|---|---|
| Android build readiness | Sprint 10B |
| Deployment runbook | Sprint 10C |
| Signed contract upload regression test with real PDF | Future |
| Result pattern consistency for remaining exception-throwing handlers | Future |
| Production hosting and domain setup | Future |
| Rate limiting on public upload endpoints | Future |
| HTTPS enforcement | Future (deployment) |
| Backup/restore procedures | Future (operations) |
| Monitoring and alerting | Future (operations) |

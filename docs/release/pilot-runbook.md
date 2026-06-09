# AKAR Pilot Release Runbook

## 1. Purpose
This is the **AKAR pilot release preparation guide**. It serves as an executable runbook for developers and operators to configure, build, and deploy the AKAR application for its initial pilot release.

## 2. Repository & Branch Baseline
* **Main Branch:** `main` serves as the release baseline after the Sprint 10 merge.
* **Sprint 10 Branch:** `feature/sprint-10-release-readiness`

## 3. Local Prerequisites
Ensure the following tools are installed before proceeding:
* Git
* Docker Desktop
* .NET 10 SDK
* Flutter SDK (3.44+)
* Android SDK / Android Studio (with cmdline-tools)
* Node.js (20+) / npm
* Google Chrome

## 4. Local Startup Sequence
To run the full stack locally:

1. **Start PostgreSQL using Docker:**
   ```bash
   cd docker
   docker compose up -d
   docker compose ps # Wait until healthy
   ```

2. **Start Backend API:**
   ```bash
   cd backend/src/Akar.Api
   dotnet run
   ```

3. **Start Flutter Web (Mobile App UI):**
   ```bash
   cd mobile
   flutter pub get
   flutter run -d chrome --web-port 8888
   ```

4. **Start Angular Admin Portal:**
   ```bash
   cd admin-portal
   npm install
   npm start
   ```

## 5. Local URLs Table
| Service | Local URL |
|---------|-----------|
| Backend Health Check | http://localhost:5000/health |
| Swagger UI | http://localhost:5000/swagger |
| Flutter Web App | http://localhost:8888 |
| Angular Admin Portal | http://localhost:4200 |
| PostgreSQL Database | localhost:5433 (mapped via Docker) |

## 6. Backend Configuration
The backend uses `appsettings.json`, environment-specific files (e.g., `appsettings.Development.json`), and environment variables. `appsettings.Example.json` provides a template.

**Key Environment Variable Overrides for Pilot:**
```bash
# Database connection
export ConnectionStrings__DefaultConnection="Host=your-prod-db;Port=5432;Database=akar_db;Username=akar_prod;Password=SECURE_PASS"

# JWT Key Guidance (MUST be changed from default, min 32 chars)
export Jwt__Key="YourSuperSecretProductionKeyAtLeast32Chars!!"

# CORS Allowed Origins (Comma-separated list of your frontend domains)
export Cors__AllowedOrigins="https://admin.akar.example.com,https://app.akar.example.com"

# File Storage Root (Absolute path on the server where files should be saved)
export Storage__LocalRootPath="/var/akar/storage"
```

## 7. Flutter Configuration
The mobile app API URL is configured at build/run time using the `--dart-define` flag.

* **Local Web Backend URL:** `http://localhost:5000` (Default if not specified)
* **Android Emulator Backend URL:** `http://10.0.2.2:5000` (Default if not specified)
* **Physical Device LAN Guidance:** Use your machine's LAN IP (e.g., `http://192.168.1.50:5000`)
* **Pilot/Production Build:**
  ```bash
  flutter build apk --release --dart-define=AKAR_API_URL=https://api.akar.example.com
  ```

## 8. Angular Configuration
The Angular admin portal configuration is found in the `src/environments/` directory.

* **Dev URL (`environment.ts`):** `http://localhost:5000/api`
* **Production Proxy (`environment.prod.ts`):** `/api`
* **Production Build Guidance:** Use `npm run build`. You should configure a reverse proxy (like Nginx) to route requests from `/api` to the backend server.

## 9. Android Build Commands
Navigate to the `mobile` directory to run these commands:

* **Debug APK (Local testing):**
  ```bash
  flutter build apk --debug --dart-define=AKAR_API_URL=http://10.0.2.2:5000
  ```
* **Release APK (Pilot distribution):**
  ```bash
  flutter build apk --release --dart-define=AKAR_API_URL=https://api.akar.example.com
  ```
* **App Bundle (Deferred until Google Play submission):**
  ```bash
  flutter build appbundle --release --dart-define=AKAR_API_URL=https://api.akar.example.com
  ```
  *(Note: Requires production release keystore configuration, see `android-build-readiness.md`)*

## 10. Security Checklist
- [x] No secrets (passwords, JWT keys) committed in Git.
- [x] No Android `.jks` or `.keystore` files committed in Git.
- [x] No password or hash exposure in API responses.
- [x] Internal server storage paths are not exposed via APIs.
- [x] Swagger UI is restricted strictly to the `Development` environment.
- [x] CORS is not open to all (`AllowAnyOrigin` is NOT used).
- [x] HTTPS is required for pilot/staging/production environments.
- [x] Cleartext HTTP is restricted via `network_security_config.xml` to `localhost` and `10.0.2.2` only.

## 11. Troubleshooting
* **Port 5000 locked:** Another process is using port 5000. Kill it (`Stop-Process -Name Akar.Api -Force` on Windows, or `kill -9 $(lsof -t -i:5000)` on Mac/Linux).
* **PostgreSQL unhealthy:** Ensure Docker is running. Run `docker compose down -v && docker compose up -d` to reset the container and data volume.
* **Flutter OOM:** If the web build runs out of memory, close unused Chrome tabs or increase `--old-gen-heap-size`.
* **Android Gradle / CMake / NDK issue:** Ensure Android SDK paths are correct. Run `flutter clean` then `flutter pub get` and try building again.
* **Angular npm install/build issue:** Delete the `node_modules` folder and `package-lock.json` and rerun `npm install`.
* **API URL mismatch:** Ensure the `--dart-define=AKAR_API_URL` or `environment.ts` points to the correct, reachable backend IP/domain.
* **CORS blocked request:** Check the backend `Cors:AllowedOrigins` configuration. It must exactly match the frontend URL (including `http/https` and port).

## 12. Pilot Readiness Checklist
- [x] Backend builds and tests pass successfully.
- [x] `flutter analyze` passes with zero issues.
- [x] Angular production build succeeds.
- [x] Android debug and release APKs build successfully.
- [x] Docker environment spins up and reports healthy.
- [x] Documentation and runbooks are up to date.
- [x] Security and secrets review passed.
- [x] Git working tree is clean.

## 13. Deferred Items
* Google Play Console setup.
* Production release keystore generation and configuration.
* iOS build readiness and provisioning.
* Real staging domain configuration and deployment hosting setup.
* Signed contract upload regression testing with a real PDF.
* `applicationId` rename to `com.meyaar.akar` before Play Store submission.
* Custom app icon and splash branding.
* Rate limiting on public upload endpoints.
* HTTPS enforcement at the deployment (reverse proxy / load balancer) level.

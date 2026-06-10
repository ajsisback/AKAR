# AKAR Android Build Readiness

## 1. Environment Inventory

| Component | Version |
|---|---|
| Flutter | 3.44.0 (stable) |
| Dart | 3.12.0 |
| Android SDK | 36.1.0 |
| Android Gradle Plugin (AGP) | 9.0.1 |
| Kotlin | 2.3.20 |
| Java compatibility | JVM 17 |
| Emulator | 36.4.9.0 |

### Android App Config

| Setting | Value |
|---|---|
| applicationId | `com.akar.akar_mobile` |
| namespace | `com.akar.akar_mobile` |
| minSdk | Flutter default (currently 21) |
| targetSdk | Flutter default (currently 35) |
| compileSdk | Flutter default (currently 35) |
| versionCode | 1 (from pubspec.yaml `1.0.0+1`) |
| versionName | 1.0.0 |

## 2. Required Tools

- **Flutter SDK** 3.44+ — installed at `C:\flutter`
- **Android SDK** 36+ — via Android Studio or standalone
- **Android Studio** — for emulator and SDK management
- **Java/JDK** — JVM 17 compatible (bundled with Android Studio)
- **Android cmdline-tools** — required for `flutter doctor` green check

### Missing Tools (Non-blocking)

- `cmdline-tools` component — install via Android Studio SDK Manager
- Android licenses — run `flutter doctor --android-licenses` to accept

## 3. API URL Configuration

The API base URL is controlled at build time via `--dart-define`:

```bash
# Local development (Android emulator)
flutter run -d android --dart-define=AKAR_API_URL=http://10.0.2.2:5000

# Physical device on same LAN
flutter run -d android --dart-define=AKAR_API_URL=http://192.168.1.XXX:5000

# Staging/Pilot
flutter build apk --dart-define=AKAR_API_URL=https://api.akar.example.com

# Default (no --dart-define)
# Web: http://localhost:5000
# Android: http://10.0.2.2:5000
```

## 4. Build Commands

### Debug APK (local development)
```bash
cd mobile
flutter pub get
flutter build apk --debug --dart-define=AKAR_API_URL=http://10.0.2.2:5000
```
Output: `build/app/outputs/flutter-apk/app-debug.apk`

### Release APK (pilot/staging)
```bash
flutter build apk --release --dart-define=AKAR_API_URL=https://api.akar.example.com
```
Output: `build/app/outputs/flutter-apk/app-release.apk`

### App Bundle (Google Play)
```bash
flutter build appbundle --release --dart-define=AKAR_API_URL=https://api.akar.example.com
```
Output: `build/app/outputs/bundle/release/app-release.aab`

> **Note**: Release builds require signing configuration. See section 7 below.

## 5. Network / Cleartext HTTP

A `network_security_config.xml` is configured to allow cleartext HTTP **only** to:
- `localhost` (for web/local)
- `10.0.2.2` (Android emulator → host machine)

All other domains require HTTPS. This is intentional:
- **Local development**: HTTP to emulator host is allowed
- **Staging/Production**: MUST use HTTPS URLs

## 6. Permissions

The Android app requests only:
- `android.permission.INTERNET` — required for API communication

No camera, location, contacts, or broad storage permissions are used.
File uploads use the Android document provider (SAF) via `file_picker`, which does not require `READ_EXTERNAL_STORAGE`.

## 7. Release Signing

### Current Status
Release builds currently use the **debug signing key** (Flutter default).
This is acceptable for internal testing but NOT for Google Play distribution.

### Creating a Release Keystore

```bash
keytool -genkey -v -keystore akar-release.jks -keyalg RSA -keysize 2048 -validity 10000 -alias akar
```

Store the keystore file **outside** the git repository.

### Configure Gradle Signing

1. Create `mobile/android/key.properties` (git-ignored):
```properties
storePassword=YOUR_STORE_PASSWORD
keyPassword=YOUR_KEY_PASSWORD
keyAlias=akar
storeFile=/path/to/akar-release.jks
```

2. Update `mobile/android/app/build.gradle.kts` to read `key.properties`:
```kotlin
val keystoreProperties = java.util.Properties()
val keystorePropertiesFile = rootProject.file("key.properties")
if (keystorePropertiesFile.exists()) {
    keystoreProperties.load(keystorePropertiesFile.inputStream())
}

android {
    signingConfigs {
        create("release") {
            keyAlias = keystoreProperties["keyAlias"] as String
            keyPassword = keystoreProperties["keyPassword"] as String
            storeFile = file(keystoreProperties["storeFile"] as String)
            storePassword = keystoreProperties["storePassword"] as String
        }
    }
    buildTypes {
        release {
            signingConfig = signingConfigs.getByName("release")
        }
    }
}
```

### Security Rules
- **NEVER** commit `*.jks`, `*.keystore`, or `key.properties` to git
- Store signing credentials in a secure vault (1Password, Bitwarden, etc.)
- Back up the keystore — losing it means you cannot update the app on Google Play

## 8. Running on Emulator

```bash
# 1. Start backend infrastructure
cd docker && docker compose up -d
cd backend/src/Akar.Api && dotnet run

# 2. Launch on emulator
cd mobile
flutter run -d android --dart-define=AKAR_API_URL=http://10.0.2.2:5000
```

### Physical Device (same network)
Find your machine's LAN IP, then:
```bash
flutter run -d android --dart-define=AKAR_API_URL=http://192.168.1.XXX:5000
```

## 9. Known Deferred Items

| Item | Target |
|---|---|
| Google Play Console setup | Future |
| Release signing configuration | Before Play Store submission |
| Custom app icon and splash screen | Future (branding sprint) |
| iOS build readiness | Future |
| Real staging/production domain | Before pilot deployment |
| ProGuard/R8 optimization rules | Future (if needed) |
| App bundle size optimization | Future |
| `applicationId` rename to `com.meyaar.akar` | Before Play Store submission |

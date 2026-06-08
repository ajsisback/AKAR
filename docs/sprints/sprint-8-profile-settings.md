# Sprint 8: Owner Profile & Project Settings

## Sprint Objective
Enable owner-first self-service profile and project settings via the Flutter mobile app, and provide a secure read-only support view via the Angular admin portal.

## Sprint 8A: Backend & Infrastructure
- Implemented `OwnerProfileDto` and `ProjectSettingsDto` and corresponding query handlers in the .NET backend.
- Ensured security boundary: `GET /api/owner/profile` and `GET /api/projects/{projectId}/settings` only expose non-sensitive metadata (no password hashes).
- Enums (`ProjectType` and `CurrentStage`) were updated to correctly serialize as strings.

## Sprint 8B: Flutter UI & Integration
- Verified Flutter screens for Owner Profile and Project Settings.
- Confirmed correct display of profile data (FullName, Email, Phone, CreatedAt).
- Confirmed correct display of project settings (Name, Type, Stage, MapLink) and handling of enums mapped from strings.

## Sprint 8C: Angular Support View
- Maintained Angular portal as a read-only support tool for internal operations.
- Implemented `OwnerProfileService` mapped to `GET /api/owner/profile` in the Angular portal.
- Implemented `ProjectService.getSettings()` mapped to `GET /api/projects/{projectId}/settings`.
- Added a new read-only Owner Profile section in the `DashboardComponent`.
- Added a new read-only Project Settings section in the `ProjectDetailsComponent`.
- Updated English and Arabic localization dictionaries (`en.json`, `ar.json`) with new keys (`profile.*`, `projectSettings.*`).
- Verified zero password fields or mutative actions in the support views.

## Status
Sprint 8 is complete. The `feature/sprint-8-owner-settings` branch is PR-ready. All Angular builds pass, and no breaking changes were introduced to the backend APIs or Flutter app.

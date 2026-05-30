# Sprint 3 — Followers & Incoming Files

## Overview

Sprint 3 adds **Project Followers** to AKAR. Followers are helpers (not full users) who can upload files to a project through secure upload links — without needing an account, login, or dashboard.

**AKAR remains Owner-first.**

## Sprint Breakdown

### Sprint 3A — Backend Followers Foundation
- `ProjectFollower` entity (Supervisor, Relative, Contractor, Designer, EngineeringOffice, Other)
- CRUD APIs for followers (GET, POST, PUT, DELETE soft-delete)
- Automatic `FollowerInbox` folder creation per follower
- Owner isolation enforced

### Sprint 3B — Follower Upload Links
- `FollowerUploadLink` entity with SHA256 token hashing
- Owner APIs: Generate, List (preview only), Revoke
- Public APIs: Get Info (minimal safe data), Upload File (multipart/form-data)
- Token returned raw only once at generation
- File validation: PDF, DOC, DOCX, XLS, XLSX, JPG, JPEG, PNG, WEBP, MP4, MOV
- Dangerous files rejected: EXE, BAT, CMD, PS1, SH, JS, MSI, DLL, ZIP
- Size limits: Documents 20MB, Images 10MB, Videos 100MB

### Sprint 3C — Flutter Owner Followers UI
- Followers list screen with type icons and status badges
- Add/Edit follower form with type dropdown
- Follower details with inbox shortcut and upload links section
- Generate upload link with save-now warning dialog
- Copy link, revoke link, list links (preview only)
- 49 AR/EN localization keys

### Sprint 3D — Public Follower Upload Page
- Public upload route: `/#/follower-upload/{token}`
- Token-based info loading (no JWT)
- File picker + upload (no JWT)
- Invalid/expired/revoked token error screens
- Language switcher on public page
- 20 AR/EN localization keys

## Security Model

| Rule | Implementation |
|------|---------------|
| Token storage | SHA256 hash in DB, raw token never stored |
| Token exposure | Raw token returned once at generation only |
| Public API auth | Token-based, no JWT |
| Owner API auth | JWT required |
| Owner isolation | All follower/link APIs validate ownership |
| Public info | Minimal: followerName, projectName, allowedTypes, maxSize, expiresAt |
| File validation | Extension + size checks, dangerous types blocked |
| Error messages | Localized, no raw exceptions |

## Follower Limitations (Sprint 3)

Followers **cannot**:
- Log in
- Access a dashboard
- Browse projects or folders
- List, download, or delete files
- Manage permissions

Followers **can only**:
- Upload files through a valid, non-revoked, non-expired upload link

## API Summary

### Owner APIs (JWT required)
| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/api/projects/{id}/followers` | List followers |
| POST | `/api/projects/{id}/followers` | Create follower |
| GET | `/api/projects/{id}/followers/{fid}` | Get follower |
| PUT | `/api/projects/{id}/followers/{fid}` | Update follower |
| DELETE | `/api/projects/{id}/followers/{fid}` | Soft-delete follower |
| POST | `/api/projects/{id}/followers/{fid}/upload-link` | Generate link |
| GET | `/api/projects/{id}/followers/{fid}/upload-links` | List links |
| POST | `…/upload-links/{lid}/revoke` | Revoke link |

### Public APIs (Token only)
| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/api/public/follower-upload/{token}/info` | Minimal upload info |
| POST | `/api/public/follower-upload/{token}/files` | Upload file |

## Deferred (Not in Sprint 3)

- Angular follower admin UI
- Follower login / authentication
- Follower dashboard
- File download via upload token
- File browsing for followers
- Notifications (email, SMS, push)
- Rate limiting on public upload
- Contracts module
- Saudi building code module
- Directory / marketplace
- OCR / AI document processing


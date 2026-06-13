# Sprint 12 — Pilot UAT Execution & MVP Gap Closure

**Status:** Completed
**Branch:** `feature/sprint-12-pilot-uat-mvp-gap-closure`
**Duration:** Sprint 12

---

## Objective

Verify end-to-end MVP readiness for Pilot UAT. No new large features. Focus on:
- End-to-end smoke testing
- MVP gap closure (subscription placeholder)
- Release readiness verification
- Documentation

---

## 12A — End-to-End UAT Smoke Test

### Owner Mobile Flow (API Smoke Tests)

| # | Test | Result |
|---|------|--------|
| 1 | Health endpoint | ✅ PASS |
| 2 | Pilot seed | ✅ PASS |
| 3 | Register new owner | ✅ PASS (201) |
| 4 | Owner login | ✅ PASS (token obtained) |
| 5 | Get dashboard | ✅ PASS |
| 6 | Get owner profile | ✅ PASS |
| 7 | Update owner profile | ✅ PASS |
| 8 | Change password | ✅ PASS (204) |
| 9 | Login with new password | ✅ PASS |
| 10 | Create project | ✅ PASS (201) |
| 11 | List project folders | ✅ PASS (13 folders) |
| 12 | Search project files | ✅ PASS |
| 13 | List followers | ✅ PASS (3 followers) |
| 14 | Get follower details | ✅ PASS |
| 15 | List upload links | ✅ PASS (1 link) |
| 16 | Get public upload info | ✅ PASS |
| 17 | Get contract templates | ✅ PASS |
| 18 | List contracts | ✅ PASS (1 contract) |
| 19 | Get contract details | ✅ PASS (Draft) |
| 20 | Get timeline | ✅ PASS (5 events) |
| 21 | Add timeline note | ✅ PASS (201) |
| 22 | Get project settings | ✅ PASS |
| 23 | Get trash | ✅ PASS |

### Admin Web Flow (API Smoke Tests)

| # | Test | Result |
|---|------|--------|
| 1 | Admin login | ✅ PASS (SuperAdmin role) |
| 2 | Admin list owners | ✅ PASS |
| 3 | Admin get owner details | ✅ PASS |
| 4 | Admin list projects | ✅ PASS |
| 5 | Admin get project details | ✅ PASS |

### Security Isolation Tests

| # | Test | Result |
|---|------|--------|
| 1 | Invalid login returns 401 | ✅ PASS |
| 2 | Unauthenticated dashboard returns 401 | ✅ PASS |
| 3 | Owner token blocked from admin API | ✅ PASS (403) |
| 4 | Admin token isolated from owner data | ✅ PASS |

---

## 12B — MVP Gap Closure

### Changes Made

- **Subscription placeholder:** Added "Pilot — Trial" badge on owner profile screen (local-only UI, no backend changes)
- **i18n keys:** Added 3 new bilingual keys (`subscription_status`, `subscription_pilot_trial`, `subscription_active`)
- **Error handling:** Verified all 30+ mapped error codes have corresponding i18n keys in both AR and EN — no missing keys found

### i18n Parity

| Language | Key Count | Status |
|----------|-----------|--------|
| English | 394 | ✅ |
| Arabic | 394 | ✅ |
| Parity | 100% | ✅ |

---

## 12C — Pilot Release Readiness

| Item | Status |
|------|--------|
| Health endpoint (/health) | ✅ Active |
| Pilot seed (dev-only) | ✅ Gated by IsDevelopment() |
| Backend env vars documented | ✅ pilot-runbook.md |
| DB migration (EF Core auto) | ✅ On startup |
| CORS documented | ✅ |
| HTTPS documented | ✅ |
| Angular prod build | ✅ 0 errors |
| Admin routes protected (adminGuard) | ✅ |
| No hardcoded dev credentials | ✅ |
| API URL env switching (--dart-define) | ✅ |
| Android applicationId | ⚠️ `com.akar.akar_mobile` — rename to `com.meyaar.akar` before Play Store |
| Custom app icon | ❌ Default Flutter icon — needed before Play Store |
| Custom splash screen | ❌ Default Flutter splash — needed before Play Store |

---

## 12D — Content Readiness Assessment (Deferred to Sprint 13)

| Module | Status | Sprint 13 Recommendation |
|--------|--------|--------------------------|
| Step-by-step building guide | ❌ Not implemented | Static JSON + simple list screen |
| Saudi Code simplified content | ❌ Not implemented | Static markdown/JSON reference |
| Lightweight directory | ❌ Not implemented | Static categories with contact info |

---

## 12E — Documentation

- Sprint 12 scope document created
- Sprint 12 checklist with UAT results
- Deferred items documented

---

## MVP System Folders (13 per project)

| Folder | Type |
|--------|------|
| Contracts | System |
| Drawings | System |
| Followers Inbox | System |
| Invoices | System |
| License | System |
| Photos | System |
| Project Location | System |
| Trash | System |
| Videos | System |
| Warranties | System |
| + Follower-specific folders (1 per follower) | Auto-created |

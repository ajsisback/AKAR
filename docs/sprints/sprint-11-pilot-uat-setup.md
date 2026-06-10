# Sprint 11 — Pilot UAT Setup

## Overview

Sprint 11 prepares AKAR for controlled pilot testing. It is split into two sub-sprints:

- **Sprint 11A** (this sprint): Backend pilot seed data + Admin/SuperAdmin baseline
- **Sprint 11B** (next): Angular Web Admin Portal UI

## Sprint 11A Scope

Backend-only. No Angular or Flutter UI changes.

### Owner vs Admin/SuperAdmin

AKAR has two completely separate user contexts:

| Context | Platform | Purpose |
|---------|----------|---------|
| **Owner** | Flutter Mobile App | Manages construction projects, files, contracts, followers |
| **Admin / SuperAdmin** | Angular Web Admin Portal | Manages and monitors the system, supports users |

- SuperAdmin is **not** an Owner.
- Owner is **not** a SuperAdmin.
- They use different JWTs with different `userType` claims.
- They access different API routes.

### Admin Roles

| Role | Access |
|------|--------|
| `SuperAdmin` | All admin read APIs, seed endpoint |
| `SupportAdmin` | Safe read-only admin APIs only |

No complex RBAC, departments, or permissions matrix in Sprint 11A.

### AdminUser Entity

New `admin_users` table:

| Field | Type | Notes |
|-------|------|-------|
| `id` | UUID | Primary key |
| `full_name` | varchar(200) | Required |
| `email` | varchar(320) | Unique, required |
| `password_hash` | text | BCrypt hashed |
| `role` | varchar(50) | `SuperAdmin` or `SupportAdmin` |
| `is_active` | boolean | Default true |
| `created_at_utc` | timestamp | Auto-set |
| `updated_at_utc` | timestamp | Auto-set |

### Admin Authentication

**Endpoint:** `POST /api/admin/auth/login`

**Request:**
```json
{
  "email": "string",
  "password": "string"
}
```

**Response:**
```json
{
  "token": "jwt",
  "admin": {
    "id": "guid",
    "fullName": "string",
    "email": "string",
    "role": "SuperAdmin | SupportAdmin"
  }
}
```

**JWT claims:**
- `sub` — admin ID
- `email` — admin email
- `fullName` — admin display name
- `userType` — `"Admin"` (Owner tokens have `"Owner"`)
- `role` — `"SuperAdmin"` or `"SupportAdmin"`
- `jti` — unique token ID

### Admin Read-Only APIs

All require `AdminOnly` policy (JWT with `userType = Admin`).

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/api/admin/owners` | List all owners with project counts |
| GET | `/api/admin/owners/{ownerId}` | Owner detail with project summaries |
| GET | `/api/admin/projects` | List all projects across system |
| GET | `/api/admin/projects/{projectId}` | Project detail with counts |

**Security:** No password hashes, token hashes, storage paths, or secrets are exposed.

### Authorization Rules

- Owner token → denied from `/api/admin/*`
- Admin token → denied from owner-only APIs (no valid owner ID in `sub`)
- `AdminOnly` policy → requires `userType = Admin` claim
- `SuperAdminOnly` policy → requires `userType = Admin` + `role = SuperAdmin`

### Pilot Seed Data

**Endpoint:** `POST /api/dev/seed/pilot`

⚠️ **Development-only** — returns 404 in non-Development environments.

**Seed accounts:**

| Type | Email | Password | FullName | Role |
|------|-------|----------|----------|------|
| SuperAdmin | `superadmin@akar.local` | `Admin@12345` | مدير النظام | SuperAdmin |
| SupportAdmin | `support@akar.local` | `Support@12345` | مسؤول الدعم | SupportAdmin |
| Pilot Owner | `pilot.owner@akar.local` | `Pilot@12345` | مالك تجربة | — |

> ⚠️ **WARNING:** These credentials are for development/pilot only. Never use in production.

**Seed scenario:**
- Pilot project: فيلا حي النرجس (Villa, Structural stage, الرياض)
- System folders: all 10 default folders
- 3 followers: أحمد المشرف, مؤسسة البناء الحديث, مكتب التصميم الهندسي
- Timeline events: stage change, follower added ×3, manual UAT note
- 1 draft contract (no PDF generation)
- 1 follower upload link

**Idempotency:** Running the seed multiple times will not create duplicate records.

### Security Guardrails

- Seed endpoint is Development-only (gated by `IsDevelopment()`)
- Passwords hashed with BCrypt (existing hasher)
- No raw passwords logged
- No token hashes exposed in APIs
- No storage paths exposed in APIs
- No real customer data

## Deferred to Sprint 11B+

- Angular Super Admin Portal UI
- Admin write/update/delete actions
- Owner impersonation
- Production seed strategy
- Audit logs
- Full RBAC matrix
- Password reset
- Billing/subscriptions

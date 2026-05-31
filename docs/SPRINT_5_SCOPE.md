# Sprint 5 — Project Stages & Timeline

## Overview

Sprint 5 adds **Project Stages & Timeline** to AKAR, enabling owners to track construction progress through defined stages and view a timeline of project activity events.

---

## Sprint 5A — Timeline Foundation

### Backend API
- Added `ProjectTimelineEvent` entity with DDD patterns
- Added `TimelineEventType` enum: StageChanged, ManualNote, FileUploaded, ContractCreated, ContractPdfGenerated, FollowerAdded, FollowerFileUploaded
- Added `TimelineSourceType` enum: Owner, System, Follower
- Stage APIs:
  - `PUT /api/projects/{id}/stage` — Update project stage (creates StageChanged event automatically)
- Timeline APIs:
  - `GET /api/projects/{id}/timeline` — List timeline events (filterable by stage, eventType)
  - `POST /api/projects/{id}/timeline/notes` — Add manual note
  - `DELETE /api/projects/{id}/timeline/{eventId}` — Delete manual note only
- System StageChanged events auto-created on stage update
- EF Core migration: `AddProjectTimeline`

### Key Decisions
- Timeline is owner-facing simplicity, not a full audit log
- System events are protected from deletion (400 error)
- All timeline APIs enforce owner isolation via JWT

---

## Sprint 5B — Automatic Timeline Events

### Integrated Events
| Trigger | Event Type | Source |
|---------|-----------|--------|
| Owner uploads file | FileUploaded | System |
| Owner creates contract | ContractCreated | System |
| Contract PDF generated | ContractPdfGenerated | System |
| Owner adds follower | FollowerAdded | System |
| Follower uploads file | FollowerFileUploaded | System |

### Duplicate Prevention
- Each automatic event uses `SourceId` (the entity ID that triggered it)
- `SourceType` tracks the origin (e.g., "ProjectFile", "ProjectContract", "ProjectFollower")
- Database uniqueness ensures no duplicate events for the same action

### Security
- System-generated events cannot be deleted
- Automatic events are read-only to the owner

---

## Sprint 5C — Flutter Timeline UI

### Screens Added
- **TimelineScreen** — Full timeline view accessible from Project Details
  - Current stage card with visual 4-stage progress bar
  - Change stage dialog (dropdown + optional note)
  - Add manual note dialog (stage, title, description)
  - Delete manual note with confirmation
  - System event delete protection (shows warning)
  - Filter by stage dropdown
  - Filter by event type dropdown
  - Pull-to-refresh
  - Color-coded event cards with icons per type
  - System/Manual event badges

### API Service Methods
- `getProjectStage(projectId)`
- `updateProjectStage(projectId, stage, note)`
- `getProjectTimeline(projectId, filters)`
- `addProjectTimelineNote(projectId, stage, title, description)`
- `deleteProjectTimelineEvent(projectId, eventId)`

### Localization
- 40 AR/EN keys each for timeline labels, event types, dialogs, errors

---

## Sprint 5D — Angular Support View & Documentation

### Angular Admin Portal
- **Read-only** timeline support section in Project Details page
- Stage progress card with visual 4-stage bar
- Timeline events table with columns: Event, Type, Stage, Source, Date
- Stage and event type filter dropdowns
- System/Manual source badges with color coding
- Event type badges with distinct colors per type

### Angular Service
- `TimelineService` with `getProjectTimeline(projectId, filters)`
- Uses existing JWT interceptor (Authorization header)

### Localization
- 30 AR/EN keys each for timeline support view

### Documentation
- Updated README.md, ARCHITECTURE.md
- Created SPRINT_5_SCOPE.md

---

## Security Model

| Check | Status |
|-------|--------|
| Owner isolation (JWT-scoped queries) | ✅ Enforced |
| System event delete protection | ✅ 400 error |
| JWT in Authorization header only | ✅ No JWT in URL |
| No storage paths exposed | ✅ |
| No raw upload tokens exposed | ✅ |
| Angular view is read-only | ✅ |

---

## Deferred Items

These features are **not** part of Sprint 5 and are deferred to future sprints:

- **Sub-stages** — Breaking stages into granular sub-steps
- **Stage approvals** — Requiring approval before stage transitions
- **Task management** — Assigning tasks within stages
- **Gantt charts** — Visual project scheduling
- **Advanced scheduling** — Date-based stage planning
- **Timeline attachments** — Attaching files directly to timeline events
- **Notifications** — Push/email notifications for timeline events
- **Angular follower admin UI** — Managing followers from admin portal
- **Follower login/auth** — Authenticated follower access

---

## Verification Summary

| Component | Status |
|-----------|--------|
| Backend build | ✅ 0 warnings, 0 errors |
| Backend tests | ✅ All passed |
| Flutter analyze | ✅ No issues |
| Angular build | ✅ Success |
| Integration tests | ✅ 27/27 passed (Sprint 5C) |
| Sprint 1–4 regression | ✅ All passed |

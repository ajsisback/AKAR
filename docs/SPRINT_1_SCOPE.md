# Sprint 1 Scope

## Channel Ownership

| Channel | Role |
|---------|------|
| **Flutter Mobile App** | PRIMARY owner application |
| **Angular Admin Portal** | Internal / admin / support portal only |
| **Backend (.NET API)** | Shared API layer |

> Sprint 1 Angular testing was accepted for admin portal verification only.
> From Sprint 2 onward, all owner-facing features must be in Flutter.

## Features Included (Sprint 1)

1. **Owner Authentication**
   - Register (email, phone, password, optional company name)
   - Login (email + password)
   - JWT token generation

2. **Owner Dashboard**
   - Total project count
   - Breakdown by construction stage (Not Started, Structural, Finishing, Completed)

3. **Project CRUD**
   - Create project (name, type, city, location, map link, stage, optional image URL)
   - List owner's projects
   - View project details

4. **Bilingual Support (Arabic/English)**
   - Arabic is the default language
   - Dynamic RTL/LTR switching
   - All UI text in translation JSON files
   - Backend returns stable error codes for frontend translation

5. **Technical Foundation**
   - .NET 10 Clean Architecture backend
   - Angular admin portal (internal use)
   - Flutter mobile skeleton (structure ready, SDK required)
   - PostgreSQL via Docker
   - EF Core code-first migrations

## Features NOT Included (Deferred)

- File upload / document storage
- Contract management
- Marketplace / contractor ERP
- Notifications
- AI features
- Payment integration
- Multi-role access control (admin roles)
- Engineering workflows
- Timeline tracking

## Sprint 2 Prerequisite

> **BLOCKER:** Flutter SDK must be installed before Sprint 2 can start.
>
> All Sprint 2 owner-facing features will be implemented in Flutter.

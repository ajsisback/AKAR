# Sprint 4 Scope — Ready Contracts

## Goal

Allow the project owner to create ready-made construction contracts from templates, generate Arabic-readable PDFs, and store them inside the project's Contracts folder in the Document Vault.

---

## Sprint 4A — Contracts Foundation

### Entities
- **ContractTemplate** — Reusable contract templates with Arabic/English names, type, and default data JSON.
- **ProjectContract** — Per-project contract linked to a template, with party information, dates, value, status, and structured data JSON.

### Enums
- **ContractType**: Designer, Electrician, GeneralFinishing, Custom, Plumber, StructuralContractor, Supervisor.
- **ContractStatus**: Draft → ReadyForPdf → PdfGenerated → SignedUploaded → Cancelled.

### Seed Data
7 practical contract templates seeded via EF Core migrations:
1. Designer Contract
2. Electrician Contract
3. General Finishing Contract
4. Custom Contract
5. Plumber Contract
6. Structural Contractor Contract
7. Supervisor Contract

### APIs
- `GET /api/contract-templates` — List all active templates.
- `GET /api/contract-templates/{id}` — Get template by ID.
- `POST /api/projects/{id}/contracts` — Create a project contract.
- `GET /api/projects/{id}/contracts` — List project contracts.
- `GET /api/projects/{id}/contracts/{contractId}` — Get contract details.
- `PUT /api/projects/{id}/contracts/{contractId}` — Update Draft contract.
- `DELETE /api/projects/{id}/contracts/{contractId}` — Soft-delete Draft contract.

### Business Rules
- Only Draft contracts can be edited or deleted.
- Owner isolation enforced — each owner can only access their own contracts.
- ContractDataJson stores structured data: scopeOfWork, paymentTerms, ownerObligations, contractorObligations, notes.

---

## Sprint 4B — PDF Generation

### Implementation
- QuestPDF-based PDF generator using Skia rendering.
- Arabic text renders correctly with connected characters and RTL direction.
- NotoSansArabic font embedded for Arabic text support.

### API
- `POST /api/projects/{id}/contracts/{contractId}/generate-pdf` — Generate PDF, save to Contracts folder, link metadata.

### Behavior
1. Validates contract is Draft or ReadyForPdf.
2. Generates PDF with contract data, party info, dates, disclaimer.
3. Saves PDF file to the project's Contracts system folder via IFileStorageService.
4. Creates ProjectFile metadata record.
5. Updates contract status to PdfGenerated.
6. Links pdfFileId to the contract.

### PDF Content
- Contract title, type, template name (AR/EN).
- Party information: name, phone, national ID.
- Contract value and dates.
- Structured data sections.
- Legal disclaimer (AR/EN).
- Signature blocks.
- No storage paths, tokens, or technical data.

---

## Sprint 4C — Flutter Ready Contracts UI

### Screens
1. **ContractsScreen** — Project contracts list with status badges, pull-to-refresh, empty state, FAB.
2. **ContractFormScreen** — Create/edit contract with template selection, date pickers, validation, disclaimer.
3. **ContractDetailsScreen** — Full details, status lifecycle, edit/delete Draft, generate PDF, secure download.

### Features
- Template selection dropdown (7 templates).
- Create contract from template.
- Edit Draft contract (pre-populated form).
- Generate PDF with confirmation dialog.
- Download PDF via secure Blob URL (Authorization header, no JWT in query string).
- Status lifecycle display with color-coded badges.
- Arabic/English localization (58 keys per language).
- Legal disclaimer in form and details.

### Navigation
- Ready Contracts card added to Project Details screen.
- Navigates to ContractsScreen → ContractFormScreen / ContractDetailsScreen.

---

## Sprint 4D — Angular Support View + Documentation

### Angular Changes
- **ReadyContractsService** — Read-only service with getProjectContracts, getProjectContract, downloadContractPdf.
- **Project Details** — Ready Contracts section added with contract list, detail panel, data sections, disclaimer, PDF download.
- View is read-only (no create/edit/delete/generate).
- PDF download uses Authorization header via interceptor, Blob URL approach.
- 34 localization keys added (AR + EN).

---

## Security

- Owner isolation enforced at API level.
- JWT Authorization header on all contract APIs.
- PDF download uses Authorization header (never JWT in query string).
- No storage paths exposed in UI or PDF.
- No raw backend exceptions shown — error codes mapped to localized messages.

## Important Notes

- Templates are practical helpers, not legally certified documents.
- Legal disclaimer included in PDF, Flutter UI, and Angular UI.
- Generated PDFs are stored using IFileStorageService in the project's Contracts folder.

## Deferred

- Signed contract upload (upload signed physical copy).
- E-signature integration.
- Angular create/edit/generate (Angular is support-only).
- Legal review workflows.
- Contract versioning.
- Advanced template builder.

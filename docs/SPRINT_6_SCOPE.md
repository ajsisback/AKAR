# Sprint 6 Scope: Signed Contract Upload

This document details the requirements and scope for Sprint 6 of the AKAR project, which focuses on providing a mechanism for owners to upload manually signed contracts.

## Overview
Sprint 6 adds the ability for project owners to take a system-generated PDF contract that has been manually signed outside the system, upload it, and attach it to the contract record. The contract status then transitions to `SignedUploaded`. This is purely an owner-facing feature via the Flutter mobile app, with support-level read-only visibility in the Angular admin portal.

## Sprint 6A (Backend)
- Add `SignedFileId` property to the `ProjectContract` entity.
- Add new API endpoint `POST /api/projects/{projectId}/contracts/{contractId}/signed-file`.
- Process: Use existing `IFileStorageService` and `ProjectFile` mechanisms to store the uploaded PDF.
- Security: Enforce 20MB limit, PDF only. Enforce strict status validation (only allow upload if contract is in `PdfGenerated` state).
- Ensure file downloads use the existing secure endpoint (`/api/projects/{projectId}/files/{fileId}/download`).

## Sprint 6B (Flutter)
- Update contract details view to show "Upload Signed Version" button if status is `PdfGenerated`.
- Handle file selection using `file_picker` (PDF only).
- Upload the file using the new API endpoint and update UI state locally.
- Add "Download Signed Version" button if status is `SignedUploaded`.
- 100% Arabic/English parity and layout constraints.

## Sprint 6C (Angular & Finalization)
- Read-only support view: Admin portal must display the signed contract status (`SignedUploaded`).
- Detail view must show a "Download Signed PDF" button if a signed file exists.
- Utilize existing secure download mechanisms (Blob URL with Authorization header).
- Explicitly NOT allowing uploading or modifying contracts from the admin portal.

## Explicit Non-Goals
- **No e-signature capabilities:** Signed upload is not e-signature. No digital certificate validation. Owner signs externally and uploads PDF.
- **No legal review workflows:** The system only acts as a document vault and status tracker.
- **No signed file replacement or deletion:** Replacement/delete are deferred.
- **No Angular upload UI:** Angular is read-only support view.

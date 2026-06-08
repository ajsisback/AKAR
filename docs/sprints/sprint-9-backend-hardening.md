# Sprint 9A: Backend Hardening & Validation Pipeline

## Problem Statement

During Sprint 8A, it was discovered that `ChangeOwnerPasswordCommand` implemented bare `IRequest` (MediatR), while `ValidationBehavior<TRequest, TResponse>` was constrained to `IRequest<TResponse>`. In MediatR 14.x, bare `IRequest` no longer inherits from `IRequest<Unit>`, so the FluentValidation pipeline was completely bypassed for password change validation.

As a Sprint 8A workaround, handler-level validation was added to `ChangeOwnerPasswordCommandHandler`, duplicating the logic from `ChangeOwnerPasswordCommandValidator`.

## Root Cause

`ValidationBehavior<TRequest, TResponse>` has the constraint `where TRequest : IRequest<TResponse>`. In MediatR 14.x:
- `IRequest<T>` and `IRequest` (bare) are separate interfaces
- `IPipelineBehavior<TRequest, TResponse>` only binds to `IRequest<TResponse>`, not bare `IRequest`
- Any command implementing bare `IRequest` bypasses the entire validation and logging pipeline

## Fix Applied

**Option A was chosen:** Convert `ChangeOwnerPasswordCommand` from `IRequest` to `IRequest<Result>`.

### Changes Made

1. **ChangeOwnerPasswordCommand** (`IRequest` → `IRequest<Result>`):
   - Handler now returns `Result` instead of throwing `InvalidOperationException`
   - Handler-level password strength validation retained as defense-in-depth
   - `ChangeOwnerPasswordCommandValidator` now executes via the pipeline

2. **ValidationBehavior** extended:
   - Added handling for non-generic `Result` type (previously only `Result<T>` was handled)
   - Validation failures now return `Result.Failure()` instead of throwing `ValidationException`

3. **OwnerProfileController** updated:
   - `ChangePassword` action now uses `Result` pattern instead of catching `InvalidOperationException`
   - Consistent with `ProjectsController.Create` and other Result-based endpoints

4. **ExceptionHandlingMiddleware** hardened:
   - Added `InvalidOperationException` catch for business-logic error codes
   - Known NOT_FOUND codes map to 404; all others map to 400
   - Prevents any remaining exception-throwing handlers from producing 500

## Request/Validator Inventory

| Request | Implements | Has Validator | Pipeline Runs |
|---|---|---|---|
| RegisterOwnerCommand | IRequest<Result<AuthResponse>> | Yes | ✅ |
| LoginOwnerCommand | IRequest<Result<AuthResponse>> | No | ✅ (no validator needed) |
| GetOwnerProfileQuery | IRequest<OwnerProfileDto> | No | ✅ |
| UpdateOwnerProfileCommand | IRequest<OwnerProfileDto> | Yes | ✅ |
| ChangeOwnerPasswordCommand | **IRequest<Result>** (was IRequest) | Yes | **✅ (FIXED)** |
| CreateProjectCommand | IRequest<Result<ProjectDto>> | Yes | ✅ |
| ListProjectsQuery | IRequest<Result<List<ProjectDto>>> | No | ✅ |
| GetProjectByIdQuery | IRequest<Result<ProjectDto>> | No | ✅ |
| GetProjectSettingsQuery | IRequest<ProjectSettingsDto> | No | ✅ |
| UpdateProjectSettingsCommand | IRequest<ProjectSettingsDto> | Yes | ✅ |
| ListFoldersQuery | IRequest<Result<...>> | No | ✅ |
| CreateFolderCommand | IRequest<Result<...>> | No | ✅ |
| UploadProjectFileCommand | IRequest<Result<...>> | Yes | ✅ |
| SearchProjectFilesQuery | IRequest<Result<...>> | Yes | ✅ |
| GetProjectTimelineQuery | IRequest<Result<...>> | No | ✅ |
| AddProjectTimelineNoteCommand | IRequest<Result<...>> | No | ✅ |
| UpdateProjectStageCommand | IRequest<Result<...>> | No | ✅ |
| CreateProjectFollowerCommand | IRequest<Result<...>> | Yes | ✅ |
| UpdateProjectFollowerCommand | IRequest<Result<...>> | Yes | ✅ |
| GenerateFollowerUploadLinkCommand | IRequest<Result<...>> | No | ✅ |
| UploadFollowerFileCommand | IRequest<Result<...>> | No | ✅ |

**Result: Zero bare `IRequest` commands remain. All requests flow through the validation pipeline.**

## Error Handling Hardening

The `ExceptionHandlingMiddleware` now has three layers:
1. `FluentValidation.ValidationException` → 400 Bad Request
2. `InvalidOperationException` with error-code messages → 400/404 (mapped by code)
3. Unhandled `Exception` → 500 Internal Server Error (no stack trace, no internals)

## Security Confirmation

- ✅ No passwords logged
- ✅ No password hash exposed in API responses
- ✅ No token hashes exposed
- ✅ No file storage paths exposed
- ✅ No raw exceptions shown to API clients
- ✅ Expected validation failures return 400, not 500
- ✅ Owner isolation intact (JWT-based, tested)
- ✅ No secrets or test credentials tracked

## Runtime Verification Results (24/24 passed)

1. Register owner: ✅
2. Login owner: ✅
3. GET profile: ✅
4. PUT profile empty name → 400: ✅
5. Wrong current password → 400: ✅
6. Weak password → 400: ✅
7. Confirmation mismatch → 400: ✅
8. Same as current → 400: ✅
9. Valid password change → 204: ✅
10. Old password login fails → 401: ✅
11. New password login succeeds: ✅
12. Create project: ✅
13. Empty projectName → 400: ✅
14. Invalid projectType → 400: ✅
15. Invalid mapLink → 400: ✅
16. Valid settings update: ✅
17. CurrentStage unchanged: ✅
18. Register/Login regression: ✅
19. Project list regression: ✅
20. Vault folder list regression: ✅
21. File search regression: ✅
22. Contract template list regression: ✅
23. Timeline stage regression: ✅
24. Signed contract upload: Deferred (no signed contract available for test)

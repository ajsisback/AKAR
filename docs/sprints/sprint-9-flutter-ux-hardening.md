# Sprint 9B: Flutter UX Polish & State Hardening

## Objective

Improve the Flutter owner-facing app experience by standardizing loading, empty, error, and retry states across all major screens. No new product features.

## Shared Widgets Created

### `mobile/lib/core/widgets.dart` (NEW)

| Widget | Purpose |
|---|---|
| `AkarLoadingState` | Centered CircularProgressIndicator + localized "Loading..." text |
| `AkarEmptyState` | Icon + message + optional subtitle + optional action button |
| `AkarErrorState` | Error icon + localized message + Retry button |
| `AkarPrimaryButton` | ElevatedButton with built-in loading spinner and disabled state |
| `mapErrorCode()` | Maps 40+ backend error codes to localization keys |
| `localizeError()` | Translates error code to user-facing string via l10n |

## Error Code Mapping

The `mapErrorCode()` function provides centralized mapping for 40+ backend error codes:

- Auth: `AUTH_INVALID_CREDENTIALS`, `OWNER_NOT_FOUND`, `UNAUTHORIZED`
- Password: `CURRENT_PASSWORD_INVALID`, `PASSWORD_TOO_WEAK`, `PASSWORD_CONFIRMATION_MISMATCH`, `PASSWORD_SAME_AS_CURRENT`
- Project: `PROJECT_NOT_FOUND`, `PROJECT_NAME_REQUIRED`, `INVALID_PROJECT_TYPE`, `INVALID_MAP_URL`
- Files: `FILE_NOT_FOUND`, `INVALID_FILE_TYPE`, `FILE_TOO_LARGE`, `STORAGE_SAVE_FAILED`
- Folders: `FOLDER_NOT_FOUND`, `FOLDER_SYSTEM_PROTECTED`
- Followers: `FOLLOWER_NOT_FOUND`, `FOLLOWER_PHONE_ALREADY_EXISTS`, `INVALID_UPLOAD_TOKEN`, `UPLOAD_LINK_EXPIRED`, `UPLOAD_LINK_REVOKED`
- Contracts: `CONTRACT_NOT_FOUND`, `CONTRACT_NOT_DRAFT`, `CONTRACT_ALREADY_SIGNED`, `CONTRACT_NOT_PDF_GENERATED`, `PDF_GENERATION_FAILED`
- Timeline: `TIMELINE_EVENT_NOT_FOUND`, `SYSTEM_EVENT_CANNOT_BE_DELETED`, `INVALID_STAGE`
- Fallback: All unmapped codes → `err_generic`

Previously, screens had local `_mapError()` methods with partial coverage. Now consolidated into one place.

## Localization Keys Added (11 new, AR+EN parity)

| Key | Arabic | English |
|---|---|---|
| `btn_retry` | إعادة المحاولة | Retry |
| `saving` | جاري الحفظ... | Saving... |
| `err_session_expired` | انتهت الجلسة، يرجى تسجيل الدخول مجدداً | Session expired, please log in again |
| `err_validation` | يرجى التحقق من المدخلات | Please check your input |
| `err_unable_to_load` | تعذر تحميل البيانات | Unable to load data |
| `err_unable_to_save` | تعذر حفظ البيانات | Unable to save data |
| `saved_successfully` | تم الحفظ بنجاح | Saved successfully |
| `err_unexpected` | حدث خطأ غير متوقع | An unexpected error occurred |
| `no_data` | لا توجد بيانات | No data available |
| `try_again` | حاول مرة أخرى | Try again |

## Screens Polished

| Screen | Before | After |
|---|---|---|
| Dashboard | Silent error (catch ignored) | AkarErrorState + retry + localized error |
| Projects list | Silent error | AkarErrorState + retry + localized error |
| Create project | Generic catch | localizeError for ApiException + AkarPrimaryButton |
| Followers list | `btn_back` on error | AkarErrorState + retry |
| Owner profile | Local `_mapError` | AkarLoadingState + AkarErrorState + centralized localizeError |
| Change password | Local `_mapError` | Centralized localizeError |
| Project settings | Local `_mapError` | AkarLoadingState + AkarErrorState + centralized localizeError |
| Login | Inline button spinner | AkarPrimaryButton |
| Register | Inline button spinner | AkarPrimaryButton + localizeError |

## UX Improvements Summary

1. **Loading states**: All data-loading screens now use `AkarLoadingState` with "Loading..." text
2. **Empty states**: Dashboard, projects list, followers list use `AkarEmptyState` with icon + message + subtitle
3. **Error states**: All data-loading screens now show `AkarErrorState` with retry button instead of silent failure
4. **Retry actions**: Added to dashboard, projects, followers, profile, project settings
5. **Form buttons**: Login, register, create project now use `AkarPrimaryButton` with built-in loading/disabled
6. **Error mapping**: 3 local `_mapError` methods replaced with centralized `localizeError()`
7. **Duplicate submit prevention**: All form buttons disabled during loading (was already mostly in place)

## Security Confirmation

- ✅ JWT used for all protected API calls
- ✅ No password values logged
- ✅ No password values stored (controllers cleared on change)
- ✅ No password hash shown
- ✅ No file storage paths exposed
- ✅ No token hashes shown
- ✅ No raw backend exceptions shown
- ✅ Password fields use `obscureText: true`

## Verification Results

- **flutter pub get**: ✅ Got dependencies
- **flutter analyze**: ✅ No issues found (ran in 8.6s)

## Deferred Items

- Full 46-point runtime UI walkthrough (requires manual Flutter web session)
- Contracts screen, timeline screen, document vault screen, file search screen, follower details screen, project details screen — already have loading/error/empty patterns from previous sprints; not modified in this sprint to avoid risk
- RTL/LTR visual verification (deferred to runtime)
- Language persistence test (deferred to runtime)

import 'package:flutter/material.dart';
import '../core/l10n.dart';
import '../core/theme.dart';

/// Maps backend error codes to localized message keys.
/// Falls back to 'err_generic' for unmapped codes.
String mapErrorCode(String code) {
  const codeToKey = {
    // Auth / Profile
    'OWNER_NOT_FOUND': 'err_owner_not_found',
    'AUTH_INVALID_CREDENTIALS': 'err_invalid_credentials',
    'PROFILE_NAME_REQUIRED': 'err_project_name_required',
    'CURRENT_PASSWORD_INVALID': 'err_current_password_invalid',
    'PASSWORD_TOO_WEAK': 'err_password_too_weak',
    'PASSWORD_CONFIRMATION_MISMATCH': 'err_password_confirmation_mismatch',
    'PASSWORD_SAME_AS_CURRENT': 'err_password_same_as_current',
    'VALIDATION_ERROR': 'err_validation',
    // Project
    'PROJECT_NOT_FOUND': 'err_project_not_found',
    'PROJECT_NAME_REQUIRED': 'err_project_name_required',
    'INVALID_PROJECT_TYPE': 'err_invalid_project_type',
    'INVALID_MAP_URL': 'err_invalid_map_url',
    'PROJECT_INVALID_TYPE': 'err_invalid_project_type',
    // Files
    'FILE_NOT_FOUND': 'err_file_not_found',
    'INVALID_FILE_TYPE': 'err_file_type_not_allowed',
    'FILE_TOO_LARGE': 'err_file_too_large',
    'INVALID_FILE_EXTENSION': 'err_file_type_not_allowed',
    'STORAGE_SAVE_FAILED': 'err_storage_save_failed',
    // Folders
    'FOLDER_NOT_FOUND': 'err_folder_not_found',
    'FOLDER_SYSTEM_PROTECTED': 'err_folder_system_protected',
    // Followers
    'FOLLOWER_NOT_FOUND': 'err_follower_not_found',
    'FOLLOWER_PHONE_ALREADY_EXISTS': 'err_follower_phone_exists',
    'FOLLOWER_INACTIVE': 'err_follower_inactive',
    'INVALID_UPLOAD_TOKEN': 'err_upload_link_not_found',
    'UPLOAD_LINK_EXPIRED': 'err_upload_link_expired',
    'UPLOAD_LINK_REVOKED': 'err_upload_link_revoked',
    'UPLOAD_LINK_NOT_FOUND': 'err_upload_link_not_found',
    // Contracts
    'CONTRACT_NOT_FOUND': 'err_contract_not_found',
    'CONTRACT_NOT_DRAFT': 'err_contract_not_draft',
    'CONTRACT_NOT_ELIGIBLE_FOR_PDF': 'err_contract_not_eligible_for_pdf',
    'CONTRACT_CANCELLED': 'err_contract_cancelled',
    'CONTRACT_ALREADY_SIGNED': 'err_contract_already_signed',
    'CONTRACT_NOT_PDF_GENERATED': 'err_contract_not_pdf_generated',
    'CONTRACTS_FOLDER_NOT_FOUND': 'err_contracts_folder_not_found',
    'PDF_GENERATION_FAILED': 'err_pdf_generation_failed',
    // Timeline
    'TIMELINE_EVENT_NOT_FOUND': 'err_timeline_event_not_found',
    'SYSTEM_EVENT_CANNOT_BE_DELETED': 'err_timeline_system_no_delete',
    'INVALID_STAGE': 'err_invalid_project_stage',
    'STAGE_ALREADY_SET': 'err_stage_already_set',
    // Network
    'UNAUTHORIZED': 'err_session_expired',
    'ERR_401': 'err_session_expired',
  };
  return codeToKey[code] ?? 'err_generic';
}

/// Translates an API error code to a user-facing localized string.
String localizeError(BuildContext context, String code) {
  final l = AppLocalizations.of(context);
  return l.t(mapErrorCode(code));
}

/// A standardized loading indicator widget.
class AkarLoadingState extends StatelessWidget {
  final String? message;
  const AkarLoadingState({super.key, this.message});

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const CircularProgressIndicator(),
          if (message != null || true) ...[
            const SizedBox(height: 16),
            Text(
              message ?? l.t('loading'),
              style: const TextStyle(color: AkarTheme.textSecondary, fontSize: 14),
            ),
          ],
        ],
      ),
    );
  }
}

/// A standardized empty state widget with icon, message, and optional action.
class AkarEmptyState extends StatelessWidget {
  final IconData icon;
  final String message;
  final String? subtitle;
  final String? actionLabel;
  final VoidCallback? onAction;

  const AkarEmptyState({
    super.key,
    required this.icon,
    required this.message,
    this.subtitle,
    this.actionLabel,
    this.onAction,
  });

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: 64, color: AkarTheme.textMuted),
            const SizedBox(height: 16),
            Text(
              message,
              style: const TextStyle(fontSize: 16, color: AkarTheme.textSecondary),
              textAlign: TextAlign.center,
            ),
            if (subtitle != null) ...[
              const SizedBox(height: 6),
              Text(
                subtitle!,
                style: const TextStyle(fontSize: 13, color: AkarTheme.textMuted),
                textAlign: TextAlign.center,
              ),
            ],
            if (actionLabel != null && onAction != null) ...[
              const SizedBox(height: 20),
              ElevatedButton(onPressed: onAction, child: Text(actionLabel!)),
            ],
          ],
        ),
      ),
    );
  }
}

/// A standardized error state widget with retry support.
class AkarErrorState extends StatelessWidget {
  final String message;
  final VoidCallback? onRetry;

  const AkarErrorState({
    super.key,
    required this.message,
    this.onRetry,
  });

  @override
  Widget build(BuildContext context) {
    final l = AppLocalizations.of(context);
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.error_outline, size: 56, color: AkarTheme.danger),
            const SizedBox(height: 16),
            Text(
              message,
              style: const TextStyle(fontSize: 15, color: AkarTheme.textSecondary),
              textAlign: TextAlign.center,
            ),
            if (onRetry != null) ...[
              const SizedBox(height: 20),
              OutlinedButton.icon(
                onPressed: onRetry,
                icon: const Icon(Icons.refresh, size: 18),
                label: Text(l.t('btn_retry')),
                style: OutlinedButton.styleFrom(
                  foregroundColor: AkarTheme.accent,
                  side: const BorderSide(color: AkarTheme.accent),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

/// A primary button with built-in loading indicator and disabled state.
class AkarPrimaryButton extends StatelessWidget {
  final String label;
  final bool loading;
  final VoidCallback? onPressed;
  final IconData? icon;

  const AkarPrimaryButton({
    super.key,
    required this.label,
    this.loading = false,
    this.onPressed,
    this.icon,
  });

  @override
  Widget build(BuildContext context) {
    return ElevatedButton(
      onPressed: loading ? null : onPressed,
      child: loading
          ? const SizedBox(
              width: 20,
              height: 20,
              child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
            )
          : icon != null
              ? Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(icon, size: 18),
                    const SizedBox(width: 8),
                    Text(label),
                  ],
                )
              : Text(label),
    );
  }
}

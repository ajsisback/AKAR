import 'dart:typed_data';

/// Stub implementation for non-web platforms (Android, iOS, desktop).
/// File download via browser Blob URL is not available on these platforms.
void downloadFileBytes(Uint8List bytes, String fileName) {
  // No-op on non-web platforms.
  // On Android/iOS, files could be saved via path_provider + file I/O,
  // but that requires additional permissions and is deferred to a future sprint.
}

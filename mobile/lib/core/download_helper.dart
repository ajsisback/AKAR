/// Platform-agnostic file download helper.
/// On web, triggers a browser download via Blob URL.
/// On mobile/desktop, this is a no-op (download not supported from this path).
///
/// Usage:
///   import 'download_helper.dart';
///   downloadFileBytes(bytes, 'filename.pdf');
///
/// The actual implementation is selected at compile time via conditional imports.
library;

export 'download_helper_stub.dart'
    if (dart.library.js_interop) 'download_helper_web.dart';

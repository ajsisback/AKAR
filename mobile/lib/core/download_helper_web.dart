import 'dart:js_interop';
import 'dart:typed_data';
import 'package:web/web.dart' as web;

/// Web implementation: triggers a browser file download via Blob URL.
void downloadFileBytes(Uint8List bytes, String fileName) {
  final jsArray = bytes.toJS;
  final blob = web.Blob([jsArray].toJS);
  final url = web.URL.createObjectURL(blob);
  final anchor = web.document.createElement('a') as web.HTMLAnchorElement
    ..href = url
    ..download = fileName
    ..style.display = 'none';
  web.document.body?.append(anchor);
  anchor.click();
  anchor.remove();
  web.URL.revokeObjectURL(url);
}

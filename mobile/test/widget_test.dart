import 'package:flutter_test/flutter_test.dart';
import 'package:akar_mobile/main.dart';

void main() {
  testWidgets('App renders', (WidgetTester tester) async {
    await tester.pumpWidget(const AkarApp());
    // App should render without errors
    expect(find.byType(AkarApp), findsOneWidget);
  });
}

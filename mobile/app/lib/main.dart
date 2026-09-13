import 'package:erp_mobile/app.dart';
import 'package:erp_mobile/service_locator.dart';
import 'package:flutter/material.dart';

Future<void> main() async {
  await initializeDependencies();
  runApp(const MyApp());
}

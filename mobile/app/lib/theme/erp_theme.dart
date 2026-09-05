import 'package:flutter/material.dart';

/// ERPPlatform Design System — Flutter Theme
/// Generated from the Angular frontend's styles.scss + design tokens.
/// The mobile dev should use these exact colors, radii, and typography
/// to match the web app's visual identity.

class ErpColors {
  // Primary
  static const Color primaryLight = Color(0xFF2563EB); // blue-600
  static const Color primaryLightHover = Color(0xFF1D4ED8); // blue-700
  static const Color primaryLightBg = Color(0xFFEFF6FF); // blue-50
  static const Color primaryDark = Color(0xFF3B82F6); // blue-500
  static const Color primaryDarkHover = Color(0xFF60A5FA); // blue-400
  static const Color primaryDarkBg = Color(0xFF1E293B); // slate-800

  // Backgrounds
  static const Color bgLightMain = Color(0xFFF8FAFC); // slate-50
  static const Color bgLightCard = Color(0xFFFFFFFF);
  static const Color bgDarkMain = Color(0xFF0F172A); // slate-900
  static const Color bgDarkCard = Color(0xFF1E293B); // slate-800

  // Text
  static const Color textLightMain = Color(0xFF0F172A); // slate-900
  static const Color textLightMuted = Color(0xFF64748B); // slate-500
  static const Color textDarkMain = Color(0xFFF8FAFC); // slate-50
  static const Color textDarkMuted = Color(0xFF94A3B8); // slate-400

  // Borders
  static const Color borderLight = Color(0xFFE2E8F0); // slate-200
  static const Color borderDark = Color(0xFF334155); // slate-700

  // Status Colors (same for light & dark)
  static const Color successBg = Color(0xFFDCFCE7);
  static const Color successText = Color(0xFF15803D);
  static const Color warningBg = Color(0xFFFEF9C3);
  static const Color warningText = Color(0xFFA16207);
  static const Color errorBg = Color(0xFFFEE2E2);
  static const Color errorText = Color(0xFFB91C1C);
  static const Color infoBg = Color(0xFFE0E7FF);
  static const Color infoText = Color(0xFF4338CA);

  // Accent Colors (KPI cards, charts, icons)
  static const Color blue50 = Color(0xFFEFF6FF);
  static const Color blue100 = Color(0xFFDBEAFE);
  static const Color blue500 = Color(0xFF3B82F6);
  static const Color blue600 = Color(0xFF2563EB);
  static const Color blue700 = Color(0xFF1D4ED8);

  static const Color indigo100 = Color(0xFFE0E7FF);
  static const Color indigo500 = Color(0xFF6366F1);

  static const Color emerald50 = Color(0xFFECFDF5);
  static const Color emerald100 = Color(0xFFD1FAE5);
  static const Color emerald400 = Color(0xFF34D399);
  static const Color emerald500 = Color(0xFF10B981);
  static const Color emerald600 = Color(0xFF059669);

  static const Color amber50 = Color(0xFFFFFBEB);
  static const Color amber100 = Color(0xFFFEF3C7);
  static const Color amber600 = Color(0xFFD97706);

  static const Color rose50 = Color(0xFFFFF1F2);
  static const Color rose100 = Color(0xFFFFE4E6);
  static const Color rose500 = Color(0xFFF43F5E);
  static const Color rose600 = Color(0xFFE11D48);

  static const Color red50 = Color(0xFFFEF2F2);
  static const Color red500 = Color(0xFFEF4444);
  static const Color red700 = Color(0xFFB91C1C);

  // Scrollbar
  static const Color scrollbarThumb = Color(0xFFCBD5E1);
  static const Color scrollbarThumbHover = Color(0xFF94A3B8);
}

class ErpRadius {
  static const double sm = 6.0;
  static const double md = 10.0;
  static const double lg = 14.0;
}

class ErpShadows {
  static List<BoxShadow> lightSm = [
    BoxShadow(color: Color(0x0D000000), blurRadius: 3, offset: Offset(0, 1)),
  ];

  static List<BoxShadow> lightMd = [
    BoxShadow(
      color: Color(0x12000000),
      blurRadius: 6,
      spreadRadius: -1,
      offset: Offset(0, 4),
    ),
    BoxShadow(
      color: Color(0x0A000000),
      blurRadius: 4,
      spreadRadius: -1,
      offset: Offset(0, 2),
    ),
  ];

  static List<BoxShadow> lightLg = [
    BoxShadow(
      color: Color(0x14000000),
      blurRadius: 15,
      spreadRadius: -3,
      offset: Offset(0, 10),
    ),
    BoxShadow(
      color: Color(0x08000000),
      blurRadius: 6,
      spreadRadius: -2,
      offset: Offset(0, 4),
    ),
  ];

  static List<BoxShadow> darkSm = [
    BoxShadow(color: Color(0x4D000000), blurRadius: 3, offset: Offset(0, 1)),
  ];

  static List<BoxShadow> darkMd = [
    BoxShadow(
      color: Color(0x66000000),
      blurRadius: 6,
      spreadRadius: -1,
      offset: Offset(0, 4),
    ),
  ];
}

/// Light Theme
ThemeData erpLightTheme = ThemeData(
  brightness: Brightness.light,
  useMaterial3: true,
  colorScheme: ColorScheme.light(
    primary: ErpColors.primaryLight,
    onPrimary: Colors.white,
    secondary: ErpColors.indigo500,
    onSecondary: Colors.white,
    surface: ErpColors.bgLightCard,
    onSurface: ErpColors.textLightMain,
    error: ErpColors.red500,
    onError: Colors.white,
    outline: ErpColors.borderLight,
  ),
  scaffoldBackgroundColor: ErpColors.bgLightMain,
  cardColor: ErpColors.bgLightCard,
  dividerColor: ErpColors.borderLight,
  fontFamily: 'Inter',
  textTheme: TextTheme(
    displayLarge: TextStyle(
      color: ErpColors.textLightMain,
      fontWeight: FontWeight.w800,
    ),
    displayMedium: TextStyle(
      color: ErpColors.textLightMain,
      fontWeight: FontWeight.w700,
    ),
    headlineLarge: TextStyle(
      color: ErpColors.textLightMain,
      fontWeight: FontWeight.w700,
    ),
    headlineMedium: TextStyle(
      color: ErpColors.textLightMain,
      fontWeight: FontWeight.w600,
    ),
    titleLarge: TextStyle(
      color: ErpColors.textLightMain,
      fontWeight: FontWeight.w600,
    ),
    titleMedium: TextStyle(
      color: ErpColors.textLightMain,
      fontWeight: FontWeight.w500,
    ),
    bodyLarge: TextStyle(
      color: ErpColors.textLightMain,
      fontWeight: FontWeight.w400,
    ),
    bodyMedium: TextStyle(
      color: ErpColors.textLightMain,
      fontWeight: FontWeight.w400,
    ),
    bodySmall: TextStyle(
      color: ErpColors.textLightMuted,
      fontWeight: FontWeight.w400,
    ),
    labelLarge: TextStyle(
      color: ErpColors.textLightMain,
      fontWeight: FontWeight.w500,
    ),
    labelSmall: TextStyle(
      color: ErpColors.textLightMuted,
      fontWeight: FontWeight.w500,
    ),
  ),
  appBarTheme: AppBarTheme(
    backgroundColor: ErpColors.bgLightCard,
    foregroundColor: ErpColors.textLightMain,
    elevation: 0,
    surfaceTintColor: Colors.transparent,
    shape: Border(bottom: BorderSide(color: ErpColors.borderLight, width: 1)),
  ),
  cardTheme: CardThemeData(
    color: ErpColors.bgLightCard,
    elevation: 0,
    shape: RoundedRectangleBorder(
      borderRadius: BorderRadius.circular(ErpRadius.md),
      side: BorderSide(color: ErpColors.borderLight, width: 1),
    ),
  ),
  elevatedButtonTheme: ElevatedButtonThemeData(
    style: ElevatedButton.styleFrom(
      backgroundColor: ErpColors.primaryLight,
      foregroundColor: Colors.white,
      elevation: 0,
      padding: EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(ErpRadius.sm),
      ),
      textStyle: TextStyle(fontWeight: FontWeight.w500),
    ),
  ),
  outlinedButtonTheme: OutlinedButtonThemeData(
    style: OutlinedButton.styleFrom(
      foregroundColor: ErpColors.textLightMain,
      side: BorderSide(color: ErpColors.borderLight),
      padding: EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(ErpRadius.sm),
      ),
    ),
  ),
  inputDecorationTheme: InputDecorationTheme(
    filled: true,
    fillColor: ErpColors.bgLightCard,
    border: OutlineInputBorder(
      borderRadius: BorderRadius.circular(ErpRadius.sm),
      borderSide: BorderSide(color: ErpColors.borderLight),
    ),
    enabledBorder: OutlineInputBorder(
      borderRadius: BorderRadius.circular(ErpRadius.sm),
      borderSide: BorderSide(color: ErpColors.borderLight),
    ),
    focusedBorder: OutlineInputBorder(
      borderRadius: BorderRadius.circular(ErpRadius.sm),
      borderSide: BorderSide(color: ErpColors.primaryLight, width: 2),
    ),
  ),
  bottomNavigationBarTheme: BottomNavigationBarThemeData(
    backgroundColor: ErpColors.bgLightCard,
    selectedItemColor: ErpColors.primaryLight,
    unselectedItemColor: ErpColors.textLightMuted,
    type: BottomNavigationBarType.fixed,
  ),
);

/// Dark Theme
ThemeData erpDarkTheme = ThemeData(
  brightness: Brightness.dark,
  useMaterial3: true,
  colorScheme: ColorScheme.dark(
    primary: ErpColors.primaryDark,
    onPrimary: Colors.white,
    secondary: ErpColors.indigo500,
    onSecondary: Colors.white,
    surface: ErpColors.bgDarkCard,
    onSurface: ErpColors.textDarkMain,
    error: ErpColors.red500,
    onError: Colors.white,
    outline: ErpColors.borderDark,
  ),
  scaffoldBackgroundColor: ErpColors.bgDarkMain,
  cardColor: ErpColors.bgDarkCard,
  dividerColor: ErpColors.borderDark,
  fontFamily: 'Inter',
  textTheme: TextTheme(
    displayLarge: TextStyle(
      color: ErpColors.textDarkMain,
      fontWeight: FontWeight.w800,
    ),
    displayMedium: TextStyle(
      color: ErpColors.textDarkMain,
      fontWeight: FontWeight.w700,
    ),
    headlineLarge: TextStyle(
      color: ErpColors.textDarkMain,
      fontWeight: FontWeight.w700,
    ),
    headlineMedium: TextStyle(
      color: ErpColors.textDarkMain,
      fontWeight: FontWeight.w600,
    ),
    titleLarge: TextStyle(
      color: ErpColors.textDarkMain,
      fontWeight: FontWeight.w600,
    ),
    titleMedium: TextStyle(
      color: ErpColors.textDarkMain,
      fontWeight: FontWeight.w500,
    ),
    bodyLarge: TextStyle(
      color: ErpColors.textDarkMain,
      fontWeight: FontWeight.w400,
    ),
    bodyMedium: TextStyle(
      color: ErpColors.textDarkMain,
      fontWeight: FontWeight.w400,
    ),
    bodySmall: TextStyle(
      color: ErpColors.textDarkMuted,
      fontWeight: FontWeight.w400,
    ),
    labelLarge: TextStyle(
      color: ErpColors.textDarkMain,
      fontWeight: FontWeight.w500,
    ),
    labelSmall: TextStyle(
      color: ErpColors.textDarkMuted,
      fontWeight: FontWeight.w500,
    ),
  ),
  appBarTheme: AppBarTheme(
    backgroundColor: ErpColors.bgDarkCard,
    foregroundColor: ErpColors.textDarkMain,
    elevation: 0,
    surfaceTintColor: Colors.transparent,
    shape: Border(bottom: BorderSide(color: ErpColors.borderDark, width: 1)),
  ),
  cardTheme: CardThemeData(
    color: ErpColors.bgDarkCard,
    elevation: 0,
    shape: RoundedRectangleBorder(
      borderRadius: BorderRadius.circular(ErpRadius.md),
      side: BorderSide(color: ErpColors.borderDark, width: 1),
    ),
  ),
  elevatedButtonTheme: ElevatedButtonThemeData(
    style: ElevatedButton.styleFrom(
      backgroundColor: ErpColors.primaryDark,
      foregroundColor: Colors.white,
      elevation: 0,
      padding: EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(ErpRadius.sm),
      ),
      textStyle: TextStyle(fontWeight: FontWeight.w500),
    ),
  ),
  outlinedButtonTheme: OutlinedButtonThemeData(
    style: OutlinedButton.styleFrom(
      foregroundColor: ErpColors.textDarkMain,
      side: BorderSide(color: ErpColors.borderDark),
      padding: EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(ErpRadius.sm),
      ),
    ),
  ),
  inputDecorationTheme: InputDecorationTheme(
    filled: true,
    fillColor: ErpColors.bgDarkCard,
    border: OutlineInputBorder(
      borderRadius: BorderRadius.circular(ErpRadius.sm),
      borderSide: BorderSide(color: ErpColors.borderDark),
    ),
    enabledBorder: OutlineInputBorder(
      borderRadius: BorderRadius.circular(ErpRadius.sm),
      borderSide: BorderSide(color: ErpColors.borderDark),
    ),
    focusedBorder: OutlineInputBorder(
      borderRadius: BorderRadius.circular(ErpRadius.sm),
      borderSide: BorderSide(color: ErpColors.primaryDark, width: 2),
    ),
  ),
  bottomNavigationBarTheme: BottomNavigationBarThemeData(
    backgroundColor: ErpColors.bgDarkCard,
    selectedItemColor: ErpColors.primaryDark,
    unselectedItemColor: ErpColors.textDarkMuted,
    type: BottomNavigationBarType.fixed,
  ),
);

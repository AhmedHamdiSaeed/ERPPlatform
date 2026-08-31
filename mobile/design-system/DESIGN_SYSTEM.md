# ERP Platform — Mobile Design System Guide

**Source:** Extracted from Angular frontend `styles.scss` + component templates.  
**Purpose:** Give the mobile dev team the exact colors, fonts, radii, shadows, and component styles to match the web app.

---

## 1. Typography

| Property | Value |
|----------|-------|
| Font Family | **Inter** (Google Fonts) |
| Weights | 300 (Light), 400 (Regular), 500 (Medium), 600 (SemiBold), 700 (Bold), 800 (ExtraBold), 900 (Black) |
| Fallback stack | `system-ui, -apple-system, sans-serif` |
| Google Fonts URL | `https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800;900&display=swap` |

**Flutter setup:** Add `google_fonts` package and use `GoogleFonts.inter()` or bundle the Inter font files locally.

---

## 2. Color System

### Light Theme

| Token | Hex | Flutter Color | Usage |
|-------|-----|---------------|-------|
| `--primary-color` | `#2563EB` | `Color(0xFF2563EB)` | Primary buttons, active nav, links |
| `--primary-color-hover` | `#1D4ED8` | `Color(0xFF1D4ED8)` | Button press/hover state |
| `--primary-light` | `#EFF6FF` | `Color(0xFFEFF6FF)` | Light primary background |
| `--bg-main` | `#F8FAFC` | `Color(0xFFF8FAFC)` | App background (slate-50) |
| `--bg-card` | `#FFFFFF` | `Color(0xFFFFFFFF)` | Card/surface background |
| `--text-main` | `#0F172A` | `Color(0xFF0F172A)` | Primary text (slate-900) |
| `--text-muted` | `#64748B` | `Color(0xFF64748B)` | Secondary/muted text (slate-500) |
| `--border-color` | `#E2E8F0` | `Color(0xFFE2E8F0)` | Borders, dividers (slate-200) |

### Dark Theme

| Token | Hex | Flutter Color | Usage |
|-------|-----|---------------|-------|
| `--primary-color` | `#3B82F6` | `Color(0xFF3B82F6)` | Primary in dark mode |
| `--primary-color-hover` | `#60A5FA` | `Color(0xFF60A5FA)` | Primary hover in dark |
| `--primary-light` | `#1E293B` | `Color(0xFF1E293B)` | Dark primary bg (slate-800) |
| `--bg-main` | `#0F172A` | `Color(0xFF0F172A)` | App background (slate-900) |
| `--bg-card` | `#1E293B` | `Color(0xFF1E293B)` | Card background (slate-800) |
| `--text-main` | `#F8FAFC` | `Color(0xFFF8FAFC)` | Primary text (slate-50) |
| `--text-muted` | `#94A3B8` | `Color(0xFF94A3B8)` | Secondary text (slate-400) |
| `--border-color` | `#334155` | `Color(0xFF334155)` | Borders (slate-700) |

---

## 3. Status Badge Colors (Same in Both Themes)

| Status | Background | Text | Used For |
|--------|-----------|------|----------|
| Success | `#DCFCE7` | `#15803D` | Active, Completed, Present, Approved |
| Warning | `#FEF9C3` | `#A16207` | Pending, In Transit, In Review |
| Error | `#FEE2E2` | `#B91C1C` | Inactive, Failed, Absent, Rejected |
| Info | `#E0E7FF` | `#4338CA` | Late, On Leave, Remote |

**Flutter:** Badge widget = `Container` with pill shape (`BorderRadius.circular(9999)`), padding `4px x 10px`, fontSize `12px`, fontWeight `w600`.

---

## 4. Accent Colors (KPI Cards, Charts, Icons)

| Color | 50 | 100 | 500 | 600 | 700 | 900 |
|-------|-----|-----|-----|-----|-----|-----|
| Blue | `#EFF6FF` | `#DBEAFE` | `#3B82F6` | `#2563EB` | `#1D4ED8` | `#1E3A8A` |
| Indigo | — | `#E0E7FF` | `#6366F1` | — | — | `#312E81` |
| Emerald | `#ECFDF5` | `#D1FAE5` | `#10B981` | `#059669` | — | `#064E3B` |
| Amber | `#FFFBEB` | `#FEF3C7` | — | `#D97706` | — | `#78350F` |
| Rose | `#FFF1F2` | `#FFE4E6` | `#F43F5E` | `#E11D48` | — | `#881337` |
| Red | `#FEF2F2` | — | `#EF4444` | — | `#B91C1C` | — |

**KPI Card Pattern (from Dashboard):** Each KPI card uses `*-50` as background and `*-600` as icon color. Pairs:
- Blue: Employees
- Emerald: Revenue/Products
- Amber: Pending/Warnings
- Rose: Overdue/Alerts

---

## 5. Border Radius

| Token | Value | Usage |
|-------|-------|-------|
| `--radius-sm` | `6px` | Buttons, inputs, small elements |
| `--radius-md` | `10px` | Cards, panels, containers |
| `--radius-lg` | `14px` | Large cards, modals, sheets |
| Badge pill | `9999px` | Status badges |

---

## 6. Shadows

### Light Theme
| Token | Value |
|-------|-------|
| `--shadow-sm` | `0 1px 3px rgba(0,0,0,0.05)` |
| `--shadow-md` | `0 4px 6px -1px rgba(0,0,0,0.07), 0 2px 4px -1px rgba(0,0,0,0.04)` |
| `--shadow-lg` | `0 10px 15px -3px rgba(0,0,0,0.08), 0 4px 6px -2px rgba(0,0,0,0.03)` |

### Dark Theme
| Token | Value |
|-------|-------|
| `--shadow-sm` | `0 1px 3px rgba(0,0,0,0.3)` |
| `--shadow-md` | `0 4px 6px -1px rgba(0,0,0,0.4)` |

---

## 7. Component Patterns

### Card (`.card-panel`)
- Background: `--bg-card`
- Border: 1px solid `--border-color`
- Border radius: `--radius-md` (10px)
- Box shadow: `--shadow-sm`
- Padding: 20px (1.25rem)
- Hover: shadow transitions to `--shadow-md`

### Primary Button (`.btn-primary`)
- Background: `--primary-color`
- Text color: white
- Padding: 8px 16px
- Border radius: `--radius-sm` (6px)
- Font weight: 500
- Hover: background → `--primary-color-hover`

### Outline Button (`.btn-outline`)
- Background: transparent
- Border: 1px solid `--border-color`
- Text: `--text-main`
- Padding: 8px 16px
- Radius: `--radius-sm` (6px)
- Hover: bg → `--primary-light`, border → `--primary-color`

### Status Badge
- Display: inline-flex pill
- Padding: 4px 10px
- Radius: 9999px (full pill)
- Font size: 12px
- Font weight: 600
- Text transform: capitalize

### Login Page
- Full-screen background: `#0F172A` (slate-900) — dark even in light mode
- Card: `#FFFFFF` / `#1E293B` with `shadow-2xl`
- Logo: blue gradient badge with white text
- Focus rings: `#3B82F6` (blue-500) with ring effect
- Error messages: `#FEF2F2` bg, `#B91C1C` text, `#FECACA` border

---

## 8. Dark Mode Strategy

- Toggle via `data-theme="dark"` attribute on root element
- All colors switch automatically via CSS variable overrides
- Default theme: **Light**
- Dark mode toggle available in Settings page
- The Flutter `erp_theme.dart` file provides both `erpLightTheme` and `erpDarkTheme` — use a `ThemeProvider` or `Riverpod` state to switch.

---

## 9. RTL / Arabic Support

- RTL direction via `dir="rtl"` on `<html>` when language is Arabic
- All layouts are mirrored in RTL mode
- Arabic translations: partial (only 4 keys in `ar.json`)
- Flutter: use `Directionality` widget or `LocalizationsDelegates` for RTL

---

## 10. Icons

- **PrimeIcons** (PrimeNG icon set) — CDN: `https://unpkg.com/primeicons/primeicons.css`
- **Bootstrap Icons** — available as fallback
- For Flutter: use **Material Icons** or **FontAwesome** as the closest equivalents. PrimeIcons class names like `pi-moon`, `pi-sun`, `pi-bell`, `pi-shopping-cart` map to Material Icons `dark_mode`, `light_mode`, `notifications`, `shopping_cart`.

---

## 11. Logo Assets

| File | Size | Usage |
|------|------|-------|
| `logo-light.png` | 33KB | Full logo (light background) |
| `logo-light-thumbnail.png` | 9KB | Thumbnail/small logo |
| `favicon.ico` | — | Browser tab icon |

All logo files are in `mobile/design-system/` alongside this guide.

---

## 12. Files Provided

| File | Description |
|------|-------------|
| `erp_theme.dart` | Ready-to-use Flutter ThemeData (light + dark) with all colors, radii, shadows |
| `design-tokens.json` | Machine-readable design tokens (for code generation or non-Flutter frameworks) |
| `logo-light.png` | Logo image |
| `logo-light-thumbnail.png` | Small logo |
| `favicon.ico` | App icon |
| `DESIGN_SYSTEM.md` | This guide |

---

## 13. Quick Start for Flutter

```dart
import 'erp_theme.dart';

MaterialApp(
  title: 'ERP Platform',
  theme: erpLightTheme,
  darkTheme: erpDarkTheme,
  themeMode: ThemeMode.system, // or .light / .dark
  home: MyApp(),
)
```

Use `ErpColors.primaryLight` / `ErpColors.primaryDark` for brand-specific colors. Use `ErpRadius.md` for card corners. Use `ErpShadows.lightMd` for card shadows.

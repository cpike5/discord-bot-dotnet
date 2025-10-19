# Design Standards

**Version:** 1.0
**Last Updated:** 2025-10-18
**Status:** Active

---

## Overview

This document defines the design tokens, color system, typography, spacing, and visual standards for the Discord bot web application. These tokens ensure visual consistency across all UI components and pages.

---

## Color Tokens

### Primary Color Palette

Based on the Coolors palette, our application uses the following primary colors:

| Token Name | Hex Value | RGB | Usage |
|------------|-----------|-----|-------|
| `--color-slate` | `#555B6E` | rgb(85, 91, 110) | Primary UI elements, sidebar, navigation |
| `--color-rose` | `#DA4167` | rgb(218, 65, 103) | Primary accent, CTA buttons, important metrics |
| `--color-lavender` | `#D3C4E3` | rgb(211, 196, 227) | Secondary accent, backgrounds, soft highlights |
| `--color-sage` | `#87B38D` | rgb(135, 179, 141) | Success states, positive metrics, active states |
| `--color-gold` | `#FED766` | rgb(254, 215, 102) | Warnings, highlights, special states |

### Semantic Colors

Colors mapped to specific UI purposes:

| Token Name | Base Color | Usage |
|------------|------------|-------|
| `--color-primary` | `--color-rose` | Primary actions, CTAs, highlights |
| `--color-secondary` | `--color-lavender` | Secondary actions, backgrounds |
| `--color-success` | `--color-sage` | Success messages, positive states |
| `--color-warning` | `--color-gold` | Warnings, caution states |
| `--color-danger` | `--color-rose` (darker) | Errors, destructive actions |
| `--color-info` | `--color-lavender` | Informational messages |

### Neutral Scale

Grayscale values derived from slate base:

| Token Name | Hex Value | Usage |
|------------|-----------|-------|
| `--color-neutral-50` | `#F8F9FA` | Lightest backgrounds |
| `--color-neutral-100` | `#F1F3F5` | Light backgrounds |
| `--color-neutral-200` | `#E9ECEF` | Borders, dividers |
| `--color-neutral-300` | `#DEE2E6` | Disabled states |
| `--color-neutral-400` | `#CED4DA` | Placeholder text |
| `--color-neutral-500` | `#ADB5BD` | Secondary text |
| `--color-neutral-600` | `#6C757D` | Body text |
| `--color-neutral-700` | `#495057` | Headings |
| `--color-neutral-800` | `#343A40` | Dark text |
| `--color-neutral-900` | `#212529` | Darkest text |

### Surface Colors

Background and surface colors for different UI layers:

| Token Name | Value | Usage |
|------------|-------|-------|
| `--surface-primary` | `--color-slate` | Main sidebar, primary navigation |
| `--surface-secondary` | `--color-neutral-100` | Secondary backgrounds |
| `--surface-card` | `#FFFFFF` | Card backgrounds |
| `--surface-overlay` | `rgba(0, 0, 0, 0.5)` | Modal overlays |

---

## Typography

### Font Families

| Token Name | Value | Usage |
|------------|-------|-------|
| `--font-primary` | `'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif` | Body text, UI elements |
| `--font-heading` | `'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif` | Headings, titles |
| `--font-mono` | `'Fira Code', 'Consolas', 'Monaco', monospace` | Code, technical data |

### Font Sizes

| Token Name | Size | Line Height | Usage |
|------------|------|-------------|-------|
| `--text-xs` | `0.75rem` (12px) | `1rem` | Small labels, captions |
| `--text-sm` | `0.875rem` (14px) | `1.25rem` | Secondary text, helper text |
| `--text-base` | `1rem` (16px) | `1.5rem` | Body text, default |
| `--text-lg` | `1.125rem` (18px) | `1.75rem` | Large body text |
| `--text-xl` | `1.25rem` (20px) | `1.75rem` | Small headings |
| `--text-2xl` | `1.5rem` (24px) | `2rem` | Section headings |
| `--text-3xl` | `1.875rem` (30px) | `2.25rem` | Page headings |
| `--text-4xl` | `2.25rem` (36px) | `2.5rem` | Large headings |

### Font Weights

| Token Name | Value | Usage |
|------------|-------|-------|
| `--font-normal` | `400` | Body text |
| `--font-medium` | `500` | Emphasized text |
| `--font-semibold` | `600` | Subheadings, labels |
| `--font-bold` | `700` | Headings, important text |

---

## Spacing

### Spacing Scale

Consistent spacing based on 4px base unit:

| Token Name | Size | Usage |
|------------|------|-------|
| `--space-1` | `0.25rem` (4px) | Minimal spacing |
| `--space-2` | `0.5rem` (8px) | Tight spacing |
| `--space-3` | `0.75rem` (12px) | Small spacing |
| `--space-4` | `1rem` (16px) | Default spacing |
| `--space-5` | `1.25rem` (20px) | Medium spacing |
| `--space-6` | `1.5rem` (24px) | Large spacing |
| `--space-8` | `2rem` (32px) | Extra large spacing |
| `--space-10` | `2.5rem` (40px) | Section spacing |
| `--space-12` | `3rem` (48px) | Large section spacing |
| `--space-16` | `4rem` (64px) | Major section spacing |

---

## Sizing

### Component Sizes

| Token Name | Size | Usage |
|------------|------|-------|
| `--size-input-sm` | `32px` | Small inputs, buttons |
| `--size-input-md` | `40px` | Default inputs, buttons |
| `--size-input-lg` | `48px` | Large inputs, buttons |
| `--size-icon-sm` | `16px` | Small icons |
| `--size-icon-md` | `24px` | Default icons |
| `--size-icon-lg` | `32px` | Large icons |

### Container Sizes

| Token Name | Size | Usage |
|------------|------|-------|
| `--container-sm` | `640px` | Small content |
| `--container-md` | `768px` | Medium content |
| `--container-lg` | `1024px` | Large content |
| `--container-xl` | `1280px` | Extra large content |
| `--sidebar-width` | `200px` | Navigation sidebar width |

---

## Border Radius

| Token Name | Size | Usage |
|------------|------|-------|
| `--radius-sm` | `4px` | Small elements, inputs |
| `--radius-md` | `8px` | Default cards, buttons |
| `--radius-lg` | `12px` | Large cards, modals |
| `--radius-xl` | `16px` | Extra large elements |
| `--radius-full` | `9999px` | Circular elements, pills |

---

## Shadows

| Token Name | Value | Usage |
|------------|-------|-------|
| `--shadow-sm` | `0 1px 2px rgba(0, 0, 0, 0.05)` | Subtle elevation |
| `--shadow-md` | `0 4px 6px rgba(0, 0, 0, 0.1)` | Default cards |
| `--shadow-lg` | `0 10px 15px rgba(0, 0, 0, 0.1)` | Elevated cards |
| `--shadow-xl` | `0 20px 25px rgba(0, 0, 0, 0.15)` | Modals, dropdowns |

---

## Transitions

| Token Name | Value | Usage |
|------------|-------|-------|
| `--transition-fast` | `150ms ease-in-out` | Quick interactions |
| `--transition-base` | `250ms ease-in-out` | Default transitions |
| `--transition-slow` | `350ms ease-in-out` | Slower animations |

---

## Z-Index Scale

| Token Name | Value | Usage |
|------------|-------|-------|
| `--z-base` | `1` | Base level |
| `--z-dropdown` | `100` | Dropdowns |
| `--z-sticky` | `200` | Sticky elements |
| `--z-overlay` | `500` | Overlays |
| `--z-modal` | `1000` | Modals |
| `--z-toast` | `1500` | Toast notifications |
| `--z-tooltip` | `2000` | Tooltips |

---

## Component-Specific Tokens

### Buttons

| Token Name | Value | Usage |
|------------|-------|-------|
| `--btn-padding-sm` | `var(--space-2) var(--space-3)` | Small buttons |
| `--btn-padding-md` | `var(--space-3) var(--space-5)` | Default buttons |
| `--btn-padding-lg` | `var(--space-4) var(--space-6)` | Large buttons |

### Cards

| Token Name | Value | Usage |
|------------|-------|-------|
| `--card-padding` | `var(--space-6)` | Card internal padding |
| `--card-bg` | `var(--surface-card)` | Card background |
| `--card-border` | `1px solid var(--color-neutral-200)` | Card border |
| `--card-radius` | `var(--radius-md)` | Card border radius |
| `--card-shadow` | `var(--shadow-md)` | Card shadow |

### Forms

| Token Name | Value | Usage |
|------------|-------|-------|
| `--input-border` | `1px solid var(--color-neutral-300)` | Input border |
| `--input-border-focus` | `2px solid var(--color-primary)` | Focused input |
| `--input-bg` | `#FFFFFF` | Input background |
| `--input-padding` | `var(--space-3) var(--space-4)` | Input padding |

---

## Usage Guidelines

### Color Usage
- Use **slate** (`#555B6E`) for primary navigation and sidebar
- Use **rose** (`#DA4167`) for primary actions and important metrics
- Use **lavender** (`#D3C4E3`) for secondary information and soft backgrounds
- Use **sage** (`#87B38D`) for success states and positive indicators
- Use **gold** (`#FED766`) for warnings and highlights

### Spacing Consistency
- Use the spacing scale consistently throughout the application
- Maintain 4px base unit for all spacing decisions
- Use larger spacing (`--space-8` and above) between major sections

### Typography Hierarchy
- Maintain clear visual hierarchy with font sizes
- Use font weights to emphasize important information
- Ensure adequate line height for readability

### Accessibility
- Ensure color contrast ratios meet WCAG AA standards (4.5:1 for normal text)
- Use semantic colors appropriately (success, warning, danger)
- Don't rely solely on color to convey information

---

## Implementation

These design tokens are implemented in CSS custom properties in `prototypes/css/tokens.css` and can be referenced throughout the application using `var(--token-name)` syntax.

Example:
```css
.button-primary {
  background-color: var(--color-primary);
  color: white;
  padding: var(--btn-padding-md);
  border-radius: var(--radius-md);
  transition: var(--transition-base);
}
```

---

## References

- Color Palette: Coolors (5555B6E, DA4167, D3C4E3, 87B38D, FED766)
- UI Reference: Dashboard mockup
- Related Document: [Prototyping Strategy](03-prototyping-strategy.md)

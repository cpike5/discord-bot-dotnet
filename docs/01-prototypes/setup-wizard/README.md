# Setup Wizard HTML/CSS Prototypes

**Created:** 2025-10-20
**Purpose:** Static HTML prototypes for the First Time Setup Middleware UI

---

## Overview

These HTML prototypes demonstrate the user interface and flow for the first-time setup wizard. They are static HTML files that can be viewed in a browser to validate the design before implementation in Blazor.

## Files

1. **setup.css** - Shared stylesheet for all setup pages
2. **01-welcome.html** - Welcome/landing page
3. **02-admin.html** - Admin account creation form
4. **03-seed.html** - Database seeding progress page
5. **04-complete.html** - Setup completion success page

## Design System

The prototypes follow the Discord Bot design system:

- **Primary Pink:** #DA4167 (buttons, accents)
- **Dark Gray:** #555B6E (text, headers)
- **Light Purple:** #D3C4E3 (borders, secondary elements)
- **Green:** #87B38D (success states)

## Features Demonstrated

### Visual Design
- Gradient background (Dark Gray → Light Purple)
- Card-based layout with shadow
- Consistent header/footer structure
- Responsive design (mobile-friendly)

### User Interactions
- Form validation states
- Loading spinners
- Error/success message boxes
- Button hover effects
- Page transitions

### States Shown
- **Welcome:** Static content with checklist
- **Admin Form:** Input fields with validation examples
- **Seeding:** Loading state → Success state (simulated)
- **Complete:** Finalizing → Success animation

## How to View

Simply open any of the HTML files in a web browser:

```bash
# From the setup-wizard directory
start 01-welcome.html  # Windows
open 01-welcome.html   # macOS
xdg-open 01-welcome.html  # Linux
```

Or navigate through the flow starting from `01-welcome.html`.

## Implementation Notes

When converting to Blazor components:

1. **Layout:** Use `SetupLayout.razor` for the common structure
2. **Forms:** Replace with Blazor `<EditForm>` and data binding
3. **Validation:** Use `DataAnnotationsValidator` and `ValidationMessage`
4. **State Management:** Use component state and service calls
5. **Navigation:** Use `NavigationManager.NavigateTo()`
6. **Loading States:** Use `@if` conditionals to toggle display

## Next Steps

Once prototypes are approved:
1. Create `SetupLayout.razor` based on common HTML structure
2. Convert each HTML page to a Blazor `.razor` component
3. Integrate with `FirstTimeSetupService`
4. Add proper form submission and error handling
5. Test the complete workflow

---

**Status:** Ready for review

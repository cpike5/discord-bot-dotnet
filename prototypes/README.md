# UI Prototypes - Discord Bot Web Application

This directory contains HTML/CSS prototypes for the Discord bot web application. All components follow the design system defined in `docs/01-planning/04-design-standards.md`.

## 📁 Directory Structure

```
prototypes/
├── css/
│   └── tokens.css                    # Design system tokens (colors, spacing, typography)
├── components/                       # Reusable UI components
│   ├── buttons.html                  # Button variants and states
│   ├── forms.html                    # Form inputs and validation
│   ├── cards.html                    # Card components
│   ├── validation-messages.html      # Error/success messages
│   ├── tables.html                   # Static table examples
│   ├── tables-interactive.html       # ✨ Interactive sortable tables
│   ├── modals.html                   # Static modal examples
│   ├── modals-interactive.html       # ✨ Fully functional modals
│   ├── badges.html                   # Status badges
│   ├── tabs.html                     # Tab navigation
│   ├── sidebar.html                  # Navigation sidebar
│   ├── toasts.html                   # Toast notifications
│   ├── dropdowns.html                # Dropdown menus
│   ├── tooltips.html                 # Basic tooltips
│   ├── tooltips-enhanced.html        # ✨ Enhanced tooltips with sizes
│   ├── loading.html                  # Loading states
│   └── empty-state.html              # Empty states
├── pages/                            # Full-page prototypes
│   ├── login.html                    # ✅ Login page
│   ├── register.html                 # ✅ Registration page
│   ├── forgot-password.html          # ✅ Forgot password page
│   ├── reset-password.html           # ✅ Reset password page
│   ├── login-2fa.html                # ✅ Two-factor authentication login
│   ├── lockout.html                  # ✅ Account lockout page
│   ├── access-denied.html            # ✅ Access denied page
│   ├── manage/                       # Account management pages
│   │   ├── profile.html              # ✅ User profile settings
│   │   ├── email.html                # ✅ Email management
│   │   ├── change-password.html      # ✅ Change password
│   │   └── two-factor.html           # ✅ Two-factor auth settings
│   ├── admin-dashboard.html          # Admin dashboard (original)
│   ├── admin-dashboard-polished.html # ✨ Polished admin dashboard
│   ├── admin-users.html              # ✨ User management
│   ├── admin-roles.html              # ✨ Role management
│   ├── admin-invite-codes.html       # Invite management (original)
│   └── admin-invites-polished.html   # ✨ Polished invite management
└── design-tokens-demo.html           # Visual reference for design tokens
```

## 🎨 Design System

### Color Palette

Based on the Coolors palette:
- **Slate** (#555B6E) - Primary navigation, sidebar
- **Rose** (#DA4167) - Primary actions, CTAs
- **Lavender** (#D3C4E3) - Secondary information
- **Sage** (#87B38D) - Success states
- **Gold** (#FED766) - Warnings

### Typography

- **Font Family**: Inter (primary), Fira Code (monospace)
- **Sizes**: xs (12px) to 4xl (36px)
- **Weights**: Normal (400), Medium (500), Semibold (600), Bold (700)

### Spacing

Based on 4px base unit:
- `--space-1` (4px) to `--space-16` (64px)

## 📊 Component Priority

### HIGH Priority (Core Features)
✅ **Completed**
- [buttons.html](components/buttons.html) - All button variants
- [forms.html](components/forms.html) - Form inputs with validation
- [cards.html](components/cards.html) - Card components
- [validation-messages.html](components/validation-messages.html) - Feedback messages

### MEDIUM Priority (Admin Features)
✅ **Completed**
- [tables-interactive.html](components/tables-interactive.html) - Sortable tables with pagination
- [modals-interactive.html](components/modals-interactive.html) - Functional modal dialogs
- [badges.html](components/badges.html) - Status indicators
- [tabs.html](components/tabs.html) - Tab navigation
- [sidebar.html](components/sidebar.html) - Navigation sidebar

### LOW Priority (Enhancements)
✅ **Completed**
- [tooltips-enhanced.html](components/tooltips-enhanced.html) - Enhanced tooltips with size variants
- [toasts.html](components/toasts.html) - Toast notifications
- [dropdowns.html](components/dropdowns.html) - Dropdown menus
- [loading.html](components/loading.html) - Loading states
- [empty-state.html](components/empty-state.html) - Empty state displays

## 📄 Pages

### Authentication Pages

All authentication pages use consistent styling with the dark slate-to-gray gradient background and follow the design token system.

- **[login.html](pages/login.html)** - User login page
  - Email/password form with validation
  - Remember me checkbox
  - Links to register, forgot password, and resend confirmation
  - External login support

- **[register.html](pages/register.html)** - User registration with invite code
  - Invite code validation (XXXX-XXXX-XXXX format)
  - Email and password fields
  - Real-time password strength checking
  - Confirm password validation

- **[forgot-password.html](pages/forgot-password.html)** - Password recovery
  - Email input for reset link
  - Informational message about the process
  - Link back to login

- **[reset-password.html](pages/reset-password.html)** - Reset password with token
  - Email and new password fields
  - Password strength indicator
  - Confirm password validation
  - Helper text for requirements

- **[login-2fa.html](pages/login-2fa.html)** - Two-factor authentication
  - 6-digit authenticator code input
  - Remember this machine option
  - Link to recovery code login
  - Monospace font for code input

- **[lockout.html](pages/lockout.html)** - Account locked page
  - Error icon and message
  - Information about why account was locked
  - Links to reset password or return to login
  - Contact support option

- **[access-denied.html](pages/access-denied.html)** - Access denied page
  - Permission error message
  - Explanation of the issue
  - Links to home or sign in
  - Contact support option

### Account Management Pages

Account management pages use a sidebar navigation layout with consistent styling across all pages.

- **[manage/profile.html](pages/manage/profile.html)** - User profile settings
  - Username display (disabled/read-only)
  - Phone number field
  - Success message feedback
  - Sidebar navigation to other settings

- **[manage/email.html](pages/manage/email.html)** - Email management
  - Current email with verification status
  - New email input
  - Send verification email option
  - Confirmation message handling

- **[manage/change-password.html](pages/manage/change-password.html)** - Change password
  - Current password verification
  - New password with strength indicator
  - Confirm new password
  - Success/error message feedback

- **[manage/two-factor.html](pages/manage/two-factor.html)** - Two-factor authentication settings
  - 2FA enabled/disabled status badge
  - Recovery codes warning
  - Authenticator app setup
  - Disable 2FA and reset options
  - Alternative state for when 2FA is disabled (commented)

### Admin Pages

All admin pages feature a consistent sidebar navigation with the slate theme, professional metrics display, and interactive functionality.

- **[admin-dashboard-polished.html](pages/admin-dashboard-polished.html)** - Main admin dashboard
  - Professional sidebar navigation
  - Metric cards with trends
  - Quick actions section
  - Recent activity feed
  - Fully responsive design

- **[admin-users.html](pages/admin-users.html)** - User management
  - Search users by name, email, or Discord ID
  - Filter by role and status
  - User statistics (total, active, locked)
  - Lock/unlock user accounts
  - Edit user information
  - User avatar display with initials

- **[admin-roles.html](pages/admin-roles.html)** - Role & permissions management
  - Card-based role display
  - System roles (Administrator, User)
  - Custom role creation
  - Permission management
  - User count per role
  - Edit and delete custom roles

- **[admin-invites-polished.html](pages/admin-invites-polished.html)** - Invite code management
  - Comprehensive invite code management
  - Search and filtering by status
  - Stats summary (total, active, used, expired)
  - Interactive table with actions
  - Functional modals (Generate & Revoke)
  - Bulk operations support

#### Original/Basic Versions
- [admin-dashboard.html](pages/admin-dashboard.html) - Basic dashboard
- [admin-invite-codes.html](pages/admin-invite-codes.html) - Basic invite management

## 🚀 Getting Started

### View Prototypes

1. **Open any HTML file in a browser**
   ```bash
   # Windows
   start prototypes/pages/admin-dashboard-polished.html

   # Mac/Linux
   open prototypes/pages/admin-dashboard-polished.html
   ```

2. **Recommended viewing order**:
   - Start with `design-tokens-demo.html` to see the design system
   - View `pages/admin-dashboard-polished.html` for the full experience
   - Explore individual components in the `components/` folder

### Interactive Features

Several components include JavaScript functionality:

- **Tables** - Sortable columns, clickable action buttons
- **Modals** - Open/close with buttons, ESC key, or clicking outside
- **Forms** - Real-time validation on registration page
- **Dropdowns** - Click to open/close menus
- **Tooltips** - Hover to reveal information

## 🎯 Key Features

### Responsive Design
All components are mobile-first and responsive:
- Sidebars collapse on mobile
- Tables scroll horizontally when needed
- Forms adapt to smaller screens

### Accessibility
- Semantic HTML elements
- ARIA labels where appropriate
- Keyboard navigation support
- Focus states on interactive elements

### Design Consistency
- All components use CSS custom properties from `tokens.css`
- Consistent spacing, colors, and typography
- Reusable component patterns

## 📝 Implementation Notes

### For Blazor Development

When converting these prototypes to Blazor components:

1. **Extract CSS to component styles**
   - Use `tokens.css` as the global stylesheet
   - Component-specific styles can be scoped

2. **Component structure**
   - HTML structure can be directly copied
   - Replace static data with `@foreach` loops
   - Add `@onclick` handlers for buttons

3. **Form validation**
   - Current prototypes show visual states
   - Integrate with Blazor's `EditForm` and validation

4. **Interactive features**
   - Replace JavaScript with Blazor event handlers
   - Use `@bind` for form inputs
   - Leverage Blazor's component model for modals/dropdowns

### Token Usage Examples

```css
/* Use design tokens in your CSS */
.my-component {
  padding: var(--space-4);
  background: var(--color-primary);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-md);
  transition: var(--transition-base);
}

.my-text {
  font-size: var(--text-base);
  font-weight: var(--font-semibold);
  color: var(--text-primary);
}
```

## 🔧 Customization

### Changing Colors

Edit `css/tokens.css` to update the color palette:

```css
:root {
  --color-primary: #DA4167;  /* Change this to your brand color */
  --color-success: #87B38D;
  /* etc. */
}
```

All components will automatically use the new colors.

### Adding New Components

1. Create a new HTML file in `components/`
2. Link to `../css/tokens.css`
3. Use existing tokens for consistency
4. Follow the established patterns from other components

## ✅ Recent Updates

### 2025-10-18 - Admin Management Pages Complete
- ✅ **Admin User Management**: Created comprehensive user management interface
  - User search and filtering by role/status
  - User statistics dashboard
  - Lock/unlock account functionality
  - User avatar display with initials
- ✅ **Admin Role Management**: Created role and permissions interface
  - Card-based role display
  - System roles vs custom roles
  - Permission management per role
  - User count tracking
- ✅ **Design Consistency**: All admin pages share sidebar navigation and design tokens

### Earlier 2025-10-18 - Identity Pages Complete
- ✅ **Authentication Pages**: Created complete set of ASP.NET Identity page prototypes
  - Forgot Password page with email validation
  - Reset Password page with strength indicator
  - Two-Factor Authentication login page
  - Account Lockout error page
  - Access Denied error page
- ✅ **Account Management Pages**: Created full account settings section
  - Profile management with sidebar navigation
  - Email management with verification status
  - Change Password with strength checking
  - Two-Factor Authentication settings
- ✅ **Design Consistency**: All pages follow design token system with slate-to-gray gradient background

### Earlier 2025-10-18
- ✅ **Tooltips**: Fixed white text visibility on dark background
- ✅ **Forms**: Added missing CSS tokens (`--input-bg-disabled`, `--input-radius`, etc.)
- ✅ **Login/Register**: Fixed padding issues with proper box-sizing reset
- ✅ **Admin Pages**: Created polished versions with professional design
- ✅ **Tables**: Created interactive version with sorting and pagination
- ✅ **Modals**: Created interactive version with full functionality
- ✅ **Tooltips**: Enhanced version with size variants (sm, md, lg)

## 📚 References

- **Design Standards**: `docs/01-planning/04-design-standards.md`
- **Prototyping Strategy**: `docs/01-planning/03-prototyping-strategy.md`
- **Functional Specs**: `docs/01-planning/02-functional-specs.md`
- **Product Requirements**: `docs/01-planning/01-product-requirements.md`

## 🎨 Design Token Reference

View the complete design token reference at:
- [design-tokens-demo.html](design-tokens-demo.html)

This interactive page shows all available:
- Colors (primary palette, semantic colors, neutral scale)
- Typography (sizes, weights)
- Spacing scale
- Border radius options
- Shadow levels

---

**Status**: ✅ All prototypes complete and ready for Blazor implementation

**Last Updated**: 2025-10-18

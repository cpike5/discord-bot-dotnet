# Account Pages Restyling Implementation Plan

**Version:** 1.0
**Last Updated:** 2025-10-20
**Status:** Active
**Estimated Duration:** 2-3 weeks
**Priority:** Medium

---

## Overview

This document outlines the implementation plan for restyling the `/Account` Blazor pages to match the custom design system established in the HTML prototypes. The goal is to transform the existing Bootstrap-based Account pages into a modern, cohesive interface that follows the design standards and provides an enhanced user experience.

**Key Objectives:**
- Replace Bootstrap styling with custom design token system
- Implement card-based layouts matching prototypes
- Enhance visual hierarchy and user experience
- Maintain responsive design across all breakpoints
- Preserve all existing functionality (no breaking changes)

---

## Table of Contents

- [1. Project Context](#1-project-context)
- [2. Current vs Target State](#2-current-vs-target-state)
- [3. Implementation Phases](#3-implementation-phases)
- [4. Detailed Implementation Tasks](#4-detailed-implementation-tasks)
- [5. File Structure](#5-file-structure)
- [6. Design Token Reference](#6-design-token-reference)
- [7. Testing Strategy](#7-testing-strategy)
- [8. References](#8-references)

---

## 1. Project Context

### 1.1 Design System

The application uses a custom design system based on the Coolors palette:

| Token | Value | Usage |
|-------|-------|-------|
| **Primary Color** | Rose (#DA4167) | Primary actions, CTAs, active states |
| **Secondary Color** | Lavender (#D3C4E3) | Secondary information, soft backgrounds |
| **Success Color** | Sage (#87B38D) | Success states, positive indicators |
| **Warning Color** | Gold (#FED766) | Warnings, highlights |
| **Base Color** | Slate (#555B6E) | Navigation, sidebar, primary UI |

**Typography:** Inter font family
**Spacing:** 4px base unit system
**Design Tokens:** See [Design Standards](../01-planning/04-design-standards.md)

### 1.2 Reference Prototypes

Complete HTML prototypes exist for all account management pages:

**✅ Available Prototypes:**
- `prototypes/pages/manage/profile.html` - Profile management
- `prototypes/pages/manage/email.html` - Email management with verification
- `prototypes/pages/manage/change-password.html` - Password change with validation
- `prototypes/pages/manage/two-factor.html` - 2FA configuration
- `prototypes/css/tokens.css` - Complete design token system

**Design Patterns Demonstrated:**
- Sidebar navigation with active states
- Content cards with headers and borders
- Form controls with labels, inputs, and helper text
- Alert messages with icons and semantic colors
- Button styles and groups
- Responsive layouts (220px sidebar → stacked on mobile)

### 1.3 Existing Implementation

**Current State:**
- Account pages use default Bootstrap classes (form-floating, nav-pills, btn-lg)
- Basic Bootstrap grid layout (col-md-3, col-md-9)
- StatusMessage component uses Bootstrap alerts
- Minimal custom styling

**Pages to Restyle:**
- `Components/Account/Shared/ManageLayout.razor`
- `Components/Account/Shared/ManageNavMenu.razor`
- `Components/Account/Shared/StatusMessage.razor`
- `Components/Account/Pages/Manage/Index.razor` (Profile)
- `Components/Account/Pages/Manage/Email.razor`
- `Components/Account/Pages/Manage/ChangePassword.razor`
- `Components/Account/Pages/Manage/TwoFactorAuthentication.razor`
- Additional manage and auth pages (12+ additional pages)

---

## 2. Current vs Target State

### 2.1 Current Implementation

**Layout Structure:**
```razor
<!-- ManageLayout.razor - Current -->
<h1>Manage your account</h1>
<div>
    <h2>Change your account settings</h2>
    <hr />
    <div class="row">
        <div class="col-md-3">
            <ManageNavMenu />
        </div>
        <div class="col-md-9">
            @Body
        </div>
    </div>
</div>
```

**Navigation - Current:**
```razor
<!-- ManageNavMenu.razor - Current -->
<ul class="nav nav-pills flex-column">
    <li class="nav-item">
        <NavLink class="nav-link" href="Account/Manage">Profile</NavLink>
    </li>
    <!-- ... -->
</ul>
```

**Status Messages - Current:**
```razor
<!-- StatusMessage.razor - Current -->
<div class="alert alert-@statusMessageClass" role="alert">
    @DisplayMessage
</div>
```

### 2.2 Target Implementation

**Layout Structure - Target:**
```html
<!-- Based on prototypes/pages/manage/profile.html -->
<div class="container">
    <div class="page-header">
        <h1>Account Settings</h1>
        <p>Manage your profile and account preferences</p>
    </div>

    <div class="manage-layout">
        <aside class="sidebar">
            <nav>
                <ul class="sidebar-nav"><!-- Navigation --></ul>
            </nav>
        </aside>
        <main>
            @Body
        </main>
    </div>
</div>
```

**Navigation - Target:**
```html
<!-- Custom sidebar navigation with tokens -->
<ul class="sidebar-nav">
    <li>
        <a href="#" class="active">Profile</a>
    </li>
</ul>

<!-- CSS using design tokens -->
.sidebar-nav a.active {
    background: var(--color-primary);
    color: var(--text-on-primary);
    font-weight: var(--font-semibold);
}
```

**Status Messages - Target:**
```html
<!-- Custom alert with icons -->
<div class="alert success">
    <!-- Icon added via ::before -->
    Your profile has been updated
</div>

<!-- CSS using design tokens -->
.alert.success {
    background-color: rgba(135, 179, 141, 0.1);
    border: 1px solid var(--color-success);
    color: var(--color-success);
}
.alert.success::before {
    content: "✓";
}
```

---

## 3. Implementation Phases

### Phase 1: Foundation & Infrastructure (2-3 days)
- Set up CSS files and design token system
- Create reusable stylesheet for account pages
- Ensure proper file references in layout

### Phase 2: Layout Components (3-4 days)
- Restyle ManageLayout.razor
- Restyle ManageNavMenu.razor
- Restyle StatusMessage.razor
- Test responsive behavior

### Phase 3: Core Manage Pages (4-5 days)
- Restyle Index.razor (Profile)
- Restyle Email.razor
- Restyle ChangePassword.razor
- Restyle TwoFactorAuthentication.razor

### Phase 4: Remaining Pages (3-4 days)
- Complete all manage pages (PersonalData, DeletePersonalData, etc.)
- Restyle authentication pages (Login, Register, etc.)
- Update AccountLayout.razor if needed

### Phase 5: Quality & Polish (2-3 days)
- Cross-browser testing
- Responsive testing (mobile, tablet, desktop)
- Accessibility audit
- Performance check
- Documentation

**Total Estimated Duration:** 14-19 days (2-3 weeks)

---

## 4. Detailed Implementation Tasks

### 4.1 Phase 1: Foundation & Infrastructure

#### Task 1.1: Copy Design Tokens
**File:** `wwwroot/css/tokens.css`

**Actions:**
1. Copy `prototypes/css/tokens.css` to `wwwroot/css/tokens.css`
2. Verify all CSS custom properties are present
3. No modifications needed - use as-is

**Acceptance Criteria:**
- [ ] tokens.css file exists in wwwroot/css/
- [ ] All design tokens from prototype are present
- [ ] File is properly formatted and documented

#### Task 1.2: Create Account Pages Stylesheet
**File:** `wwwroot/css/account-pages.css`

**Actions:**
1. Extract common styles from prototypes:
   - `.container`, `.page-header` styles
   - `.manage-layout` grid system
   - `.sidebar`, `.sidebar-nav` styles
   - `.content-card`, `.content-header` styles
   - `.form-group`, `.form-label`, `.form-input` styles
   - `.btn` variations (primary, secondary, danger, link)
   - `.alert` variations (success, error, warning, info)
   - `.helper-text`, `.error-message` styles
   - Responsive media queries

2. Organize with clear section comments
3. Use design tokens exclusively (no hardcoded values)

**Reference Files:**
- `prototypes/pages/manage/profile.html` (lines 8-213)
- `prototypes/pages/manage/email.html` (lines 8-265)
- `prototypes/pages/manage/change-password.html` (lines 8-283)
- `prototypes/pages/manage/two-factor.html` (lines 8-289)

**Acceptance Criteria:**
- [ ] account-pages.css created with all common styles
- [ ] All styles use design tokens (no hardcoded colors/sizes)
- [ ] Clear section comments for organization
- [ ] Responsive breakpoints included
- [ ] Mobile-first approach maintained

#### Task 1.3: Update Layout References
**Files:**
- `Components/Account/Shared/AccountLayout.razor`
- `Components/Account/Shared/ManageLayout.razor`

**Actions:**
1. Add CSS references to AccountLayout.razor head section:
   ```html
   <link rel="stylesheet" href="css/tokens.css" />
   <link rel="stylesheet" href="css/account-pages.css" />
   ```

2. Verify CSS loads properly
3. Ensure proper cascade order (tokens → account-pages)

**Acceptance Criteria:**
- [ ] CSS files properly referenced
- [ ] Styles load in correct order
- [ ] No console errors
- [ ] Design tokens accessible via var()

---

### 4.2 Phase 2: Layout Components

#### Task 2.1: Restyle ManageLayout.razor
**File:** `Components/Account/Shared/ManageLayout.razor`

**Current Structure:**
```razor
<h1>Manage your account</h1>
<div>
    <h2>Change your account settings</h2>
    <hr />
    <div class="row">
        <div class="col-md-3">
            <ManageNavMenu />
        </div>
        <div class="col-md-9">
            @Body
        </div>
    </div>
</div>
```

**Target Structure:**
```razor
@inherits LayoutComponentBase
@layout AccountLayout

<div class="container">
    <div class="page-header">
        <h1>Account Settings</h1>
        <p>Manage your profile and account preferences</p>
    </div>

    <div class="manage-layout">
        <aside class="sidebar">
            <nav>
                <ManageNavMenu />
            </nav>
        </aside>
        <main>
            @Body
        </main>
    </div>
</div>
```

**CSS Required:**
```css
/* Already in account-pages.css */
.container {
    max-width: var(--container-lg);
    margin: 0 auto;
    padding: var(--space-6);
}

.page-header {
    margin-bottom: var(--space-8);
}

.page-header h1 {
    font-size: var(--text-3xl);
    font-weight: var(--font-bold);
    color: var(--text-primary);
    margin-bottom: var(--space-2);
}

.page-header p {
    font-size: var(--text-base);
    color: var(--text-secondary);
}

.manage-layout {
    display: grid;
    grid-template-columns: 220px 1fr;
    gap: var(--space-8);
}

@media (max-width: 768px) {
    .manage-layout {
        grid-template-columns: 1fr;
    }
}
```

**Reference:** `prototypes/pages/manage/profile.html` (lines 216-222, 42-46)

**Acceptance Criteria:**
- [ ] Bootstrap row/col removed
- [ ] Custom grid layout implemented
- [ ] Page header added with title and description
- [ ] Semantic HTML (aside, nav, main) used
- [ ] Responsive on mobile (sidebar stacks above content)

#### Task 2.2: Restyle ManageNavMenu.razor
**File:** `Components/Account/Shared/ManageNavMenu.razor`

**Current Structure:**
```razor
<ul class="nav nav-pills flex-column">
    <li class="nav-item">
        <NavLink class="nav-link" href="Account/Manage" Match="NavLinkMatch.All">
            Profile
        </NavLink>
    </li>
    <!-- ... more items ... -->
</ul>
```

**Target Structure:**
```razor
@using Microsoft.AspNetCore.Identity
@using DiscordBot.Blazor.Data

@inject SignInManager<ApplicationUser> SignInManager

<ul class="sidebar-nav">
    <li>
        <NavLink href="Account/Manage" Match="NavLinkMatch.All">
            Profile
        </NavLink>
    </li>
    <li>
        <NavLink href="Account/Manage/Email">Email</NavLink>
    </li>
    <li>
        <NavLink href="Account/Manage/ChangePassword">Password</NavLink>
    </li>
    @if (hasExternalLogins)
    {
        <li>
            <NavLink href="Account/Manage/ExternalLogins">External logins</NavLink>
        </li>
    }
    <li>
        <NavLink href="Account/Manage/TwoFactorAuthentication">Two-Factor Auth</NavLink>
    </li>
    <li>
        <NavLink href="Account/Manage/PersonalData">Personal data</NavLink>
    </li>
</ul>

@code {
    private bool hasExternalLogins;

    protected override async Task OnInitializedAsync()
    {
        hasExternalLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync()).Any();
    }
}
```

**CSS Required:**
```css
/* Already in account-pages.css */
.sidebar {
    background: var(--surface-card);
    border-radius: var(--radius-md);
    padding: var(--space-4);
    box-shadow: var(--shadow-sm);
    height: fit-content;
}

.sidebar-nav {
    list-style: none;
    margin: 0;
    padding: 0;
}

.sidebar-nav li {
    margin-bottom: var(--space-1);
}

.sidebar-nav a {
    display: block;
    padding: var(--space-3) var(--space-4);
    color: var(--text-primary);
    text-decoration: none;
    font-size: var(--text-sm);
    border-radius: var(--radius-sm);
    transition: var(--transition-fast);
}

.sidebar-nav a:hover {
    background: var(--color-neutral-50);
}

.sidebar-nav a.active {
    background: var(--color-primary);
    color: var(--text-on-primary);
    font-weight: var(--font-semibold);
}

@media (max-width: 768px) {
    .sidebar {
        margin-bottom: var(--space-6);
    }

    .sidebar-nav {
        display: flex;
        flex-wrap: wrap;
        gap: var(--space-2);
    }

    .sidebar-nav li {
        margin: 0;
    }
}
```

**Reference:** `prototypes/pages/manage/profile.html` (lines 48-82, 199-212)

**Acceptance Criteria:**
- [ ] Bootstrap nav classes removed
- [ ] Custom sidebar-nav styling applied
- [ ] Active state shows rose background
- [ ] Hover states work correctly
- [ ] Mobile responsive (horizontal layout)
- [ ] NavLink active class maps to .active CSS

#### Task 2.3: Restyle StatusMessage.razor
**File:** `Components/Account/Shared/StatusMessage.razor`

**Current Structure:**
```razor
@if (!string.IsNullOrEmpty(DisplayMessage))
{
    var statusMessageClass = DisplayMessage.StartsWith("Error") ? "danger" : "success";
    <div class="alert alert-@statusMessageClass" role="alert">
        @DisplayMessage
    </div>
}
```

**Target Structure:**
```razor
@if (!string.IsNullOrEmpty(DisplayMessage))
{
    var statusClass = GetStatusClass(DisplayMessage);
    <div class="alert @statusClass" role="alert">
        @DisplayMessage
    </div>
}

@code {
    private string? messageFromCookie;

    [Parameter]
    public string? Message { get; set; }

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    private string? DisplayMessage => Message ?? messageFromCookie;

    protected override void OnInitialized()
    {
        messageFromCookie = HttpContext.Request.Cookies[IdentityRedirectManager.StatusCookieName];

        if (messageFromCookie is not null)
        {
            HttpContext.Response.Cookies.Delete(IdentityRedirectManager.StatusCookieName);
        }
    }

    private string GetStatusClass(string message)
    {
        if (message.StartsWith("Error", StringComparison.OrdinalIgnoreCase))
            return "error";
        if (message.Contains("warning", StringComparison.OrdinalIgnoreCase))
            return "warning";
        if (message.Contains("info", StringComparison.OrdinalIgnoreCase))
            return "info";
        return "success";
    }
}
```

**CSS Required:**
```css
/* Already in account-pages.css */
.alert {
    padding: var(--space-4);
    border-radius: var(--radius-sm);
    margin-bottom: var(--space-6);
    font-size: var(--text-sm);
    display: flex;
    align-items: center;
    gap: var(--space-2);
}

.alert::before {
    font-size: var(--text-lg);
    font-weight: var(--font-bold);
}

.alert.success {
    background-color: rgba(135, 179, 141, 0.1);
    border: 1px solid var(--color-success);
    color: var(--color-success);
}

.alert.success::before {
    content: "✓";
}

.alert.error {
    background-color: rgba(211, 47, 81, 0.1);
    border: 1px solid var(--color-danger);
    color: var(--color-danger);
}

.alert.error::before {
    content: "⚠";
}

.alert.warning {
    background-color: rgba(254, 215, 102, 0.1);
    border: 1px solid var(--color-warning);
    color: #a68600;
}

.alert.warning::before {
    content: "⚠";
}

.alert.info {
    background-color: rgba(211, 196, 227, 0.1);
    border: 1px solid var(--color-info);
    color: var(--color-neutral-700);
}

.alert.info::before {
    content: "ℹ";
}
```

**Reference:** `prototypes/pages/manage/profile.html` (lines 104-124)

**Acceptance Criteria:**
- [ ] Bootstrap alert-danger/alert-success classes removed
- [ ] Custom alert styling with icons
- [ ] Success, error, warning, info states supported
- [ ] Icons display via CSS ::before
- [ ] Proper color contrast for accessibility

---

### 4.3 Phase 3: Core Manage Pages

#### Task 3.1: Restyle Index.razor (Profile)
**File:** `Components/Account/Pages/Manage/Index.razor`

**Changes Required:**
1. Wrap form in `.content-card`
2. Add `.content-header` section
3. Replace Bootstrap form classes:
   - Remove `col-md-6`, `row` wrappers
   - Remove `form-floating`, `mb-3`
   - Remove `w-100`, `btn-lg`
4. Apply custom form classes:
   - `.form-group` wrappers
   - `.form-label` for labels
   - `.form-input` for inputs
   - `.helper-text` for descriptions
   - `.btn.btn-primary` for submit button

**Target Structure:**
```razor
@page "/Account/Manage"

@using System.ComponentModel.DataAnnotations
@using Microsoft.AspNetCore.Identity
@using DiscordBot.Blazor.Data

@inject UserManager<ApplicationUser> UserManager
@inject SignInManager<ApplicationUser> SignInManager
@inject IdentityUserAccessor UserAccessor
@inject IdentityRedirectManager RedirectManager

<PageTitle>Profile</PageTitle>

<div class="content-card">
    <div class="content-header">
        <h2>Profile</h2>
    </div>

    <StatusMessage />

    <EditForm Model="Input" FormName="profile" OnValidSubmit="OnValidSubmitAsync" method="post">
        <DataAnnotationsValidator />
        <ValidationSummary class="text-danger" role="alert" />

        <div class="form-group">
            <label for="username" class="form-label">Username</label>
            <input type="text"
                   id="username"
                   value="@username"
                   class="form-input"
                   disabled />
            <div class="helper-text">
                Your username cannot be changed
            </div>
        </div>

        <div class="form-group">
            <label for="phone-number" class="form-label">Phone Number</label>
            <InputText @bind-Value="Input.PhoneNumber"
                       id="phone-number"
                       class="form-input"
                       placeholder="Please enter your phone number." />
            <ValidationMessage For="() => Input.PhoneNumber" class="text-danger" />
            <div class="helper-text">
                Optional. Used for account recovery
            </div>
        </div>

        <button type="submit" class="btn btn-primary">Save Changes</button>
    </EditForm>
</div>

@code {
    // ... existing code unchanged ...
}
```

**Reference:** `prototypes/pages/manage/profile.html` (lines 236-278)

**Acceptance Criteria:**
- [ ] Content wrapped in content-card
- [ ] Content-header with h2 title
- [ ] Bootstrap form classes removed
- [ ] Custom form-group, form-label, form-input applied
- [ ] Helper text added for fields
- [ ] Button styled with btn btn-primary
- [ ] Disabled input styled correctly
- [ ] Form validation still works

#### Task 3.2: Restyle Email.razor
**File:** `Components/Account/Pages/Manage/Email.razor`

**Key Changes:**
1. Wrap in `.content-card` with `.content-header`
2. Add verified email indicator (`.input-group` with checkmark)
3. Style inputs with `.form-input`
4. Add `.helper-text` for descriptions
5. Add `.btn-link` style for resend verification option

**Special Features:**
- Input group showing verified status with green checkmark
- Conditional display for verified vs unverified state
- Link button for resending verification

**Reference:** `prototypes/pages/manage/email.html` (lines 288-333)

**Acceptance Criteria:**
- [ ] Content card structure applied
- [ ] Verified email shows with checkmark indicator
- [ ] Input group styling for verified field
- [ ] Helper text explains verification process
- [ ] Resend verification link styled as btn-link
- [ ] Form styling consistent with design system

#### Task 3.3: Restyle ChangePassword.razor
**File:** `Components/Account/Pages/Manage/ChangePassword.razor`

**Key Changes:**
1. Wrap in `.content-card` with `.content-header`
2. Add required field indicators (`.form-label.required`)
3. Style validation errors (`.error-message`, `.form-input.error`)
4. Consider adding password strength indicator (optional)
5. Apply button styling

**Special Features:**
- Required field asterisks
- Error state styling for inputs
- Error messages shown below fields
- Password strength indicator (optional enhancement)

**Reference:** `prototypes/pages/manage/change-password.html` (lines 306-380)

**Acceptance Criteria:**
- [ ] Content card structure applied
- [ ] Required labels show red asterisk
- [ ] Error states styled for inputs
- [ ] Error messages display below fields
- [ ] Password strength indicator (if implemented)
- [ ] Form validation works correctly
- [ ] Button styling applied

#### Task 3.4: Restyle TwoFactorAuthentication.razor
**File:** `Components/Account/Pages/Manage/TwoFactorAuthentication.razor`

**Key Changes:**
1. Add status badges (`.badge.enabled` / `.badge.disabled`)
2. Use `.status-card` for current state display
3. Apply `.btn-group` for multiple actions
4. Style different button types (primary, secondary, danger)
5. Use appropriate alerts for warnings

**Special Features:**
- Badge showing 2FA enabled/disabled status
- Status card with border accent
- Button groups for related actions
- Warning alerts for low recovery codes

**Reference:** `prototypes/pages/manage/two-factor.html` (lines 313-333)

**Acceptance Criteria:**
- [ ] Badge component shows status
- [ ] Status card displays current 2FA state
- [ ] Button group layouts actions
- [ ] Primary, secondary, danger buttons styled
- [ ] Warning alerts for recovery codes
- [ ] Conditional rendering works

---

### 4.4 Phase 4: Remaining Pages

#### Task 4.1: Complete Remaining Manage Pages

Apply similar styling patterns to:

**Pages to Complete:**
1. `PersonalData.razor` - Simple content card with description and download button
2. `DeletePersonalData.razor` - Warning card, password confirmation, danger button
3. `Disable2fa.razor` - Warning alert, confirmation button
4. `EnableAuthenticator.razor` - Multi-step with QR code, input for verification
5. `ExternalLogins.razor` - Table/list of linked accounts, add/remove buttons
6. `GenerateRecoveryCodes.razor` - Code display area, warning messages
7. `ResetAuthenticator.razor` - Warning card, confirmation button
8. `SetPassword.razor` - Form similar to ChangePassword

**Common Patterns:**
- All use `.content-card` wrapper
- All have `.content-header` with h2 title
- Forms use `.form-group`, `.form-label`, `.form-input`
- Buttons use `.btn` with appropriate variant
- Alerts use custom `.alert` styles
- Helper text uses `.helper-text`

**Acceptance Criteria:**
- [ ] All manage pages styled consistently
- [ ] Forms follow common patterns
- [ ] Buttons styled appropriately for context
- [ ] Alerts and warnings displayed correctly
- [ ] All functionality preserved

#### Task 4.2: Restyle Authentication Pages

Apply styling to auth pages (these may use centered card layout):

**Pages:**
1. `Login.razor` - Centered auth card
2. `Register.razor` - Multi-step or single card
3. `ForgotPassword.razor` - Simple centered card
4. `ResetPassword.razor` - Form with password strength
5. `Lockout.razor` - Info card
6. `AccessDenied.razor` - Error card
7. `LoginWith2fa.razor` - Code input form
8. `LoginWithRecoveryCode.razor` - Code input form

**Note:** Auth pages may have different layout (centered card) vs manage pages (sidebar layout)

**Acceptance Criteria:**
- [ ] All auth pages styled
- [ ] Centered card layout for login/register
- [ ] Consistent form styling
- [ ] Error states handled
- [ ] Responsive on all devices

---

### 4.5 Phase 5: Quality & Polish

#### Task 5.1: Cross-Browser Testing

**Browsers to Test:**
- Chrome/Edge (latest)
- Firefox (latest)
- Safari (latest, if available)

**Test Cases:**
- [ ] CSS custom properties work correctly
- [ ] Grid layouts render properly
- [ ] Transitions and hover effects work
- [ ] Form validation displays correctly
- [ ] Buttons and inputs styled correctly

#### Task 5.2: Responsive Testing

**Breakpoints to Test:**
- Mobile (< 768px)
- Tablet (768px - 1024px)
- Desktop (> 1024px)

**Test Cases:**
- [ ] Sidebar navigation stacks on mobile
- [ ] Forms are usable on small screens
- [ ] Buttons are appropriately sized
- [ ] Touch targets meet minimum size (44px)
- [ ] No horizontal scrolling
- [ ] Content readable at all sizes

#### Task 5.3: Accessibility Audit

**WCAG AA Requirements:**
- [ ] Color contrast ratios meet 4.5:1 for normal text
- [ ] Color contrast ratios meet 3:1 for large text
- [ ] Form labels properly associated with inputs
- [ ] Error messages announced to screen readers
- [ ] Focus states visible and clear
- [ ] Semantic HTML used appropriately
- [ ] ARIA attributes added where needed

**Tools:**
- Browser DevTools accessibility checker
- axe DevTools extension
- Manual keyboard navigation testing

#### Task 5.4: Performance Check

**Metrics:**
- [ ] CSS file sizes reasonable (< 50KB total)
- [ ] No unused CSS
- [ ] No layout shifts (CLS)
- [ ] Fast paint times

**Optimization:**
- Consider minification for production
- Ensure design tokens cached properly

#### Task 5.5: Documentation

**Documentation to Update:**
1. Add code comments to account-pages.css explaining sections
2. Document any deviations from prototypes
3. Create visual regression test screenshots (optional)
4. Update developer onboarding docs with styling guidelines

**Acceptance Criteria:**
- [ ] CSS well-commented
- [ ] Deviations documented
- [ ] Guidelines available for future pages

---

## 5. File Structure

### 5.1 New Files Created

```
wwwroot/
└── css/
    ├── tokens.css               # Design token system (copied from prototypes)
    └── account-pages.css        # Account-specific styles

docs/
└── 03-implementation-plans/
    └── 03-account-pages-restyling.md  # This document
```

### 5.2 Files Modified

```
Components/Account/
├── Shared/
│   ├── AccountLayout.razor      # Add CSS references
│   ├── ManageLayout.razor       # Complete restructure
│   ├── ManageNavMenu.razor      # Restyle navigation
│   └── StatusMessage.razor      # Custom alert styling
└── Pages/
    ├── Manage/
    │   ├── Index.razor          # Profile page restyle
    │   ├── Email.razor          # Email management restyle
    │   ├── ChangePassword.razor # Password change restyle
    │   ├── TwoFactorAuthentication.razor  # 2FA restyle
    │   ├── PersonalData.razor   # Simple restyle
    │   ├── DeletePersonalData.razor  # Warning card restyle
    │   ├── Disable2fa.razor     # Confirmation restyle
    │   ├── EnableAuthenticator.razor  # Multi-step restyle
    │   ├── ExternalLogins.razor # List/table restyle
    │   ├── GenerateRecoveryCodes.razor  # Code display restyle
    │   ├── ResetAuthenticator.razor  # Warning restyle
    │   └── SetPassword.razor    # Form restyle
    ├── Login.razor              # Auth page restyle
    ├── Register.razor           # Auth page restyle
    ├── ForgotPassword.razor     # Auth page restyle
    ├── ResetPassword.razor      # Auth page restyle
    ├── Lockout.razor           # Info page restyle
    ├── AccessDenied.razor      # Error page restyle
    ├── LoginWith2fa.razor      # 2FA login restyle
    └── LoginWithRecoveryCode.razor  # Recovery restyle
```

---

## 6. Design Token Reference

### 6.1 Most Commonly Used Tokens

**Colors:**
```css
var(--color-primary)        /* Rose #DA4167 - CTAs, active states */
var(--color-success)        /* Sage #87B38D - Success messages */
var(--color-warning)        /* Gold #FED766 - Warnings */
var(--color-danger)         /* Dark Rose - Errors, delete actions */
var(--color-neutral-50)     /* Light gray - Page background */
var(--color-neutral-200)    /* Medium gray - Borders */
var(--surface-card)         /* White - Card backgrounds */
```

**Typography:**
```css
var(--text-3xl)            /* 30px - Page titles */
var(--text-2xl)            /* 24px - Section headers */
var(--text-base)           /* 16px - Body text */
var(--text-sm)             /* 14px - Labels, helper text */
var(--font-bold)           /* 700 - Headings */
var(--font-semibold)       /* 600 - Subheadings, labels */
var(--font-medium)         /* 500 - Emphasized text */
```

**Spacing:**
```css
var(--space-2)             /* 8px - Tight spacing */
var(--space-3)             /* 12px - Small spacing */
var(--space-4)             /* 16px - Default spacing */
var(--space-6)             /* 24px - Large spacing, card padding */
var(--space-8)             /* 32px - Section spacing */
```

**Components:**
```css
var(--radius-md)           /* 8px - Buttons, cards, inputs */
var(--shadow-sm)           /* Subtle - Cards */
var(--transition-base)     /* 250ms - Interactive elements */
var(--input-padding)       /* Form inputs */
var(--btn-padding-md)      /* Medium buttons */
```

### 6.2 Component-Specific Classes

**Layout:**
- `.container` - Max-width container with padding
- `.page-header` - Page title section
- `.manage-layout` - Grid layout (sidebar + main)
- `.sidebar` - Sidebar card container
- `.sidebar-nav` - Navigation list

**Content:**
- `.content-card` - White card with shadow
- `.content-header` - Section header with bottom border

**Forms:**
- `.form-group` - Field wrapper with spacing
- `.form-label` - Label styling
- `.form-label.required` - Adds red asterisk
- `.form-input` - Input field styling
- `.form-input.error` - Error state
- `.helper-text` - Description below field
- `.error-message` - Error text below field

**Buttons:**
- `.btn` - Base button styling
- `.btn-primary` - Rose background
- `.btn-secondary` - Gray background
- `.btn-danger` - Red background for destructive actions
- `.btn-link` - Text link styled as button

**Alerts:**
- `.alert` - Base alert with icon
- `.alert.success` - Green success message
- `.alert.error` - Red error message
- `.alert.warning` - Yellow warning message
- `.alert.info` - Lavender info message

**Special:**
- `.badge` - Small status indicator
- `.badge.enabled` - Green badge
- `.badge.disabled` - Gray badge
- `.status-card` - Info card with left border accent
- `.btn-group` - Flex container for multiple buttons
- `.input-group` - Input with adjacent element

---

## 7. Testing Strategy

### 7.1 Unit Testing
- No new unit tests required (UI changes only)
- Existing functionality tests should still pass

### 7.2 Integration Testing
- Manual testing of all form submissions
- Verify validation still works
- Test success/error message display
- Test navigation between pages

### 7.3 Visual Regression Testing (Optional)
- Take screenshots before changes
- Take screenshots after changes
- Compare for unintended changes
- Document intentional differences

### 7.4 User Acceptance Testing
- Test all user flows:
  - Profile update
  - Email change
  - Password change
  - 2FA setup/disable
  - Personal data download/delete
- Verify messages display correctly
- Confirm responsive behavior

---

## 8. References

### 8.1 Design Documentation
- [Prototyping Strategy](../01-planning/03-prototyping-strategy.md)
- [Design Standards](../01-planning/04-design-standards.md)
- [Design Tokens CSS](../../prototypes/css/tokens.css)

### 8.2 Prototype Files
- [Profile Page](../../prototypes/pages/manage/profile.html)
- [Email Page](../../prototypes/pages/manage/email.html)
- [Change Password Page](../../prototypes/pages/manage/change-password.html)
- [Two-Factor Auth Page](../../prototypes/pages/manage/two-factor.html)

### 8.3 Component Files
- [ManageLayout.razor](../../src/DiscordBot/DiscordBot.Blazor/Components/Account/Shared/ManageLayout.razor)
- [ManageNavMenu.razor](../../src/DiscordBot/DiscordBot.Blazor/Components/Account/Shared/ManageNavMenu.razor)
- [StatusMessage.razor](../../src/DiscordBot/DiscordBot.Blazor/Components/Account/Shared/StatusMessage.razor)
- [Account Pages](../../src/DiscordBot/DiscordBot.Blazor/Components/Account/Pages/)

### 8.4 External Resources
- [ASP.NET Core Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [CSS Custom Properties (MDN)](https://developer.mozilla.org/en-US/docs/Web/CSS/--*)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)

---

## Appendix A: CSS Class Mapping

### Bootstrap → Custom Class Mapping

| Bootstrap Class | Custom Class | Notes |
|----------------|--------------|-------|
| `row` | `.manage-layout` | Grid system |
| `col-md-3` | `.sidebar` (in grid) | Sidebar column |
| `col-md-9` | `<main>` (in grid) | Main content column |
| `nav-pills` | `.sidebar-nav` | Navigation list |
| `nav-item` | `<li>` | List item (no class needed) |
| `nav-link` | Inherits from `a` in `.sidebar-nav` | Link styling |
| `alert-success` | `.alert.success` | Success message |
| `alert-danger` | `.alert.error` | Error message |
| `form-floating` | `.form-group` | Form field wrapper |
| `form-control` | `.form-input` | Input styling |
| `form-label` | `.form-label` | Label styling |
| `btn-primary` | `.btn.btn-primary` | Primary button |
| `btn-lg` | `.btn` (default size) | Button size |
| `w-100` | (inline style if needed) | Full width |
| `mb-3` | (spacing via .form-group) | Bottom margin |

---

## Appendix B: Example Before/After

### Before (Bootstrap):
```razor
<div class="row">
    <div class="col-md-6">
        <EditForm Model="Input" FormName="profile">
            <div class="form-floating mb-3">
                <input type="text" class="form-control" />
                <label class="form-label">Username</label>
            </div>
            <button class="w-100 btn btn-lg btn-primary">Save</button>
        </EditForm>
    </div>
</div>
```

### After (Custom Design System):
```razor
<div class="content-card">
    <div class="content-header">
        <h2>Profile</h2>
    </div>

    <EditForm Model="Input" FormName="profile">
        <div class="form-group">
            <label class="form-label">Username</label>
            <input type="text" class="form-input" />
            <div class="helper-text">Your username cannot be changed</div>
        </div>
        <button class="btn btn-primary">Save Changes</button>
    </EditForm>
</div>
```

### CSS Used:
```css
.content-card {
    background: var(--surface-card);
    border-radius: var(--radius-md);
    padding: var(--space-6);
    box-shadow: var(--shadow-sm);
}

.form-group {
    margin-bottom: var(--space-6);
}

.form-label {
    display: block;
    font-size: var(--text-sm);
    font-weight: var(--font-medium);
    color: var(--text-primary);
    margin-bottom: var(--space-2);
}

.form-input {
    width: 100%;
    max-width: 500px;
    padding: var(--input-padding);
    border: var(--input-border);
    border-radius: var(--input-radius);
    font-size: var(--input-font-size);
    font-family: var(--font-primary);
}

.helper-text {
    font-size: var(--text-sm);
    color: var(--text-secondary);
    margin-top: var(--space-2);
}

.btn-primary {
    background: var(--color-primary);
    color: var(--text-on-primary);
    padding: var(--btn-padding-md);
    border-radius: var(--btn-border-radius);
}
```

---

**End of Implementation Plan**

# Authentication & Registration System Implementation Plan

**Version:** 1.0
**Last Updated:** 2025-10-20
**Status:** Active
**Estimated Duration:** 4-5 weeks
**Priority:** High

---

## Overview

This document outlines the comprehensive implementation plan for the user authentication and registration system with Discord invite code integration. The system enables Discord users to create web application accounts through a secure, invite-code-based registration flow, linking their Discord identity with ASP.NET Identity authentication.

**Key Features:**
- Discord invite code-based registration
- Multi-step registration workflow
- OAuth integration (Discord, Microsoft, Google)
- Custom-styled authentication pages matching design system
- Real-time form validation with user feedback
- Secure account linking between Discord and ASP.NET Identity

---

## Table of Contents

- [1. Project Context](#1-project-context)
- [2. Architecture Overview](#2-architecture-overview)
- [3. Implementation Phases](#3-implementation-phases)
- [4. Detailed Implementation Tasks](#4-detailed-implementation-tasks)
- [5. File Structure](#5-file-structure)
- [6. Testing Strategy](#6-testing-strategy)
- [7. Deployment Checklist](#7-deployment-checklist)
- [8. References](#8-references)

---

## 1. Project Context

### 1.1 Design System

The application uses a custom design system with:

| Token | Value | Usage |
|-------|-------|-------|
| **Primary Color** | Rose (#DA4167) | CTAs, buttons, accents |
| **Secondary Color** | Lavender (#D3C4E3) | Backgrounds, soft highlights |
| **Success Color** | Sage (#87B38D) | Success states, validations |
| **Warning Color** | Gold (#FED766) | Warnings, alerts |
| **Base Color** | Slate (#555B6E) | Navigation, primary UI |

**Typography:** Inter font family
**Spacing:** 4px base unit system
**Design Tokens:** See [Design Standards](../01-planning/04-design-standards.md)

### 1.2 Security Architecture

**Authentication Model:**
- **Linked Accounts:** Discord ID ↔ ASP.NET Identity User
- **Registration Flow:** Discord `/register` command → Invite Code → Web Registration
- **Code Lifecycle:** 24-hour expiration, one-time use, audit trail

See [Security Architecture](../02-architecture/04-security-architecture.md) for complete details.

### 1.3 Existing Infrastructure

**✅ Already Implemented:**
- `ApplicationUser` entity with Discord integration fields
- `InviteCode` entity with full lifecycle support
- `IInviteCodeService` interface definition
- Basic Blazor authentication pages (require customization)
- HTML prototypes with complete design system

**📋 Requires Implementation:**
- Custom-styled Blazor components
- Invite code validation workflow
- Account linking service
- Real-time validation components
- OAuth integration flows

---

## 2. Architecture Overview

### 2.1 System Components

```
┌─────────────────────────────────────────────────────────┐
│                    Discord Bot                          │
│                  (/register command)                    │
└────────────────────┬────────────────────────────────────┘
                     │
                     │ 1. Generate invite code
                     │    Send via DM
                     ▼
┌─────────────────────────────────────────────────────────┐
│              Web Application (Blazor)                   │
│  ┌────────────────────────────────────────────────┐    │
│  │  Authentication Pages                          │    │
│  │  - Login.razor                                 │    │
│  │  - Register.razor (with invite code)           │    │
│  │  - OAuth flows                                 │    │
│  └────────────┬───────────────────────────────────┘    │
│               │                                         │
│               │ 2. User enters code + credentials       │
│               ▼                                         │
│  ┌────────────────────────────────────────────────┐    │
│  │  Services Layer                                │    │
│  │  - InviteCodeService (validate, mark used)     │    │
│  │  - AccountLinkingService (link Discord ↔ User) │    │
│  │  - ValidationService (real-time feedback)      │    │
│  └────────────┬───────────────────────────────────┘    │
│               │                                         │
│               │ 3. Create user + link account           │
│               ▼                                         │
│  ┌────────────────────────────────────────────────┐    │
│  │  Data Layer                                    │    │
│  │  - ApplicationUser (with DiscordUserId)        │    │
│  │  - InviteCode (marked as used)                 │    │
│  │  - ASP.NET Identity (roles, claims)            │    │
│  └────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

### 2.2 Registration Flow

```
User runs /register in Discord
         │
         ▼
Bot generates invite code (XXXX-XXXX-XXXX)
         │
         ▼
User receives DM with code + registration URL
         │
         ▼
User visits web registration page
         │
         ▼
[Step 1] Enter & validate invite code
         │
         ├─ Invalid format → Error message
         ├─ Expired code → Error with instructions
         ├─ Already used → Error message
         │
         ▼ Valid code
[Step 2] Choose authentication method
         │
         ├─ Discord OAuth → OAuth flow
         ├─ Microsoft OAuth → OAuth flow
         ├─ Google OAuth → OAuth flow
         │
         ▼ Email/Password
[Step 3] Enter credentials
         │
         ├─ Email validation
         ├─ Password strength check
         ├─ Confirm password match
         │
         ▼ All valid
Create ApplicationUser
         │
         ├─ Set email/password
         ├─ Link DiscordUserId
         ├─ Mark invite code as used
         ├─ Assign "User" role
         │
         ▼
Sign in user → Redirect to dashboard
```

### 2.3 Data Model

**ApplicationUser** (extends `IdentityUser`):
```csharp
public class ApplicationUser : IdentityUser
{
    public ulong? DiscordUserId { get; set; }           // Unique, indexed
    public string? DiscordUsername { get; set; }
    public string? DiscordDiscriminator { get; set; }
    public string? DiscordAvatarHash { get; set; }
    public DateTime? DiscordLinkedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<InviteCode> GeneratedInviteCodes { get; set; }
}
```

**InviteCode**:
```csharp
public class InviteCode
{
    public Guid Id { get; set; }
    public string Code { get; set; }                    // XXXX-XXXX-XXXX
    public ulong DiscordUserId { get; set; }
    public string DiscordUsername { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }            // CreatedAt + 24 hours
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public string? UsedByApplicationUserId { get; set; }

    public virtual ApplicationUser? UsedByApplicationUser { get; set; }
}
```

---

## 3. Implementation Phases

### Phase 1: Backend Services (Week 1)
**Goal:** Implement core services for invite code validation and account linking

**Tasks:**
1. Verify and enhance `InviteCodeService`
2. Create `AccountLinkingService`
3. Build validation utilities
4. Write unit tests

**Deliverables:**
- Fully functional `IInviteCodeService` implementation
- `IAccountLinkingService` with Discord account linking
- Validation helpers for invite codes and credentials
- 90%+ test coverage for services

### Phase 2: Design System Integration (Week 2)
**Goal:** Port design tokens and create reusable UI components

**Tasks:**
1. Create CSS design tokens for Blazor
2. Build shared authentication components
3. Create custom form controls
4. Update account layout

**Deliverables:**
- `wwwroot/css/design-tokens.css`
- `wwwroot/css/account-pages.css`
- Reusable components (InviteCodeInput, PasswordStrengthIndicator)
- Public layout for authentication pages

### Phase 3: Registration Page (Week 3)
**Goal:** Build multi-step registration with invite code integration

**Tasks:**
1. Update Register.razor with invite code field
2. Implement real-time validation
3. Add step-by-step wizard (optional)
4. Integrate with backend services
5. Add loading states and error handling

**Deliverables:**
- Fully functional registration page
- Real-time validation with visual feedback
- Error messages for all failure scenarios
- Success state with auto-redirect

### Phase 4: Login & OAuth (Week 4)
**Goal:** Update login page and add OAuth support

**Tasks:**
1. Restyle login page with design system
2. Add Discord OAuth integration
3. Add Microsoft/Google OAuth (optional)
4. Implement "Remember Me" functionality
5. Add password recovery links

**Deliverables:**
- Custom-styled login page
- Working OAuth flows
- External login provider UI
- Password reset integration

### Phase 5: Testing & Polish (Week 5)
**Goal:** Comprehensive testing, accessibility, and refinements

**Tasks:**
1. Write integration tests
2. Perform accessibility audit
3. User acceptance testing
4. Performance optimization
5. Bug fixes and refinements

**Deliverables:**
- Integration test suite
- WCAG 2.1 AA compliance
- Performance benchmarks
- Production-ready code

---

## 4. Detailed Implementation Tasks

### 4.1 Backend Services

#### Task 1.1: Enhance InviteCodeService

**File:** `src/DiscordBot/DiscordBot.Blazor/Services/InviteCodeService.cs`

**Requirements:**
- [ ] Implement `ValidateCodeAsync` with detailed error states
- [ ] Add format validation (XXXX-XXXX-XXXX regex)
- [ ] Check expiration with clear error messages
- [ ] Verify code not already used
- [ ] Return typed result (Success, Expired, Invalid, Used)

**Example Implementation:**
```csharp
// src/DiscordBot/DiscordBot.Blazor/Services/InviteCodeService.cs

public async Task<InviteCodeValidationResult> ValidateCodeAsync(string code)
{
    // Format validation
    if (!Regex.IsMatch(code, @"^[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$"))
    {
        return InviteCodeValidationResult.InvalidFormat();
    }

    // Retrieve code
    var inviteCode = await _repository.GetByCodeAsync(code);
    if (inviteCode == null)
    {
        return InviteCodeValidationResult.NotFound();
    }

    // Check if already used
    if (inviteCode.IsUsed)
    {
        return InviteCodeValidationResult.AlreadyUsed();
    }

    // Check expiration
    if (inviteCode.IsExpired)
    {
        return InviteCodeValidationResult.Expired();
    }

    // Check if Discord user already linked
    var existingUser = await _accountLinkingService
        .GetUserByDiscordIdAsync(inviteCode.DiscordUserId);
    if (existingUser != null)
    {
        return InviteCodeValidationResult.DiscordAccountAlreadyLinked();
    }

    return InviteCodeValidationResult.Success(inviteCode);
}
```

**Validation Result DTO:**
```csharp
// src/DiscordBot/DiscordBot.Core/Models/InviteCodeValidationResult.cs

public class InviteCodeValidationResult
{
    public bool IsValid { get; init; }
    public InviteCodeValidationError? Error { get; init; }
    public InviteCode? InviteCode { get; init; }

    public static InviteCodeValidationResult Success(InviteCode code)
        => new() { IsValid = true, InviteCode = code };

    public static InviteCodeValidationResult InvalidFormat()
        => new()
        {
            IsValid = false,
            Error = InviteCodeValidationError.InvalidFormat
        };

    public static InviteCodeValidationResult Expired()
        => new()
        {
            IsValid = false,
            Error = InviteCodeValidationError.Expired
        };

    public static InviteCodeValidationResult AlreadyUsed()
        => new()
        {
            IsValid = false,
            Error = InviteCodeValidationError.AlreadyUsed
        };

    public static InviteCodeValidationResult NotFound()
        => new()
        {
            IsValid = false,
            Error = InviteCodeValidationError.NotFound
        };

    public static InviteCodeValidationResult DiscordAccountAlreadyLinked()
        => new()
        {
            IsValid = false,
            Error = InviteCodeValidationError.DiscordAccountAlreadyLinked
        };
}

public enum InviteCodeValidationError
{
    InvalidFormat,
    NotFound,
    Expired,
    AlreadyUsed,
    DiscordAccountAlreadyLinked
}
```

#### Task 1.2: Create AccountLinkingService

**File:** `src/DiscordBot/DiscordBot.Core/Services/IAccountLinkingService.cs`

**Interface:**
```csharp
// src/DiscordBot/DiscordBot.Core/Services/IAccountLinkingService.cs

namespace DiscordBot.Core.Services
{
    /// <summary>
    /// Service for managing Discord account linking with ASP.NET Identity users.
    /// </summary>
    public interface IAccountLinkingService
    {
        /// <summary>
        /// Retrieves an ApplicationUser by Discord user ID.
        /// </summary>
        Task<ApplicationUser?> GetUserByDiscordIdAsync(ulong discordId);

        /// <summary>
        /// Links a Discord account to an ApplicationUser.
        /// </summary>
        Task<bool> LinkDiscordAccountAsync(
            string userId,
            ulong discordId,
            string username,
            string? discriminator = null,
            string? avatarHash = null);

        /// <summary>
        /// Checks if a Discord ID is already linked to any account.
        /// </summary>
        Task<bool> IsDiscordIdLinkedAsync(ulong discordId);

        /// <summary>
        /// Unlinks a Discord account from an ApplicationUser.
        /// Requires admin authorization or email verification.
        /// </summary>
        Task<bool> UnlinkDiscordAccountAsync(string userId, string adminUserId = null);

        /// <summary>
        /// Updates cached Discord username/avatar information.
        /// Called periodically or on user login.
        /// </summary>
        Task UpdateDiscordInfoAsync(ulong discordId, string username, string? discriminator, string? avatarHash);
    }
}
```

**Implementation:**
```csharp
// src/DiscordBot/DiscordBot.Blazor/Services/AccountLinkingService.cs

public class AccountLinkingService : IAccountLinkingService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AccountLinkingService> _logger;

    public async Task<bool> LinkDiscordAccountAsync(
        string userId,
        ulong discordId,
        string username,
        string? discriminator = null,
        string? avatarHash = null)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Attempted to link Discord account to non-existent user {UserId}", userId);
            return false;
        }

        // Check if Discord ID is already linked
        if (await IsDiscordIdLinkedAsync(discordId))
        {
            _logger.LogWarning("Discord ID {DiscordId} is already linked to another account", discordId);
            return false;
        }

        // Link the account
        user.DiscordUserId = discordId;
        user.DiscordUsername = username;
        user.DiscordDiscriminator = discriminator;
        user.DiscordAvatarHash = avatarHash;
        user.DiscordLinkedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation(
                "Successfully linked Discord account {DiscordId} ({Username}) to user {UserId}",
                discordId, username, userId);
        }

        return result.Succeeded;
    }

    // ... other methods
}
```

#### Task 1.3: Create Validation Utilities

**File:** `src/DiscordBot/DiscordBot.Blazor/Validation/RegistrationValidator.cs`

```csharp
// src/DiscordBot/DiscordBot.Blazor/Validation/RegistrationValidator.cs

public static class RegistrationValidator
{
    private static readonly Regex InviteCodeRegex = new(@"^[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$");
    private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");

    public static bool IsValidInviteCodeFormat(string code)
        => !string.IsNullOrWhiteSpace(code) && InviteCodeRegex.IsMatch(code);

    public static bool IsValidEmail(string email)
        => !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email);

    public static PasswordValidationResult ValidatePassword(string password)
    {
        var result = new PasswordValidationResult();

        if (string.IsNullOrWhiteSpace(password))
        {
            result.IsValid = false;
            return result;
        }

        result.HasMinimumLength = password.Length >= 6;
        result.HasUppercase = password.Any(char.IsUpper);
        result.HasLowercase = password.Any(char.IsLower);
        result.HasDigit = password.Any(char.IsDigit);

        result.IsValid = result.HasMinimumLength
            && result.HasUppercase
            && result.HasLowercase
            && result.HasDigit;

        return result;
    }

    public static string FormatInviteCode(string input)
    {
        // Remove non-alphanumeric characters and convert to uppercase
        var cleaned = new string(input.Where(char.IsLetterOrDigit).ToArray()).ToUpper();

        // Format as XXXX-XXXX-XXXX
        var formatted = new StringBuilder();
        for (int i = 0; i < cleaned.Length && i < 12; i++)
        {
            if (i > 0 && i % 4 == 0)
                formatted.Append('-');
            formatted.Append(cleaned[i]);
        }

        return formatted.ToString();
    }
}

public class PasswordValidationResult
{
    public bool IsValid { get; set; }
    public bool HasMinimumLength { get; set; }
    public bool HasUppercase { get; set; }
    public bool HasLowercase { get; set; }
    public bool HasDigit { get; set; }
}
```

---

### 4.2 Design System Integration

#### Task 2.1: Create Design Tokens CSS

**File:** `src/DiscordBot/DiscordBot.Blazor/wwwroot/css/design-tokens.css`

```css
/* design-tokens.css */
/* Design tokens ported from prototypes/css/tokens.css */

:root {
  /* ===== COLOR PALETTE ===== */
  --color-slate: #555B6E;
  --color-rose: #DA4167;
  --color-lavender: #D3C4E3;
  --color-sage: #87B38D;
  --color-gold: #FED766;

  /* ===== SEMANTIC COLORS ===== */
  --color-primary: var(--color-rose);
  --color-primary-hover: #C53758;
  --color-primary-active: #B02D49;
  --color-secondary: var(--color-lavender);
  --color-success: var(--color-sage);
  --color-warning: var(--color-gold);
  --color-danger: #D32F51;
  --color-info: var(--color-lavender);

  /* ===== NEUTRAL SCALE ===== */
  --color-neutral-50: #F8F9FA;
  --color-neutral-100: #F1F3F5;
  --color-neutral-200: #E9ECEF;
  --color-neutral-300: #DEE2E6;
  --color-neutral-400: #CED4DA;
  --color-neutral-500: #ADB5BD;
  --color-neutral-600: #6C757D;
  --color-neutral-700: #495057;
  --color-neutral-800: #343A40;
  --color-neutral-900: #212529;

  /* ===== TEXT COLORS ===== */
  --text-primary: var(--color-neutral-900);
  --text-secondary: var(--color-neutral-600);
  --text-muted: var(--color-neutral-500);
  --text-disabled: var(--color-neutral-400);
  --text-on-primary: #FFFFFF;

  /* ===== SURFACE COLORS ===== */
  --surface-primary: var(--color-slate);
  --surface-secondary: var(--color-neutral-100);
  --surface-card: #FFFFFF;
  --surface-overlay: rgba(0, 0, 0, 0.5);

  /* ===== BACKGROUND COLORS ===== */
  --bg-primary: #FFFFFF;
  --bg-secondary: var(--color-neutral-50);
  --bg-gradient: linear-gradient(135deg, var(--surface-primary) 0%, var(--color-neutral-800) 100%);

  /* ===== BORDER COLORS ===== */
  --border-light: var(--color-neutral-200);
  --border-medium: var(--color-neutral-300);
  --border-dark: var(--color-neutral-400);

  /* ===== TYPOGRAPHY ===== */
  --font-primary: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  --font-heading: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  --font-mono: 'Fira Code', 'Consolas', 'Monaco', monospace;

  /* Font Sizes */
  --text-xs: 0.75rem;    /* 12px */
  --text-sm: 0.875rem;   /* 14px */
  --text-base: 1rem;     /* 16px */
  --text-lg: 1.125rem;   /* 18px */
  --text-xl: 1.25rem;    /* 20px */
  --text-2xl: 1.5rem;    /* 24px */
  --text-3xl: 1.875rem;  /* 30px */
  --text-4xl: 2.25rem;   /* 36px */

  /* Font Weights */
  --font-normal: 400;
  --font-medium: 500;
  --font-semibold: 600;
  --font-bold: 700;

  /* ===== SPACING ===== */
  --space-1: 0.25rem;  /* 4px */
  --space-2: 0.5rem;   /* 8px */
  --space-3: 0.75rem;  /* 12px */
  --space-4: 1rem;     /* 16px */
  --space-5: 1.25rem;  /* 20px */
  --space-6: 1.5rem;   /* 24px */
  --space-8: 2rem;     /* 32px */
  --space-10: 2.5rem;  /* 40px */
  --space-12: 3rem;    /* 48px */
  --space-16: 4rem;    /* 64px */

  /* ===== SIZING ===== */
  --size-input-sm: 32px;
  --size-input-md: 40px;
  --size-input-lg: 48px;
  --size-icon-sm: 16px;
  --size-icon-md: 24px;
  --size-icon-lg: 32px;

  /* ===== BORDER RADIUS ===== */
  --radius-sm: 4px;
  --radius-md: 8px;
  --radius-lg: 12px;
  --radius-xl: 16px;
  --radius-full: 9999px;

  /* ===== SHADOWS ===== */
  --shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.05);
  --shadow-md: 0 4px 6px rgba(0, 0, 0, 0.1);
  --shadow-lg: 0 10px 15px rgba(0, 0, 0, 0.1);
  --shadow-xl: 0 20px 25px rgba(0, 0, 0, 0.15);

  /* ===== TRANSITIONS ===== */
  --transition-fast: 150ms ease-in-out;
  --transition-base: 250ms ease-in-out;
  --transition-slow: 350ms ease-in-out;

  /* ===== Z-INDEX ===== */
  --z-base: 1;
  --z-dropdown: 100;
  --z-sticky: 200;
  --z-overlay: 500;
  --z-modal: 1000;
  --z-toast: 1500;
  --z-tooltip: 2000;

  /* ===== FORM ELEMENTS ===== */
  --input-border: 1px solid var(--border-medium);
  --input-border-focus: 2px solid var(--color-primary);
  --input-bg: #FFFFFF;
  --input-bg-disabled: var(--color-neutral-100);
  --input-padding: var(--space-3) var(--space-4);
  --input-radius: var(--radius-md);
  --input-font-size: var(--text-base);

  /* ===== BUTTONS ===== */
  --btn-padding-sm: var(--space-2) var(--space-3);
  --btn-padding-md: var(--space-3) var(--space-5);
  --btn-padding-lg: var(--space-4) var(--space-6);
  --btn-border-radius: var(--radius-md);
  --btn-font-weight: var(--font-semibold);
  --btn-transition: var(--transition-base);

  /* ===== CARDS ===== */
  --card-padding: var(--space-6);
  --card-bg: var(--surface-card);
  --card-border: 1px solid var(--border-light);
  --card-radius: var(--radius-md);
  --card-shadow: var(--shadow-md);
}
```

#### Task 2.2: Create Account Pages CSS

**File:** `src/DiscordBot/DiscordBot.Blazor/wwwroot/css/account-pages.css`

```css
/* account-pages.css */
/* Authentication page specific styles */

.auth-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-4);
  background: var(--bg-gradient);
}

.auth-card {
  background: var(--card-bg);
  border-radius: var(--card-radius);
  box-shadow: var(--shadow-xl);
  padding: var(--space-8);
  width: 100%;
  max-width: 480px;
}

.auth-header {
  text-align: center;
  margin-bottom: var(--space-8);
}

.auth-header h1 {
  font-size: var(--text-3xl);
  font-weight: var(--font-bold);
  color: var(--text-primary);
  margin-bottom: var(--space-2);
}

.auth-header p {
  font-size: var(--text-sm);
  color: var(--text-secondary);
}

/* Form Elements */
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

.form-label.required::after {
  content: " *";
  color: var(--color-danger);
}

.form-input {
  width: 100%;
  padding: var(--input-padding);
  border: var(--input-border);
  border-radius: var(--input-radius);
  font-size: var(--input-font-size);
  font-family: var(--font-primary);
  background-color: var(--input-bg);
  color: var(--text-primary);
  transition: var(--transition-base);
}

.form-input:focus {
  outline: none;
  border: var(--input-border-focus);
  box-shadow: 0 0 0 3px rgba(218, 65, 103, 0.1);
}

.form-input:disabled {
  background-color: var(--input-bg-disabled);
  color: var(--text-disabled);
  cursor: not-allowed;
}

/* Input States */
.form-input.error {
  border-color: var(--color-danger);
  background-color: rgba(211, 47, 81, 0.05);
}

.form-input.success {
  border-color: var(--color-success);
  background-color: rgba(135, 179, 141, 0.05);
}

/* Validation Messages */
.helper-text {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  margin-top: var(--space-2);
  font-size: var(--text-sm);
  color: var(--text-secondary);
}

.error-message {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  margin-top: var(--space-2);
  padding: var(--space-3);
  background-color: rgba(211, 47, 81, 0.1);
  border: 1px solid var(--color-danger);
  border-radius: var(--radius-sm);
  color: var(--color-danger);
  font-size: var(--text-sm);
}

.success-message {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  margin-top: var(--space-2);
  padding: var(--space-3);
  background-color: rgba(135, 179, 141, 0.1);
  border: 1px solid var(--color-success);
  border-radius: var(--radius-sm);
  color: var(--color-success);
  font-size: var(--text-sm);
}

/* Buttons */
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: var(--btn-padding-md);
  border-radius: var(--btn-border-radius);
  font-size: var(--text-base);
  font-weight: var(--btn-font-weight);
  font-family: var(--font-primary);
  text-align: center;
  cursor: pointer;
  transition: var(--btn-transition);
  border: none;
  text-decoration: none;
  width: 100%;
}

.btn-primary {
  background-color: var(--color-primary);
  color: var(--text-on-primary);
}

.btn-primary:hover:not(:disabled) {
  background-color: var(--color-primary-hover);
}

.btn-primary:active:not(:disabled) {
  background-color: var(--color-primary-active);
}

.btn-primary:disabled {
  background-color: var(--color-neutral-300);
  color: var(--text-disabled);
  cursor: not-allowed;
  opacity: 0.6;
}

/* Footer */
.form-footer {
  text-align: center;
  margin-top: var(--space-6);
  padding-top: var(--space-6);
  border-top: 1px solid var(--border-light);
}

.form-footer p {
  font-size: var(--text-sm);
  color: var(--text-secondary);
}

.form-footer a {
  color: var(--color-primary);
  font-weight: var(--font-medium);
  text-decoration: none;
}

.form-footer a:hover {
  text-decoration: underline;
}

/* Loading Spinner */
.spinner {
  display: inline-block;
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
  margin-right: var(--space-2);
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Responsive */
@media (max-width: 600px) {
  .auth-card {
    padding: var(--space-6);
    max-width: 100%;
  }

  .auth-header h1 {
    font-size: var(--text-2xl);
  }
}
```

#### Task 2.3: Create Shared Components

**File:** `src/DiscordBot/DiscordBot.Blazor/Components/Account/Shared/InviteCodeInput.razor`

```razor
@* InviteCodeInput.razor *@
@* Reusable invite code input with auto-formatting and validation *@

<div class="form-group">
    <label for="@Id" class="form-label required">Invite Code</label>
    <div class="input-wrapper">
        <input
            type="text"
            id="@Id"
            class="form-input invite-code-input @CssClass"
            placeholder="XXXX-XXXX-XXXX"
            maxlength="14"
            autocomplete="off"
            value="@Value"
            @oninput="HandleInput"
            @onblur="HandleBlur" />
        @if (ShowValidationIcon && !string.IsNullOrWhiteSpace(Value))
        {
            <span class="validation-icon">
                @if (IsValid)
                {
                    <span class="success-icon">✓</span>
                }
                else
                {
                    <span class="error-icon">✗</span>
                }
            </span>
        }
    </div>
    <div class="helper-text">
        <span>💡</span>
        <span>Get your code from Discord using <code>/register</code></span>
    </div>
    @if (!string.IsNullOrWhiteSpace(ErrorMessage))
    {
        <div class="error-message">
            <span>⚠️</span>
            <span>@ErrorMessage</span>
        </div>
    }
    @if (!string.IsNullOrWhiteSpace(SuccessMessage))
    {
        <div class="success-message">
            <span>✓</span>
            <span>@SuccessMessage</span>
        </div>
    }
</div>

@code {
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public EventCallback<string> OnValidated { get; set; }
    [Parameter] public string Id { get; set; } = "invite-code";
    [Parameter] public bool ShowValidationIcon { get; set; } = true;
    [Parameter] public string? ErrorMessage { get; set; }
    [Parameter] public string? SuccessMessage { get; set; }

    private bool IsValid => !string.IsNullOrWhiteSpace(Value)
        && Regex.IsMatch(Value, @"^[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$");

    private string CssClass => string.IsNullOrWhiteSpace(Value) ? ""
        : IsValid ? "success" : "error";

    private async Task HandleInput(ChangeEventArgs e)
    {
        var input = e.Value?.ToString() ?? "";

        // Auto-format: Remove non-alphanumeric and convert to uppercase
        var cleaned = new string(input.Where(char.IsLetterOrDigit).ToArray()).ToUpper();

        // Format as XXXX-XXXX-XXXX
        var formatted = new StringBuilder();
        for (int i = 0; i < cleaned.Length && i < 12; i++)
        {
            if (i > 0 && i % 4 == 0)
                formatted.Append('-');
            formatted.Append(cleaned[i]);
        }

        Value = formatted.ToString();
        await ValueChanged.InvokeAsync(Value);
    }

    private async Task HandleBlur()
    {
        if (!string.IsNullOrWhiteSpace(Value) && IsValid)
        {
            await OnValidated.InvokeAsync(Value);
        }
    }
}
```

**CSS for InviteCodeInput:**
```css
/* Add to account-pages.css */

.invite-code-input {
  font-family: var(--font-mono);
  font-size: var(--text-lg);
  text-align: center;
  letter-spacing: 2px;
  text-transform: uppercase;
}

.input-wrapper {
  position: relative;
}

.validation-icon {
  position: absolute;
  right: var(--space-3);
  top: 50%;
  transform: translateY(-50%);
  font-size: var(--text-lg);
  pointer-events: none;
}

.success-icon {
  color: var(--color-success);
  font-weight: var(--font-bold);
}

.error-icon {
  color: var(--color-danger);
}
```

---

### 4.3 Registration Page Implementation

#### Task 3.1: Update Register.razor

**File:** `src/DiscordBot/DiscordBot.Blazor/Components/Account/Pages/Register.razor`

```razor
@page "/Account/Register"
@using System.ComponentModel.DataAnnotations
@using System.Text
@using Microsoft.AspNetCore.Identity
@using DiscordBot.Core.Entities
@using DiscordBot.Core.Services
@inject UserManager<ApplicationUser> UserManager
@inject IUserStore<ApplicationUser> UserStore
@inject SignInManager<ApplicationUser> SignInManager
@inject IInviteCodeService InviteCodeService
@inject IAccountLinkingService AccountLinkingService
@inject ILogger<Register> Logger
@inject NavigationManager NavigationManager

<PageTitle>Create Account</PageTitle>

<div class="auth-container">
    <div class="auth-card">
        <!-- Header -->
        <header class="auth-header">
            <h1>Create Account</h1>
            <p>Link your Discord account to get started</p>
        </header>

        <!-- Error Alert -->
        @if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            <div class="alert alert-danger" role="alert">
                <strong>Registration Failed</strong>
                <p>@errorMessage</p>
            </div>
        }

        <!-- Success Alert -->
        @if (registrationSucceeded)
        {
            <div class="alert alert-success" role="alert">
                <strong>Success!</strong>
                <p>Your account has been created. Redirecting to login...</p>
            </div>
        }

        @if (!registrationSucceeded)
        {
            <EditForm Model="Input" OnValidSubmit="RegisterUser" FormName="register">
                <DataAnnotationsValidator />

                <!-- Step 1: Invite Code -->
                <InviteCodeInput
                    @bind-Value="Input.InviteCode"
                    OnValidated="ValidateInviteCode"
                    ErrorMessage="@inviteCodeError"
                    SuccessMessage="@inviteCodeSuccess" />

                <!-- Discord User Info (shown after code validation) -->
                @if (validatedInviteCode != null)
                {
                    <div class="info-box mb-4">
                        <div class="info-box-title">
                            <span>✓</span>
                            <span>Invite Code Verified</span>
                        </div>
                        <div class="info-box-content">
                            <p><strong>Discord User:</strong> @validatedInviteCode.DiscordUsername</p>
                            <p><strong>Code:</strong> @validatedInviteCode.Code</p>
                        </div>
                    </div>
                }

                <!-- Email Field -->
                <div class="form-group">
                    <label for="email" class="form-label required">Email</label>
                    <InputText
                        @bind-Value="Input.Email"
                        class="form-input"
                        type="email"
                        autocomplete="email"
                        placeholder="you@example.com" />
                    <ValidationMessage For="() => Input.Email" class="error-message" />
                </div>

                <!-- Password Field -->
                <div class="form-group">
                    <label for="password" class="form-label required">Password</label>
                    <InputText
                        type="password"
                        @bind-Value="Input.Password"
                        class="form-input"
                        autocomplete="new-password"
                        placeholder="Create a strong password" />
                    <ValidationMessage For="() => Input.Password" class="error-message" />

                    <!-- Password Requirements -->
                    @if (!string.IsNullOrWhiteSpace(Input.Password))
                    {
                        <PasswordStrengthIndicator Password="@Input.Password" />
                    }
                </div>

                <!-- Confirm Password Field -->
                <div class="form-group">
                    <label for="confirm-password" class="form-label required">Confirm Password</label>
                    <InputText
                        type="password"
                        @bind-Value="Input.ConfirmPassword"
                        class="form-input"
                        autocomplete="new-password"
                        placeholder="Re-enter your password" />
                    <ValidationMessage For="() => Input.ConfirmPassword" class="error-message" />
                </div>

                <!-- Submit Button -->
                <button type="submit" class="btn btn-primary" disabled="@isProcessing">
                    @if (isProcessing)
                    {
                        <span class="spinner"></span>
                        <span>Creating Account...</span>
                    }
                    else
                    {
                        <span>Create Account</span>
                    }
                </button>
            </EditForm>

            <!-- Footer -->
            <div class="form-footer">
                <p>Already have an account? <a href="/Account/Login">Sign in here</a></p>
            </div>
        }
    </div>
</div>

@code {
    private string? errorMessage;
    private string? inviteCodeError;
    private string? inviteCodeSuccess;
    private InviteCode? validatedInviteCode;
    private bool isProcessing = false;
    private bool registrationSucceeded = false;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = new();

    [SupplyParameterFromQuery]
    private string? Code { get; set; }

    protected override void OnInitialized()
    {
        // Pre-fill invite code from query parameter
        if (!string.IsNullOrWhiteSpace(Code))
        {
            Input.InviteCode = Code;
        }
    }

    private async Task ValidateInviteCode(string code)
    {
        inviteCodeError = null;
        inviteCodeSuccess = null;
        validatedInviteCode = null;

        var result = await InviteCodeService.ValidateCodeAsync(code);

        if (result.IsValid)
        {
            validatedInviteCode = result.InviteCode;
            inviteCodeSuccess = "Valid invite code. You can continue with registration.";
        }
        else
        {
            inviteCodeError = result.Error switch
            {
                InviteCodeValidationError.InvalidFormat =>
                    "Invalid code format. Expected: XXXX-XXXX-XXXX",
                InviteCodeValidationError.NotFound =>
                    "Invite code not found. Please check your code and try again.",
                InviteCodeValidationError.Expired =>
                    "This invite code has expired. Please return to Discord and use /register to get a new code.",
                InviteCodeValidationError.AlreadyUsed =>
                    "This invite code has already been used. Contact support if you need assistance.",
                InviteCodeValidationError.DiscordAccountAlreadyLinked =>
                    "This Discord account is already registered. Please log in instead.",
                _ => "An error occurred validating your invite code."
            };
        }
    }

    public async Task RegisterUser()
    {
        errorMessage = null;

        // Validate invite code first
        if (validatedInviteCode == null)
        {
            errorMessage = "Please enter and validate your invite code first.";
            return;
        }

        isProcessing = true;

        try
        {
            var user = CreateUser();

            // Set user properties
            await UserStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            var emailStore = GetEmailStore();
            await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            // Create the user account
            var result = await UserManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                return;
            }

            Logger.LogInformation("User created account with invite code {Code}", validatedInviteCode.Code);

            // Link Discord account
            var linkResult = await AccountLinkingService.LinkDiscordAccountAsync(
                user.Id,
                validatedInviteCode.DiscordUserId,
                validatedInviteCode.DiscordUsername);

            if (!linkResult)
            {
                Logger.LogError("Failed to link Discord account for user {UserId}", user.Id);
                errorMessage = "Account created but failed to link Discord account. Please contact support.";
                return;
            }

            // Mark invite code as used
            await InviteCodeService.MarkCodeAsUsedAsync(validatedInviteCode.Code, user.Id);

            // Assign "User" role
            await UserManager.AddToRoleAsync(user, "User");

            Logger.LogInformation("User {UserId} successfully linked Discord account {DiscordId}",
                user.Id, validatedInviteCode.DiscordUserId);

            registrationSucceeded = true;

            // Redirect to login after 2 seconds
            await Task.Delay(2000);
            NavigationManager.NavigateTo("/Account/Login?registered=true");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during registration");
            errorMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            isProcessing = false;
        }
    }

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'.");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!UserManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<ApplicationUser>)UserStore;
    }

    private sealed class InputModel
    {
        [Required(ErrorMessage = "Invite code is required")]
        [RegularExpression(@"^[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$",
            ErrorMessage = "Invalid invite code format")]
        public string InviteCode { get; set; } = "";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = "";
    }
}
```

**Additional Component Needed:**

**File:** `src/DiscordBot/DiscordBot.Blazor/Components/Account/Shared/PasswordStrengthIndicator.razor`

```razor
@* PasswordStrengthIndicator.razor *@

<div class="password-requirements">
    <p>Password must contain:</p>
    <ul>
        <li class="@GetRequirementClass(HasMinimumLength)">
            At least 6 characters
        </li>
        <li class="@GetRequirementClass(HasUppercase)">
            One uppercase letter
        </li>
        <li class="@GetRequirementClass(HasLowercase)">
            One lowercase letter
        </li>
        <li class="@GetRequirementClass(HasDigit)">
            One number
        </li>
    </ul>
</div>

@code {
    [Parameter] public string? Password { get; set; }

    private bool HasMinimumLength => !string.IsNullOrWhiteSpace(Password) && Password.Length >= 6;
    private bool HasUppercase => !string.IsNullOrWhiteSpace(Password) && Password.Any(char.IsUpper);
    private bool HasLowercase => !string.IsNullOrWhiteSpace(Password) && Password.Any(char.IsLower);
    private bool HasDigit => !string.IsNullOrWhiteSpace(Password) && Password.Any(char.IsDigit);

    private string GetRequirementClass(bool isMet)
    {
        if (string.IsNullOrWhiteSpace(Password))
            return "";
        return isMet ? "valid" : "invalid";
    }
}
```

**CSS for PasswordStrengthIndicator:**
```css
/* Add to account-pages.css */

.password-requirements {
  margin-top: var(--space-3);
  padding: var(--space-3);
  background-color: var(--color-neutral-50);
  border-radius: var(--radius-sm);
  font-size: var(--text-sm);
}

.password-requirements p {
  font-weight: var(--font-medium);
  margin-bottom: var(--space-2);
  color: var(--text-primary);
}

.password-requirements ul {
  list-style: none;
  margin: 0;
  padding: 0;
}

.password-requirements li {
  padding: var(--space-1) 0;
  color: var(--text-secondary);
  display: flex;
  align-items: center;
  gap: var(--space-2);
}

.password-requirements li::before {
  content: "○";
  color: var(--color-neutral-400);
}

.password-requirements li.valid {
  color: var(--color-success);
}

.password-requirements li.valid::before {
  content: "✓";
  color: var(--color-success);
  font-weight: var(--font-bold);
}

.password-requirements li.invalid {
  color: var(--color-danger);
}

.password-requirements li.invalid::before {
  content: "✗";
  color: var(--color-danger);
}

.info-box {
  background: rgba(135, 179, 141, 0.1);
  border: 1px solid var(--color-success);
  border-radius: var(--radius-md);
  padding: var(--space-4);
  margin-bottom: var(--space-6);
}

.info-box-title {
  font-weight: var(--font-semibold);
  color: var(--color-success);
  margin-bottom: var(--space-2);
  display: flex;
  align-items: center;
  gap: var(--space-2);
}

.info-box-content {
  font-size: var(--text-sm);
  color: var(--text-primary);
  line-height: 1.5;
}

.info-box-content p {
  margin: var(--space-1) 0;
}

.alert {
  padding: var(--space-4);
  border-radius: var(--radius-sm);
  margin-bottom: var(--space-6);
  font-size: var(--text-sm);
}

.alert-danger {
  background-color: rgba(211, 47, 81, 0.1);
  border: 1px solid var(--color-danger);
  color: var(--color-danger);
}

.alert-success {
  background-color: rgba(135, 179, 141, 0.1);
  border: 1px solid var(--color-success);
  color: var(--color-success);
}

.alert strong {
  display: block;
  margin-bottom: var(--space-1);
}
```

---

### 4.4 Login Page Update

#### Task 4.1: Update Login.razor

**File:** `src/DiscordBot/DiscordBot.Blazor/Components/Account/Pages/Login.razor`

```razor
@page "/Account/Login"
@using System.ComponentModel.DataAnnotations
@using Microsoft.AspNetCore.Authentication
@using Microsoft.AspNetCore.Identity
@using DiscordBot.Core.Entities
@inject SignInManager<ApplicationUser> SignInManager
@inject ILogger<Login> Logger
@inject NavigationManager NavigationManager

<PageTitle>Log In</PageTitle>

<div class="auth-container">
    <div class="auth-card">
        <!-- Header -->
        <header class="auth-header">
            <h1>Welcome Back</h1>
            <p>Sign in to your account</p>
        </header>

        <!-- Success Message (from registration) -->
        @if (showRegistrationSuccess)
        {
            <div class="alert alert-success" role="alert">
                <strong>Registration Successful!</strong>
                <p>You can now log in with your credentials.</p>
            </div>
        }

        <!-- Error Message -->
        @if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            <div class="alert alert-danger" role="alert">
                @errorMessage
            </div>
        }

        <!-- Login Form -->
        <EditForm Model="Input" method="post" OnValidSubmit="LoginUser" FormName="login">
            <DataAnnotationsValidator />

            <!-- Email Field -->
            <div class="form-group">
                <label for="email" class="form-label required">Email</label>
                <InputText
                    @bind-Value="Input.Email"
                    class="form-input"
                    type="email"
                    autocomplete="username"
                    placeholder="you@example.com" />
                <ValidationMessage For="() => Input.Email" class="error-message" />
            </div>

            <!-- Password Field -->
            <div class="form-group">
                <label for="password" class="form-label required">Password</label>
                <InputText
                    type="password"
                    @bind-Value="Input.Password"
                    class="form-input"
                    autocomplete="current-password"
                    placeholder="Enter your password" />
                <ValidationMessage For="() => Input.Password" class="error-message" />
            </div>

            <!-- Remember Me -->
            <div class="form-group">
                <label class="checkbox-label">
                    <InputCheckbox @bind-Value="Input.RememberMe" />
                    <span>Remember me</span>
                </label>
            </div>

            <!-- Submit Button -->
            <button type="submit" class="btn btn-primary" disabled="@isProcessing">
                @if (isProcessing)
                {
                    <span class="spinner"></span>
                    <span>Signing in...</span>
                }
                else
                {
                    <span>Sign In</span>
                }
            </button>
        </EditForm>

        <!-- Footer Links -->
        <div class="form-footer">
            <p>
                <a href="/Account/ForgotPassword">Forgot your password?</a>
            </p>
            <p>
                Don't have an account?
                <a href="/Account/Register">Register with invite code</a>
            </p>
        </div>

        <!-- External Login Section (Optional) -->
        @if (externalLogins?.Count > 0)
        {
            <div class="divider">or</div>
            <div class="external-logins">
                <h3>Sign in with</h3>
                <ExternalLoginPicker />
            </div>
        }
    </div>
</div>

@code {
    private string? errorMessage;
    private bool isProcessing = false;
    private bool showRegistrationSuccess = false;
    private IList<AuthenticationScheme>? externalLogins;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = new();

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    [SupplyParameterFromQuery]
    private string? Registered { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (HttpMethods.IsGet(HttpContext.Request.Method))
        {
            // Clear the existing external cookie
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Check if user just registered
            showRegistrationSuccess = Registered == "true";

            // Get external login providers
            externalLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }
    }

    public async Task LoginUser()
    {
        errorMessage = null;
        isProcessing = true;

        try
        {
            var result = await SignInManager.PasswordSignInAsync(
                Input.Email,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                Logger.LogInformation("User {Email} logged in", Input.Email);

                // Update LastLoginAt timestamp
                var user = await SignInManager.UserManager.FindByEmailAsync(Input.Email);
                if (user != null)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    await SignInManager.UserManager.UpdateAsync(user);
                }

                NavigationManager.NavigateTo(ReturnUrl ?? "/", forceLoad: true);
            }
            else if (result.RequiresTwoFactor)
            {
                NavigationManager.NavigateTo(
                    $"/Account/LoginWith2fa?returnUrl={Uri.EscapeDataString(ReturnUrl ?? "/")}");
            }
            else if (result.IsLockedOut)
            {
                Logger.LogWarning("User {Email} account locked out", Input.Email);
                NavigationManager.NavigateTo("/Account/Lockout");
            }
            else
            {
                errorMessage = "Invalid email or password. Please try again.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during login for user {Email}", Input.Email);
            errorMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            isProcessing = false;
        }
    }

    private sealed class InputModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
```

**Additional CSS for Login:**
```css
/* Add to account-pages.css */

.checkbox-label {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  font-size: var(--text-sm);
  color: var(--text-primary);
  cursor: pointer;
  user-select: none;
}

.checkbox-label input[type="checkbox"] {
  width: 18px;
  height: 18px;
  cursor: pointer;
}

.divider {
  display: flex;
  align-items: center;
  gap: var(--space-4);
  margin: var(--space-6) 0;
  color: var(--text-muted);
  font-size: var(--text-sm);
}

.divider::before,
.divider::after {
  content: '';
  flex: 1;
  height: 1px;
  background: var(--border-medium);
}

.external-logins {
  margin-top: var(--space-6);
}

.external-logins h3 {
  font-size: var(--text-sm);
  font-weight: var(--font-medium);
  color: var(--text-secondary);
  text-align: center;
  margin-bottom: var(--space-4);
}
```

---

## 5. File Structure

### 5.1 New Files to Create

```
src/DiscordBot/
├── DiscordBot.Core/
│   ├── Models/
│   │   └── InviteCodeValidationResult.cs          [NEW]
│   └── Services/
│       └── IAccountLinkingService.cs               [NEW]
│
├── DiscordBot.Blazor/
│   ├── Services/
│   │   └── AccountLinkingService.cs                [NEW]
│   ├── Validation/
│   │   └── RegistrationValidator.cs                [NEW]
│   ├── Components/
│   │   └── Account/
│   │       ├── Shared/
│   │       │   ├── InviteCodeInput.razor           [NEW]
│   │       │   └── PasswordStrengthIndicator.razor [NEW]
│   │       └── Pages/
│   │           ├── Login.razor                     [UPDATE]
│   │           └── Register.razor                  [UPDATE]
│   └── wwwroot/
│       └── css/
│           ├── design-tokens.css                   [NEW]
│           └── account-pages.css                   [NEW]
```

### 5.2 Files to Modify

```
src/DiscordBot/DiscordBot.Blazor/
├── Components/Account/Pages/
│   ├── Login.razor                    - Apply design system
│   └── Register.razor                 - Add invite code integration
├── Components/Account/Shared/
│   └── AccountLayout.razor            - Update for public layout
├── Program.cs                         - Register new services
└── _Imports.razor                     - Add using statements
```

### 5.3 Service Registration

**File:** `src/DiscordBot/DiscordBot.Blazor/Program.cs`

Add to service registration:
```csharp
// Account linking service
builder.Services.AddScoped<IAccountLinkingService, AccountLinkingService>();

// Ensure InviteCodeService is registered (should already exist)
builder.Services.AddScoped<IInviteCodeService, InviteCodeService>();
builder.Services.AddScoped<IInviteCodeRepository, InviteCodeRepository>();
```

---

## 6. Testing Strategy

### 6.1 Unit Tests

**Test Suite:** `DiscordBot.Tests/Services/InviteCodeServiceTests.cs`

```csharp
[Fact]
public async Task ValidateCodeAsync_ValidCode_ReturnsSuccess()
{
    // Arrange
    var code = "A1B2-C3D4-E5F6";
    var inviteCode = new InviteCode
    {
        Code = code,
        DiscordUserId = 123456789,
        ExpiresAt = DateTime.UtcNow.AddHours(1),
        IsUsed = false
    };

    _mockRepository.Setup(r => r.GetByCodeAsync(code))
        .ReturnsAsync(inviteCode);
    _mockAccountLinking.Setup(a => a.GetUserByDiscordIdAsync(123456789))
        .ReturnsAsync((ApplicationUser?)null);

    // Act
    var result = await _service.ValidateCodeAsync(code);

    // Assert
    Assert.True(result.IsValid);
    Assert.Equal(inviteCode, result.InviteCode);
}

[Fact]
public async Task ValidateCodeAsync_ExpiredCode_ReturnsExpiredError()
{
    // Arrange
    var code = "X7Y8-Z9A0-B1C2";
    var inviteCode = new InviteCode
    {
        Code = code,
        ExpiresAt = DateTime.UtcNow.AddHours(-1), // Expired
        IsUsed = false
    };

    _mockRepository.Setup(r => r.GetByCodeAsync(code))
        .ReturnsAsync(inviteCode);

    // Act
    var result = await _service.ValidateCodeAsync(code);

    // Assert
    Assert.False(result.IsValid);
    Assert.Equal(InviteCodeValidationError.Expired, result.Error);
}

// Additional tests:
// - InvalidFormat_ReturnsInvalidFormatError
// - AlreadyUsedCode_ReturnsAlreadyUsedError
// - DiscordAccountLinked_ReturnsLinkedError
// - NotFound_ReturnsNotFoundError
```

**Test Suite:** `DiscordBot.Tests/Services/AccountLinkingServiceTests.cs`

```csharp
[Fact]
public async Task LinkDiscordAccountAsync_ValidData_LinksSuccessfully()
{
    // Arrange
    var userId = "user-123";
    var discordId = 987654321ul;
    var user = new ApplicationUser { Id = userId };

    _mockUserManager.Setup(m => m.FindByIdAsync(userId))
        .ReturnsAsync(user);
    _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
        .ReturnsAsync(IdentityResult.Success);
    _mockDbContext.Setup(db => db.Users.AnyAsync(u => u.DiscordUserId == discordId, default))
        .ReturnsAsync(false);

    // Act
    var result = await _service.LinkDiscordAccountAsync(userId, discordId, "testuser");

    // Assert
    Assert.True(result);
    Assert.Equal(discordId, user.DiscordUserId);
    Assert.NotNull(user.DiscordLinkedAt);
}

// Additional tests:
// - LinkDiscordAccountAsync_AlreadyLinked_ReturnsFalse
// - LinkDiscordAccountAsync_UserNotFound_ReturnsFalse
// - GetUserByDiscordIdAsync_ExistingUser_ReturnsUser
// - IsDiscordIdLinkedAsync_LinkedId_ReturnsTrue
```

**Test Suite:** `DiscordBot.Tests/Validation/RegistrationValidatorTests.cs`

```csharp
[Theory]
[InlineData("A1B2-C3D4-E5F6", true)]
[InlineData("XXXX-YYYY-ZZZZ", true)]
[InlineData("1234-5678-9012", true)]
[InlineData("ABC-DEF-GHI", false)]      // Too short
[InlineData("ABCD-EFGH-IJKL-MNOP", false)] // Too long
[InlineData("ABCD-EFGH", false)]         // Incomplete
[InlineData("", false)]                   // Empty
public void IsValidInviteCodeFormat_VariousFormats_ReturnsExpected(string code, bool expected)
{
    var result = RegistrationValidator.IsValidInviteCodeFormat(code);
    Assert.Equal(expected, result);
}

[Theory]
[InlineData("test@example.com", true)]
[InlineData("user.name@domain.co.uk", true)]
[InlineData("invalid-email", false)]
[InlineData("@domain.com", false)]
[InlineData("", false)]
public void IsValidEmail_VariousEmails_ReturnsExpected(string email, bool expected)
{
    var result = RegistrationValidator.IsValidEmail(email);
    Assert.Equal(expected, result);
}

[Fact]
public void ValidatePassword_StrongPassword_ReturnsAllValid()
{
    var result = RegistrationValidator.ValidatePassword("Password123");

    Assert.True(result.IsValid);
    Assert.True(result.HasMinimumLength);
    Assert.True(result.HasUppercase);
    Assert.True(result.HasLowercase);
    Assert.True(result.HasDigit);
}

[Fact]
public void FormatInviteCode_UnformattedInput_ReturnsFormatted()
{
    var result = RegistrationValidator.FormatInviteCode("a1b2c3d4e5f6");
    Assert.Equal("A1B2-C3D4-E5F6", result);
}
```

### 6.2 Integration Tests

**Test Suite:** `DiscordBot.IntegrationTests/RegistrationFlowTests.cs`

```csharp
public class RegistrationFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task CompleteRegistrationFlow_ValidInviteCode_CreatesUserAndLinksAccount()
    {
        // Arrange
        var inviteCode = await GenerateTestInviteCode();
        var registrationData = new
        {
            InviteCode = inviteCode.Code,
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/Account/Register", registrationData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify user created
        var user = await _userManager.FindByEmailAsync(registrationData.Email);
        Assert.NotNull(user);
        Assert.Equal(inviteCode.DiscordUserId, user.DiscordUserId);

        // Verify invite code marked as used
        var usedCode = await _inviteCodeRepository.GetByCodeAsync(inviteCode.Code);
        Assert.True(usedCode.IsUsed);
        Assert.Equal(user.Id, usedCode.UsedByApplicationUserId);
    }

    // Additional integration tests:
    // - ExpiredCode_ShowsErrorMessage
    // - AlreadyUsedCode_ShowsErrorMessage
    // - DuplicateEmail_ShowsErrorMessage
    // - LoginAfterRegistration_Succeeds
}
```

### 6.3 UI/E2E Tests (Optional - bUnit)

```csharp
public class RegisterComponentTests : TestContext
{
    [Fact]
    public void InviteCodeInput_AutoFormatsOnInput()
    {
        // Arrange
        var cut = RenderComponent<InviteCodeInput>();
        var input = cut.Find("input");

        // Act
        input.Change("a1b2c3d4e5f6");

        // Assert
        Assert.Equal("A1B2-C3D4-E5F6", cut.Instance.Value);
    }

    [Fact]
    public void PasswordStrengthIndicator_WeakPassword_ShowsInvalidRequirements()
    {
        // Arrange
        var cut = RenderComponent<PasswordStrengthIndicator>(parameters =>
            parameters.Add(p => p.Password, "weak"));

        // Assert
        var invalidItems = cut.FindAll(".invalid");
        Assert.True(invalidItems.Count > 0);
    }
}
```

### 6.4 Accessibility Testing

- [ ] WCAG 2.1 AA compliance check
- [ ] Screen reader testing (NVDA/JAWS)
- [ ] Keyboard navigation (Tab, Enter, Esc)
- [ ] Color contrast validation (4.5:1 minimum)
- [ ] Focus indicators visible
- [ ] ARIA labels on form fields
- [ ] Error announcements for screen readers

---

## 7. Deployment Checklist

### 7.1 Pre-Deployment

- [ ] All unit tests passing (90%+ coverage)
- [ ] Integration tests passing
- [ ] Code review completed
- [ ] Security review completed
- [ ] Performance testing done
- [ ] Accessibility audit passed
- [ ] Browser compatibility tested (Chrome, Firefox, Edge, Safari)
- [ ] Mobile responsive testing completed

### 7.2 Database

- [ ] Migration created for any schema changes
- [ ] Migration tested on staging database
- [ ] Rollback plan documented
- [ ] Backup created before migration

### 7.3 Configuration

- [ ] Environment variables configured
  - `Bot:InviteCodeExpirationHours`
  - `Security:InitialAdminDiscordId`
  - Discord OAuth credentials (if used)
- [ ] Email sender configured (for password reset)
- [ ] Logging configured
- [ ] Application Insights/monitoring configured

### 7.4 Security

- [ ] HTTPS enforced
- [ ] CORS configured correctly
- [ ] Rate limiting enabled
- [ ] Input validation tested
- [ ] SQL injection prevention verified
- [ ] XSS protection verified
- [ ] CSRF tokens enabled

### 7.5 Post-Deployment

- [ ] Smoke tests on production
- [ ] Monitor error logs
- [ ] Monitor performance metrics
- [ ] Verify invite code generation from Discord bot
- [ ] Test complete registration flow end-to-end
- [ ] Verify email confirmations (if enabled)

---

## 8. References

### 8.1 Internal Documentation

- [Design Standards](../01-planning/04-design-standards.md)
- [Security Architecture](../02-architecture/04-security-architecture.md)
- [Database Design](../02-architecture/02-database-design.md)
- [Product Requirements](../01-planning/01-product-requirements.md)
- [Functional Specifications](../01-planning/02-functional-specs.md)

### 8.2 External Resources

- [ASP.NET Core Identity Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Blazor Forms and Validation](https://learn.microsoft.com/en-us/aspnet/core/blazor/forms-and-validation)
- [Discord OAuth2 Documentation](https://discord.com/developers/docs/topics/oauth2)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)

### 8.3 Design Prototypes

- [prototypes/pages/login.html](../../prototypes/pages/login.html)
- [prototypes/pages/register.html](../../prototypes/pages/register.html)
- [prototypes/pages/register-multistep.html](../../prototypes/pages/register-multistep.html)
- [prototypes/css/tokens.css](../../prototypes/css/tokens.css)

---

## Change Log

### Version 1.0 (2025-10-20)
- Initial implementation plan created
- All phases and tasks defined
- File structure documented
- Testing strategy outlined
- Deployment checklist added

---

**Next Steps:**
1. Review and approve this implementation plan
2. Begin Phase 1: Backend Services implementation
3. Set up testing infrastructure
4. Schedule code review sessions
5. Plan user acceptance testing

**Questions or Clarifications:**
- Preferred OAuth providers priority?
- Multi-step vs. single-page registration preference?
- Email confirmation requirement?
- 2FA requirement for initial release?

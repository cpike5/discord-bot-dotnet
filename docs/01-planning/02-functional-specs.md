# Functional Specifications

**Version:** 1.0
**Last Updated:** 2025-10-18

---

## 1. User Stories

### US-001: Request Registration Code
**As a** Discord server member
**I want to** request an invite code via a slash command
**So that** I can register on the web application

**Acceptance Criteria:**
- User can execute `/register` command in Discord
- Bot validates user is not already registered
- Bot generates unique invite code
- Code is sent via Direct Message
- DM includes registration URL and expiration time
- Command responds with confirmation message

---

### US-002: Register with Invite Code
**As a** Discord user with an invite code
**I want to** register on the web application using my code
**So that** my Discord identity is linked to my account

**Acceptance Criteria:**
- Registration page has invite code input field
- Code is validated before account creation
- Discord user ID is stored with the account
- Invite code is marked as used
- User receives confirmation of successful registration
- Invalid/expired codes show appropriate error messages

---

### US-003: Manage Invite Codes (Admin)
**As a** Discord server administrator
**I want to** manage invite codes manually
**So that** I can assist users and maintain control

**Acceptance Criteria:**
- Admin can generate codes for specific users
- Admin can revoke active codes
- Admin can list all active invite codes
- Commands are only accessible to administrators
- All actions are logged

---

## 2. User Flows

### Flow 1: New User Registration

```
1. User opens Discord
2. User types /register command
3. Bot checks if user already has account
   ├─ YES → Bot responds: "You already have an account"
   └─ NO → Continue to step 4
4. Bot checks if user has active invite code
   ├─ YES → Bot sends existing code via DM
   └─ NO → Bot generates new code
5. Bot sends DM to user with:
   - Invite code (e.g., "X7K9-M2P4-Q8N6")
   - Registration URL
   - Expiration time (24 hours)
6. Bot responds to command: "Check your DMs!"
7. User clicks registration URL
8. User fills out registration form:
   - Invite code (required)
   - Email (required)
   - Password (required)
   - Confirm password (required)
9. Form validates invite code:
   ├─ INVALID → Show error, allow retry
   └─ VALID → Continue to step 10
10. Account is created
11. Discord user ID is linked to account
12. Invite code is marked as used
13. User is redirected to login page
14. User logs in successfully
```

---

### Flow 2: Admin Generates Code for User

```
1. Admin types /admin generate-code @username
2. Bot validates admin has permission
3. Bot checks if target user already registered
   ├─ YES → Bot responds: "User already registered"
   └─ NO → Continue to step 4
4. Bot generates invite code
5. Bot sends DM to target user with code
6. Bot responds to admin with confirmation
7. Code is logged in database
```

---

### Flow 3: Admin Revokes Code

```
1. Admin types /admin revoke-code X7K9-M2P4-Q8N6
2. Bot validates admin has permission
3. Bot looks up code in database
   ├─ NOT FOUND → Bot responds: "Code not found"
   ├─ ALREADY USED → Bot responds: "Code already used"
   └─ VALID → Continue to step 4
4. Bot marks code as expired/revoked
5. Bot responds with confirmation
6. Action is logged
```

---

## 3. Functional Requirements

### FR-001: Invite Code Generation
**Priority**: HIGH
**Description**: System must generate cryptographically secure invite codes

**Rules**:
- Code format: `XXXX-XXXX-XXXX` (12 alphanumeric characters)
- Exclude ambiguous characters (0, O, 1, I, l)
- Globally unique across all codes
- Associated with Discord user ID who requested it

---

### FR-002: Code Expiration
**Priority**: HIGH
**Description**: Invite codes must expire after 24 hours

**Rules**:
- Expiration calculated as `CreatedAt + 24 hours`
- Expired codes cannot be used for registration
- Expired codes can be cleaned up after 7 days (optional)

---

### FR-003: One-to-One Discord Mapping
**Priority**: HIGH
**Description**: One Discord user can only have one web account

**Rules**:
- `DiscordUserId` has unique constraint in database
- Attempting to register with code for different Discord user fails
- `/register` command checks for existing account first
- Registration form validates Discord user not already linked

---

### FR-004: Single Active Code Per User
**Priority**: MEDIUM
**Description**: Discord user can only have one unused, non-expired code

**Rules**:
- Before generating new code, check for active code
- If active code exists, resend existing code instead
- Active = `IsUsed = false AND ExpiresAt > NOW()`

---

### FR-005: Code Usage Tracking
**Priority**: HIGH
**Description**: System tracks when and by whom codes are used

**Rules**:
- When code is used: set `IsUsed = true`
- Record `UsedAt = NOW()`
- Record `UsedByApplicationUserId = <new user ID>`
- Used codes cannot be reused

---

### FR-006: Direct Message Delivery
**Priority**: HIGH
**Description**: Bot sends invite codes via DM

**Rules**:
- Attempt to send DM to user
- If DM fails (user has DMs disabled):
  - Log error
  - Respond with ephemeral message: "Could not send DM. Please enable DMs and try again"
- DM content includes:
  - Invite code
  - Registration URL
  - Expiration time
  - Instructions

---

### FR-007: Administrator Permissions
**Priority**: HIGH
**Description**: Admin commands require Discord administrator permission

**Rules**:
- Check user has `GuildPermission.Administrator`
- If not authorized, respond with error
- All admin actions logged with user ID and timestamp

---

## 4. Non-Functional Requirements

### NFR-001: Response Time
- Bot commands respond within 2 seconds
- Registration form validation completes within 1 second

### NFR-002: Availability
- Bot maintains 99% uptime
- Auto-reconnects on disconnection
- Hosted service starts with web application

### NFR-003: Scalability
- Support up to 10,000 active invite codes
- Handle 100 concurrent registrations
- Database queries optimized with indexes

### NFR-004: Security
- Invite codes are cryptographically secure
- No predictable patterns in code generation
- Rate limiting prevents abuse
- SQL injection prevented via parameterized queries

### NFR-005: Usability
- Clear error messages for all failure scenarios
- Registration form validates in real-time
- Admin commands provide confirmation feedback

---

## 5. Validation Rules

### Invite Code Validation

| Rule | Description | Error Message |
|------|-------------|---------------|
| Required | Code cannot be empty | "Invite code is required" |
| Format | Must match `XXXX-XXXX-XXXX` | "Invalid invite code format" |
| Exists | Code must exist in database | "Invalid invite code" |
| Not Used | `IsUsed = false` | "Invite code has already been used" |
| Not Expired | `ExpiresAt > NOW()` | "Invite code has expired. Use /register in Discord to get a new code" |

### Registration Form Validation

| Field | Rules |
|-------|-------|
| Invite Code | Required, valid format, exists, not used, not expired |
| Email | Required, valid email format, unique in database |
| Password | Required, minimum 6 characters, complexity requirements |
| Confirm Password | Required, must match password |

---

## 6. Error Scenarios

### Scenario 1: User Already Registered
- **Trigger**: User executes `/register` but already has account
- **Response**: "You already have a registered account linked to this Discord user"
- **Action**: No code generated

### Scenario 2: DM Delivery Failed
- **Trigger**: User has DMs disabled from server members
- **Response**: "I couldn't send you a DM. Please enable DMs from server members and try again"
- **Action**: Code is still generated and stored

### Scenario 3: Expired Code Used
- **Trigger**: User tries to register with expired code
- **Response**: "This invite code has expired. Please return to Discord and use the /register command to get a new code"
- **Action**: Registration form cleared, error shown

### Scenario 4: Code for Different Discord User
- **Trigger**: User A's code used by User B
- **Response**: "This invite code was issued for a different Discord user"
- **Action**: Registration blocked

### Scenario 5: Admin Permission Denied
- **Trigger**: Non-admin tries to use `/admin` commands
- **Response**: "You do not have permission to use this command"
- **Action**: Command execution blocked

---

## 7. Success Messages

### `/register` Command Success
```
✅ Invite code sent!
Check your DMs for your registration code and link.
The code will expire in 24 hours.
```

### Registration Success
```
✅ Registration successful!
Your Discord account has been linked.
You can now log in.
```

### Admin Code Generation Success
```
✅ Invite code generated for @username
Code sent via DM.
Code: X7K9-M2P4-Q8N6
```

### Admin Code Revocation Success
```
✅ Invite code X7K9-M2P4-Q8N6 has been revoked
```

---

## 8. UI/UX Specifications

### Discord DM Template
```
🎟️ Your Registration Invite Code

Code: X7K9-M2P4-Q8N6

Register here: https://yourdomain.com/Account/Register

⏰ This code expires in 24 hours.

Instructions:
1. Click the link above
2. Enter this code in the registration form
3. Complete your account setup

Need help? Contact a server administrator.
```

### Registration Page Updates
- Add "Invite Code" field at top of form
- Show helper text: "Get your code from Discord using /register"
- Real-time validation on blur
- Success/error states with icons
- Preserve form data on validation errors

---

## 9. Audit & Logging

### Events to Log

| Event | Data Logged |
|-------|-------------|
| Code Generated | Discord user ID, username, code, timestamp |
| Code Sent | Discord user ID, success/failure, timestamp |
| Code Used | Code, application user ID, timestamp |
| Code Revoked | Code, admin user ID, timestamp |
| Registration Success | Discord user ID, application user ID, timestamp |
| Validation Failure | Code, reason, timestamp |

### Log Levels
- **Info**: Successful operations
- **Warning**: Failed DM delivery, expired code usage
- **Error**: Database errors, unexpected failures

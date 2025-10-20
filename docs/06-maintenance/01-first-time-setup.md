# First Time Setup - Application Documentation

**Version:** 1.0
**Last Updated:** 2025-10-20
**Status:** Active

---

## Overview

The First Time Setup feature provides an automated, user-friendly workflow for configuring the Discord Bot application when it is run for the first time. This ensures that the application is properly initialized with essential data and an admin account before regular operations begin.

---

## Table of Contents

- [1. What is First Time Setup?](#1-what-is-first-time-setup)
- [2. When Does Setup Run?](#2-when-does-setup-run)
- [3. Setup Workflow](#3-setup-workflow)
- [4. Administrator Guide](#4-administrator-guide)
- [5. Troubleshooting](#5-troubleshooting)
- [6. Security Considerations](#6-security-considerations)
- [7. Advanced Topics](#7-advanced-topics)

---

## 1. What is First Time Setup?

### 1.1 Purpose

The First Time Setup feature automates the initial configuration of the Discord Bot application, including:

- **Admin Account Creation**: Creates the first administrator account with full system access
- **Role Initialization**: Seeds the database with required user roles (SuperAdmin, Admin, Moderator, User)
- **Database Configuration**: Ensures the database schema is properly initialized
- **System Readiness**: Validates that all required components are configured

### 1.2 Key Benefits

✅ **Zero Manual Configuration**: No need to manually edit databases or configuration files
✅ **Guided Experience**: Step-by-step wizard makes setup intuitive
✅ **Error Prevention**: Validates inputs and prevents common configuration mistakes
✅ **Security**: Enforces strong passwords and proper role assignments
✅ **One-Time Process**: Automatically detects completion and never runs again

---

## 2. When Does Setup Run?

### 2.1 Automatic Detection

Setup runs automatically when:

1. **Fresh Installation**: The application is started for the first time after installation
2. **Empty Database**: No users exist in the database
3. **Missing Setup Record**: The `SetupStatus` table indicates setup is incomplete

### 2.2 Setup Completion

Setup is considered complete when:

- ✅ At least one user with the `SuperAdmin` role exists
- ✅ All required roles are created (SuperAdmin, Admin, Moderator, User)
- ✅ The `SetupStatus` database record is marked as complete

### 2.3 Middleware Behavior

The First Time Setup Middleware intercepts **all requests** until setup is complete:

**Allowed During Setup:**
- Setup pages (`/setup/*`)
- Static resources (CSS, JavaScript, images)
- Blazor framework files

**Blocked Until Setup Complete:**
- All other pages (home, login, account pages, etc.)
- Admin panels
- API endpoints

Once setup is complete, the middleware allows all requests to proceed normally.

---

## 3. Setup Workflow

### 3.1 Step-by-Step Process

The setup workflow consists of four main steps:

```
┌─────────────────────┐
│  1. Welcome Page    │  Overview of what will be configured
├─────────────────────┤
│  2. Create Admin    │  Create the first administrator account
├─────────────────────┤
│  3. Database Seed   │  Initialize roles and system data
├─────────────────────┤
│  4. Completion      │  Finalize and redirect to login
└─────────────────────┘
```

---

### 3.2 Step 1: Welcome

**URL:** `/setup/welcome`

**Description:**
The welcome page provides an overview of the setup process and lists what will be configured.

**What You'll See:**
- Welcome message
- Checklist of configuration items
- Estimated time (2-3 minutes)
- "Get Started" button

**Action:**
Click **"Get Started"** to proceed to admin account creation.

---

### 3.3 Step 2: Create Admin Account

**URL:** `/setup/admin`

**Description:**
This step creates the first administrator account with full system access (SuperAdmin role).

**Required Information:**

| Field | Requirements | Example |
|-------|--------------|---------|
| **Email** | Valid email address | admin@example.com |
| **Username** | Min. 3 characters | admin |
| **Password** | Min. 6 characters, uppercase, lowercase, number | Admin123! |
| **Confirm Password** | Must match password | Admin123! |

**Password Requirements:**
- ✅ At least 6 characters long
- ✅ Contains at least one uppercase letter (A-Z)
- ✅ Contains at least one lowercase letter (a-z)
- ✅ Contains at least one digit (0-9)

**Validation:**
- Real-time validation displays errors as you type
- Form cannot be submitted until all requirements are met
- Email uniqueness is checked (prevents duplicate accounts)

**Action:**
Fill in all fields and click **"Create Admin Account"**.

---

### 3.4 Step 3: Database Seeding

**URL:** `/setup/seed`

**Description:**
This step automatically seeds the database with required roles and system data.

**What Happens:**
1. Creates four user roles:
   - **SuperAdmin**: Full system access, cannot be removed
   - **Admin**: Administrative access
   - **Moderator**: Moderation capabilities
   - **User**: Standard user access (default role)

2. Assigns the SuperAdmin role to the created admin account

3. Initializes system configuration (future)

**Duration:**
This process typically takes 5-10 seconds.

**Progress Indicator:**
A loading spinner displays while seeding is in progress.

**Action:**
This step is automatic. Once complete, click **"Continue"**.

---

### 3.5 Step 4: Completion

**URL:** `/setup/complete`

**Description:**
Finalizes the setup process and marks it as complete in the database.

**What Happens:**
1. Validates that all setup steps completed successfully
2. Updates the `SetupStatus` database record
3. Records the completion timestamp
4. Displays success message

**Success Indicators:**
- ✅ Green checkmark animation
- ✅ Summary of what was configured
- ✅ "Go to Login" button

**Action:**
Click **"Go to Login"** to navigate to the login page.

---

## 4. Administrator Guide

### 4.1 Pre-Setup Checklist

Before starting setup, ensure:

- [ ] Database is accessible (SQLite file path is writable)
- [ ] Connection string is configured in `appsettings.json`
- [ ] Application has write permissions to the database directory
- [ ] No firewall rules blocking localhost access (for web UI)

### 4.2 Running Setup

**Steps:**

1. **Start the Application:**
   ```bash
   cd src/DiscordBot/DiscordBot.Blazor
   dotnet run
   ```

2. **Open Browser:**
   - Navigate to the URL shown in the console (usually `https://localhost:5001`)
   - You will be automatically redirected to `/setup/welcome`

3. **Complete Setup Workflow:**
   - Follow the on-screen instructions
   - Fill in admin account details
   - Wait for database seeding to complete
   - Proceed to login

4. **Login with Admin Account:**
   - Use the email and password you created during setup
   - You now have full access to the application

---

### 4.3 Post-Setup Actions

After completing setup, you should:

1. **Verify Roles:**
   - Navigate to user management
   - Confirm all roles exist (SuperAdmin, Admin, Moderator, User)

2. **Create Additional Users:**
   - Use the Discord bot `/register` command to generate invite codes
   - Share invite codes with users to register

3. **Configure Discord Bot:**
   - Ensure Discord bot token is configured
   - Verify bot is connected and online

4. **Test Permissions:**
   - Try accessing admin panels
   - Verify role-based access control works

---

### 4.4 Resetting Setup (Advanced)

**⚠️ Warning:** This will reset the setup state and require re-configuration.

If you need to re-run setup (e.g., for testing):

**Option 1: Delete Database (SQLite)**
```bash
rm src/DiscordBot/DiscordBot.Blazor/DiscordBot.db
dotnet ef database update --project src/DiscordBot/DiscordBot.Blazor
```

**Option 2: Update Setup Status (Manual)**
```sql
-- Connect to database
UPDATE SetupStatus SET IsComplete = 0, CompletedAt = NULL, AdminUserId = NULL WHERE Id = 1;

-- Delete admin user (if needed)
DELETE FROM AspNetUsers WHERE Email = 'admin@example.com';
```

**Option 3: Fresh Migration**
```bash
dotnet ef database drop --project src/DiscordBot/DiscordBot.Blazor
dotnet ef database update --project src/DiscordBot/DiscordBot.Blazor
```

---

## 5. Troubleshooting

### 5.1 Common Issues

#### Issue: Setup Page Doesn't Load

**Symptoms:**
- Browser shows "Connection Refused" or "Page Not Found"
- Application fails to start

**Possible Causes:**
- Application is not running
- Incorrect URL or port
- Firewall blocking access

**Solutions:**
1. Verify application is running: `dotnet run`
2. Check console output for correct URL
3. Disable firewall temporarily
4. Try `http://localhost:5000` (non-HTTPS)

---

#### Issue: Admin Account Creation Fails

**Symptoms:**
- Error message: "Failed to create admin account"
- Validation errors on form

**Possible Causes:**
- Password doesn't meet requirements
- Email already exists
- Database connection error

**Solutions:**
1. Ensure password meets all requirements:
   - At least 6 characters
   - Contains uppercase, lowercase, and number
2. Verify email is unique
3. Check database connection string
4. Review application logs for detailed errors

---

#### Issue: Database Seeding Fails

**Symptoms:**
- Error message: "Seeding failed"
- Stuck on seeding page with errors

**Possible Causes:**
- Database permissions issue
- Roles already exist
- Database connection lost

**Solutions:**
1. Check database file permissions
2. Verify connection string in `appsettings.json`
3. Review application logs for specific error
4. Try restarting the application

---

#### Issue: Setup Marked Complete But No Admin Exists

**Symptoms:**
- Setup workflow completes
- Cannot log in (no admin account found)

**Possible Causes:**
- Transaction failure during setup
- Database rollback occurred
- Admin user was deleted manually

**Solutions:**
1. Check if admin user exists in database:
   ```sql
   SELECT * FROM AspNetUsers WHERE Email = 'your-admin-email';
   ```
2. If user doesn't exist, reset setup state and re-run
3. Check application logs for transaction errors

---

#### Issue: Cannot Access Application After Setup

**Symptoms:**
- Still redirected to setup pages
- Login page not accessible

**Possible Causes:**
- Setup status not updated
- Middleware not detecting completion

**Solutions:**
1. Verify setup status in database:
   ```sql
   SELECT * FROM SetupStatus WHERE Id = 1;
   ```
2. If `IsComplete = 0`, manually update:
   ```sql
   UPDATE SetupStatus SET IsComplete = 1, CompletedAt = CURRENT_TIMESTAMP WHERE Id = 1;
   ```
3. Restart application
4. Clear browser cache

---

### 5.2 Error Messages

| Error Message | Meaning | Solution |
|---------------|---------|----------|
| "All fields are required" | Missing required form input | Fill in all fields |
| "Invalid email address" | Email format is incorrect | Use valid email format |
| "Password must be at least 6 characters" | Password too short | Use longer password |
| "Passwords do not match" | Confirm password doesn't match | Re-enter matching password |
| "An admin account already exists" | Setup already completed | Cannot re-run setup |
| "Failed to create role: [RoleName]" | Role creation failed | Check database permissions |
| "Setup validation failed" | System state is inconsistent | Review logs, contact support |

---

### 5.3 Viewing Logs

To diagnose issues, check application logs:

**Console Output (Development):**
```bash
dotnet run
# Logs appear in console
```

**Log Files (Production):**
```bash
# Logs location depends on configuration
# Check appsettings.json for logging configuration
tail -f /var/log/discord-bot/app.log
```

**Enable Debug Logging:**
```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Debug",
      "DiscordBot": "Debug"
    }
  }
}
```

---

## 6. Security Considerations

### 6.1 Admin Account Security

**Best Practices:**

1. **Use Strong Password:**
   - Minimum 12 characters recommended (system requires 6)
   - Include uppercase, lowercase, numbers, and symbols
   - Avoid common words or patterns

2. **Secure Email Address:**
   - Use a dedicated admin email
   - Enable 2FA on the email account
   - Don't share the admin email publicly

3. **Account Protection:**
   - Change the password periodically
   - Don't share credentials
   - Use password manager to store credentials securely

### 6.2 Setup Security Features

**Built-in Protections:**

- ✅ **One-Time Setup**: Cannot be re-run without database access
- ✅ **Password Validation**: Enforces minimum security requirements
- ✅ **Auto-Confirmed Email**: Admin account is immediately active (no email confirmation needed)
- ✅ **Role Assignment**: SuperAdmin role automatically assigned
- ✅ **Atomic Operations**: All setup steps succeed or fail together (rollback on error)

### 6.3 Post-Setup Security

**After setup completes:**

1. **Change Default Ports (Production):**
   - Don't expose application on default ports
   - Use reverse proxy (Nginx, Apache)

2. **Enable HTTPS:**
   - Configure SSL certificates
   - Force HTTPS redirect

3. **Backup Database:**
   - Regular automated backups
   - Store backups securely off-site

4. **Monitor Admin Access:**
   - Review login logs regularly
   - Set up alerts for failed login attempts

---

## 7. Advanced Topics

### 7.1 Database Schema

The setup process creates and populates the following database structure:

**SetupStatus Table:**
```sql
CREATE TABLE SetupStatus (
    Id INTEGER PRIMARY KEY,
    IsComplete BIT NOT NULL DEFAULT 0,
    CompletedAt DATETIME NULL,
    AdminUserId NVARCHAR(450) NULL,
    SetupVersion NVARCHAR(10) NOT NULL,
    FOREIGN KEY (AdminUserId) REFERENCES AspNetUsers(Id) ON DELETE SET NULL
);

-- Initial seed record
INSERT INTO SetupStatus (Id, IsComplete, SetupVersion)
VALUES (1, 0, '1.0');
```

**Roles Created:**
```sql
-- SuperAdmin: Full system access
-- Admin: Administrative access
-- Moderator: Moderation capabilities
-- User: Standard user access (default)
```

---

### 7.2 Middleware Architecture

The `FirstTimeSetupMiddleware` is registered early in the middleware pipeline:

```csharp
// Program.cs
app.UseHttpsRedirection();
app.UseFirstTimeSetup();        // <-- Registered here
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
```

**Execution Flow:**
1. Request arrives
2. HTTPS redirection
3. **First Time Setup Middleware checks completion status**
4. If incomplete → Redirect to `/setup/welcome`
5. If complete → Continue to next middleware
6. Static files, authentication, etc.

---

### 7.3 Extending Setup

You can extend the setup process by modifying the `FirstTimeSetupService`:

**Example: Add Custom Seed Data**

```csharp
// FirstTimeSetupService.cs
public async Task<SeedResult> SeedDatabaseAsync()
{
    var result = new SeedResult();

    // Existing role seeding...

    // Add custom seed data
    await SeedDefaultSettings();
    await SeedSystemNotifications();

    result.Success = result.Errors.Count == 0;
    return result;
}

private async Task SeedDefaultSettings()
{
    // Add your custom seeding logic
    var setting = new ApplicationSetting
    {
        Key = "WelcomeMessage",
        Value = "Welcome to Discord Bot!"
    };

    _context.Settings.Add(setting);
    await _context.SaveChangesAsync();
}
```

---

### 7.4 Version Management

The `SetupVersion` field tracks which version of setup was completed:

**Current Version:** `1.0`

**Future Use Cases:**
- Detect when setup needs to be re-run for new features
- Migrate existing setups to new schema versions
- Provide upgrade paths for breaking changes

**Example Version Check:**
```csharp
public async Task<bool> RequiresUpgrade()
{
    var setupStatus = await _context.SetupStatus.FirstOrDefaultAsync(s => s.Id == 1);

    if (setupStatus == null || !setupStatus.IsComplete)
        return false;

    // Compare versions
    var currentVersion = new Version(setupStatus.SetupVersion);
    var latestVersion = new Version("2.0");

    return currentVersion < latestVersion;
}
```

---

## Appendix A: Configuration Reference

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=DiscordBot.db"
  },
  "Identity": {
    "PasswordRequirements": {
      "RequireDigit": true,
      "RequireLowercase": true,
      "RequireUppercase": true,
      "RequireNonAlphanumeric": false,
      "RequiredLength": 6
    }
  }
}
```

---

## Appendix B: Related Documentation

- [Implementation Plan](../03-implementation-plans/02-first-time-setup-middleware.md) - Technical implementation details
- [Database Design](../02-architecture/02-database-design.md) - Complete database schema
- [Security Architecture](../02-architecture/04-security-architecture.md) - Security design and best practices
- [User Management Guide](../03-guides/05-user-management.md) - Managing users after setup

---

## Change Log

### Version 1.0 (2025-10-20)
- Initial documentation created
- Complete workflow documentation
- Troubleshooting guide
- Security considerations
- Advanced topics

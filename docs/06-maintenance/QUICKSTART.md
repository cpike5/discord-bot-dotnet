# Quick Start: Create Admin with Discord

**Need to quickly set up your first admin account with Discord integration? Follow these steps:**

## TL;DR - Fastest Method

1. **Get your Discord ID:**
   - Discord Settings → Advanced → Enable Developer Mode
   - Right-click your username → Copy User ID

2. **Run these commands:**

```bash
# Step 1: Ensure roles exist
curl -X POST "https://localhost:5001/api/maintenance/ensure-roles"

# Step 2: Create admin with Discord (replace values below)
curl -X POST "https://localhost:5001/api/maintenance/create-admin-with-discord" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "your-email@example.com",
    "username": "admin",
    "password": "YourSecurePassword123!",
    "discordId": 123456789012345678,
    "discordUsername": "YourDiscordName"
  }'

# Step 3: Verify
curl -X GET "https://localhost:5001/api/maintenance/diagnostics"
```

**Done!** You now have an admin account linked to your Discord.

---

## Already Have an Account?

If you already created an account and just need to link Discord:

```bash
# Link Discord to existing account
curl -X POST "https://localhost:5001/api/maintenance/link-discord/by-email" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "your-email@example.com",
    "discordId": 123456789012345678,
    "discordUsername": "YourDiscordName"
  }'
```

---

## What This Does

✅ Creates an admin user account
✅ Links it to your Discord account
✅ Gives you full system access
✅ Enables bot command permissions
✅ Auto-confirms your email

---

## Next Steps

1. **Disable setup mode** after creating your admin:
   - Set `Maintenance:SetupModeEnabled: false` in appsettings.json
   - Restart the application

2. **Test bot commands** that require admin role

3. **Create additional users** through the normal registration flow

---

## Troubleshooting

**"Discord ID already linked"**
- That Discord account is already connected to another user
- Use a different Discord account or unlink the existing one

**"Setup mode is disabled"**
- Enable it in appsettings.json: `"Maintenance": { "SetupModeEnabled": true }`
- Restart the application

**"User not found"**
- For linking: Make sure the user account exists first
- For creating: Check that email/username aren't already taken

---

For complete documentation, see [Admin Setup Guide](./admin-setup-guide.md)

# Users Management Implementation Plan

**Version:** 1.0
**Created:** 2025-10-20
**Status:** Planning

---

## 1. Overview

This document outlines the implementation plan for the users management pages in the Discord Bot web application. The implementation follows the security architecture, design standards, and existing prototypes to create a comprehensive user management system for administrators.

### Objectives

- Implement administrative pages for user management
- Enable role assignment and user account management
- Display user information including Discord account linking
- Provide search, filtering, and pagination capabilities
- Follow the established design system and security model

---

## 2. Pages Required

### 2.1 Admin Users List Page
**Route:** `/Admin/Users`
**Purpose:** Display all registered users with management capabilities
**Authorization:** Requires `Admin` role

**Features:**
- Paginated table of all users
- Search by username, email, or Discord ID
- Filter by role (Admin, Moderator, Premium, User)
- Filter by status (Active, Locked)
- Display user statistics (total users, active, locked, admins)
- Quick actions: Manage Roles, Lock/Unlock account
- Show Discord account linking status
- Display last login time
- Indicate server owners/admins with special badge

### 2.2 User Details/Edit Page
**Route:** `/Admin/Users/{userId}`
**Purpose:** View and edit detailed user information
**Authorization:** Requires `Admin` role

**Features:**
- Display complete user profile
- Show Discord account details (username, ID, avatar)
- Display account linking timestamp
- List assigned roles with ability to modify
- View account creation date
- View last login timestamp
- Lock/Unlock account
- View audit history for this user
- Unlink Discord account (with confirmation)

### 2.3 Manage User Roles Modal
**Component:** `ManageUserRolesModal.razor`
**Purpose:** Assign/revoke roles for a specific user
**Triggered from:** Users list page or user details page

**Features:**
- Display current user roles
- Checkbox list of available roles
- Warning for Admin role changes
- Confirmation before changes
- Success/error feedback
- Real-time role cache invalidation

---

## 3. Components to Create

### 3.1 Page Components

| Component Name | Location | Purpose |
|----------------|----------|---------|
| `UsersList.razor` | `Components/Pages/Admin/` | Main users list page |
| `UserDetails.razor` | `Components/Pages/Admin/` | User details/edit page |
| `ManageUserRolesModal.razor` | `Components/Admin/` | Modal for role management |
| `UserSearchBar.razor` | `Components/Admin/` | Search and filter controls |
| `UserStatsCards.razor` | `Components/Admin/` | Statistics cards |
| `UserTable.razor` | `Components/Admin/` | Reusable user table component |
| `UserTableRow.razor` | `Components/Admin/` | Individual user row |
| `DiscordAccountBadge.razor` | `Components/Shared/` | Discord account linking indicator |

### 3.2 Shared UI Components

| Component Name | Location | Purpose |
|----------------|----------|---------|
| `Modal.razor` | `Components/Shared/` | Reusable modal wrapper |
| `ConfirmDialog.razor` | `Components/Shared/` | Confirmation dialog |
| `Badge.razor` | `Components/Shared/` | Reusable badge component |
| `Pagination.razor` | `Components/Shared/` | Pagination controls |
| `LoadingSpinner.razor` | `Components/Shared/` | Loading indicator |
| `EmptyState.razor` | `Components/Shared/` | Empty state display |

---

## 4. Services and Repositories

### 4.1 New Services Required

#### IUserManagementService
**Location:** `DiscordBot.Core/Services/IUserManagementService.cs`

```csharp
public interface IUserManagementService
{
    // User retrieval
    Task<PagedResult<UserDto>> GetUsersAsync(UserSearchCriteria criteria);
    Task<UserDetailDto?> GetUserByIdAsync(string userId);
    Task<UserStatistics> GetUserStatisticsAsync();

    // Role management
    Task<bool> AssignRoleAsync(string userId, string roleName, string adminUserId);
    Task<bool> RevokeRoleAsync(string userId, string roleName, string adminUserId);
    Task<IEnumerable<string>> GetUserRolesAsync(string userId);
    Task<IEnumerable<RoleDto>> GetAvailableRolesAsync();

    // Account management
    Task<bool> LockUserAsync(string userId, string adminUserId, string reason);
    Task<bool> UnlockUserAsync(string userId, string adminUserId);
    Task<bool> UnlinkDiscordAccountAsync(string userId, string adminUserId);

    // Audit
    Task<IEnumerable<AuditLogEntry>> GetUserAuditLogAsync(string userId);
}
```

#### IUserRepository
**Location:** `DiscordBot.Core/Repositories/IUserRepository.cs`

```csharp
public interface IUserRepository
{
    Task<PagedResult<ApplicationUser>> GetUsersAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        string? roleFilter = null,
        bool? isLockedFilter = null);

    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetActiveUsersCountAsync();
    Task<int> GetUsersInRoleCountAsync(string roleName);
    Task<int> GetLockedUsersCountAsync();
}
```

### 4.2 DTOs Required

**Location:** `DiscordBot.Core/DTOs/`

```csharp
public class UserDto
{
    public string Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Discord info
    public ulong? DiscordUserId { get; set; }
    public string? DiscordUsername { get; set; }
    public string? DiscordAvatarUrl { get; set; }
    public DateTime? DiscordLinkedAt { get; set; }

    // Roles
    public IEnumerable<string> Roles { get; set; }

    // Server owner/admin flag
    public bool IsDiscordServerOwner { get; set; }
    public bool IsDiscordServerAdmin { get; set; }
}

public class UserDetailDto : UserDto
{
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public int AccessFailedCount { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class UserSearchCriteria
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public string? RoleFilter { get; set; }
    public bool? IsLockedFilter { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}

public class UserStatistics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int Administrators { get; set; }
    public int LockedAccounts { get; set; }
}

public class RoleDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public int UserCount { get; set; }
}

public class AuditLogEntry
{
    public DateTime Timestamp { get; set; }
    public string Action { get; set; }
    public string? PerformedBy { get; set; }
    public string? Details { get; set; }
}
```

---

## 5. Implementation Steps

### Phase 1: Foundation (2-3 days)

**Step 1.1: Create DTOs and Models**
- [ ] Create `UserDto`, `UserDetailDto` in `Core/DTOs/`
- [ ] Create `PagedResult<T>` generic class
- [ ] Create `UserSearchCriteria` class
- [ ] Create `UserStatistics` class
- [ ] Create `RoleDto` class

**Step 1.2: Create Service Interfaces**
- [ ] Create `IUserManagementService` interface
- [ ] Create `IUserRepository` interface
- [ ] Add XML documentation comments

**Step 1.3: Implement Repositories**
- [ ] Implement `UserRepository` in `Blazor/Repositories/`
- [ ] Add efficient queries with includes
- [ ] Add pagination support
- [ ] Add search and filtering
- [ ] Add unit tests

**Step 1.4: Implement Services**
- [ ] Implement `UserManagementService` in `Blazor/Services/`
- [ ] Add role assignment/revocation logic
- [ ] Add account lock/unlock logic
- [ ] Integrate with `UserManager<ApplicationUser>`
- [ ] Integrate with `RoleManager<IdentityRole>`
- [ ] Add audit logging
- [ ] Add unit tests

**Step 1.5: Register Services**
- [ ] Register in `Program.cs`
- [ ] Configure DI lifetimes

### Phase 2: Shared Components (2-3 days)

**Step 2.1: Create Reusable UI Components**
- [ ] Create `Modal.razor` based on prototype
- [ ] Create `ConfirmDialog.razor`
- [ ] Create `Badge.razor` with color variants
- [ ] Create `Pagination.razor`
- [ ] Create `LoadingSpinner.razor`
- [ ] Create `EmptyState.razor`
- [ ] Add CSS files for each component

**Step 2.2: Create Domain Components**
- [ ] Create `DiscordAccountBadge.razor`
- [ ] Create `UserStatsCards.razor`
- [ ] Create `UserSearchBar.razor`
- [ ] Add CSS files for styling

### Phase 3: Users List Page (2-3 days)

**Step 3.1: Create Users List Page**
- [ ] Create `UsersList.razor` in `Components/Pages/Admin/`
- [ ] Add authorization (`@attribute [Authorize(Roles = "Admin")]`)
- [ ] Implement search functionality
- [ ] Implement filtering (role, status)
- [ ] Implement pagination
- [ ] Add statistics cards
- [ ] Add user table with sorting

**Step 3.2: Create User Table Components**
- [ ] Create `UserTable.razor`
- [ ] Create `UserTableRow.razor`
- [ ] Add action buttons (Manage Roles, Lock/Unlock)
- [ ] Add Discord badge indicators
- [ ] Add role badges
- [ ] Add responsive design

**Step 3.3: Add Routing**
- [ ] Update `Routes.razor` or navigation config
- [ ] Add to admin navigation menu
- [ ] Test navigation

### Phase 4: User Details Page (2 days)

**Step 4.1: Create User Details Page**
- [ ] Create `UserDetails.razor` in `Components/Pages/Admin/`
- [ ] Add route parameter for `userId`
- [ ] Display user profile information
- [ ] Display Discord account details
- [ ] Display assigned roles
- [ ] Add action buttons

**Step 4.2: Add Edit Capabilities**
- [ ] Add Lock/Unlock functionality
- [ ] Add Unlink Discord Account functionality
- [ ] Add confirmation dialogs
- [ ] Add success/error messaging

### Phase 5: Role Management Modal (2 days)

**Step 5.1: Create Role Management Modal**
- [ ] Create `ManageUserRolesModal.razor`
- [ ] Display current roles
- [ ] Display available roles as checkboxes
- [ ] Add role descriptions
- [ ] Show user counts per role

**Step 5.2: Implement Role Assignment**
- [ ] Handle role assignment
- [ ] Handle role revocation
- [ ] Add confirmation for Admin role changes
- [ ] Show loading state during save
- [ ] Invalidate role cache after changes
- [ ] Add success/error feedback

### Phase 6: Styling and Design (1-2 days)

**Step 6.1: Apply Design System**
- [ ] Apply design tokens from `design-standards.md`
- [ ] Match prototype styling from `admin-users.html`
- [ ] Ensure color consistency
- [ ] Apply proper spacing
- [ ] Add hover states and transitions

**Step 6.2: Responsive Design**
- [ ] Test on mobile devices
- [ ] Adjust table for small screens
- [ ] Make modals responsive
- [ ] Test sidebar collapse

### Phase 7: Testing and Polish (2-3 days)

**Step 7.1: Unit Tests**
- [ ] Test `UserManagementService`
- [ ] Test `UserRepository`
- [ ] Mock dependencies
- [ ] Test edge cases

**Step 7.2: Integration Tests**
- [ ] Test full user management flow
- [ ] Test role assignment flow
- [ ] Test search and filtering
- [ ] Test pagination

**Step 7.3: Manual Testing**
- [ ] Test with real data
- [ ] Test as different user roles
- [ ] Test error scenarios
- [ ] Test loading states
- [ ] Test with locked accounts
- [ ] Test with unlinked Discord accounts

**Step 7.4: Security Testing**
- [ ] Verify authorization on all pages
- [ ] Test direct URL access without permissions
- [ ] Verify admin role requirement
- [ ] Test CSRF protection
- [ ] Test SQL injection prevention

**Step 7.5: Polish**
- [ ] Add loading indicators
- [ ] Improve error messages
- [ ] Add tooltips where needed
- [ ] Ensure consistent messaging
- [ ] Add keyboard navigation support

---

## 6. Technical Specifications

### 6.1 Pagination

- Default page size: 20 users
- Page size options: 10, 20, 50, 100
- Show total count and current range
- Previous/Next buttons + page numbers
- Jump to first/last page

### 6.2 Search

- Debounce input: 300ms
- Search fields: Username, Email, Discord Username, Discord ID
- Case-insensitive search
- Clear search button

### 6.3 Filtering

- Filter by Role: All, Admin, Moderator, Premium, User
- Filter by Status: All, Active, Locked
- Filters combine with AND logic
- Persist filters in URL query params

### 6.4 Sorting

- Default: Created date descending
- Sortable columns: Username, Email, Created Date, Last Login
- Toggle ascending/descending
- Visual indicator for sorted column

### 6.5 Caching

- Use `IMemoryCache` for role cache (already implemented)
- Cache user statistics for 5 minutes
- Invalidate cache on role changes
- Invalidate cache on account lock/unlock

### 6.6 Error Handling

- Display user-friendly error messages
- Log errors server-side
- Show retry option for transient failures
- Graceful degradation for missing data

---

## 7. UI/UX Specifications

### 7.1 Layout

```
┌─────────────────────────────────────────────────────┐
│ Sidebar │ Page Header                               │
│         │ ┌─────────────────────────────────────┐   │
│ Nav     │ │ Statistics Cards (4 columns)        │   │
│ Menu    │ └─────────────────────────────────────┘   │
│         │ ┌─────────────────────────────────────┐   │
│         │ │ Toolbar (Search + Filters)          │   │
│         │ └─────────────────────────────────────┘   │
│         │ ┌─────────────────────────────────────┐   │
│         │ │                                     │   │
│         │ │ Users Table                         │   │
│         │ │                                     │   │
│         │ └─────────────────────────────────────┘   │
│         │ ┌─────────────────────────────────────┐   │
│         │ │ Pagination                          │   │
│         │ └─────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

### 7.2 Color Usage

- **Primary (Rose #DA4167)**: Action buttons, active states
- **Slate (#555B6E)**: Sidebar, navigation
- **Sage (#87B38D)**: Success states, active badges
- **Lavender (#D3C4E3)**: Secondary info, backgrounds
- **Gold (#FED766)**: Warnings, server owner/admin badges
- **Neutral scale**: Text, borders, backgrounds

### 7.3 Typography

- Page title: `--text-3xl` (30px), `--font-bold`
- Section headers: `--text-xl` (20px), `--font-semibold`
- Body text: `--text-base` (16px), `--font-normal`
- Table text: `--text-sm` (14px)
- Helper text: `--text-xs` (12px), `--text-secondary` color

### 7.4 Spacing

- Page padding: `--space-6` (24px)
- Section spacing: `--space-6` (24px)
- Card padding: `--space-5` (20px)
- Element gaps: `--space-4` (16px)
- Tight spacing: `--space-2` (8px)

### 7.5 Interactive States

- Buttons: hover transform + shadow
- Table rows: hover background change
- Links: color change on hover
- Transitions: `--transition-base` (250ms ease-in-out)

---

## 8. Security Considerations

### 8.1 Authorization

- All admin pages require `[Authorize(Roles = "Admin")]`
- Service methods validate caller has Admin role
- Prevent admins from removing their own Admin role
- Enforce minimum 1 admin user in system

### 8.2 Audit Logging

Log the following actions:
- Role assigned/revoked (target user, role, admin user, timestamp)
- Account locked/unlocked (target user, admin user, reason, timestamp)
- Discord account unlinked (target user, admin user, timestamp)
- Failed authorization attempts

### 8.3 Data Protection

- Never expose password hashes
- Limit sensitive data in DTOs
- Use HTTPS for all requests
- Implement CSRF protection (built into Blazor)
- Sanitize user input

### 8.4 Rate Limiting

- Consider rate limiting on role changes (prevent abuse)
- Log excessive failed authorization attempts
- Implement cooldown on bulk operations

---

## 9. API Endpoints (if needed)

If creating separate API controllers:

```
GET    /api/admin/users                 - Get paginated users
GET    /api/admin/users/{id}            - Get user details
GET    /api/admin/users/statistics      - Get user statistics
POST   /api/admin/users/{id}/roles      - Assign role
DELETE /api/admin/users/{id}/roles      - Revoke role
POST   /api/admin/users/{id}/lock       - Lock account
POST   /api/admin/users/{id}/unlock     - Unlock account
DELETE /api/admin/users/{id}/discord    - Unlink Discord account
GET    /api/admin/users/{id}/audit      - Get audit log
```

**Note**: For Blazor Server, may not need separate API endpoints. Services can be called directly from components.

---

## 10. Database Considerations

### 10.1 Indexes Required

Already in place from migrations:
- Index on `ApplicationUser.DiscordUserId` (unique)
- Index on `ApplicationUser.DiscordUsername`

Consider adding:
- Index on `ApplicationUser.Email`
- Index on `ApplicationUser.LastLoginAt` (for sorting)
- Composite index on frequently filtered columns

### 10.2 Query Optimization

- Use `.AsNoTracking()` for read-only queries
- Include related data with `.Include()` to avoid N+1 queries
- Use projections to select only needed fields
- Consider compiled queries for frequently used queries

---

## 11. Testing Strategy

### 11.1 Unit Tests

**UserManagementService Tests:**
- `GetUsersAsync_WithSearchTerm_ReturnsFilteredResults`
- `AssignRoleAsync_ValidUser_ReturnsTrue`
- `AssignRoleAsync_InvalidUser_ReturnsFalse`
- `RevokeRoleAsync_LastAdmin_ThrowsException`
- `LockUserAsync_ValidUser_ReturnsTrue`
- `UnlinkDiscordAccountAsync_ValidUser_ReturnsTrue`

**UserRepository Tests:**
- `GetUsersAsync_WithPagination_ReturnsCorrectPage`
- `GetUsersAsync_WithRoleFilter_ReturnsFilteredResults`
- `GetUserStatisticsAsync_ReturnsCorrectCounts`

### 11.2 Integration Tests

- Full user list page load and rendering
- Search functionality end-to-end
- Role assignment flow
- Account lock/unlock flow
- Pagination navigation

### 11.3 UI Tests (Manual)

- Test with 0 users (empty state)
- Test with 1000+ users (performance)
- Test search with no results
- Test all filter combinations
- Test modal interactions
- Test on different screen sizes
- Test with different user roles

---

## 12. Performance Targets

- Page load time: < 1 second (for 20 users)
- Search debounce: 300ms
- API response time: < 500ms
- Database query time: < 200ms
- Support up to 10,000 users without pagination issues

---

## 13. Future Enhancements

### Phase 2 Features (Post-MVP)

- Bulk actions (assign role to multiple users)
- Export users to CSV/Excel
- Advanced filtering (date ranges, multiple roles)
- User impersonation (for support)
- Detailed audit log viewer
- User activity timeline
- Email verification management
- Two-factor authentication management
- Discord account sync (update usernames)
- User deletion (with data retention policy)
- User merge (combine duplicate accounts)

---

## 14. Dependencies

### NuGet Packages

Already installed:
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.Extensions.Caching.Memory`

May need:
- None additional required for basic implementation

### External Services

- Discord API (for avatar URLs)
- Email service (for notifications) - future

---

## 15. File Structure

```
src/DiscordBot/
├── DiscordBot.Core/
│   ├── DTOs/
│   │   ├── UserDto.cs
│   │   ├── UserDetailDto.cs
│   │   ├── PagedResult.cs
│   │   ├── UserSearchCriteria.cs
│   │   ├── UserStatistics.cs
│   │   ├── RoleDto.cs
│   │   └── AuditLogEntry.cs
│   ├── Services/
│   │   └── IUserManagementService.cs
│   └── Repositories/
│       └── IUserRepository.cs
│
├── DiscordBot.Blazor/
│   ├── Services/
│   │   └── UserManagementService.cs
│   ├── Repositories/
│   │   └── UserRepository.cs
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── Admin/
│   │   │       ├── UsersList.razor
│   │   │       ├── UsersList.razor.css
│   │   │       ├── UserDetails.razor
│   │   │       └── UserDetails.razor.css
│   │   ├── Admin/
│   │   │   ├── ManageUserRolesModal.razor
│   │   │   ├── ManageUserRolesModal.razor.css
│   │   │   ├── UserStatsCards.razor
│   │   │   ├── UserStatsCards.razor.css
│   │   │   ├── UserSearchBar.razor
│   │   │   ├── UserSearchBar.razor.css
│   │   │   ├── UserTable.razor
│   │   │   ├── UserTable.razor.css
│   │   │   ├── UserTableRow.razor
│   │   │   └── UserTableRow.razor.css
│   │   └── Shared/
│   │       ├── Modal.razor
│   │       ├── Modal.razor.css
│   │       ├── ConfirmDialog.razor
│   │       ├── ConfirmDialog.razor.css
│   │       ├── Badge.razor
│   │       ├── Badge.razor.css
│   │       ├── Pagination.razor
│   │       ├── Pagination.razor.css
│   │       ├── LoadingSpinner.razor
│   │       ├── LoadingSpinner.razor.css
│   │       ├── EmptyState.razor
│   │       ├── EmptyState.razor.css
│   │       ├── DiscordAccountBadge.razor
│   │       └── DiscordAccountBadge.razor.css
│   └── wwwroot/
│       └── css/
│           └── admin.css (if needed for shared admin styles)
```

---

## 16. Acceptance Criteria

### Must Have (MVP)

- [x] Admin can view paginated list of all users
- [x] Admin can search users by name, email, or Discord ID
- [x] Admin can filter users by role
- [x] Admin can filter users by account status (active/locked)
- [x] Admin can view user statistics on dashboard
- [x] Admin can assign roles to users
- [x] Admin can revoke roles from users
- [x] Admin can lock user accounts
- [x] Admin can unlock user accounts
- [x] System prevents removing last admin
- [x] Discord server owners/admins are visually indicated
- [x] All actions are logged for audit
- [x] Non-admin users cannot access admin pages
- [x] UI follows design standards
- [x] UI is responsive

### Should Have (Post-MVP)

- [ ] Admin can view detailed user profile
- [ ] Admin can view user audit history
- [ ] Admin can unlink Discord accounts
- [ ] Admin can sort users by different columns
- [ ] System sends notifications on role changes
- [ ] Bulk role assignment
- [ ] Export user list

### Nice to Have (Future)

- [ ] User activity timeline
- [ ] Advanced search with multiple criteria
- [ ] User impersonation
- [ ] Account merging
- [ ] Automated role expiration

---

## 17. Risks and Mitigations

| Risk | Impact | Likelihood | Mitigation |
|------|--------|-----------|------------|
| Performance issues with large user base | High | Medium | Implement efficient queries, pagination, indexing |
| Admin accidentally removes all admins | High | Low | Prevent last admin removal, confirmation dialogs |
| Unauthorized access to admin pages | High | Low | Proper authorization attributes, security testing |
| Role cache not invalidated | Medium | Medium | Implement cache invalidation on all role changes |
| UI doesn't match design standards | Low | Medium | Regular design reviews, use design tokens |
| Complex state management in modals | Medium | Medium | Use proper Blazor state management patterns |

---

## 18. Success Metrics

- Page load time < 1 second for 20 users
- Search results returned < 500ms
- Zero unauthorized access incidents
- Admin tasks completed in < 5 clicks
- User satisfaction score > 4/5
- < 5% error rate on role assignments
- 100% test coverage on critical paths

---

## 19. Timeline Estimate

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| Phase 1: Foundation | 2-3 days | None |
| Phase 2: Shared Components | 2-3 days | Phase 1 |
| Phase 3: Users List Page | 2-3 days | Phase 1, Phase 2 |
| Phase 4: User Details Page | 2 days | Phase 1, Phase 2 |
| Phase 5: Role Management Modal | 2 days | Phase 1, Phase 2 |
| Phase 6: Styling and Design | 1-2 days | Phase 3, Phase 4, Phase 5 |
| Phase 7: Testing and Polish | 2-3 days | All previous phases |
| **Total** | **13-18 days** | |

**Note:** This is an estimate assuming a single developer working full-time. Adjust based on team size and availability.

---

## 20. References

- [Security Architecture](../02-architecture/04-security-architecture.md)
- [Design Standards](../01-planning/04-design-standards.md)
- [Functional Specifications](../01-planning/02-functional-specs.md)
- [Database Design](../02-architecture/02-database-design.md)
- Prototype: [admin-users.html](../../prototypes/pages/admin-users.html)
- Prototype: [admin-roles.html](../../prototypes/pages/admin-roles.html)

---

## 21. Approval

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Project Lead | | | |
| Technical Lead | | | |
| UX/UI Designer | | | |

---

**Document Control:**
- **Created By:** System Architect
- **Last Updated:** 2025-10-20
- **Next Review:** After Phase 1 completion

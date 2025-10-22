# Invite Code Management Implementation Plan

**Version:** 1.0
**Created:** 2025-10-20
**Status:** Planning

---

## 1. Overview

This document outlines the implementation plan for the invite code management pages in the Discord Bot web application. The implementation enables administrators to manage Discord invite codes used for user registration, including generation, revocation, monitoring, and analytics.

### Objectives

- Implement administrative pages for invite code management
- Enable invite code generation with customizable expiration times
- Provide code revocation and deletion capabilities
- Display comprehensive statistics and analytics
- Implement search, filtering, and pagination
- Follow the established design system and security model

---

## 2. Current Implementation Status

### 2.1 Already Implemented ✅

**Backend Models:**
- `InviteCode` entity with all required properties ([InviteCode.cs](../../src/DiscordBot/DiscordBot.Core/Entities/InviteCode.cs))
- `ApplicationUser` entity with Discord integration ([ApplicationUser.cs](../../src/DiscordBot/DiscordBot.Core/Entities/ApplicationUser.cs))

**Backend Services:**
- `IInviteCodeService` interface ([IInviteCodeService.cs](../../src/DiscordBot/DiscordBot.Core/Services/IInviteCodeService.cs))
- `InviteCodeService` implementation ([InviteCodeService.cs](../../src/DiscordBot/DiscordBot.Blazor/Services/InviteCodeService.cs))
- Cryptographically secure code generation (XXXX-XXXX-XXXX format)
- Code validation and expiration logic
- Code revocation functionality
- User registration checking
- Cleanup for expired codes

**Backend Repositories:**
- `IInviteCodeRepository` interface
- `InviteCodeRepository` implementation

### 2.2 To Be Implemented 🔨

**Backend Enhancements:**
- Pagination support for invite codes
- Search and filtering by status/username/code
- Statistics aggregation methods
- Bulk operations support (optional)

**Frontend UI:**
- Admin invite codes page
- Statistics dashboard cards
- Data table with filtering
- Generate code modal
- Revoke/delete confirmation modals
- Export functionality (optional)

---

## 3. Pages Required

### 3.1 Admin Invite Codes Page
**Route:** `/Admin/InviteCodes`
**Purpose:** Display and manage all invite codes
**Authorization:** Requires `Admin` or `SuperAdmin` role

**Features:**
- Statistics cards showing:
  - Active codes count
  - Used codes count
  - Expired codes count
  - Revoked codes count
- Search bar (by code or Discord username)
- Status filter dropdown (All, Active, Used, Expired, Revoked)
- Paginated table with columns:
  - Code (formatted as `XXXX-XXXX-XXXX`)
  - Discord User (avatar + username)
  - Created Date
  - Expires Date
  - Status Badge (color-coded)
  - Actions (Copy, Revoke, Delete)
- Pagination controls
- "Generate Code" button
- "Export" button (optional)

### 3.2 Generate Code Modal
**Component:** `GenerateCodeModal.razor`
**Purpose:** Create new invite codes for Discord users
**Triggered from:** Main invite codes page

**Features:**
- Discord User input field (username or ID)
- Expiration time selector:
  - 24 hours (default)
  - 48 hours
  - 72 hours
  - 7 days
  - Custom (date picker)
- Notes field (optional, for admin reference)
- Form validation
- Success/error feedback
- Copy code to clipboard on generation

### 3.3 Revoke Code Modal
**Component:** `RevokeCodeModal.razor`
**Purpose:** Confirm code revocation
**Triggered from:** Table row action button

**Features:**
- Display code being revoked
- Show Discord user associated with code
- Warning message about irreversibility
- Confirmation buttons
- Success/error feedback

---

## 4. Components to Create

### 4.1 Page Components

| Component Name | Location | Purpose |
|----------------|----------|---------|
| `InviteCodes.razor` | `Components/Pages/Admin/` | Main invite codes page |
| `InviteCodes.razor.css` | `Components/Pages/Admin/` | Page-specific styling |

### 4.2 Shared Components for Invite Codes

| Component Name | Location | Purpose |
|----------------|----------|---------|
| `InviteCodeTable.razor` | `Components/Admin/InviteCodes/` | Reusable invite code table |
| `InviteCodeTable.razor.css` | `Components/Admin/InviteCodes/` | Table styling |
| `InviteCodeStatsCard.razor` | `Components/Admin/InviteCodes/` | Statistics card component |
| `InviteCodeStatsCard.razor.css` | `Components/Admin/InviteCodes/` | Stats card styling |
| `GenerateCodeModal.razor` | `Components/Admin/InviteCodes/` | Generate code modal |
| `GenerateCodeModal.razor.css` | `Components/Admin/InviteCodes/` | Modal styling |
| `RevokeCodeModal.razor` | `Components/Admin/InviteCodes/` | Revoke confirmation modal |
| `RevokeCodeModal.razor.css` | `Components/Admin/InviteCodes/` | Modal styling |
| `CodeBadge.razor` | `Components/Shared/` | Formatted code display |
| `StatusBadge.razor` | `Components/Shared/` | Status badge (reusable) |

### 4.3 Reusable UI Components

**Note:** Some of these may already exist from user management implementation:

| Component Name | Location | Purpose |
|----------------|----------|---------|
| `Modal.razor` | `Components/Shared/` | Reusable modal wrapper |
| `ConfirmDialog.razor` | `Components/Shared/` | Confirmation dialog |
| `Pagination.razor` | `Components/Shared/` | Pagination controls |
| `LoadingSpinner.razor` | `Components/Shared/` | Loading indicator |
| `EmptyState.razor` | `Components/Shared/` | Empty state display |

---

## 5. Services and Repositories

### 5.1 Service Enhancements Required

**Extend `IInviteCodeService`:**

```csharp
// Add to IInviteCodeService interface

/// <summary>
/// Retrieves a paginated list of invite codes with optional filtering.
/// </summary>
Task<PagedResult<InviteCode>> GetCodesPagedAsync(
    int page,
    int pageSize,
    string? status = null,
    string? searchTerm = null,
    string? sortBy = null,
    bool sortDescending = false);

/// <summary>
/// Gets statistics about invite codes for admin dashboard.
/// </summary>
Task<InviteCodeStatistics> GetStatisticsAsync();

/// <summary>
/// Generates invite code for a Discord user by Discord user ID.
/// Admin-initiated code generation.
/// </summary>
Task<InviteCode> GenerateCodeForUserAsync(
    ulong discordUserId,
    string discordUsername,
    int expirationHours,
    string? notes = null);

/// <summary>
/// Deletes expired invite codes permanently.
/// </summary>
Task<bool> DeleteExpiredCodeAsync(string code);

/// <summary>
/// Exports invite codes to CSV format.
/// </summary>
Task<byte[]> ExportToCsvAsync(string? status = null);
```

### 5.2 Repository Enhancements Required

**Extend `IInviteCodeRepository`:**

```csharp
// Add to IInviteCodeRepository interface

/// <summary>
/// Gets paginated invite codes with filtering and sorting.
/// </summary>
Task<(IEnumerable<InviteCode> Codes, int TotalCount)> GetPagedAsync(
    int skip,
    int take,
    string? statusFilter = null,
    string? searchTerm = null,
    string? sortBy = null,
    bool sortDescending = false);

/// <summary>
/// Gets invite code statistics.
/// </summary>
Task<InviteCodeStatistics> GetStatisticsAsync();

/// <summary>
/// Deletes a specific invite code by code string.
/// </summary>
Task<bool> DeleteByCodeAsync(string code);
```

### 5.3 DTOs Required

**Location:** `DiscordBot.Core/DTOs/`

```csharp
/// <summary>
/// Statistics about invite codes for admin dashboard.
/// </summary>
public class InviteCodeStatistics
{
    /// <summary>
    /// Number of active (unused, non-expired) codes.
    /// </summary>
    public int ActiveCount { get; set; }

    /// <summary>
    /// Number of used codes.
    /// </summary>
    public int UsedCount { get; set; }

    /// <summary>
    /// Number of expired codes.
    /// </summary>
    public int ExpiredCount { get; set; }

    /// <summary>
    /// Number of revoked codes.
    /// </summary>
    public int RevokedCount { get; set; }

    /// <summary>
    /// Total number of codes ever generated.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Codes generated in the last 7 days.
    /// </summary>
    public int GeneratedLast7Days { get; set; }

    /// <summary>
    /// Codes used in the last 7 days.
    /// </summary>
    public int UsedLast7Days { get; set; }
}

/// <summary>
/// Paged result wrapper for collections.
/// </summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling(TotalCount / (double)PageSize)
        : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

/// <summary>
/// Status enum for filtering.
/// </summary>
public enum InviteCodeStatus
{
    All,
    Active,
    Used,
    Expired,
    Revoked
}
```

---

## 6. Implementation Steps

### Phase 1: Backend Enhancements (1-2 days)

**Step 1.1: Create DTOs**
- [ ] Create `InviteCodeStatistics.cs` in `Core/DTOs/`
- [ ] Create `PagedResult.cs` in `Core/DTOs/` (if not already exists)
- [ ] Create `InviteCodeStatus.cs` enum in `Core/DTOs/`
- [ ] Add XML documentation comments

**Step 1.2: Extend Repository**
- [ ] Add `GetPagedAsync()` method to `IInviteCodeRepository`
- [ ] Add `GetStatisticsAsync()` method to `IInviteCodeRepository`
- [ ] Add `DeleteByCodeAsync()` method to `IInviteCodeRepository`
- [ ] Implement methods in `InviteCodeRepository`
- [ ] Add efficient LINQ queries with proper indexing
- [ ] Add unit tests for repository methods

**Step 1.3: Extend Service**
- [ ] Add pagination methods to `IInviteCodeService`
- [ ] Add statistics method to `IInviteCodeService`
- [ ] Add admin-specific code generation method
- [ ] Add delete method for expired codes
- [ ] Add export to CSV method (optional)
- [ ] Implement methods in `InviteCodeService`
- [ ] Add logging for new operations
- [ ] Add unit tests for service methods

**Step 1.4: Verify Existing Functionality**
- [ ] Test code generation
- [ ] Test code validation
- [ ] Test code revocation
- [ ] Test expiration logic
- [ ] Test cleanup method

### Phase 2: Shared Components (2-3 days)

**Step 2.1: Create/Verify Reusable UI Components**
- [ ] Create or verify `Modal.razor` exists
- [ ] Create or verify `ConfirmDialog.razor` exists
- [ ] Create or verify `Pagination.razor` exists
- [ ] Create or verify `LoadingSpinner.razor` exists
- [ ] Create or verify `EmptyState.razor` exists
- [ ] Add CSS files for each component
- [ ] Test components in isolation

**Step 2.2: Create Domain-Specific Components**
- [ ] Create `CodeBadge.razor` (monospace font, copy button)
- [ ] Create `StatusBadge.razor` (color-coded status indicator)
- [ ] Create `InviteCodeStatsCard.razor`
- [ ] Add CSS files for styling
- [ ] Match design tokens from prototype

### Phase 3: Main Invite Codes Page (2-3 days)

**Step 3.1: Create Page Structure**
- [ ] Create `InviteCodes.razor` in `Components/Pages/Admin/`
- [ ] Add authorization (`@attribute [Authorize(Roles = "Admin,SuperAdmin")]`)
- [ ] Create page layout (header, stats, search, table, pagination)
- [ ] Inject `IInviteCodeService` dependency
- [ ] Add page metadata and title

**Step 3.2: Implement Statistics Cards**
- [ ] Load statistics on page init
- [ ] Display 4 stat cards (Active, Used, Expired, Revoked)
- [ ] Add loading state
- [ ] Add error handling
- [ ] Apply design tokens for styling

**Step 3.3: Implement Search and Filters**
- [ ] Create search input with debouncing (300ms)
- [ ] Create status filter dropdown
- [ ] Wire up filter change handlers
- [ ] Reset to page 1 on filter change
- [ ] Add "Clear" button to reset filters

**Step 3.4: Implement Invite Code Table**
- [ ] Create `InviteCodeTable.razor` component
- [ ] Add table columns (Code, Discord User, Created, Expires, Status, Actions)
- [ ] Display user avatars (from Discord)
- [ ] Format dates consistently
- [ ] Add status badges with colors
- [ ] Add action buttons (Copy, Revoke/Delete)
- [ ] Add hover states
- [ ] Make table responsive

**Step 3.5: Implement Pagination**
- [ ] Add pagination component
- [ ] Wire up previous/next handlers
- [ ] Display page info (showing X-Y of Z)
- [ ] Handle edge cases (first/last page)
- [ ] Preserve filters when changing pages

**Step 3.6: Create Scoped CSS**
- [ ] Create `InviteCodes.razor.css`
- [ ] Apply design tokens from `tokens.css`
- [ ] Match prototype styling
- [ ] Ensure responsive design
- [ ] Add transitions and hover effects

### Phase 4: Generate Code Modal (2 days)

**Step 4.1: Create Modal Component**
- [ ] Create `GenerateCodeModal.razor`
- [ ] Create modal layout (header, body, footer)
- [ ] Add form fields (Discord user, expiration, notes)
- [ ] Add form validation
- [ ] Style with design tokens

**Step 4.2: Implement Form Logic**
- [ ] Add Discord user lookup/validation
- [ ] Add expiration time selector
- [ ] Handle form submission
- [ ] Call `IInviteCodeService.GenerateCodeAsync()`
- [ ] Display generated code
- [ ] Add "Copy to Clipboard" functionality
- [ ] Add success/error messaging
- [ ] Clear form on close

**Step 4.3: Add Modal Interactions**
- [ ] Open modal on button click
- [ ] Close on cancel
- [ ] Close on successful generation
- [ ] Close on Escape key
- [ ] Close on overlay click
- [ ] Prevent body scroll when open
- [ ] Add loading state during generation

### Phase 5: Revoke/Delete Modals (1 day)

**Step 5.1: Create Revoke Modal**
- [ ] Create `RevokeCodeModal.razor`
- [ ] Display code being revoked
- [ ] Display Discord user info
- [ ] Add warning message
- [ ] Add confirmation buttons
- [ ] Call `IInviteCodeService.RevokeCodeAsync()`
- [ ] Add success/error messaging

**Step 5.2: Create Delete Modal**
- [ ] Create confirmation dialog for deletion
- [ ] Display code being deleted
- [ ] Add warning (permanent action)
- [ ] Add confirmation buttons
- [ ] Call delete service method
- [ ] Refresh table on success

**Step 5.3: Wire Up Modals**
- [ ] Open modals from table actions
- [ ] Pass code data to modals
- [ ] Refresh table after operations
- [ ] Update statistics after operations

### Phase 6: Additional Features (1-2 days)

**Step 6.1: Copy to Clipboard**
- [ ] Add copy button for each code
- [ ] Use JavaScript interop for clipboard API
- [ ] Show "Copied!" tooltip/message
- [ ] Handle copy failures gracefully

**Step 6.2: Export Functionality (Optional)**
- [ ] Add "Export" button
- [ ] Generate CSV of invite codes
- [ ] Download file with appropriate headers
- [ ] Filter exported data by current filters

**Step 6.3: Sorting (Optional)**
- [ ] Add sortable column headers
- [ ] Implement sort by Created, Expires, Status
- [ ] Add visual indicators for sorted column
- [ ] Toggle ascending/descending

### Phase 7: Integration and Navigation (1 day)

**Step 7.1: Add Navigation Menu Item**
- [ ] Update sidebar navigation
- [ ] Add "Invite Codes" link with icon (🎫)
- [ ] Add badge showing active code count
- [ ] Highlight active menu item

**Step 7.2: Update Routing**
- [ ] Verify route is registered
- [ ] Test direct URL access
- [ ] Test navigation from other pages

**Step 7.3: Verify Authorization**
- [ ] Test as Admin user
- [ ] Test as SuperAdmin user
- [ ] Test as regular user (should deny)
- [ ] Test unauthorized direct URL access

### Phase 8: Testing and Polish (2-3 days)

**Step 8.1: Unit Tests**
- [ ] Test `InviteCodeService` new methods
- [ ] Test `InviteCodeRepository` new methods
- [ ] Test pagination logic
- [ ] Test filtering logic
- [ ] Test statistics calculations
- [ ] Test edge cases

**Step 8.2: Integration Tests**
- [ ] Test full page load
- [ ] Test search functionality
- [ ] Test filtering by status
- [ ] Test pagination navigation
- [ ] Test code generation flow
- [ ] Test code revocation flow
- [ ] Test statistics updates

**Step 8.3: Manual Testing**
- [ ] Test with empty database (0 codes)
- [ ] Test with sample data
- [ ] Test with large dataset (1000+ codes)
- [ ] Test search with no results
- [ ] Test all filter combinations
- [ ] Test modal interactions
- [ ] Test keyboard navigation
- [ ] Test on mobile devices
- [ ] Test on different browsers
- [ ] Test with slow network

**Step 8.4: Security Testing**
- [ ] Verify authorization on page
- [ ] Test direct URL access without auth
- [ ] Test as different user roles
- [ ] Verify CSRF protection
- [ ] Test input validation
- [ ] Test for XSS vulnerabilities
- [ ] Test SQL injection prevention

**Step 8.5: Performance Testing**
- [ ] Measure page load time
- [ ] Measure search response time
- [ ] Measure pagination performance
- [ ] Test with 10,000+ codes
- [ ] Optimize slow queries
- [ ] Add indexes if needed

**Step 8.6: Polish**
- [ ] Add loading indicators
- [ ] Improve error messages
- [ ] Add tooltips where needed
- [ ] Ensure consistent messaging
- [ ] Add keyboard shortcuts
- [ ] Fine-tune animations
- [ ] Verify accessibility (ARIA labels)
- [ ] Test with screen readers

---

## 7. Technical Specifications

### 7.1 Pagination

- Default page size: 20 codes
- Page size options: 10, 20, 50, 100
- Show total count and current range ("Showing 1-20 of 87 codes")
- Previous/Next buttons
- Disable Previous on first page
- Disable Next on last page

### 7.2 Search

- Debounce input: 300ms
- Search fields: Code, Discord Username
- Case-insensitive search
- Clear search button
- Preserve search across page changes
- Reset to page 1 on new search

### 7.3 Filtering

- Filter by Status: All, Active, Used, Expired, Revoked
- Status definitions:
  - Active: `!IsUsed && ExpiresAt > Now`
  - Used: `IsUsed == true`
  - Expired: `!IsUsed && ExpiresAt <= Now`
  - Revoked: `!IsUsed && ExpiresAt <= Now` (manually expired)
- Filters combine with search (AND logic)
- Persist filters in component state
- Optional: Persist in URL query params

### 7.4 Sorting

- Default: Created date descending (newest first)
- Sortable columns: Code, Created Date, Expires Date, Status
- Toggle ascending/descending on header click
- Visual indicator for sorted column (▲▼)
- Preserve sort across page changes

### 7.5 Status Badge Colors

**Design Token Mapping:**

- **Active**: `--color-success` (#87B38D - Sage green)
- **Used**: `--color-neutral-300` (#DEE2E6 - Gray)
- **Expiring Soon** (< 24h remaining): `--color-warning` (#FED766 - Gold)
- **Expired**: `--color-danger` (#C2335D - Dark Rose)
- **Revoked**: `--color-neutral-600` (#6C757D - Dark gray)

### 7.6 Code Display

- Font: `--font-mono` (Fira Code, Consolas, Monaco)
- Background: `--bg-tertiary` (#F1F3F5)
- Padding: `--space-1 --space-2`
- Border radius: `--radius-sm`
- Font size: `--text-xs` or `--text-sm`
- Font weight: `--font-semibold`

### 7.7 Error Handling

- Display user-friendly error messages
- Log errors server-side with Serilog
- Show retry option for transient failures
- Graceful degradation for missing Discord avatars
- Handle service unavailability

### 7.8 Caching

- Cache statistics for 5 minutes using `IMemoryCache`
- Invalidate cache on code generation
- Invalidate cache on code revocation
- Invalidate cache on code deletion
- Cache key format: `invite-code-stats`

---

## 8. UI/UX Specifications

### 8.1 Layout

```
┌─────────────────────────────────────────────────────┐
│ Sidebar │ Page Header (Title + Generate Button)     │
│         │ ┌─────────────────────────────────────┐   │
│ Nav     │ │ Statistics Cards (4 columns)        │   │
│ Menu    │ │ [Active] [Used] [Expired] [Revoked] │   │
│         │ └─────────────────────────────────────┘   │
│ - Home  │ ┌─────────────────────────────────────┐   │
│ - Users │ │ Search + Filters                    │   │
│ - Codes │ │ [Search...] [Status ▼] [Clear]     │   │
│         │ └─────────────────────────────────────┘   │
│         │ ┌─────────────────────────────────────┐   │
│         │ │ Code | User | Created | Expires |...│   │
│         │ │ ──────────────────────────────────  │   │
│         │ │ X7K9 | @alice | Oct 17 | Oct 18 | ● │   │
│         │ │ B3N7 | @bob   | Oct 16 | Oct 17 | ● │   │
│         │ │ ...                                  │   │
│         │ └─────────────────────────────────────┘   │
│         │ ┌─────────────────────────────────────┐   │
│         │ │ Showing 1-10 of 87 [◄ 1 ►]         │   │
│         │ └─────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

### 8.2 Color Usage

**From Design Standards:**

- **Primary (Rose #DA4167)**: Generate button, active states, primary actions
- **Slate (#555B6E)**: Sidebar, navigation
- **Sage (#87B38D)**: Active code badges, success states
- **Lavender (#D3C4E3)**: Secondary info, subtle backgrounds
- **Gold (#FED766)**: Expiring soon badges, warnings
- **Neutral scale**: Text, borders, backgrounds, expired/used badges

### 8.3 Typography

**From Design Tokens:**

- Page title: `--text-3xl` (30px), `--font-bold`
- Stat card values: `--text-3xl`, `--font-bold`
- Stat card labels: `--text-sm`, `--font-semibold`, uppercase
- Table headers: `--text-sm`, `--font-semibold`
- Table body: `--text-sm`, `--font-normal`
- Codes: `--text-xs`, `--font-mono`, `--font-semibold`
- Badges: `--text-xs`, `--font-semibold`, uppercase

### 8.4 Spacing

**From Design Tokens:**

- Page padding: `--space-8` (32px)
- Section spacing: `--space-6` (24px)
- Card padding: `--space-5` (20px)
- Element gaps: `--space-4` (16px)
- Tight spacing: `--space-2` (8px)
- Table cell padding: `--space-4`
- Button padding: `--btn-padding-md`

### 8.5 Interactive States

- **Buttons**:
  - Hover: background color change + `--transition-base`
  - Active: slight scale/shadow
- **Table rows**:
  - Hover: background `--bg-secondary`
  - Transition: `--transition-fast`
- **Action buttons**:
  - Hover: background change, scale up slightly
  - Copy button: Show "Copied!" on click
- **Modals**:
  - Overlay: `--modal-overlay` with fade-in
  - Modal: Slide down animation
  - Close: Fade out

### 8.6 Responsive Design

**Breakpoints:**

- Mobile: < 768px
  - Hide sidebar (or collapsible)
  - Stack stat cards vertically
  - Make table horizontally scrollable
  - Full-width search/filters

- Tablet: 768px - 1024px
  - 2-column stat cards
  - Adjust table column widths

- Desktop: > 1024px
  - 4-column stat cards
  - Full table display
  - Standard layout

---

## 9. Security Considerations

### 9.1 Authorization

- All admin pages require `[Authorize(Roles = "Admin,SuperAdmin")]`
- Service methods validate caller has appropriate role
- Prevent code generation for already-registered Discord users
- Log all administrative actions

### 9.2 Input Validation

- Validate Discord user ID format (ulong)
- Validate expiration hours (positive integer, max 720 hours / 30 days)
- Sanitize search input to prevent SQL injection
- Validate code format on all operations
- Prevent XSS in notes field

### 9.3 Audit Logging

**Log the following actions:**

| Action | Data Logged |
|--------|-------------|
| Code Generated | Discord user ID, username, code, expiration, admin user ID, timestamp |
| Code Revoked | Code, Discord user ID, admin user ID, reason, timestamp |
| Code Deleted | Code, Discord user ID, admin user ID, timestamp |
| Code Used | Code, Discord user ID, application user ID, timestamp |
| Failed Generation | Discord user ID, reason (already registered), timestamp |
| Unauthorized Access | Requested URL, user ID, timestamp |

### 9.4 Data Protection

- Never expose sensitive user data in logs
- Limit code visibility to authorized admins
- Use HTTPS for all requests
- Implement CSRF protection (built into Blazor)
- Rate limit code generation to prevent abuse

### 9.5 Rate Limiting

- Consider rate limiting on code generation (prevent spam)
- Log excessive failed operations
- Implement cooldown on bulk operations
- Alert on suspicious patterns

---

## 10. Database Considerations

### 10.1 Indexes Required

**Existing:**
- Primary key on `InviteCode.Id`
- Index on `InviteCode.Code` (unique)
- Index on `InviteCode.DiscordUserId`

**Consider Adding:**
- Index on `InviteCode.CreatedAt` (for sorting)
- Index on `InviteCode.ExpiresAt` (for filtering active/expired)
- Index on `InviteCode.IsUsed` (for filtering)
- Composite index on `(IsUsed, ExpiresAt)` (for active code queries)

### 10.2 Query Optimization

- Use `.AsNoTracking()` for read-only queries
- Use projections to select only needed fields
- Avoid N+1 queries with proper `.Include()`
- Consider compiled queries for frequently used queries
- Use efficient LINQ for statistics calculations

**Example Efficient Query:**
```csharp
var statistics = await _context.InviteCodes
    .AsNoTracking()
    .GroupBy(c => 1)
    .Select(g => new InviteCodeStatistics
    {
        ActiveCount = g.Count(c => !c.IsUsed && c.ExpiresAt > DateTime.UtcNow),
        UsedCount = g.Count(c => c.IsUsed),
        ExpiredCount = g.Count(c => !c.IsUsed && c.ExpiresAt <= DateTime.UtcNow),
        TotalCount = g.Count()
    })
    .FirstOrDefaultAsync();
```

---

## 11. Testing Strategy

### 11.1 Unit Tests

**InviteCodeService Tests:**
- `GetCodesPagedAsync_WithFilters_ReturnsFilteredResults`
- `GetCodesPagedAsync_WithSearch_ReturnsMatchingCodes`
- `GetStatisticsAsync_ReturnsCorrectCounts`
- `GenerateCodeForUserAsync_ValidUser_ReturnsCode`
- `GenerateCodeForUserAsync_AlreadyRegistered_ThrowsException`
- `DeleteExpiredCodeAsync_ExpiredCode_ReturnsTrue`
- `DeleteExpiredCodeAsync_ActiveCode_ReturnsFalse`

**InviteCodeRepository Tests:**
- `GetPagedAsync_WithPagination_ReturnsCorrectPage`
- `GetPagedAsync_WithStatusFilter_ReturnsFilteredResults`
- `GetPagedAsync_WithSearch_ReturnsMatchingResults`
- `GetStatisticsAsync_ReturnsAccurateCounts`
- `DeleteByCodeAsync_ExistingCode_ReturnsTrue`

### 11.2 Integration Tests

- Full page load and data fetch
- Search functionality end-to-end
- Filter by status end-to-end
- Pagination navigation
- Code generation flow
- Code revocation flow
- Copy to clipboard
- Statistics refresh after operations

### 11.3 UI Tests (Manual)

- [ ] Test with 0 codes (empty state)
- [ ] Test with 1 code
- [ ] Test with 100+ codes (pagination)
- [ ] Test with 1000+ codes (performance)
- [ ] Test search with no results
- [ ] Test all status filters
- [ ] Test combined search + filter
- [ ] Test modal interactions
- [ ] Test on different screen sizes
- [ ] Test on different browsers (Chrome, Firefox, Safari, Edge)
- [ ] Test keyboard navigation
- [ ] Test with slow network (throttling)

---

## 12. Performance Targets

- Page load time: < 1 second (for 20 codes)
- Search debounce: 300ms
- Search results: < 500ms response time
- Statistics calculation: < 200ms
- Database query time: < 200ms
- Code generation: < 1 second
- Pagination navigation: < 300ms
- Support up to 100,000 invite codes without performance degradation

---

## 13. Acceptance Criteria

### Must Have (MVP)

- [ ] Admin can view paginated list of all invite codes
- [ ] Admin can search codes by code or Discord username
- [ ] Admin can filter codes by status (All, Active, Used, Expired, Revoked)
- [ ] Admin can view invite code statistics (Active, Used, Expired, Revoked counts)
- [ ] Admin can generate new invite codes with custom expiration
- [ ] Admin can copy invite codes to clipboard
- [ ] Admin can revoke active invite codes
- [ ] Admin can delete expired invite codes
- [ ] System displays Discord user avatars in table
- [ ] System shows color-coded status badges
- [ ] System prevents code generation for already-registered users
- [ ] All actions are logged for audit
- [ ] Non-admin users cannot access admin pages
- [ ] UI follows design standards
- [ ] UI is responsive (mobile, tablet, desktop)

### Should Have (Post-MVP)

- [ ] Admin can export invite codes to CSV
- [ ] Admin can sort codes by different columns
- [ ] System sends notifications when codes are about to expire
- [ ] Admin can set custom expiration dates (not just preset hours)
- [ ] Admin can add notes to generated codes
- [ ] Admin can view code usage history
- [ ] System shows "expiring soon" warnings (< 24h remaining)

### Nice to Have (Future)

- [ ] Bulk code generation (generate multiple codes at once)
- [ ] Code analytics (usage patterns, conversion rates)
- [ ] Automated code cleanup (schedule expired code deletion)
- [ ] Email/DM code directly to Discord user
- [ ] QR code generation for invite codes
- [ ] Custom code format options
- [ ] Code templates for recurring use cases

---

## 14. Risks and Mitigations

| Risk | Impact | Likelihood | Mitigation |
|------|--------|-----------|------------|
| Performance issues with large code dataset | High | Medium | Implement efficient queries, pagination, proper indexing |
| Confusion between expired and revoked codes | Medium | Medium | Clear status badges, distinct colors, tooltips |
| Accidental code deletion | Medium | Low | Confirmation dialogs, audit logging, soft delete option |
| Code copy not working on all browsers | Low | Medium | Fallback clipboard method, browser compatibility testing |
| UI doesn't match prototype design | Low | Medium | Regular design reviews, use design tokens consistently |
| Discord API rate limiting for avatars | Low | Low | Cache avatar URLs, graceful fallback to initials |
| Statistics not updating in real-time | Medium | Medium | Implement cache invalidation, refresh on operations |

---

## 15. Success Metrics

- Page load time < 1 second for 20 codes
- Search results returned < 500ms
- Zero unauthorized access incidents
- Admin tasks completed in < 5 clicks
- User satisfaction score > 4/5
- < 2% error rate on code operations
- 95%+ test coverage on critical paths
- Statistics accuracy: 100%

---

## 16. Timeline Estimate

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| Phase 1: Backend Enhancements | 1-2 days | None |
| Phase 2: Shared Components | 2-3 days | Phase 1 |
| Phase 3: Main Page | 2-3 days | Phase 1, Phase 2 |
| Phase 4: Generate Modal | 2 days | Phase 1, Phase 2 |
| Phase 5: Revoke/Delete Modals | 1 day | Phase 1, Phase 2 |
| Phase 6: Additional Features | 1-2 days | Phase 3 |
| Phase 7: Integration | 1 day | All previous |
| Phase 8: Testing and Polish | 2-3 days | All previous |
| **Total** | **12-17 days** | |

**Note:** Estimate assumes single developer working full-time. Adjust based on team size and availability.

---

## 17. File Structure

```
src/DiscordBot/
├── DiscordBot.Core/
│   ├── DTOs/
│   │   ├── InviteCodeStatistics.cs         [NEW]
│   │   ├── PagedResult.cs                  [NEW - if not exists]
│   │   └── InviteCodeStatus.cs             [NEW]
│   ├── Services/
│   │   └── IInviteCodeService.cs           [EXTEND]
│   └── Repositories/
│       └── IInviteCodeRepository.cs        [EXTEND]
│
├── DiscordBot.Blazor/
│   ├── Services/
│   │   └── InviteCodeService.cs            [EXTEND]
│   ├── Repositories/
│   │   └── InviteCodeRepository.cs         [EXTEND]
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── Admin/
│   │   │       ├── InviteCodes.razor       [NEW]
│   │   │       └── InviteCodes.razor.css   [NEW]
│   │   ├── Admin/
│   │   │   └── InviteCodes/
│   │   │       ├── InviteCodeTable.razor             [NEW]
│   │   │       ├── InviteCodeTable.razor.css         [NEW]
│   │   │       ├── InviteCodeStatsCard.razor         [NEW]
│   │   │       ├── InviteCodeStatsCard.razor.css     [NEW]
│   │   │       ├── GenerateCodeModal.razor           [NEW]
│   │   │       ├── GenerateCodeModal.razor.css       [NEW]
│   │   │       ├── RevokeCodeModal.razor             [NEW]
│   │   │       └── RevokeCodeModal.razor.css         [NEW]
│   │   └── Shared/
│   │       ├── CodeBadge.razor             [NEW]
│   │       ├── CodeBadge.razor.css         [NEW]
│   │       ├── StatusBadge.razor           [NEW - or extend existing]
│   │       ├── StatusBadge.razor.css       [NEW - or extend existing]
│   │       ├── Modal.razor                 [VERIFY/CREATE]
│   │       ├── ConfirmDialog.razor         [VERIFY/CREATE]
│   │       ├── Pagination.razor            [VERIFY/CREATE]
│   │       ├── LoadingSpinner.razor        [VERIFY/CREATE]
│   │       └── EmptyState.razor            [VERIFY/CREATE]
│   └── wwwroot/
│       ├── js/
│       │   └── clipboard.js                [NEW - for copy functionality]
│       └── css/
│           └── tokens.css                  [EXISTING]
```

---

## 18. Dependencies

### NuGet Packages

**Already Installed:**
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` - For user management
- `Microsoft.EntityFrameworkCore.Sqlite` - Database provider
- `Microsoft.Extensions.Caching.Memory` - For caching
- `Serilog` - For logging

**May Need:**
- `CsvHelper` - For CSV export (if implementing export feature)

### External Services

- **Discord CDN** - For user avatar images
  - URL format: `https://cdn.discordapp.com/avatars/{userId}/{avatarHash}.png`
  - Fallback to initials if avatar not available

### JavaScript Libraries

- Native Clipboard API (modern browsers)
- Fallback method for older browsers

---

## 19. Design Reference

**Primary Reference:**
- Prototype: [admin-invites-polished.html](../../prototypes/pages/admin-invites-polished.html)

**Supporting References:**
- Design Tokens: [tokens.css](../../src/DiscordBot/DiscordBot.Blazor/wwwroot/css/tokens.css)
- Design Standards: [design-standards.md](../01-planning/04-design-standards.md)
- Users Page (similar pattern): [Users.razor](../../src/DiscordBot/DiscordBot.Blazor/Components/Pages/Users.razor)

**Key Design Elements from Prototype:**

1. **Statistics Cards:** 4-column grid, large numbers, uppercase labels
2. **Search Bar:** Full-width input with icon, debounced
3. **Filter Controls:** Dropdown + clear button
4. **Data Table:** Clean borders, hover states, monospace codes
5. **Status Badges:** Pill shape, color-coded, uppercase
6. **Action Buttons:** Small, icon-based, hover tooltips
7. **Modals:** Centered, overlay, rounded corners, shadow
8. **Pagination:** Previous/Next buttons, page info text

---

## 20. Future Enhancements

### Phase 2 Features (Post-MVP)

- **Advanced Analytics:**
  - Code conversion rate (generated vs. used)
  - Average time from generation to usage
  - Peak usage times
  - User registration funnel metrics

- **Bulk Operations:**
  - Generate multiple codes at once
  - Bulk revocation (with confirmation)
  - Bulk expiration date adjustment

- **Notifications:**
  - Email admins when codes are about to expire
  - Notify when unusual code generation patterns occur
  - Alert when code usage spikes

- **Code Templates:**
  - Save common expiration settings
  - Quick generate with predefined settings
  - Role-based default expirations

- **Enhanced Export:**
  - Excel format support
  - JSON export for API integration
  - Scheduled automated exports

- **Audit Dashboard:**
  - Detailed audit log viewer
  - Filter by admin, action type, date range
  - Export audit logs

- **Discord Integration:**
  - Direct message code to user from admin panel
  - Verify Discord user exists before generation
  - Sync Discord usernames automatically

---

## 21. References

- [Product Requirements](../01-planning/01-product-requirements.md)
- [Functional Specifications](../01-planning/02-functional-specs.md)
- [Design Standards](../01-planning/04-design-standards.md)
- [Security Architecture](../02-architecture/04-security-architecture.md)
- [Database Design](../02-architecture/02-database-design.md)
- Prototype: [admin-invites-polished.html](../../prototypes/pages/admin-invites-polished.html)
- Design Tokens: [tokens.css](../../prototypes/css/tokens.css)

---

## 22. Approval

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
- **Status:** Planning - Ready for Implementation

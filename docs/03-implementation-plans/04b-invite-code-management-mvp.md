# Invite Code Management - MVP Implementation

**Version:** 1.0
**Created:** 2025-10-20
**Status:** Ready for Implementation
**Estimated Time:** 5-7 days

---

## MVP Goal

Create a minimal but functional admin interface for managing Discord invite codes with essential CRUD operations, basic filtering, and statistics.

---

## What's In Scope (MVP)

### ✅ Core Features

1. **View Invite Codes** - Paginated table of all codes
2. **Statistics Dashboard** - 4 stat cards (Active, Used, Expired, Revoked)
3. **Search** - By code or Discord username
4. **Filter** - By status (All, Active, Used, Expired)
5. **Generate Code** - Modal to create new codes
6. **Revoke Code** - Mark active codes as expired
7. **Copy to Clipboard** - Quick copy functionality
8. **Basic Pagination** - Previous/Next navigation

### 🎯 MVP Success Criteria

- [ ] Admin can view all invite codes in a table
- [ ] Admin can see statistics at a glance
- [ ] Admin can search for specific codes or users
- [ ] Admin can filter by status
- [ ] Admin can generate new codes with 24h expiration
- [ ] Admin can revoke active codes
- [ ] Admin can copy codes to clipboard
- [ ] Page loads in < 2 seconds
- [ ] Non-admin users cannot access the page

---

## What's Out of Scope (Post-MVP)

❌ **Deferred Features:**

- Custom expiration times (use 24h default)
- Sorting by columns
- Notes field on codes
- Delete functionality (only revoke)
- Export to CSV
- Bulk operations
- Advanced analytics
- Email/DM notifications
- QR code generation

---

## Implementation Plan

### Phase 1: Backend Minimal Extensions (1 day)

**Goal:** Add only what's absolutely necessary

**Tasks:**
- [ ] Create `InviteCodeStatistics` DTO
- [ ] Add `GetStatisticsAsync()` to service
- [ ] Add `GetPagedAsync()` to repository (with search/filter)
- [ ] Test new methods

**Files to Create/Modify:**
```
Core/DTOs/InviteCodeStatistics.cs         [NEW]
Core/Services/IInviteCodeService.cs       [EXTEND - 2 methods]
Blazor/Services/InviteCodeService.cs      [EXTEND - 2 methods]
Blazor/Repositories/InviteCodeRepository.cs [EXTEND - 1 method]
```

**Code Additions:**

```csharp
// InviteCodeStatistics.cs
public class InviteCodeStatistics
{
    public int ActiveCount { get; set; }
    public int UsedCount { get; set; }
    public int ExpiredCount { get; set; }
    public int RevokedCount { get; set; }
}

// IInviteCodeService additions
Task<InviteCodeStatistics> GetStatisticsAsync();
Task<(IEnumerable<InviteCode> Codes, int TotalCount)> GetPagedAsync(
    int page, int pageSize, string? status = null, string? search = null);
```

---

### Phase 2: Single Page Component (2-3 days)

**Goal:** One `.razor` file with everything inline (no separate components yet)

**Tasks:**
- [ ] Create `InviteCodes.razor` page
- [ ] Add authorization attribute
- [ ] Fetch and display statistics
- [ ] Create search input (with debounce)
- [ ] Create status filter dropdown
- [ ] Create table with all columns
- [ ] Add pagination controls
- [ ] Wire up service calls

**File to Create:**
```
Components/Pages/Admin/InviteCodes.razor      [NEW - ~400-500 lines]
Components/Pages/Admin/InviteCodes.razor.css  [NEW]
```

**Page Structure:**
```razor
@page "/admin/invitecodes"
@attribute [Authorize(Roles = "Admin,SuperAdmin")]

<!-- Statistics Cards -->
<div class="stats-grid">
  <div class="stat-card">Active: @stats.ActiveCount</div>
  <!-- ... other stats -->
</div>

<!-- Search and Filter -->
<div class="toolbar">
  <input @bind="search" @bind:event="oninput" />
  <select @bind="statusFilter">...</select>
  <button @onclick="GenerateCode">Generate Code</button>
</div>

<!-- Table -->
<table>
  <thead>...</thead>
  <tbody>
    @foreach (var code in codes)
    {
      <tr>
        <td><code>@code.Code</code></td>
        <td>@code.DiscordUsername</td>
        <!-- ... other columns -->
        <td>
          <button @onclick="() => CopyCode(code.Code)">Copy</button>
          <button @onclick="() => RevokeCode(code.Code)">Revoke</button>
        </td>
      </tr>
    }
  </tbody>
</table>

<!-- Pagination -->
<div class="pagination">
  <button @onclick="PreviousPage">Previous</button>
  <span>Page @currentPage</span>
  <button @onclick="NextPage">Next</button>
</div>

@code {
  // All logic inline
}
```

---

### Phase 3: Simple Modals (1-2 days)

**Goal:** Basic modals for generate and revoke (inline in same file or simple separate components)

**Option A - Inline Modals (Faster):**
```razor
@if (showGenerateModal)
{
  <div class="modal-overlay" @onclick="CloseModal">
    <div class="modal" @onclick:stopPropagation>
      <h3>Generate Invite Code</h3>
      <input @bind="discordUsername" placeholder="Discord Username" />
      <button @onclick="GenerateNewCode">Generate</button>
      <button @onclick="CloseModal">Cancel</button>
    </div>
  </div>
}
```

**Option B - Separate Components (Cleaner):**
```
Components/Admin/InviteCodes/GenerateCodeModal.razor  [NEW]
Components/Admin/InviteCodes/RevokeCodeModal.razor    [NEW]
```

**Tasks:**
- [ ] Create generate code modal UI
- [ ] Add form validation (required username)
- [ ] Call service to generate code
- [ ] Show generated code
- [ ] Add copy-to-clipboard
- [ ] Create revoke confirmation modal
- [ ] Wire up revoke service call
- [ ] Refresh table after operations

---

### Phase 4: Styling (1 day)

**Goal:** Make it look good using existing design tokens

**Tasks:**
- [ ] Apply design tokens from `tokens.css`
- [ ] Style statistics cards (match prototype)
- [ ] Style table (borders, hover states)
- [ ] Style status badges (color-coded)
- [ ] Style modals (overlay, centered)
- [ ] Add responsive breakpoints
- [ ] Test on mobile

**CSS File:**
```css
/* InviteCodes.razor.css */

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--space-4);
}

.stat-card {
  background: var(--bg-primary);
  padding: var(--space-5);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
}

/* ... rest of styling */
```

---

### Phase 5: JavaScript for Clipboard (0.5 day)

**Goal:** Copy to clipboard functionality

**Tasks:**
- [ ] Create `clipboard.js` file
- [ ] Add Clipboard API function
- [ ] Call from Blazor with JSInterop
- [ ] Show "Copied!" feedback

**File to Create:**
```
wwwroot/js/clipboard.js  [NEW]
```

**JavaScript:**
```javascript
window.copyToClipboard = function(text) {
  return navigator.clipboard.writeText(text)
    .then(() => true)
    .catch(() => false);
}
```

**Blazor:**
```csharp
[Inject] IJSRuntime JS { get; set; }

private async Task CopyCode(string code)
{
  var success = await JS.InvokeAsync<bool>("copyToClipboard", code);
  // Show feedback
}
```

---

### Phase 6: Testing (0.5-1 day)

**Goal:** Ensure core functionality works

**Manual Test Checklist:**
- [ ] Page loads without errors
- [ ] Statistics show correct counts
- [ ] Search filters results
- [ ] Status filter works
- [ ] Pagination works
- [ ] Generate code creates new code
- [ ] Revoke code updates status
- [ ] Copy to clipboard works
- [ ] Non-admin cannot access page
- [ ] Mobile layout works

**Unit Tests (Optional for MVP):**
- Test service methods
- Test statistics calculations
- Test pagination logic

---

## Simplified Architecture

### MVP File Structure

```
src/DiscordBot/
├── DiscordBot.Core/
│   ├── DTOs/
│   │   └── InviteCodeStatistics.cs              [NEW]
│   └── Services/
│       └── IInviteCodeService.cs                [EXTEND +2 methods]
│
├── DiscordBot.Blazor/
│   ├── Services/
│   │   └── InviteCodeService.cs                 [EXTEND +2 methods]
│   ├── Repositories/
│   │   └── InviteCodeRepository.cs              [EXTEND +1 method]
│   ├── Components/
│   │   └── Pages/
│   │       └── Admin/
│   │           ├── InviteCodes.razor            [NEW - main page]
│   │           └── InviteCodes.razor.css        [NEW - styling]
│   └── wwwroot/
│       └── js/
│           └── clipboard.js                     [NEW - copy function]
```

**Total New Files:** 4
**Total Modified Files:** 3

---

## Minimal UI Specification

### Statistics Cards (4 columns)

```
┌─────────────┬─────────────┬─────────────┬─────────────┐
│   ACTIVE    │    USED     │   EXPIRED   │   REVOKED   │
│     87      │     342     │     156     │      23     │
└─────────────┴─────────────┴─────────────┴─────────────┘
```

### Toolbar

```
┌─────────────────────────────────────────────────────────┐
│ [Search: ________________] [Status: All ▼] [Generate]   │
└─────────────────────────────────────────────────────────┘
```

### Table (Essential Columns Only)

```
┌──────────────┬─────────────┬────────────┬────────────┬────────┬─────────┐
│ Code         │ Discord User│ Created    │ Expires    │ Status │ Actions │
├──────────────┼─────────────┼────────────┼────────────┼────────┼─────────┤
│ X7K9-M2P4-Q8N│ @alice      │ Oct 17     │ Oct 18     │ Active │ [C][R]  │
│ B3N7-R5T2-K8P│ @bob        │ Oct 16     │ Oct 17     │ Expired│ [C][R]  │
└──────────────┴─────────────┴────────────┴────────────┴────────┴─────────┘

[C] = Copy button
[R] = Revoke button
```

### Pagination

```
┌─────────────────────────────────────────────────────────┐
│ Showing 1-20 of 87    [← Previous]  Page 1  [Next →]   │
└─────────────────────────────────────────────────────────┘
```

---

## Status Badge Colors

| Status | Color | CSS Variable |
|--------|-------|--------------|
| Active | Green | `var(--color-success)` |
| Used   | Gray  | `var(--color-neutral-300)` |
| Expired| Red   | `var(--color-danger)` |

---

## Simplified Component Code

### Statistics Card (Inline)

```razor
<div class="stat-card">
  <div class="stat-label">ACTIVE</div>
  <div class="stat-value">@statistics.ActiveCount</div>
</div>
```

### Table Row (Inline)

```razor
<tr>
  <td><code class="code-badge">@code.Code</code></td>
  <td>@code.DiscordUsername</td>
  <td>@code.CreatedAt.ToString("MMM dd, yyyy")</td>
  <td>@code.ExpiresAt.ToString("MMM dd, yyyy")</td>
  <td><span class="badge badge-@GetStatusClass(code)">@GetStatus(code)</span></td>
  <td>
    <button class="btn-sm" @onclick="() => CopyCode(code.Code)">Copy</button>
    @if (!code.IsUsed && !code.IsExpired)
    {
      <button class="btn-sm btn-danger" @onclick="() => ShowRevokeModal(code)">Revoke</button>
    }
  </td>
</tr>
```

### Generate Modal (Inline)

```razor
@if (showGenerateModal)
{
  <div class="modal-overlay" @onclick="CloseGenerateModal">
    <div class="modal" @onclick:stopPropagation>
      <div class="modal-header">
        <h3>Generate Invite Code</h3>
        <button class="btn-close" @onclick="CloseGenerateModal">&times;</button>
      </div>
      <div class="modal-body">
        <label>Discord Username</label>
        <input @bind="newCodeUsername" placeholder="@username" />
        @if (!string.IsNullOrEmpty(errorMessage))
        {
          <div class="error">@errorMessage</div>
        }
        @if (generatedCode != null)
        {
          <div class="success">
            Code generated: <code>@generatedCode.Code</code>
            <button @onclick="() => CopyCode(generatedCode.Code)">Copy</button>
          </div>
        }
      </div>
      <div class="modal-footer">
        <button class="btn btn-secondary" @onclick="CloseGenerateModal">Cancel</button>
        <button class="btn btn-primary" @onclick="GenerateCode">Generate</button>
      </div>
    </div>
  </div>
}
```

---

## Backend Implementation (Minimal)

### InviteCodeStatistics.cs

```csharp
namespace DiscordBot.Core.DTOs
{
    public class InviteCodeStatistics
    {
        public int ActiveCount { get; set; }
        public int UsedCount { get; set; }
        public int ExpiredCount { get; set; }
        public int RevokedCount { get; set; }
    }
}
```

### Service Method: GetStatisticsAsync

```csharp
public async Task<InviteCodeStatistics> GetStatisticsAsync()
{
    var allCodes = await _inviteCodeRepository.GetAllAsync();
    var now = DateTime.UtcNow;

    return new InviteCodeStatistics
    {
        ActiveCount = allCodes.Count(c => !c.IsUsed && c.ExpiresAt > now),
        UsedCount = allCodes.Count(c => c.IsUsed),
        ExpiredCount = allCodes.Count(c => !c.IsUsed && c.ExpiresAt <= now),
        RevokedCount = allCodes.Count(c => !c.IsUsed && c.ExpiresAt <= now)
    };
}
```

### Repository Method: GetPagedAsync

```csharp
public async Task<(IEnumerable<InviteCode> Codes, int TotalCount)> GetPagedAsync(
    int skip,
    int take,
    string? statusFilter = null,
    string? searchTerm = null)
{
    var query = _context.InviteCodes.AsNoTracking();
    var now = DateTime.UtcNow;

    // Filter by status
    if (!string.IsNullOrEmpty(statusFilter))
    {
        query = statusFilter.ToLower() switch
        {
            "active" => query.Where(c => !c.IsUsed && c.ExpiresAt > now),
            "used" => query.Where(c => c.IsUsed),
            "expired" => query.Where(c => !c.IsUsed && c.ExpiresAt <= now),
            _ => query
        };
    }

    // Search
    if (!string.IsNullOrEmpty(searchTerm))
    {
        query = query.Where(c =>
            c.Code.Contains(searchTerm) ||
            c.DiscordUsername.Contains(searchTerm));
    }

    var totalCount = await query.CountAsync();
    var codes = await query
        .OrderByDescending(c => c.CreatedAt)
        .Skip(skip)
        .Take(take)
        .ToListAsync();

    return (codes, totalCount);
}
```

---

## MVP Timeline

| Day | Tasks | Deliverable |
|-----|-------|-------------|
| 1 | Backend extensions | Service/repo methods working |
| 2 | Main page structure | Table showing codes |
| 3 | Search, filter, pagination | Functional filtering |
| 4 | Modals (generate, revoke) | CRUD operations complete |
| 5 | Styling | Matches design system |
| 6 | Copy to clipboard, testing | Fully functional MVP |
| 7 | Bug fixes, polish | Production-ready |

**Total: 5-7 days**

---

## MVP Checklist

### Day 1: Backend ✅
- [ ] Create `InviteCodeStatistics.cs`
- [ ] Add `GetStatisticsAsync()` to interface
- [ ] Implement `GetStatisticsAsync()` in service
- [ ] Add `GetPagedAsync()` to repository interface
- [ ] Implement `GetPagedAsync()` in repository
- [ ] Test methods with sample data

### Day 2: Page Structure ✅
- [ ] Create `InviteCodes.razor`
- [ ] Add authorization
- [ ] Inject services
- [ ] Load and display statistics
- [ ] Create basic table
- [ ] Display codes in table

### Day 3: Search & Filter ✅
- [ ] Add search input
- [ ] Implement debounce (300ms)
- [ ] Add status filter dropdown
- [ ] Wire up filter handlers
- [ ] Add pagination controls
- [ ] Test filtering combinations

### Day 4: Modals ✅
- [ ] Create generate code modal
- [ ] Add form validation
- [ ] Call generate service
- [ ] Display generated code
- [ ] Create revoke modal
- [ ] Call revoke service
- [ ] Refresh data after operations

### Day 5: Styling ✅
- [ ] Create CSS file
- [ ] Style stat cards
- [ ] Style table
- [ ] Style badges
- [ ] Style modals
- [ ] Add responsive CSS
- [ ] Test on mobile

### Day 6: Features ✅
- [ ] Add clipboard.js
- [ ] Implement copy to clipboard
- [ ] Add "Copied!" feedback
- [ ] Add loading states
- [ ] Add error handling
- [ ] Test all features

### Day 7: Polish ✅
- [ ] Fix any bugs
- [ ] Improve error messages
- [ ] Add navigation link
- [ ] Update sidebar badge count
- [ ] Final testing
- [ ] Deploy to staging

---

## Acceptance Criteria (MVP)

**Must Pass:**
- ✅ Page loads for admin users
- ✅ Statistics display correct counts
- ✅ Search works for codes and usernames
- ✅ Filter works for Active, Used, Expired
- ✅ Pagination works (Previous/Next)
- ✅ Generate creates new codes (24h expiration)
- ✅ Revoke marks codes as expired
- ✅ Copy to clipboard works
- ✅ Non-admin users denied access
- ✅ Page is responsive
- ✅ No console errors
- ✅ Matches design tokens

**Nice to Have (defer if needed):**
- ⭕ Loading spinners
- ⭕ Empty state message
- ⭕ Toast notifications
- ⭕ Keyboard shortcuts

---

## Post-MVP Enhancements

**Phase 2 (1-2 days):**
- Custom expiration times
- Sort by columns
- Delete expired codes
- Export to CSV
- Notes field

**Phase 3 (2-3 days):**
- Bulk operations
- Advanced analytics
- Email notifications
- Code templates
- Audit log viewer

---

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Scope creep | Stick to checklist, defer extras |
| Performance issues | Use pagination, limit to 20 per page |
| Design inconsistency | Use existing tokens, copy Users page patterns |
| Modal complexity | Start inline, refactor later if needed |
| Clipboard doesn't work | Use JavaScript interop, test early |

---

## Success Metrics

**MVP Complete When:**
- All checklist items checked ✅
- Manual tests pass
- Admin can manage codes efficiently
- Page loads in < 2 seconds
- No blocking bugs

---

## References

**Quick Links:**
- Full Plan: [04-invite-code-management.md](./04-invite-code-management.md)
- Prototype: [admin-invites-polished.html](../../prototypes/pages/admin-invites-polished.html)
- Design Tokens: [tokens.css](../../src/DiscordBot/DiscordBot.Blazor/wwwroot/css/tokens.css)
- Similar Page: [Users.razor](../../src/DiscordBot/DiscordBot.Blazor/Components/Pages/Users.razor)

---

**Ready to Start? Begin with Day 1: Backend Extensions!**

# Turnus — UI Consolidation Pass Prompt

## Context

You are working inside the Turnus workspace — an ASP.NET Core MVC shift scheduling system for a bar/events organization. The MVP backend is complete and fully functional. Your task is a **UI consolidation pass only**.

### Strict constraints — read before writing any code:
- **MVC only** — no Blazor, no React, no JavaScript frameworks. Razor Views (`.cshtml`) and controllers only.
- **Bootstrap utility classes only** — no custom CSS files, no inline styles beyond what Bootstrap provides, no design work.
- **No new model classes** — do not create any new `.cs` files in `Models/`.
- **No new migrations** — do not run `Add-Migration` or modify `TurnusContext.cs` in any way.
- **No new DbSets** — do not add properties to `TurnusContext`.
- **No new database tables** — work entirely within the existing schema.
- **`[ValidateAntiForgeryToken]`** on every POST action.
- **All admin routes must return 403** (not redirect to login) for authenticated non-Manager users — use `[Authorize(Roles = "Manager")]` explicitly on admin controllers and actions, not just top-level `[Authorize]`.
- **`EmployeeId` in any POST action must always be derived server-side** from `UserManager.GetUserId(User)` — never accepted from a form field or hidden input.

---

## Existing data model — do not modify

The following models exist in `Models/`. Paste each file as context before starting.

```
Venue                   — Id, Name
Role                    — Id, Name
VenueStaffingRequirement — Id, VenueId (FK), RoleId (FK), RequiredCount, IsShiftScoped (bool)
ShiftDefinition         — Id, VenueId (FK), Name, StartTime (TimeSpan), EndTime (TimeSpan)
ScheduledDay            — Id, VenueId (FK), Date
ScheduledShift          — Id, ScheduledDayId (FK), ShiftDefinitionId (FK)
Availability            — Id, EmployeeId (FK→ApplicationUser), ScheduledShiftId (FK), IsAvailable (bool)
ShiftAssignment         — Id, ScheduledShiftId (FK), EmployeeId (FK→ApplicationUser), RoleId (FK)
DayAssignment           — Id, ScheduledDayId (FK), EmployeeId (FK→ApplicationUser), RoleId (FK)
ApplicationUser         — extends IdentityUser, adds FullName (string)
```

Key relationships:
- `ScheduledShift` connects a `ScheduledDay` (venue + date) to a `ShiftDefinition` (shift pattern with times)
- `VenueStaffingRequirement.IsShiftScoped = true` → role needed once per `ScheduledShift` (e.g. Bartender)
- `VenueStaffingRequirement.IsShiftScoped = false` → role needed once per `ScheduledDay` regardless of shift count (e.g. PersonInCharge)
- `ShiftAssignment` tracks who fills a shift-scoped role on a specific `ScheduledShift`
- `DayAssignment` tracks who fills a day-scoped role on a specific `ScheduledDay`
- `Availability` is employee self-reported availability per `ScheduledShift` (no day-scoped availability mechanism exists yet)

---

## What to build — 5 features

---

### FR1 + FR2 — Schedule Index (replaces current home page)

**Route:** `GET /` or `GET /Schedule/Index`

**What it shows:**
A weekly staffing table. The logged-in user lands here immediately after login — both Manager and Employee.

**Table structure:**
- **X-axis:** Days of the current week, Monday to Sunday, each showing the date
- **Y-axis (rows):**
  - **Row 1 (top):** The logged-in user's own confirmed shift assignments for each day of the week (`ShiftAssignment` or `DayAssignment` where `EmployeeId == currentUserId`)
  - **Row 2:** Open slots — shifts that have available positions matching any role for the current week, where assigned count < required count
  - **Remaining rows:** All other employees' assignments, grouped by employee, one row per employee

**Header above the table:**
- Current week number and year (e.g. "Week 28, 2026")
- Active venue name — if only one venue exists, show it; if multiple, show a selector
- Navigation arrows — previous week (←) and next week (→)
- Navigation limit: ±6 months from today. Disable arrows at the boundary.
- Current week reflected in URL query parameter: `?week=2026-W28`
- Week is determined server-side, not from the client

**Controller:** `ScheduleController` (new), action `Index(string? week = null)`
- Parse the `week` parameter to determine the date range (Monday–Sunday)
- Default to current week if null
- Query: load all `ScheduledShift`s for the week (via `ScheduledDay.Date` range), with `.Include()` for `ScheduledDay.Venue`, `ShiftDefinition`, `ShiftAssignment.Employee`, `DayAssignment.Employee`
- No N+1 queries — fetch all data in a small number of queries, not one per row
- Pass structured view model to the view (define a `WeekScheduleViewModel` in the controller file as a nested class, not a separate model file)

**Non-functional:**
- Must load within 2 seconds for a typical week
- Row 1 must never show another user's assignments
- Empty cells in Row 1 must display "No shift" visually, not be blank
- Table must be keyboard-navigable with ARIA labels on interactive cells

---

### FR3 — Interactive shift cells

**What a cell shows (always visible):**
- Venue name
- Shift name (e.g. "Early", "Late")
- Start time
- End time

**On click — detail panel or modal (no full page redirect):**
- Full shift details: venue, date, shift name, start/end, required roles and their assigned employees
- Available / Unavailable button — marks the current user's `Availability` for this `ScheduledShift`
- After marking, button state updates to reflect current status without full page reload (use a simple form POST with redirect-after-POST pattern if JS is avoided, or a minimal fetch if acceptable)

**POST action:** `AvailabilitiesController.SetAvailability(int scheduledShiftId, bool isAvailable)`
- Already exists — do not duplicate it
- `EmployeeId` from `UserManager.GetUserId(User)` server-side only
- Upsert pattern — update existing `Availability` row if found, insert if not

**Non-functional:**
- Cells large enough for all 4 fields on desktop; truncation with tooltip acceptable on mobile
- Availability update must visually confirm immediately (button state change or color)
- No duplicate `Availability` rows for same user+shift

---

### FR4 — User profile page

**Route:** `GET /Profile`

**What it shows:**
- Profile information: `FullName`, `Email`, `PhoneNumber` (only fields currently on `ApplicationUser` — do not add new fields)
- Upcoming shifts summary: the user's confirmed `ShiftAssignment`s and `DayAssignment`s, sorted chronologically, future dates only
- Each shift displayed in the same cell format as FR3 (Venue, Shift name, Start, End)
- A settings section — placeholder only, no functionality required now
- Past shifts accessible via a toggle or separate section, not shown by default

**Controller:** `ProfileController` (new), action `Index()`
- Load current user's data from `UserManager.GetUserAsync(User)`
- Load assignments in a single query with `.Include()` — no per-shift separate queries
- Filter to `Date >= DateTime.Today`

**Security:**
- Must only show data for the currently authenticated user
- Accessing another user's profile by URL manipulation must return 403

**Non-functional:**
- Single query for all assignment data
- Chronological sort on the server side, not in the view

---

### FR5 — Admin dashboard

**Access:** Manager-only. A button in the top-right header, visible only to users in the "Manager" Identity Role. Navigates to `/Admin/Dashboard`.

**Authorization:** `[Authorize(Roles = "Manager")]` on the entire `AdminController`. Non-Manager authenticated users hitting admin routes get 403, not a redirect to login.

**Dashboard layout — two sections:**

#### Section A: Venue Settings

A guided sequential flow with visual completion indicators (checkmarks or progress steps):

1. **Venues** — list existing venues, link to create/edit. Step marked complete when ≥1 venue exists.
2. **Roles** — list existing roles, link to create/edit. Step marked complete when ≥1 role exists.
3. **Shift Definitions** — list `ShiftDefinition`s for the selected venue, link to create/edit. Step marked complete when ≥1 `ShiftDefinition` exists for the selected venue.
4. **Staffing Requirements** — list `VenueStaffingRequirement`s for the selected venue (showing Role, RequiredCount, IsShiftScoped). Link to create/edit. Step marked complete when ≥1 requirement exists for the selected venue.

Steps 3 and 4 are visually disabled (greyed out with tooltip) until the venue is selected and steps 1–2 are complete.

#### Section B: Scheduled

A guided flow, entirely disabled with a clear message until Section A is fully configured for the selected venue:

1. **Scheduled Days** — list `ScheduledDay`s for the selected venue, link to create. Each row links directly to its shifts.
2. **Scheduled Shifts** — for a given `ScheduledDay`, list its `ScheduledShift`s, link to create. Inline link from each `ScheduledDay` row — manager never navigates to `/ScheduledShifts` as a top-level page.
3. **Review & Assign** — the existing `ScheduleReview/Review` page, linked inline from each `ScheduledDay`. Shows required roles (split by `IsShiftScoped`), available employees per shift, and assign buttons. Already implemented — just link to it from the dashboard.

**Server-side gate (not just UI):**
Before allowing creation of a `ScheduledShift`, the controller must verify that at least one `VenueStaffingRequirement` and one `ShiftDefinition` exist for the venue of the selected `ScheduledDay`. Return a validation error message if not — do not throw a database exception.

**Duplicate prevention:**
Before inserting a `ScheduledShift`, check that no existing `ScheduledShift` has the same `ScheduledDayId` + `ShiftDefinitionId`. Return a clear validation message if duplicate detected.

**Venue selector:**
If multiple venues exist, show a venue selector at the top of the dashboard. All sections filter by selected venue. This must be first-class in the flow from the start, even if only one venue exists now.

**Non-functional:**
- Managers must never need to type a URL manually to reach any admin function
- All existing separate CRUD URLs (`/Roles`, `/ShiftDefinitions`, `/VenueStaffingRequirements`, `/ScheduledDays`, `/ScheduledShifts`) remain functional but are no longer the primary entry point — the dashboard is
- `[ValidateAntiForgeryToken]` on all POST actions
- `[Authorize(Roles = "Manager")]` on all admin actions, not just controller level

---

## Navigation — update `_Layout.cshtml`

Add to the navbar:
- **Schedule** link → `/Schedule/Index` (visible to all logged-in users)
- **My Profile** link → `/Profile` (visible to all logged-in users, top-right)
- **Admin** link → `/Admin/Dashboard` (visible only to Manager role users, top-right, alongside logout)

Use `@if (User.IsInRole("Manager"))` in the layout to conditionally show the Admin link.

---

## Key query patterns to follow

**Loading a week's schedule (FR1):**
```csharp
var shifts = await _context.ScheduledShift
    .Include(s => s.ScheduledDay)
        .ThenInclude(d => d.Venue)
    .Include(s => s.ShiftDefinition)
    .Include(s => s.ShiftAssignments)
        .ThenInclude(a => a.Employee)
    .Where(s => s.ScheduledDay.Date >= weekStart && s.ScheduledDay.Date <= weekEnd)
    .ToListAsync();
```

**Loading availability for current user:**
```csharp
var myAvailability = await _context.Availability
    .Where(a => a.EmployeeId == userId && shiftIds.Contains(a.ScheduledShiftId))
    .ToListAsync();
```

**Upsert availability (already implemented in AvailabilitiesController — do not duplicate):**
```csharp
var existing = await _context.Availability
    .FirstOrDefaultAsync(a => a.EmployeeId == userId && a.ScheduledShiftId == scheduledShiftId);
if (existing != null) { existing.IsAvailable = isAvailable; }
else { _context.Availability.Add(new Availability { ... }); }
await _context.SaveChangesAsync();
```

**Checking IsShiftScoped in review (already implemented in ScheduleReviewController — do not duplicate):**
```csharp
var dayScoped = requirements.Where(r => !r.IsShiftScoped).ToList();
var shiftScoped = requirements.Where(r => r.IsShiftScoped).ToList();
```

---

## Existing controllers — do not duplicate their logic

These already exist and work correctly. Reference them, link to them from the dashboard, but do not rewrite them:

- `VenuesController` — full CRUD
- `RolesController` — full CRUD
- `ShiftDefinitionsController` — full CRUD
- `VenueStaffingRequirementsController` — full CRUD
- `ScheduledDaysController` — full CRUD
- `ScheduledShiftsController` — full CRUD
- `AvailabilitiesController` — `Index` + `SetAvailability` (POST, upsert, session-scoped)
- `ScheduleReviewController` — `Review` (GET), `AssignShift` (POST), `AssignDay` (POST)

---

## What "done" looks like

A user (employee or manager) can open Turnus, log in, and immediately see the week's schedule without navigating anywhere. An employee can click a shift cell, see its details, and mark their availability from that same view. A manager can access the admin dashboard, set up a venue end-to-end, schedule shifts, review availability, and assign staff — all from one consolidated flow without typing URLs. Any logged-in user can view their profile and upcoming shifts.

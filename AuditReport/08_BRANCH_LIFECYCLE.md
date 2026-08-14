# Module 08 — Branch Lifecycle

## Scope

Verify branch creation, slug/public access, active state, reactivation, branch scope, and branch-specific menus.

## Expected Lifecycle

An authorized tenant administrator creates a branch, it becomes publicly addressable when active, deactivation removes public access, and reactivation restores it.

## What Was Actually Executed

Used onboarding for the first branch, created a second branch through `/Branches/Create`, deactivated and reactivated the first branch, and tested branch-manager visibility and public branch routes.

## Database Evidence

Two active branches were persisted for the audit tenant with unique slugs and audit events for creation/status changes.

## Functional Result

PASS. The branch create form persisted the second branch; inactive public route returned 404 and reactivation restored 200.

## Security Result

PASS for tenant-admin mutation policy and branch-manager restriction to the assigned branch.

## Tenant Isolation Result

PASS. Branch identifiers and public slugs were resolved within the active tenant context.

## Dynamic Data Result

PASS for branch records; status/sort filter labels remain code-defined under Module 06.

## UI Result

PASS for branch list/details and public route responses.

## Regression Result

PASS after menu assignments, QR generation, and manager-scope checks.

## Defects Found

No exercised branch lifecycle defect found.

## Evidence

- `Web/Controllers/BranchesController.cs`
- Live branch IDs and slugs recorded in working audit evidence.
- Public branch 200/404/200 sequence for active/deactivated/reactivated state.

# Module 09 — Menu Lifecycle

## Scope

Verify menu creation, branch assignment, scope, draft/published state, visibility, and public branch filtering.

## Expected Lifecycle

An authorized editor creates a menu, assigns it globally or to selected branches, adds content, publishes it, and sees it only on eligible active branch menus.

## What Was Actually Executed

Created three additional menus through the real form, assigned one to both branches, one to branch one, and one to branch two, added categories/items, published them, and requested both public branch routes.

## Database Evidence

The audit tenant had four menus total after onboarding; SQL showed the selected branch assignments and published statuses.

## Functional Result

PASS. Branch one displayed the shared and branch-one menus but not the branch-two menu; branch two displayed the shared and branch-two menus but not the branch-one menu.

## Security Result

PASS for tenant-scoped creation and manager denial of menu creation.

## Tenant Isolation Result

PASS. Menu assignments and public results stayed within the audit tenant and branch scope.

## Dynamic Data Result

PASS for menu records/types/scopes consumed from the database; workflow status values remain code-defined.

## UI Result

PASS for menu list/create/details/public menu responses.

## Regression Result

PASS after category move, draft suppression, republish, and pricing operations.

## Defects Found

One initial selected-branch POST in the audit harness used an incorrect array encoding and returned validation; retrying with indexed form keys succeeded. This was recorded as test-harness correction, not an application defect.

## Evidence

- `Web/Controllers/MenusController.cs`
- `Infrastructure/Services/MenuService.cs`
- Public branch one and branch two response content checks.

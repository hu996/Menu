# Module 04 — Authorization and Role Scope

## Scope

Verify role policies, tenant-owner administration, branch-manager restrictions, protected routes, and denial behavior.

## Expected Lifecycle

Users can perform only actions permitted by their role and branch scope; denied requests reach the access-denied path or 404 without leaking records.

## What Was Actually Executed

Created a real branch-manager membership scoped to the second branch, logged in as that manager, opened the branch list, attempted the first branch and its menu-management route directly, attempted menu creation, and tested lockout behavior.

## Database Evidence

The manager membership is active and linked to only the second branch. SQL and response checks showed only that branch in the manager's branch list.

## Functional Result

PASS for the exercised scope. The manager saw only the scoped branch; direct access to the other branch returned 404, and menu creation ended at AccessDenied.

## Security Result

PASS for policy enforcement, branch claims, route denial, antiforgery-protected mutation forms, and lockout. Full role matrix coverage for every menu editor/viewer/platform-admin endpoint was not claimed.

## Tenant Isolation Result

PASS in the exercised manager and cross-tenant ID cases; foreign identifiers did not expose records.

## Dynamic Data Result

PARTIAL under the strict rule because role choices are code-defined, though memberships themselves are stored in SQL.

## UI Result

PASS for visible scoped navigation and the AccessDenied page reached in the live run.

## Regression Result

PASS. Tenant-owner login and manager denial checks remained valid after the category-update change.

## Defects Found

No exercised authorization bypass found. A complete role-by-role endpoint matrix remains a verification gap.

## Evidence

- `Web/Controllers/UsersController.cs`
- `Web/Controllers/BranchesController.cs`
- `Web/Controllers/MenusController.cs`
- `Web/Program.cs` authorization policies
- Live manager account and scoped branch requests.

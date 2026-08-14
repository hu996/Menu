# Module 21 — Audit Trail

## Scope

Verify audit logging for authentication, tenant creation, catalog mutations, state transitions, pricing, billing, and actor/tenant context.

## Expected Lifecycle

Material mutations and security events create queryable tenant-scoped audit records without storing secrets.

## What Was Actually Executed

Created a tenant and catalog, changed menu/branch/item states, applied pricing, created a modifier/allergen, logged in/out, exercised failures, and queried AuditLogs.

## Database Evidence

Observed actions included `login.succeeded`, `login.failed`, `menu-item.created`, `menu.status-changed`, `pricing.bulk-applied`, `menu-item.updated`, `menu-category.created`, `menu.created`, `branch.status-changed`, `tenant.created`, and `subscription.created`.

## Functional Result

PASS for the exercised events and query page.

## Security Result

PASS for no password value being recorded in the audit evidence and for tenant-admin protection on the audit page.

## Tenant Isolation Result

PASS for audit records associated with the active tenant; a complete foreign-tenant audit query matrix was not run.

## Dynamic Data Result

PASS for persisted audit action/value data; action names are technical constants.

## UI Result

PASS for the authenticated audit page response.

## Regression Result

PASS. Later public/login checks did not produce runtime errors.

## Defects Found

No exercised audit-trail defect. Retention/export/alerting policy was not verified.

## Evidence

- `Infrastructure/Services/AuditService.cs`
- `Web/Controllers/AuditController.cs`
- Live SQL AuditLogs action counts.

# Module 05 — Tenant Isolation

## Scope

Verify tenant resolution, tenant-owned writes, tenant route scoping, cross-tenant identifiers, public slug/media isolation, and claim context.

## Expected Lifecycle

Every authenticated operation resolves one tenant context server-side; records, branches, menus, media, analytics, and mutations cannot cross tenant boundaries.

## What Was Actually Executed

Created a new tenant through the real onboarding flow, completed its catalog lifecycle, attempted cross-tenant public media access by slug and GUID, used tenant-route mismatch checks, and exercised branch-manager scope.

## Database Evidence

The audit tenant received a distinct ID and isolated rows for branches, menus, categories, products, associations, images, subscriptions, payments, audit logs, and analytics. The existing demo tenant remained separate.

## Functional Result

PASS for the bounded adversarial scenarios. New onboarding redirected into its tenant dashboard, and all tested public/private context resolution behaved as expected.

## Security Result

PASS for middleware tenant resolution, global filters, composite tenant foreign keys, and 404 behavior for mismatches.

## Tenant Isolation Result

PASS. A public media URL under the other tenant's slug and the same media path under the other tenant's GUID both returned 404.

## Dynamic Data Result

PASS for tenant-specific operational data; strict dynamic-option findings are isolated to Module 06.

## UI Result

PASS for tenant-branded dashboard/public menu responses observed during the live run.

## Regression Result

PASS after category and login verification. No foreign tenant data appeared in the retested public or authenticated responses.

## Defects Found

No bounded tenant-isolation defect found. A fully automated all-endpoint IDOR matrix was not completed.

## Evidence

- `Web/Middleware/TenantMiddleware.cs`
- `Infrastructure/Persistence/AppDbContext.cs`
- Live tenant slug `audit-live-20260813040604` and distinct SQL tenant ID.

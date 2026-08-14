# Module 27 — Blockers, Defects, and Fix Record

## Scope

Preserve original failures, distinguish application defects from test-harness mistakes, record fixes, and map unresolved blockers to release impact.

## Expected Lifecycle

Every failed check is either fixed and retested with evidence or remains explicitly blocking; no failure is silently discarded.

## What Was Actually Executed

Maintained the audit sequence while recording the category-update failure, selected-menu/bulk-form encoding corrections, sandbox payment result, production reset behavior, and production configuration findings.

## Database Evidence

The corrected category move and pricing history/audit records are present in SQL. Sandbox payment remains Pending and webhook configuration is empty.

## Functional Result

PASS for resolved application defect; FAIL for open production blockers.

## Security Result

PASS for the category fix's current-tenant validation and normal auth flow. Payment/reset operational controls are incomplete.

## Tenant Isolation Result

PASS for the retested category/media/branch/manager paths.

## Dynamic Data Result

P2-002 remains open for strict compliance.

## UI Result

PASS for the corrected category selector and login form; visual evidence remains partial.

## Regression Result

PASS for all directly affected retests on the current audit build.

## Defects Found

- **Resolved:** product edit ignored `CategoryId`; fixed in DTO, service, controller, and view, then live-moved a product and verified public output.
- **Not a product defect:** initial selected-branch and bulk requests used incorrect PowerShell array encoding; indexed form keys passed.
- **Open P1-001:** sandbox-only payments/unconfigured webhook.
- **Open P1-002:** production reset has no email delivery.
- **Open P1-003:** local SQL/storage, wildcard hosts, empty secret, no verified backup/restore/deployment controls.
- **Open P2-001/P2-002:** volume and dynamic-data evidence/compliance gaps.

## Evidence

- `Application/DTOs/MenuDtos.cs`
- `Infrastructure/Services/MenuService.cs`
- `Web/Controllers/MenusController.cs`
- `Web/Views/Menus/EditItem.cshtml`
- `00_EXECUTIVE_VERDICT.md`

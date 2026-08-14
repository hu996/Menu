# Module 17 — QR Code Lifecycle

## Scope

Verify QR generation, branch/table labels, valid resolution, invalid code, SVG delivery, and inactive-branch behavior.

## Expected Lifecycle

An authorized administrator generates codes for a branch, scans resolve to the correct public menu, invalid codes fail safely, and disabled branch/code state blocks public access.

## What Was Actually Executed

Generated two QR cards for the first branch with table labels, requested the valid code route and SVG, requested an invalid code, deactivated/reactivated the branch around a valid QR request.

## Database Evidence

Two QR rows persisted for the branch with active flags and audit history. Code values were not copied into this report.

## Functional Result

PASS. Valid QR route returned 200 with the correct public menu; invalid code returned 404; SVG asset returned 200; disabled branch returned 404 and reactivation restored 200.

## Security Result

PASS for active branch/code checks and no public result for invalid/inactive state.

## Tenant Isolation Result

PASS for branch-owned QR resolution.

## Dynamic Data Result

PASS for persisted QR records and labels.

## UI Result

PASS for QR management response and generated SVG delivery; physical-device scan and screenshot visual review were not performed.

## Regression Result

PASS after branch, menu, category, and public lifecycle tests.

## Defects Found

No exercised QR defect.

## Evidence

- `Web/Controllers/QrCodesController.cs`
- `Web/Controllers/PublicMenuController.cs`
- `Infrastructure/Services/QrCodeService.cs`
- Valid/invalid/inactive public route status sequence.

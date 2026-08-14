# Module 16 — Publish, Draft, and Preview Lifecycle

## Scope

Verify draft isolation, publish transition, public visibility, item changes while draft, and restoration after publish.

## Expected Lifecycle

Draft content is editable and previewable to authorized users but hidden from anonymous public menus; publishing makes the final content visible.

## What Was Actually Executed

Set the Main Food menu to Draft, edited a product while draft, requested the public branch menu, published the menu again, and requested the same public route.

## Database Evidence

SQL showed the menu status transition and product update. The menu was restored to Published at the end of the run.

## Functional Result

PASS. Draft menu and its product were absent publicly; after publishing, the menu/product returned with the new content and effective price.

## Security Result

PASS for public published-state filtering and authorized status mutation.

## Tenant Isolation Result

PASS. Status and public filtering were applied within the audit tenant/branch assignments.

## Dynamic Data Result

PASS for persisted menu state; status values are workflow constants.

## UI Result

PASS for menu list/status actions and public content response.

## Regression Result

PASS after restoring Published; later QR, public, login, and media checks remained valid.

## Defects Found

No publish defect. An earlier audit attempt used an onboarding item that also existed in another published menu; the corrected test used a Main Food-only item and confirmed true draft suppression.

## Evidence

- `Web/Controllers/MenusController.cs`
- `Infrastructure/Services/MenuService.cs`
- Public branch response before/after the draft-to-published transition.

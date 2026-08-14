# Module 10 — Category Lifecycle

## Scope

Verify category creation, menu ownership, product placement, category move, active/public behavior, and tenant safety.

## Expected Lifecycle

An editor creates a category under a tenant menu, places products in it, can move a product to another valid category, and public output follows the resulting published structure.

## What Was Actually Executed

Created categories under the audit menus, discovered that editing a product did not initially apply the posted `CategoryId`, fixed the DTO/service/controller/view path, rebuilt, and moved a live product to the dessert category.

## Database Evidence

SQL after the fix showed the product's `MenuCategoryId` changed to `Audit Desserts` under the published dessert menu. The category records and menu relationships remained tenant-owned.

## Functional Result

PASS after fix. The edit form rendered four category options; the live POST returned 200 and the public menu showed the moved product in the dessert section.

## Security Result

PASS. The service now loads the target category through the current tenant and rejects a missing/foreign category rather than trusting a raw identifier.

## Tenant Isolation Result

PASS for the corrected move path and category ownership query.

## Dynamic Data Result

PASS for category records; category status/workflow labels remain subject to Module 06.

## UI Result

PASS after the edit view added a database-backed category selector and retained the current category display.

## Regression Result

PASS. Build had zero warnings/errors; public menu, draft suppression, republish, and login were retested afterward.

## Defects Found

Resolved defect: `CategoryId` was previously ignored by `UpdateItemAsync`. The fix is recorded rather than hidden because it was found during the audit.

## Evidence

- `Application/DTOs/MenuDtos.cs`
- `Infrastructure/Services/MenuService.cs`
- `Web/Controllers/MenusController.cs`
- `Web/Views/Menus/EditItem.cshtml`
- Live SQL/public verification of the moved product.

# Module 11 — Product Lifecycle

## Scope

Verify product creation, edit, price, availability, category placement, associations, public rendering, and volume behavior.

## Expected Lifecycle

An editor creates products under categories, edits them safely, associates managed ingredients/allergens/modifiers, toggles availability, and publishes only intended data.

## What Was Actually Executed

Created 10 products plus onboarding and dessert/drink products, edited a product's name/price/category, associated ingredient/allergen/modifier data, toggled availability, ran a draft/published lifecycle, and performed bulk pricing across 21 selected products.

## Database Evidence

The tenant reached 23 products. Product association rows, price histories, and pricing audit rows were persisted; one sampled product retained its associations after edit.

## Functional Result

PASS for exercised operations. Unavailable products remained visible with an unavailable state, draft-menu products were hidden publicly, and republishing restored visibility.

## Security Result

PASS for tenant-scoped edit and category validation. Full-volume performance at the prompt's larger target was not run.

## Tenant Isolation Result

PASS for the live tenant and public branch results.

## Dynamic Data Result

PASS for product type/currency/category/association data consumed from SQL; availability/sort labels are code-defined UI choices.

## UI Result

PASS for create/edit, association, availability, pricing, and public menu responses.

## Regression Result

PASS after the category fix and subsequent login/public/security retests.

## Defects Found

P2-001 remains as a volume-evidence gap. The first bulk pricing request used a PowerShell array encoding that posted one value and correctly returned validation; the indexed-key retry succeeded and is not an application defect.

## Evidence

- `Web/Controllers/MenusController.cs`
- `Web/Controllers/ProductsController.cs`
- `Infrastructure/Services/MenuService.cs`
- SQL product/association/history counts and public response checks.

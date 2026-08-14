# Module 13 — Ingredients and Allergens

## Scope

Verify managed ingredient/allergen creation, product associations, public disclosure, and tenant ownership.

## Expected Lifecycle

Tenant administrators create managed values, editors associate them to products, and the public menu discloses the active values without leaking other tenants.

## What Was Actually Executed

Created one ingredient and one allergen through their real forms, associated both to a product, edited that product, and checked the public menu text.

## Database Evidence

SQL contained the ingredient, allergen, and corresponding product link rows for the audit tenant. Audit events recorded the created catalog values.

## Functional Result

PASS for the exercised create, associate, preserve-on-edit, and public disclosure flow.

## Security Result

PASS for tenant-scoped services and authorized management routes. Deactivate/reactivate behavior for these values was not separately forced.

## Tenant Isolation Result

PASS for the audit tenant's associations and public response.

## Dynamic Data Result

PASS. Values are database-backed and editable; status/filter labels remain code-defined as noted in Module 06.

## UI Result

PASS for management forms and public `Ingredients`/`Contains` output observed.

## Regression Result

PASS. Associations survived product edit, category move, and publication checks.

## Defects Found

No exercised functional defect. Deactivation behavior remains a coverage gap.

## Evidence

- `Web/Controllers/IngredientsController.cs`
- `Web/Controllers/AllergensController.cs`
- `Infrastructure/Services/IngredientService.cs`
- `Infrastructure/Services/AllergenService.cs`
- SQL association rows and public menu excerpt.

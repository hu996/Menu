# Module 06 — Dynamic Data and Configuration

## Scope

Determine whether business-facing values are database-backed and editable rather than duplicated as hardcoded lists in controllers/views.

## Expected Lifecycle

Tenant and platform operators should manage menus, categories, products, lookups, currencies, ingredients, allergens, modifiers, plans, statuses, and options from the application/configuration without code edits.

## What Was Actually Executed

Created and edited database-backed restaurant, branch, menus, categories, products, ingredients, allergens, modifiers, and pricing data. Inspected views/controllers/enums for literal lists.

## Database Evidence

Lookup types and values, menu types/scopes, product types, currencies, catalog records, plans, and modifier data are persisted and were consumed by the live UI.

## Functional Result

PARTIAL. Core catalog and lookup lifecycle is dynamic and worked; strict project-wide compliance is not met.

## Security Result

PASS for the observed lookup administration policy and tenant scoping. Role/workflow enums remain code-defined for security and state transitions.

## Tenant Isolation Result

PASS for the created tenant's data; no foreign lookup/catalog data was returned in exercised tenant routes.

## Dynamic Data Result

FAIL under the prompt's strict interpretation for complete compliance. Status filters, sort choices, branch-menu labels, menu workflow statuses, and role choices remain literal in Razor/controllers/enums.

## UI Result

PASS for current behavior; the issue is maintainability/configurability rather than a rendering failure.

## Regression Result

PASS. Database-backed options continued to render after the category fix and current build verification.

## Defects Found

P2-002: convert remaining business-facing choice lists to managed lookup/configuration where required, while retaining immutable security/workflow constants where appropriate and documenting the boundary.

## Evidence

- `Web/Views/Branches/Index.cshtml`
- `Web/Views/Allergens/Index.cshtml`
- `Web/Views/BranchMenus/Edit.cshtml`
- `Web/Views/Users/Create.cshtml`
- `Domain/Enums/`
- `Web/Controllers/MenusController.cs` lookup-backed options.

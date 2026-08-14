# Module 18 — Public Menu and Branch Experience

## Scope

Verify anonymous public menu routes, branch/menu scope, language requests, public media, availability, ingredients/allergens/modifiers, and invalid routes.

## Expected Lifecycle

An anonymous visitor can reach only an active branch's published menus and sees current product content, prices, availability, managed disclosures, images, and supported language output.

## What Was Actually Executed

Requested both branch slugs in English, requested Arabic output, checked active/inactive product state, public media, ingredient/allergen/modifier text, invalid branch, invalid QR, and protected route redirects.

## Database Evidence

Public responses matched SQL menu assignments/statuses, product state, branch price overrides, and managed association rows.

## Functional Result

PASS for bounded public behavior. Branch one and branch two showed their assigned menus only; invalid branch/QR/media paths returned 404.

## Security Result

PASS for anonymous published/active filtering and no public access to private dashboard routes.

## Tenant Isolation Result

PASS for public slug/menu/media isolation.

## Dynamic Data Result

PASS for menu/product/catalog content; remaining static labels are Module 06 findings.

## UI Result

PASS for HTTP-rendered English/Arabic responses and content semantics. Full mobile visual/browser screenshot validation was not possible because the browser connector was unavailable.

## Regression Result

PASS after category move, image changes, draft/publish, QR, and login checks.

## Defects Found

No bounded public routing/content defect found. Visual accessibility and device matrix remain partial.

## Evidence

- `Web/Controllers/PublicMenuController.cs`
- `Web/Views/PublicMenu/`
- Branch one/two 200 responses and invalid-route 404 responses.

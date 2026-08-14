# Module 15 — Pricing and Price History

## Scope

Verify product/category/branch scopes, percentage/fixed increase/decrease, exact set, preview/apply, bulk selection, effective prices, audit, and history.

## Expected Lifecycle

An authorized user previews a scoped change, applies it deliberately, sees branch-effective prices, and the system records immutable history/audit data.

## What Was Actually Executed

Executed product percentage increase/decrease, fixed increase/decrease, exact set, category fixed increase, branch percentage increase, and a bulk preview/apply over 21 selected products using real forms and corrected indexed form keys.

## Database Evidence

The live tenant accumulated 112 price-history rows and 10 pricing-audit rows; 23 products had recent pricing data at the end of the exercised run.

## Functional Result

PASS. Each operation returned preview/apply success and effective public prices reflected branch overrides where applicable.

## Security Result

PASS for authorized pricing routes, tenant scoping, and deliberate preview/apply workflow. A forced rollback/failure transaction was not separately injected.

## Tenant Isolation Result

PASS for the audit tenant's products, category, and branch pricing contexts.

## Dynamic Data Result

PASS for pricing records and currency values; operation labels are workflow constants.

## UI Result

PASS for pricing page, preview results, apply result, and public effective prices.

## Regression Result

PASS. Pricing remained correct after category movement, draft/publish, and public menu checks.

## Defects Found

No pricing defect found. The initial bulk request's non-indexed PowerShell array was a harness encoding error; the real indexed POST passed.

## Evidence

- `Web/Controllers/PricingController.cs`
- `Infrastructure/Services/PricingService.cs`
- SQL price-history/audit counts and public effective-price response.

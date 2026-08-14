# Module 02 — Database Schema and Migrations

## Scope

Verify SQL Server connectivity, migration state, schema relationships, indexes, tenant keys, and persistence of real lifecycle events.

## Expected Lifecycle

The application must restore, connect to SQL Server, apply migrations idempotently, enforce relational integrity, and persist tenant-scoped records and history.

## What Was Actually Executed

Ran restore, `dotnet ef database update`, queried the live database, created a tenant through onboarding, and persisted branches, menus, categories, products, images, pricing history, payments, audit logs, and analytics events.

## Database Evidence

Live schema: 30 tables, 32 foreign keys, 88 indexes, 29 applied migrations, and 24 tenant-id columns. The audit tenant contained 2 branches, 4 menus, 4 categories, and 23 products after the exercised lifecycle.

## Functional Result

PASS. Restore completed; EF reported no pending migrations; inserts and updates were visible through subsequent SQL queries and public responses.

## Security Result

PASS for observed composite tenant relationships, foreign keys, unique token indexes, hashed password persistence, and query-filtered services.

## Tenant Isolation Result

PASS for the exercised cross-tenant media and route attempts; no foreign-tenant record was returned.

## Dynamic Data Result

PASS for persisted operational data and lookup-backed catalog fields; strict UI option compliance is assessed in Module 06.

## UI Result

PASS for pages backed by the live schema, including dashboard, menus, pricing, billing, users, QR codes, and public menus.

## Regression Result

PASS. Database update was idempotent and the post-fix build connected to the same database without schema drift errors.

## Defects Found

No migration or relational-integrity blocker found. Production SQL endpoint and backup/restore process are not configured or verified.

## Evidence

- `Infrastructure/Persistence/AppDbContext.cs`
- `Infrastructure/Persistence/Migrations/`
- `dotnet ef database update --project Infrastructure --startup-project Web --no-build`
- Live database counts recorded in this package and `00_EXECUTIVE_VERDICT.md`.

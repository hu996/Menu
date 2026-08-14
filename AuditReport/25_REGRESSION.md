# Module 25 — Regression and Post-Fix Verification

## Scope

Verify that the build, startup, auth, tenant context, public menu, media, QR, authorization, and pricing behavior remained sound after fixes.

## Expected Lifecycle

A defect fix is compiled, deployed to the tested runtime, and followed by retests of directly affected and critical adjacent paths.

## What Was Actually Executed

Built the solution to `build-audit2` with zero warnings/errors, restarted the Development audit runtime on port 5199, reran category move/public visibility, draft/publish, media isolation, manager denial, and current development login.

## Database Evidence

The post-fix product category change, menu status, image metadata, pricing history, audit events, and tenant memberships were visible in SQL.

## Functional Result

PASS for the exercised regression set.

## Security Result

PASS for tenant-aware category validation, login/authentication, manager scope, public media filtering, and logout.

## Tenant Isolation Result

PASS in post-fix public media, branch/menu, manager, and login context checks.

## Dynamic Data Result

PASS for category options and catalog values loaded from live data; strict static option findings remain.

## UI Result

PASS for rendered edit/login/public responses; visual browser evidence unavailable.

## Regression Result

PASS. Runtime stdout contained Application started, zero runtime stderr, and zero unhandled/fail markers. The only observed warning was expected HTTPS-port discovery while intentionally running Development over HTTP.

## Defects Found

The category-update defect is resolved and retested. Production blockers are not regression defects and remain open.

## Evidence

- Isolated build command and output.
- `audit-runtime2.out.log`, `audit-runtime2.err.log`
- Current live login result ending at `/Dashboard`.

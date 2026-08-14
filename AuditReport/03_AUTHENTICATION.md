# Module 03 — Authentication and Account Recovery

## Scope

Verify login, logout, failed attempts, lockout, password hashing, password reset, development-only defaults, and production behavior.

## Expected Lifecycle

The form submits to the real authentication service; valid credentials create the normal claims/cookie flow, invalid attempts fail and lock out, passwords remain hashed, and production never receives development form defaults.

## What Was Actually Executed

Loaded the current Development login form, verified both fields were prefilled, submitted those form values, followed the redirect to the tenant dashboard, logged out, tested protected-route redirect, exercised development password recovery, and tested a manager account's failed-attempt lockout. A Production login GET was checked separately.

## Database Evidence

The development account has an Identity-style password hash and active tenant membership; no plaintext password was logged or stored. Audit logs recorded successful and failed login events. The manager account locked after five invalid attempts.

## Functional Result

PASS in Development. Current build result: login GET 200 with both defaults, real POST 200 ending at `/Dashboard`, dashboard contained the tenant context, and logout returned to the public home page.

## Security Result

PASS for normal password verification, lockout, claims, antiforgery, and no auto-login/bypass. Password recovery is production-incomplete because no email provider is wired.

## Tenant Isolation Result

PASS. Successful login resolved the configured tenant membership and dashboard context; route tenant mismatch checks were also exercised.

## Dynamic Data Result

PASS for environment gating. Development defaults are read from `DevelopmentLoginDefaults`; Production login fields were empty and the response contained none of the development literals.

## UI Result

PASS for the rendered login and reset forms. The password input uses an explicitly encoded value because the standard password tag helper suppresses posted values.

## Regression Result

PASS after the category fix: development defaults still rendered and the real login still reached the dashboard.

## Defects Found

P1-002 remains: production password reset has no delivery channel. This does not bypass authentication, but it blocks a sellable production recovery lifecycle.

## Evidence

- `Web/Controllers/AccountController.cs`
- `Web/Views/Account/Login.cshtml`
- `Web/appsettings.Development.json`
- `Infrastructure/Identity/AuthService.cs`
- `Infrastructure/Identity/PasswordService.cs`
- Live HTTP login and Production-empty-field checks.

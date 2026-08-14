# Module 20 — Payments and Webhooks

## Scope

Verify payment initiation, provider reference, pending/success/failure state, webhook authentication/configuration, idempotency, and subscription activation.

## Expected Lifecycle

A real provider checkout is initiated, the provider callback is authenticated and idempotent, payment status changes correctly, and a verified payment activates the intended tenant subscription.

## What Was Actually Executed

Initiated the Starter plan payment through the real Billing form, inspected the resulting transaction, and posted a webhook request with the configured production-style endpoint behavior.

## Database Evidence

The newest payment transaction had provider `sandbox`, a sandbox reference, and `Pending` status. No paid activation was produced.

## Functional Result

FAIL for production. Initiation returns a pending sandbox transaction; the webhook endpoint returns 503 because the webhook secret is not configured.

## Security Result

The endpoint deliberately requires configuration and does not accept an unconfigured callback. A real provider signature/idempotency/paid-state path was not available to verify.

## Tenant Isolation Result

PASS for the pending transaction's tenant ownership.

## Dynamic Data Result

PARTIAL. Plan/amount/currency are database-backed; provider implementation and secret are not production-configured.

## UI Result

PASS for the pending/sandbox state; no successful-payment UI was available.

## Regression Result

PASS for safe failure behavior, but the release remains blocked.

## Defects Found

P1-001: replace `SandboxPaymentGateway` with a real configured provider, store secrets outside source config, implement signed/idempotent webhooks, and verify paid activation.

## Evidence

- `Infrastructure/DependencyInjection.cs`
- `Infrastructure/Payments/SandboxPaymentGateway.cs`
- `Web/Controllers/PaymentWebhookController.cs`
- `Web/appsettings.json` empty webhook secret.

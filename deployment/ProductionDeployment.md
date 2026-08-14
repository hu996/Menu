# Production deployment runbook

This runbook is the repeatable deployment contract for RestaurantMenuPlatform. Production configuration is supplied by the hosting environment; no production secret or connection string belongs in this repository.

## Prerequisites

- .NET 8 SDK for the migration/publish workstation, or Docker with access to the image registry.
- A production SQL Server database and an identity with permission to apply the reviewed EF migration artifact.
- HTTPS termination at the application or a trusted reverse proxy. The proxy must forward X-Forwarded-Proto, X-Forwarded-For, and the original host from configured trusted proxy addresses.
- An S3-compatible object-storage bucket with private-by-default access and credentials scoped to the application prefix.
- External secret injection for ConnectionStrings__DefaultConnection, Storage__AccessKey, Storage__SecretKey, and Payments__WebhookSecret.
- A configured SMTP/provider integration before password-reset delivery can be considered operational.

## Required production configuration

Set these values through the process environment or a secret manager:

    ASPNETCORE_ENVIRONMENT=Production
    ASPNETCORE_URLS=https://+:443
    AllowedHosts=menu.example.com;*.menu.example.com
    Security__RequireHttps=true
    ConnectionStrings__DefaultConnection=<encrypted SQL Server connection string>
    Storage__Provider=ObjectStorage
    Storage__Endpoint=https://object-storage.example.com
    Storage__Bucket=restaurant-menu-production
    Storage__Region=<provider region>
    Storage__AccessKey=<secret manager value>
    Storage__SecretKey=<secret manager value>
    Storage__UsePathStyle=true
    Payments__Provider=External
    Payments__WebhookSecret=<secret manager value>
    Email__Provider=Smtp

The production validator rejects local SQL Server, unencrypted SQL connections, wildcard hosts, HTTP-only mode, local storage, sandbox payments, missing webhook secrets, and Development-only configuration.

## Database deployment

1. Build the release artifact from a reviewed commit.
2. Generate and review the idempotent migration script:

       $env:ConnectionStrings__DefaultConnection = '<injected outside source control>'
       $env:EF_DESIGN_TIME = 'true'
       dotnet ef migrations script --idempotent --project Infrastructure --startup-project Web --output deployment\artifacts\RestaurantMenuPlatform.sql
       Remove-Item Env:EF_DESIGN_TIME

3. Review the generated SQL for destructive operations and execute it with the database release process, or run scripts\Apply-ProductionMigrations.ps1 using the already injected environment variable.
4. Confirm the target database has no pending migrations before deploying the application.

The web process does not apply migrations in Production. It fails closed when pending migrations are detected.

## Application deployment

       pwsh .\scripts\Publish-Production.ps1

Deploy the resulting deployment\artifacts\publish directory or the Docker image. Inject configuration at runtime, start the service, then verify /health/live and /health/ready over HTTPS.

Health responses expose only Healthy or Unhealthy, never connection details.

## Storage and media

The application stores only generated, tenant-prefixed object keys through IImageStorage. It does not expose bucket paths. Private media requires the authenticated tenant claim and a metadata row belonging to that tenant. Anonymous media is served only when the image belongs to a published menu for the requested restaurant slug.

The object-storage adapter uses SigV4-compatible requests. Provider connectivity and least-privilege bucket policy remain external deployment checks and are NOT VERIFIED by this repository-only environment.

## Backup and restore

Run scripts\Backup-Database.sql through the SQL Server backup operator using a backup destination outside the application host. On SQL Server Express, omit optional compression because the edition does not support it. Record the backup LSN/time and checksum result. For a restore drill:

1. Restore the backup to an isolated SQL Server database using scripts\Restore-Database.sql.
2. Point a staging process at the isolated database with ASPNETCORE_ENVIRONMENT=Staging and an external connection string.
3. Run /health/ready, log in with an existing test account, open Dashboard, Branches, Menus, Products, Pricing, and QR, and open the anonymous public menu.
4. Confirm representative tenants, memberships, menus, products, prices, QR records, audit records, and subscriptions are present.

No production backup/restore drill was possible in the restricted environment; the result is NOT VERIFIED until an operator executes it against the real SQL Server.

## Rollback

Application rollback is a deployment-slot/container rollback to the previous immutable publish artifact after the health and smoke checks fail. Do not delete or overwrite the previous artifact.

Database rollback is separate. Do not automatically run Down migrations in production. Restore the last known-good backup to an isolated database, validate the core flow there, and promote it according to the database change-control procedure. For additive migrations, the preferred recovery is usually application rollback plus a forward-compatible corrective migration.

## External integrations

- Production SQL Server: NOT VERIFIED in this workspace because integrated SQL authentication was unavailable to the audit process.
- Production object storage: NOT VERIFIED; the provider adapter and configuration contract are present, but no real bucket was available.
- Production HTTPS certificate, DNS, proxy, and callback URLs: NOT VERIFIED; these belong to the hosting environment.
- Production email delivery: NOT VERIFIED; the application has no verified provider credentials or delivery proof.
- Production payment provider: NOT VERIFIED and intentionally not simulated; the sandbox gateway is Development-only and the production gateway remains unavailable until a real provider is integrated.

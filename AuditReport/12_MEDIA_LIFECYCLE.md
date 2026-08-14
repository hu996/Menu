# Module 12 — Media and Image Lifecycle

## Scope

Verify multi-image upload, ordering, primary image, replace, delete, public delivery, and cross-tenant media denial.

## Expected Lifecycle

An editor can manage multiple images per product, one image is primary, ordering changes persist, replacements are safe, deleted images disappear, and public media is tenant/menu scoped.

## What Was Actually Executed

Uploaded four real multipart image fixtures to one product, set a different primary image, moved an image, deleted one, replaced another, queried the database, requested the public media URL, and attempted access through another tenant slug and GUID.

## Database Evidence

The product went from four image rows to three. The selected replacement retained the expected image content type/original name/alt text; primary and sort-order flags persisted.

## Functional Result

PASS. Public media returned 200 for the active published product and the invalid/cross-tenant paths returned 404.

## Security Result

PASS after the media controller fix: anonymous delivery requires an image belonging to an active category in a published menu assigned to the active branch.

## Tenant Isolation Result

PASS. The same storage asset path was not retrievable under the other tenant slug or GUID.

## Dynamic Data Result

PASS for image metadata and storage records; storage backend remains local-file only for production readiness.

## UI Result

PASS for the image management actions reached through the real product editor; visual gallery screenshot review was unavailable.

## Regression Result

PASS. Public menu and cross-tenant media checks remained correct after the category and login retests.

## Defects Found

P1-003 includes local file storage as a production deployment gap. The tested lifecycle itself passed.

## Evidence

- `Web/Controllers/MediaController.cs`
- `Web/Controllers/ImagesController.cs`
- `Infrastructure/Storage/LocalImageStorage.cs`
- SQL image rows and public media 200/404 checks.

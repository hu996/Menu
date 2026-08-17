using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Domain.Enums;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize]
public sealed class MediaController : Controller
{
    private readonly AppDbContext _db;
    private readonly IImageStorage _storage;
    private readonly ITenantContext _tenantContext;

    public MediaController(AppDbContext db, IImageStorage storage, ITenantContext tenantContext)
    {
        _db = db;
        _storage = storage;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<IActionResult> MenuItem(Guid mediaTenantId, string fileName, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId != mediaTenantId ||
            string.IsNullOrWhiteSpace(fileName) ||
            !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal) ||
            fileName.Contains('\\'))
            return NotFound();

        var url = $"/media/{mediaTenantId:D}/menu-items/{fileName}";
        var image = await _db.MenuItemImages
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.TenantId == mediaTenantId && x.Url == url, cancellationToken);
        if (image is null)
            return NotFound();

        var stream = await _storage.OpenReadAsync(mediaTenantId, url, cancellationToken);
        return stream is null
            ? NotFound()
            : File(stream, image.ContentType ?? "application/octet-stream", enableRangeProcessing: true);
    }

    [HttpGet]
    public async Task<IActionResult> Branding(Guid mediaTenantId, string fileName, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId != mediaTenantId ||
            string.IsNullOrWhiteSpace(fileName) ||
            !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal) ||
            fileName.Contains('\\'))
            return NotFound();

        var url = $"/media/{mediaTenantId:D}/branding/{fileName}";
        var image = await _db.TenantBrandingImages
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.TenantId == mediaTenantId && x.Url == url, cancellationToken);
        if (image is null)
            return NotFound();

        var stream = await _storage.OpenBrandingReadAsync(mediaTenantId, url, cancellationToken);
        return stream is null
            ? NotFound()
            : File(stream, image.ContentType, enableRangeProcessing: true);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> PublicMenuItem(
        string restaurantSlug,
        string fileName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(restaurantSlug) ||
            string.IsNullOrWhiteSpace(fileName) ||
            !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal) ||
            fileName.Contains('\\'))
            return NotFound();

        var tenantId = _tenantContext.IsPublic ? _tenantContext.TenantId : null;
        if (!tenantId.HasValue)
            return NotFound();

        var internalUrl = $"/media/{tenantId.Value:D}/menu-items/{fileName}";
        var image = await _db.MenuItemImages
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId.Value &&
                x.Url == internalUrl &&
                x.MenuItem.MenuCategory.IsActive &&
                x.MenuItem.MenuCategory.Menu.Status == MenuStatus.Published &&
                x.MenuItem.MenuCategory.Menu.BranchMenus.Any(assignment =>
                    assignment.IsActive && assignment.Branch.IsActive))
            .Select(x => new { x.Url, x.ContentType })
            .SingleOrDefaultAsync(cancellationToken);
        if (image is null)
            return NotFound();

        var stream = await _storage.OpenReadAsync(tenantId.Value, image.Url, cancellationToken);
        if (stream is null)
            return NotFound();

        Response.Headers.CacheControl = "public,max-age=604800,immutable";
        return File(stream, image.ContentType ?? "application/octet-stream", enableRangeProcessing: true);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> PublicBranding(string restaurantSlug, string fileName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(restaurantSlug) ||
            string.IsNullOrWhiteSpace(fileName) ||
            !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal) ||
            fileName.Contains('\\'))
            return NotFound();

        var tenantId = _tenantContext.IsPublic ? _tenantContext.TenantId : null;
        if (!tenantId.HasValue)
            return NotFound();

        var internalUrl = $"/media/{tenantId.Value:D}/branding/{fileName}";
        var image = await _db.TenantBrandingImages
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId.Value &&
                x.Url == internalUrl &&
                _db.Menus.Any(menu =>
                    menu.TenantId == tenantId.Value &&
                    menu.Status == MenuStatus.Published &&
                    menu.BranchMenus.Any(assignment => assignment.IsActive && assignment.Branch.IsActive)))
            .Select(x => new { x.Url, x.ContentType })
            .SingleOrDefaultAsync(cancellationToken);
        if (image is null)
            return NotFound();

        var stream = await _storage.OpenBrandingReadAsync(tenantId.Value, image.Url, cancellationToken);
        if (stream is null)
            return NotFound();

        Response.Headers.CacheControl = "public,max-age=604800,immutable";
        return File(stream, image.ContentType, enableRangeProcessing: true);
    }
}

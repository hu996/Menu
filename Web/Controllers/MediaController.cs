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

        var tenant = await _db.Tenants
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Slug == restaurantSlug && x.IsActive, cancellationToken);
        if (tenant is null || _tenantContext.TenantId != tenant.Id)
            return NotFound();

        var image = (await _db.MenuItemImages
                .AsNoTracking()
                .Where(x => x.TenantId == tenant.Id)
                .ToListAsync(cancellationToken))
            .SingleOrDefault(x => string.Equals(Path.GetFileName(x.Url), fileName, StringComparison.Ordinal));
        if (image is null)
            return NotFound();

        var belongsToPublishedMenu = await _db.MenuItems
            .AsNoTracking()
            .Where(item => item.Id == image.MenuItemId &&
                           item.MenuCategory.IsActive &&
                           item.MenuCategory.Menu.Status == MenuStatus.Published &&
                           item.MenuCategory.Menu.BranchMenus.Any(assignment =>
                               assignment.IsActive && assignment.Branch.IsActive))
            .AnyAsync(cancellationToken);
        if (!belongsToPublishedMenu)
            return NotFound();

        var stream = await _storage.OpenReadAsync(tenant.Id, image.Url, cancellationToken);
        return stream is null
            ? NotFound()
            : File(stream, image.ContentType ?? "application/octet-stream", enableRangeProcessing: true);
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

        var tenant = await _db.Tenants.IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Slug == restaurantSlug && x.IsActive, cancellationToken);
        if (tenant is null || _tenantContext.TenantId != tenant.Id)
            return NotFound();

        var image = (await _db.TenantBrandingImages.IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.TenantId == tenant.Id)
                .ToListAsync(cancellationToken))
            .SingleOrDefault(x => string.Equals(Path.GetFileName(x.Url), fileName, StringComparison.Ordinal));
        if (image is null)
            return NotFound();

        var published = await _db.Menus.IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == tenant.Id && x.Status == MenuStatus.Published && x.BranchMenus.Any(y => y.IsActive && y.Branch.IsActive), cancellationToken);
        if (!published)
            return NotFound();

        var stream = await _storage.OpenBrandingReadAsync(tenant.Id, image.Url, cancellationToken);
        return stream is null
            ? NotFound()
            : File(stream, image.ContentType, enableRangeProcessing: true);
    }
}

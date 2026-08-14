using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class ImageManagementService : IImageManagementService
{
    private readonly AppDbContext _db;
    private readonly IImageStorage _storage;
    private readonly ITenantContext _tenantContext;

    public ImageManagementService(AppDbContext db, IImageStorage storage, ITenantContext tenantContext)
    {
        _db = db;
        _storage = storage;
        _tenantContext = tenantContext;
    }

    public async Task<MenuItemImageDto?> UploadAsync(
        Guid menuItemId,
        Stream content,
        string originalFileName,
        string contentType,
        long length,
        string? altText = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = RequireTenant();
        var item = await _db.MenuItems.SingleOrDefaultAsync(x => x.Id == menuItemId, cancellationToken);
        if (item is null)
            return null;

        var stored = await _storage.SaveAsync(tenantId, content, originalFileName, contentType, length, cancellationToken);
        try
        {
            var hasPrimary = await _db.MenuItemImages.AnyAsync(x => x.MenuItemId == menuItemId && x.IsPrimary, cancellationToken);
            var nextOrder = await _db.MenuItemImages
                .Where(x => x.MenuItemId == menuItemId)
                .Select(x => (int?)x.SortOrder)
                .MaxAsync(cancellationToken) ?? 0;
            var image = new MenuItemImage
            {
                TenantId = tenantId,
                MenuItemId = menuItemId,
                Url = stored.Url,
                StorageKey = stored.StoredFileName,
                OriginalFileName = Path.GetFileName(originalFileName),
                ContentType = contentType,
                AltText = NormalizeAltText(altText, originalFileName),
                IsPrimary = !hasPrimary,
                SortOrder = nextOrder + 1
            };
            _db.MenuItemImages.Add(image);
            await _db.SaveChangesAsync(cancellationToken);
            return ToDto(image);
        }
        catch
        {
            await _storage.DeleteAsync(tenantId, stored.Url, cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyList<MenuItemImageDto>?> GetForItemAsync(Guid menuItemId, CancellationToken cancellationToken = default)
    {
        if (!await _db.MenuItems.AnyAsync(x => x.Id == menuItemId, cancellationToken))
            return null;
        return await _db.MenuItemImages
            .AsNoTracking()
            .Where(x => x.MenuItemId == menuItemId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new MenuItemImageDto(x.Id, x.MenuItemId, x.Url, x.IsPrimary, x.SortOrder, x.OriginalFileName, x.AltText, x.ContentType, x.CreatedAtUtc, x.UpdatedAtUtc, x.StorageKey))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid imageId, CancellationToken cancellationToken = default)
    {
        var image = await _db.MenuItemImages.SingleOrDefaultAsync(x => x.Id == imageId, cancellationToken);
        if (image is null)
            return false;
        var tenantId = RequireTenant();
        _db.MenuItemImages.Remove(image);
        await _db.SaveChangesAsync(cancellationToken);
        await _storage.DeleteAsync(tenantId, image.Url, cancellationToken);

        if (image.IsPrimary)
        {
            var replacement = await _db.MenuItemImages.OrderBy(x => x.SortOrder).FirstOrDefaultAsync(x => x.MenuItemId == image.MenuItemId, cancellationToken);
            if (replacement is not null)
            {
                replacement.IsPrimary = true;
                await _db.SaveChangesAsync(cancellationToken);
            }
        }
        return true;
    }

    public async Task<bool> MoveAsync(Guid imageId, bool moveUp, CancellationToken cancellationToken = default)
    {
        var image = await _db.MenuItemImages.SingleOrDefaultAsync(x => x.Id == imageId, cancellationToken);
        if (image is null)
            return false;

        var siblings = await _db.MenuItemImages
            .Where(x => x.MenuItemId == image.MenuItemId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
        var index = siblings.FindIndex(x => x.Id == imageId);
        var target = moveUp ? index - 1 : index + 1;
        if (index < 0 || target < 0 || target >= siblings.Count)
            return true;

        (siblings[index].SortOrder, siblings[target].SortOrder) =
            (siblings[target].SortOrder, siblings[index].SortOrder);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<MenuItemImageDto?> ReplaceAsync(
        Guid imageId,
        Stream content,
        string originalFileName,
        string contentType,
        long length,
        string? altText = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = RequireTenant();
        var image = await _db.MenuItemImages.SingleOrDefaultAsync(x => x.Id == imageId, cancellationToken);
        if (image is null)
            return null;

        var stored = await _storage.SaveAsync(tenantId, content, originalFileName, contentType, length, cancellationToken);
        var previousUrl = image.Url;
        try
        {
            image.Url = stored.Url;
            image.StorageKey = stored.StoredFileName;
            image.OriginalFileName = Path.GetFileName(originalFileName);
            image.ContentType = contentType;
            image.AltText = NormalizeAltText(altText, originalFileName);
            image.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            await _storage.DeleteAsync(tenantId, previousUrl, cancellationToken);
            return ToDto(image);
        }
        catch
        {
            await _storage.DeleteAsync(tenantId, stored.Url, cancellationToken);
            throw;
        }
    }

    public async Task<bool> SetPrimaryAsync(Guid imageId, CancellationToken cancellationToken = default)
    {
        var image = await _db.MenuItemImages.SingleOrDefaultAsync(x => x.Id == imageId, cancellationToken);
        if (image is null)
            return false;
        var images = await _db.MenuItemImages.Where(x => x.MenuItemId == image.MenuItemId).ToListAsync(cancellationToken);
        foreach (var item in images)
            item.IsPrimary = item.Id == imageId;
        image.SortOrder = 0;
        var order = 1;
        foreach (var item in images.Where(x => x.Id != imageId).OrderBy(x => x.SortOrder))
            item.SortOrder = order++;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private Guid RequireTenant() => _tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context is required.");

    private static MenuItemImageDto ToDto(MenuItemImage image) =>
        new(image.Id, image.MenuItemId, image.Url, image.IsPrimary, image.SortOrder, image.OriginalFileName, image.AltText, image.ContentType, image.CreatedAtUtc, image.UpdatedAtUtc, image.StorageKey);

    private static string? NormalizeAltText(string? altText, string originalFileName)
    {
        var value = string.IsNullOrWhiteSpace(altText)
            ? Path.GetFileNameWithoutExtension(originalFileName)
            : altText.Trim();
        return value.Length > 300 ? value[..300] : value;
    }
}

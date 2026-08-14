namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IImageStorage
{
    Task<StoredImage> SaveAsync(
        Guid tenantId,
        Stream content,
        string originalFileName,
        string contentType,
        long length,
        CancellationToken cancellationToken = default);

    Task<StoredImage> SaveBrandingAsync(
        Guid tenantId,
        Stream content,
        string originalFileName,
        string contentType,
        long length,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid tenantId, string url, CancellationToken cancellationToken = default);

    Task DeleteBrandingAsync(Guid tenantId, string url, CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(
        Guid tenantId,
        string url,
        CancellationToken cancellationToken = default);

    Task<Stream?> OpenBrandingReadAsync(
        Guid tenantId,
        string url,
        CancellationToken cancellationToken = default);
}

public sealed record StoredImage(string Url, string StoredFileName);

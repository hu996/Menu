using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IImageManagementService
{
    Task<MenuItemImageDto?> UploadAsync(
        Guid menuItemId,
        Stream content,
        string originalFileName,
        string contentType,
        long length,
        string? altText = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MenuItemImageDto>?> GetForItemAsync(Guid menuItemId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid imageId, CancellationToken cancellationToken = default);
    Task<bool> SetPrimaryAsync(Guid imageId, CancellationToken cancellationToken = default);
    Task<bool> MoveAsync(Guid imageId, bool moveUp, CancellationToken cancellationToken = default);
    Task<MenuItemImageDto?> ReplaceAsync(
        Guid imageId,
        Stream content,
        string originalFileName,
        string contentType,
        long length,
        string? altText = null,
        CancellationToken cancellationToken = default);
}

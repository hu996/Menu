using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IRestaurantService
{
    Task<RestaurantSettingsDto?> GetAsync(CancellationToken cancellationToken = default);
    Task<RestaurantCreationResult> CreateAsync(RestaurantCreationInput input, CancellationToken cancellationToken = default);
    Task<RestaurantSettingsDto?> UpdateAsync(RestaurantSettingsInput input, CancellationToken cancellationToken = default);
    Task<TenantBrandingImageDto?> UploadBrandingAsync(
        Domain.Enums.TenantBrandingKind kind,
        Stream content,
        string originalFileName,
        string contentType,
        long length,
        CancellationToken cancellationToken = default);
    Task<bool> DeleteBrandingAsync(Domain.Enums.TenantBrandingKind kind, CancellationToken cancellationToken = default);
}

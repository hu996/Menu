using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IQrCodeService
{
    Task<QrCodeDto?> GetOrCreateAsync(
        Guid branchId,
        string? tableLabel,
        string baseUrl,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QrCodeDto>> GetForBranchAsync(
        Guid branchId,
        string baseUrl,
        CancellationToken cancellationToken = default);

    Task<QrCodeDto?> GetAsync(
        Guid id,
        string baseUrl,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QrCodeDto>> GetOrCreateBatchAsync(
        Guid branchId,
        IReadOnlyList<string> tableLabels,
        string baseUrl,
        CancellationToken cancellationToken = default);

    Task<QrCodeDto?> GetOrCreateForTableAsync(
        Guid tableId,
        string baseUrl,
        CancellationToken cancellationToken = default);

    Task<PublicOrderingContextDto?> ResolvePublicContextAsync(
        string restaurantSlug,
        string branchSlug,
        string code,
        CancellationToken cancellationToken = default);

    Task<QrCodeAssetDto?> RenderAsync(
        Guid id,
        string baseUrl,
        string format,
        CancellationToken cancellationToken = default);

    Task<bool> IsActiveAsync(string code, Guid branchId, CancellationToken cancellationToken = default);
    Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
}

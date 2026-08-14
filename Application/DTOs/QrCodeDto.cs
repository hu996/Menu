namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record QrCodeDto(
    Guid Id,
    Guid BranchId,
    string BranchName,
    string? TableLabel,
    string Code,
    string TargetUrl,
    bool IsActive,
    string? BranchNameAr = null,
    Guid? TableId = null,
    string? TableName = null,
    string? TableNameAr = null,
    string? RestaurantName = null,
    DateTime? CreatedAtUtc = null,
    DateTime? UpdatedAtUtc = null);

public sealed record PublicOrderingContextDto(
    Guid TenantId,
    string RestaurantName,
    string RestaurantSlug,
    Guid BranchId,
    string BranchName,
    string BranchSlug,
    Guid TableId,
    string TableName,
    string? TableNameAr,
    Guid QrCodeId,
    string QrCodeCode);

public sealed record QrCodeAssetDto(
    byte[] Content,
    string ContentType,
    string FileName);

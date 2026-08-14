namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record RestaurantTableDto(
    Guid Id,
    Guid BranchId,
    string BranchName,
    string Name,
    string? NameAr,
    bool IsActive,
    bool HasActiveQr,
    string? ActiveQrCode);

public sealed record RestaurantTableInput(string Name, string? NameAr);

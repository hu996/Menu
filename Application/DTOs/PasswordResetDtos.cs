namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record PasswordResetRequestResult(
    string? Token,
    string? RecipientEmail = null,
    string? RecipientName = null);

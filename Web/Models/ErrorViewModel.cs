namespace RestaurantMenuPlatform.Web.Models;

public sealed record ErrorViewModel(
    int StatusCode,
    string Title,
    string Message,
    string? RequestId = null);

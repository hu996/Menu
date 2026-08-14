using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IPasswordResetService
{
    Task<PasswordResetRequestResult> RequestAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> ResetAsync(
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);
}

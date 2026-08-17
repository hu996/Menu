using RestaurantMenuPlatform.Application.Interfaces;

namespace RestaurantMenuPlatform.Infrastructure.Email;

public sealed class UnconfiguredEmailSender : IEmailSender
{
    public Task SendPasswordResetAsync(
        string recipientEmail,
        string recipientName,
        string resetUrl,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Email delivery is not configured.");
}

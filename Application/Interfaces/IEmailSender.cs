namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IEmailSender
{
    Task SendPasswordResetAsync(
        string recipientEmail,
        string recipientName,
        string resetUrl,
        CancellationToken cancellationToken = default);
}

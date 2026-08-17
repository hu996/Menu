using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using RestaurantMenuPlatform.Application.Interfaces;

namespace RestaurantMenuPlatform.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _fromAddress;
    private readonly string _fromName;
    private readonly string? _username;
    private readonly string? _password;
    private readonly bool _enableSsl;
    private readonly int _timeoutMilliseconds;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _host = Required(configuration["Email:Smtp:Host"], "Email:Smtp:Host");
        _port = PositiveInt(configuration["Email:Smtp:Port"], 587);
        _fromAddress = Required(configuration["Email:FromAddress"], "Email:FromAddress");
        _fromName = configuration["Email:FromName"]?.Trim() ?? "Restaurant Menu Platform";
        _username = NullIfWhiteSpace(configuration["Email:Smtp:Username"]);
        _password = NullIfWhiteSpace(configuration["Email:Smtp:Password"]);
        _enableSsl = !bool.TryParse(configuration["Email:Smtp:EnableSsl"], out var enabled) || enabled;
        _timeoutMilliseconds = PositiveInt(configuration["Email:Smtp:TimeoutMilliseconds"], 15_000);
    }

    public async Task SendPasswordResetAsync(
        string recipientEmail,
        string recipientName,
        string resetUrl,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(resetUrl, UriKind.Absolute, out var resetUri) ||
            (resetUri.Scheme != Uri.UriSchemeHttps && resetUri.Scheme != Uri.UriSchemeHttp))
            throw new InvalidOperationException("The password reset URL is invalid.");

        using var message = new MailMessage
        {
            From = new MailAddress(_fromAddress, _fromName, Encoding.UTF8),
            Subject = "Reset your Restaurant Menu Platform password",
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8,
            IsBodyHtml = true,
            Body = BuildBody(recipientName, resetUri.AbsoluteUri)
        };
        message.To.Add(new MailAddress(recipientEmail, recipientName, Encoding.UTF8));
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
            $"Reset your password using this link: {resetUri.AbsoluteUri}",
            Encoding.UTF8,
            "text/plain"));

        using var client = new SmtpClient(_host, _port)
        {
            EnableSsl = _enableSsl,
            Timeout = _timeoutMilliseconds,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = _username is null ? null : new NetworkCredential(_username, _password)
        };

        await client.SendMailAsync(message).WaitAsync(cancellationToken);
    }

    private static string BuildBody(string recipientName, string resetUrl)
    {
        var safeName = WebUtility.HtmlEncode(recipientName);
        var safeUrl = WebUtility.HtmlEncode(resetUrl);
        return $"""
            <!doctype html>
            <html><body style="font-family:Arial,sans-serif;color:#202421;line-height:1.6">
              <h1 style="font-size:22px">Reset your password</h1>
              <p>Hello {safeName},</p>
              <p>Use the secure link below to set a new password. The link expires in one hour and can be used once.</p>
              <p><a href="{safeUrl}">Reset password</a></p>
              <p>If you did not request this, you can safely ignore this email.</p>
            </body></html>
            """;
    }

    private static string Required(string? value, string key) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{key} must be configured.")
            : value.Trim();

    private static int PositiveInt(string? value, int fallback) =>
        int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

using System.Security.Cryptography;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;

namespace RestaurantMenuPlatform.Web.Controllers;

[ApiController]
[Route("payments/webhook")]
[AllowAnonymous]
[IgnoreAntiforgeryToken]
[EnableRateLimiting("payment-webhook")]
public sealed class PaymentWebhookController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public PaymentWebhookController(
        IPaymentService paymentService,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _paymentService = paymentService;
        _configuration = configuration;
        _environment = environment;
    }

    [HttpPost]
    [Consumes("application/json")]
    [RequestSizeLimit(64 * 1024)]
    public async Task<IActionResult> Receive(
        [FromHeader(Name = "X-Payment-Timestamp")] string? timestampHeader,
        [FromHeader(Name = "X-Payment-Signature")] string? signature,
        [FromHeader(Name = "X-Payment-Webhook-Key")] string? webhookKey,
        CancellationToken cancellationToken)
    {
        var configuredKey = _configuration["Payments:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(configuredKey))
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Payment webhooks are not configured.");

        await using var bodyStream = new MemoryStream();
        await Request.Body.CopyToAsync(bodyStream, cancellationToken);
        if (bodyStream.Length > 64 * 1024)
            return StatusCode(StatusCodes.Status413PayloadTooLarge);
        var body = bodyStream.ToArray();

        var validSignature = HasValidSignature(timestampHeader, signature, body, configuredKey);
        var developmentLegacyKey = _environment.IsDevelopment() && HasMatchingKey(webhookKey, configuredKey);
        if (!validSignature && !developmentLegacyKey)
            return Unauthorized();

        PaymentWebhookRequest? request;
        try
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(new JsonStringEnumConverter());
            request = JsonSerializer.Deserialize<PaymentWebhookRequest>(body, options);
        }
        catch (JsonException)
        {
            return BadRequest();
        }
        if (request is null)
            return BadRequest();

        var processed = await _paymentService.ProcessWebhookAsync(request, cancellationToken);
        return processed ? Ok() : NotFound();
    }

    private static bool HasValidSignature(
        string? timestampHeader,
        string? suppliedSignature,
        ReadOnlySpan<byte> body,
        string secret)
    {
        if (!long.TryParse(timestampHeader, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unixSeconds) ||
            string.IsNullOrWhiteSpace(suppliedSignature))
            return false;

        DateTimeOffset timestamp;
        try
        {
            timestamp = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
        if ((DateTimeOffset.UtcNow - timestamp).Duration() > TimeSpan.FromMinutes(5))
            return false;

        var normalizedSignature = suppliedSignature.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)
            ? suppliedSignature[7..]
            : suppliedSignature;
        byte[] suppliedBytes;
        try
        {
            suppliedBytes = Convert.FromHexString(normalizedSignature);
        }
        catch (FormatException)
        {
            return false;
        }

        var timestampBytes = Encoding.UTF8.GetBytes(timestampHeader);
        var signedPayload = new byte[timestampBytes.Length + 1 + body.Length];
        timestampBytes.CopyTo(signedPayload, 0);
        signedPayload[timestampBytes.Length] = (byte)'.';
        body.CopyTo(signedPayload.AsSpan(timestampBytes.Length + 1));
        var expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), signedPayload);
        return suppliedBytes.Length == expected.Length &&
               CryptographicOperations.FixedTimeEquals(suppliedBytes, expected);
    }

    private static bool HasMatchingKey(string? suppliedKey, string configuredKey)
    {
        if (string.IsNullOrWhiteSpace(suppliedKey))
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(suppliedKey),
            Encoding.UTF8.GetBytes(configuredKey));
    }
}

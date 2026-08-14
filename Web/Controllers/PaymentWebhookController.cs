using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;

namespace RestaurantMenuPlatform.Web.Controllers;

[ApiController]
[Route("payments/webhook")]
[AllowAnonymous]
[IgnoreAntiforgeryToken]
public sealed class PaymentWebhookController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _configuration;

    public PaymentWebhookController(
        IPaymentService paymentService,
        IConfiguration configuration)
    {
        _paymentService = paymentService;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> Receive(
        [FromBody] PaymentWebhookRequest request,
        [FromHeader(Name = "X-Payment-Webhook-Key")] string? webhookKey,
        CancellationToken cancellationToken)
    {
        var configuredKey = _configuration["Payments:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(configuredKey))
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Payment webhooks are not configured.");

        if (!HasMatchingKey(webhookKey, configuredKey))
            return Unauthorized();

        var processed = await _paymentService.ProcessWebhookAsync(request, cancellationToken);
        return processed ? Ok() : NotFound();
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

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;

namespace RestaurantMenuPlatform.Infrastructure.Payments;

/// <summary>
/// Production adapter for a payment orchestrator. The remote endpoint must create
/// the provider checkout and return a stable provider reference and HTTPS URL.
/// Provider-specific secrets and SDKs stay outside the core application.
/// </summary>
public sealed class ExternalPaymentGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly Uri _initiationEndpoint;
    private readonly string _apiKey;
    private readonly Uri _successUrl;
    private readonly Uri _cancelUrl;
    private readonly HashSet<string> _allowedCheckoutHosts;

    public ExternalPaymentGateway(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var baseUrl = RequiredHttpsUri(configuration["Payments:ApiBaseUrl"], "Payments:ApiBaseUrl");
        var initiationPath = configuration["Payments:InitiatePath"]?.Trim() ?? "/payments";
        _initiationEndpoint = new Uri(baseUrl, initiationPath);
        _apiKey = Required(configuration["Payments:ApiKey"], "Payments:ApiKey");
        _successUrl = RequiredHttpsUri(configuration["Payments:SuccessUrl"], "Payments:SuccessUrl");
        _cancelUrl = RequiredHttpsUri(configuration["Payments:CancelUrl"], "Payments:CancelUrl");
        _allowedCheckoutHosts = configuration.GetSection("Payments:AllowedCheckoutHosts").GetChildren()
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim().ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (_allowedCheckoutHosts.Count == 0)
            throw new InvalidOperationException("Payments:AllowedCheckoutHosts must contain at least one checkout host.");
    }

    public string Provider => "external";

    public async Task<PaymentGatewayInitiation> InitiateAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, _initiationEndpoint)
        {
            Content = JsonContent.Create(new
            {
                transactionId = request.TransactionId,
                amount = request.Amount,
                currency = request.Currency,
                successUrl = _successUrl.AbsoluteUri,
                cancelUrl = _cancelUrl.AbsoluteUri
            })
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        message.Headers.TryAddWithoutValidation("Idempotency-Key", request.TransactionId.ToString("N"));

        using var response = await _httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"The payment provider rejected checkout creation ({(int)response.StatusCode}).");

        var payload = await response.Content.ReadFromJsonAsync<ExternalInitiationResponse>(
            new JsonSerializerOptions(JsonSerializerDefaults.Web),
            cancellationToken);
        if (payload is null || string.IsNullOrWhiteSpace(payload.ProviderReference))
            throw new InvalidOperationException("The payment provider returned an invalid reference.");
        var provider = string.IsNullOrWhiteSpace(payload.Provider) ? Provider : payload.Provider.Trim();
        if (provider.Length > 64 || payload.ProviderReference.Trim().Length > 200)
            throw new InvalidOperationException("The payment provider returned an oversized identifier.");
        if (!Uri.TryCreate(payload.CheckoutUrl, UriKind.Absolute, out var checkoutUrl) ||
            checkoutUrl.Scheme != Uri.UriSchemeHttps ||
            !_allowedCheckoutHosts.Contains(checkoutUrl.Host))
            throw new InvalidOperationException("The payment provider returned an untrusted checkout URL.");

        return new PaymentGatewayInitiation(
            provider,
            payload.ProviderReference.Trim(),
            checkoutUrl.AbsoluteUri);
    }

    private static string Required(string? value, string key) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{key} must be configured.")
            : value.Trim();

    private static Uri RequiredHttpsUri(string? value, string key)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException($"{key} must be an absolute HTTPS URL.");
        return uri;
    }

    private sealed record ExternalInitiationResponse(
        string? Provider,
        string? ProviderReference,
        string? CheckoutUrl);
}

using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;

namespace RestaurantMenuPlatform.Infrastructure.Payments;

/// <summary>
/// Keeps the application safe when a real payment provider has not been configured.
/// It never simulates a payment and leaves the feature explicitly unavailable.
/// </summary>
public sealed class UnconfiguredPaymentGateway : IPaymentGateway
{
    public string Provider => "unconfigured";

    public Task<PaymentGatewayInitiation> InitiateAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("A production payment provider is not configured.");
}

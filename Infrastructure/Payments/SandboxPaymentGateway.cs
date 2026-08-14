using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;

namespace RestaurantMenuPlatform.Infrastructure.Payments;

/// <summary>
/// Safe default gateway for local development. It deliberately returns Pending;
/// only a verified provider webhook is allowed to move a transaction to success.
/// </summary>
public sealed class SandboxPaymentGateway : IPaymentGateway
{
    public string Provider => "sandbox";

    public Task<PaymentGatewayInitiation> InitiateAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PaymentGatewayInitiation(
            Provider,
            $"sandbox-{request.TransactionId:N}",
            null));
    }
}

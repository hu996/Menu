using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IPaymentGateway
{
    string Provider { get; }

    Task<PaymentGatewayInitiation> InitiateAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken = default);
}

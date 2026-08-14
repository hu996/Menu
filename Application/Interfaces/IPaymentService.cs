using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentTransactionDto> InitiateAsync(
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ProcessWebhookAsync(
        PaymentWebhookRequest request,
        CancellationToken cancellationToken = default);
}

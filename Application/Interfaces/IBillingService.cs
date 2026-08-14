using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IBillingService
{
    Task<BillingOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<PaymentTransactionDto> InitiatePlanPaymentAsync(Guid planId, CancellationToken cancellationToken = default);
    Task<bool> CancelCurrentSubscriptionAsync(CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class BillingService : IBillingService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IPaymentService _paymentService;
    private readonly IEntitlementService _entitlementService;

    public BillingService(
        AppDbContext db,
        ITenantContext tenantContext,
        IPaymentService paymentService,
        IEntitlementService entitlementService)
    {
        _db = db;
        _tenantContext = tenantContext;
        _paymentService = paymentService;
        _entitlementService = entitlementService;
    }

    public async Task<BillingOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.TenantId.HasValue)
            throw new InvalidOperationException("Tenant context is required.");

        var subscriptions = await _db.Subscriptions
            .AsNoTracking()
            .Include(x => x.Plan)
                .ThenInclude(x => x.Features)
            .OrderByDescending(x => x.StartsAtUtc)
            .ToListAsync(cancellationToken);

        var plans = await _db.Plans
            .AsNoTracking()
            .Include(x => x.Features)
            .Where(x => x.IsActive)
            .OrderBy(x => x.MonthlyPrice)
            .Select(x => new PlanDto(
                x.Id,
                x.Name,
                x.MonthlyPrice,
                x.Currency,
                x.MaxBranches,
                x.MaxMenuItems,
                x.MaxUsers,
                x.AdvancedAnalytics,
                x.CustomBranding,
                x.IsActive,
                x.Features.OrderBy(f => f.FeatureKey)
                    .Select(f => new PlanFeatureDto(f.FeatureKey, f.Enabled, f.LimitValue))
                    .ToList()))
            .ToListAsync(cancellationToken);

        var payments = await _db.PaymentTransactions
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(100)
            .Select(x => new PaymentTransactionDto(
                x.Id,
                x.Amount,
                x.Currency,
                x.Provider,
                x.ProviderReference,
                x.Status,
                x.CreatedAtUtc,
                x.CompletedAtUtc,
                x.CheckoutUrl))
            .ToListAsync(cancellationToken);

        var subscriptionDtos = subscriptions.Select(ToDto).ToList();
        return new BillingOverviewDto(
            subscriptionDtos.FirstOrDefault(),
            plans,
            subscriptionDtos,
            payments,
            await _entitlementService.GetCurrentAsync(cancellationToken));
    }

    public async Task<PaymentTransactionDto> InitiatePlanPaymentAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.TenantId.HasValue)
            throw new InvalidOperationException("Tenant context is required.");

        var current = await _db.Subscriptions
            .OrderByDescending(x => x.StartsAtUtc)
            .FirstOrDefaultAsync(x => x.Status == Domain.Enums.SubscriptionStatus.Trial || x.Status == Domain.Enums.SubscriptionStatus.Active || x.Status == Domain.Enums.SubscriptionStatus.PastDue, cancellationToken);
        Guid? subscriptionId = current?.PlanId == planId ? current.Id : null;
        return await _paymentService.InitiateAsync(new PaymentInitiationRequest(planId, subscriptionId), cancellationToken);
    }

    public async Task<bool> CancelCurrentSubscriptionAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.TenantId.HasValue)
            throw new InvalidOperationException("Tenant context is required.");
        var subscription = await _db.Subscriptions
            .OrderByDescending(x => x.StartsAtUtc)
            .FirstOrDefaultAsync(x => x.Status == Domain.Enums.SubscriptionStatus.Trial || x.Status == Domain.Enums.SubscriptionStatus.Active || x.Status == Domain.Enums.SubscriptionStatus.PastDue, cancellationToken);
        if (subscription is null)
            return false;
        subscription.Status = Domain.Enums.SubscriptionStatus.Cancelled;
        subscription.EndsAtUtc = DateTime.UtcNow;
        subscription.UpdatedAtUtc = DateTime.UtcNow;
        var tenant = await _db.Tenants.SingleOrDefaultAsync(x => x.Id == _tenantContext.TenantId.Value, cancellationToken);
        if (tenant is not null)
        {
            tenant.SubscriptionStatus = Domain.Enums.SubscriptionStatus.Cancelled;
            tenant.UpdatedAtUtc = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static SubscriptionDto ToDto(Domain.Entities.Subscription subscription) => new(
        subscription.Id,
        subscription.PlanId,
        subscription.Plan.Name,
        subscription.Status,
        subscription.StartsAtUtc,
        subscription.EndsAtUtc,
        subscription.PaymentProvider,
        subscription.ExternalSubscriptionId);
}

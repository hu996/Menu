using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Exceptions;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Enums;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IPaymentGateway _gateway;
    private readonly IAuditLogService? _auditLogService;

    public PaymentService(
        AppDbContext db,
        ITenantContext tenantContext,
        IPaymentGateway gateway,
        IAuditLogService? auditLogService = null)
    {
        _db = db;
        _tenantContext = tenantContext;
        _gateway = gateway;
        _auditLogService = auditLogService;
    }

    public async Task<PaymentTransactionDto> InitiateAsync(
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = RequireTenant();
        var plan = await _db.Plans
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.PlanId && x.IsActive, cancellationToken);
        if (plan is null)
            throw new EntitlementViolationException("The selected plan is not available.", "plan");

        if (request.SubscriptionId.HasValue && !await _db.Subscriptions
                .AnyAsync(x => x.Id == request.SubscriptionId.Value, cancellationToken))
            throw new EntitlementViolationException("The selected subscription was not found.", "subscription");

        var reusable = await _db.PaymentTransactions
            .AsNoTracking()
            .Where(x => x.RequestedPlanId == request.PlanId &&
                        x.SubscriptionId == request.SubscriptionId &&
                        x.Status == PaymentStatus.Pending &&
                        x.CreatedAtUtc >= DateTime.UtcNow.AddMinutes(-15) &&
                        x.CheckoutUrl != null)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        if (reusable is not null)
            return ToDto(reusable);

        var transaction = new PaymentTransaction
        {
            TenantId = tenantId,
            SubscriptionId = request.SubscriptionId,
            RequestedPlanId = plan.Id,
            Amount = plan.MonthlyPrice,
            Currency = plan.Currency,
            Provider = _gateway.Provider,
            ProviderReference = $"pending-{Guid.NewGuid():N}",
            Status = PaymentStatus.Initiated
        };
        _db.PaymentTransactions.Add(transaction);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            var gatewayPayment = await _gateway.InitiateAsync(
                new PaymentGatewayRequest(transaction.Id, transaction.Amount, transaction.Currency),
                cancellationToken);
            transaction.Provider = gatewayPayment.Provider;
            transaction.ProviderReference = gatewayPayment.ProviderReference;
            transaction.CheckoutUrl = gatewayPayment.CheckoutUrl;
            transaction.Status = PaymentStatus.Pending;
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The provider may have accepted the request even if the caller disconnected.
            // Keep the transaction initiated so a reconciliation/webhook can settle it.
            throw;
        }
        catch
        {
            transaction.Status = PaymentStatus.Failed;
            transaction.CompletedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            throw;
        }

        return ToDto(transaction);
    }

    public async Task<bool> ProcessWebhookAsync(
        PaymentWebhookRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Provider) ||
            string.IsNullOrWhiteSpace(request.ProviderReference) ||
            request.Status is PaymentStatus.Initiated ||
            !Enum.IsDefined(request.Status))
            return false;

        var transaction = await _db.PaymentTransactions
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                x => x.Provider == request.Provider && x.ProviderReference == request.ProviderReference,
                cancellationToken);
        if (transaction is null)
            return false;

        // A provider may replay a webhook. The same final state is a no-op;
        // contradictory terminal states are rejected instead of being replayed.
        if (transaction.Status == request.Status || transaction.Status == PaymentStatus.Refunded)
            return true;
        if (transaction.Status == PaymentStatus.Successful && request.Status != PaymentStatus.Refunded)
            return true;
        if (transaction.Status is PaymentStatus.Failed or PaymentStatus.Cancelled &&
            request.Status is PaymentStatus.Successful or PaymentStatus.Refunded)
            return false;
        if (request.Status == PaymentStatus.Refunded && transaction.Status != PaymentStatus.Successful)
            return false;

        var oldStatus = transaction.Status;
        transaction.Status = request.Status;
        if (request.Status is PaymentStatus.Successful or PaymentStatus.Failed or PaymentStatus.Cancelled or PaymentStatus.Refunded)
            transaction.CompletedAtUtc = DateTime.UtcNow;

        _tenantContext.SetTenant(transaction.TenantId);
        var subscriptionChanges = new List<SubscriptionStatusChange>();
        if (request.Status == PaymentStatus.Successful)
            subscriptionChanges = await ActivateSubscriptionAsync(transaction, cancellationToken);
        else if (transaction.SubscriptionId.HasValue && request.Status is PaymentStatus.Failed or PaymentStatus.Cancelled)
            subscriptionChanges = await MarkSubscriptionAfterFailedPaymentAsync(transaction, request.Status, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            "payment.status-changed",
            "PaymentTransaction",
            transaction.Id,
            new { Status = oldStatus },
            new { Status = transaction.Status, transaction.ProviderReference },
            cancellationToken);
        foreach (var change in subscriptionChanges)
        {
            await WriteAuditAsync(
                "subscription.status-changed",
                "Subscription",
                change.SubscriptionId,
                change.OldStatus is null ? null : new { Status = change.OldStatus },
                new { Status = change.NewStatus },
                cancellationToken);
        }
        return true;
    }

    private async Task<List<SubscriptionStatusChange>> ActivateSubscriptionAsync(
        PaymentTransaction transaction,
        CancellationToken cancellationToken)
    {
        var changes = new List<SubscriptionStatusChange>();
        Subscription? subscription = null;
        if (transaction.SubscriptionId.HasValue)
        {
            subscription = await _db.Subscriptions
                .SingleOrDefaultAsync(x => x.Id == transaction.SubscriptionId.Value, cancellationToken);
        }
        else if (transaction.RequestedPlanId.HasValue)
        {
            var planExists = await _db.Plans.AnyAsync(x => x.Id == transaction.RequestedPlanId.Value && x.IsActive, cancellationToken);
            if (!planExists)
                throw new InvalidOperationException("The paid plan is no longer available.");

            var existing = await _db.Subscriptions
                .Where(x => x.Status == SubscriptionStatus.Trial || x.Status == SubscriptionStatus.Active || x.Status == SubscriptionStatus.PastDue)
                .ToListAsync(cancellationToken);
            foreach (var prior in existing)
            {
                changes.Add(new SubscriptionStatusChange(prior.Id, prior.Status, SubscriptionStatus.Cancelled));
                prior.Status = SubscriptionStatus.Cancelled;
            }

            subscription = new Subscription
            {
                TenantId = transaction.TenantId,
                PlanId = transaction.RequestedPlanId.Value,
                Status = SubscriptionStatus.Active,
                StartsAtUtc = DateTime.UtcNow
            };
            _db.Subscriptions.Add(subscription);
            transaction.Subscription = subscription;
            transaction.SubscriptionId = subscription.Id;
            changes.Add(new SubscriptionStatusChange(subscription.Id, null, SubscriptionStatus.Active));
        }

        if (subscription is not null)
        {
            if (subscription.Status != SubscriptionStatus.Active)
                changes.Add(new SubscriptionStatusChange(subscription.Id, subscription.Status, SubscriptionStatus.Active));
            subscription.Status = SubscriptionStatus.Active;
            subscription.EndsAtUtc = null;
            subscription.UpdatedAtUtc = DateTime.UtcNow;
        }

        var tenant = await _db.Tenants.SingleOrDefaultAsync(x => x.Id == transaction.TenantId, cancellationToken);
        if (tenant is not null)
        {
            tenant.SubscriptionStatus = SubscriptionStatus.Active;
            tenant.UpdatedAtUtc = DateTime.UtcNow;
        }

        return changes;
    }

    private async Task<List<SubscriptionStatusChange>> MarkSubscriptionAfterFailedPaymentAsync(
        PaymentTransaction transaction,
        PaymentStatus status,
        CancellationToken cancellationToken)
    {
        var changes = new List<SubscriptionStatusChange>();
        var subscription = await _db.Subscriptions
            .SingleOrDefaultAsync(x => x.Id == transaction.SubscriptionId!.Value, cancellationToken);
        if (subscription is null)
            return changes;
        var newStatus = status == PaymentStatus.Cancelled
            ? SubscriptionStatus.Cancelled
            : SubscriptionStatus.PastDue;
        if (subscription.Status != newStatus)
            changes.Add(new SubscriptionStatusChange(subscription.Id, subscription.Status, newStatus));
        subscription.Status = newStatus;
        subscription.UpdatedAtUtc = DateTime.UtcNow;
        var tenant = await _db.Tenants.SingleOrDefaultAsync(x => x.Id == transaction.TenantId, cancellationToken);
        if (tenant is not null)
        {
            tenant.SubscriptionStatus = subscription.Status;
            tenant.UpdatedAtUtc = DateTime.UtcNow;
        }

        return changes;
    }

    private Task WriteAuditAsync(
        string action,
        string entityType,
        Guid entityId,
        object? oldValue,
        object? newValue,
        CancellationToken cancellationToken) =>
        _auditLogService?.WriteAsync(action, entityType, entityId, oldValue, newValue, cancellationToken)
        ?? Task.CompletedTask;

    private Guid RequireTenant() => _tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context is required.");

    private sealed record SubscriptionStatusChange(
        Guid SubscriptionId,
        SubscriptionStatus? OldStatus,
        SubscriptionStatus NewStatus);

    private static PaymentTransactionDto ToDto(PaymentTransaction transaction) => new(
        transaction.Id,
        transaction.Amount,
        transaction.Currency,
        transaction.Provider,
        transaction.ProviderReference,
        transaction.Status,
        transaction.CreatedAtUtc,
        transaction.CompletedAtUtc,
        transaction.CheckoutUrl);
}

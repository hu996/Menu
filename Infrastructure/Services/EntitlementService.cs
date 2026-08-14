using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Exceptions;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Enums;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class EntitlementService : IEntitlementService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public EntitlementService(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<EntitlementDto?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        RequireTenant();

        var subscription = await _db.Subscriptions
            .AsNoTracking()
            .Include(x => x.Plan)
                .ThenInclude(x => x.Features)
            .OrderByDescending(x => x.StartsAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription is null)
            return null;

        var tenantId = _tenantContext.TenantId!.Value;
        var features = subscription.Plan.Features
            .GroupBy(x => x.FeatureKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Last().Enabled, StringComparer.OrdinalIgnoreCase);

        var branches = await _db.Branches.CountAsync(cancellationToken);
        var menuItems = await _db.MenuItems.CountAsync(cancellationToken);
        var users = await _db.Memberships
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var effectiveStatus = subscription.EndsAtUtc.HasValue && subscription.EndsAtUtc.Value <= DateTime.UtcNow &&
                              subscription.Status is SubscriptionStatus.Trial or SubscriptionStatus.Active
            ? SubscriptionStatus.Expired
            : subscription.Status;

        return new EntitlementDto(
            subscription.Plan.Name,
            effectiveStatus,
            subscription.StartsAtUtc,
            subscription.EndsAtUtc,
            branches,
            subscription.Plan.MaxBranches,
            menuItems,
            subscription.Plan.MaxMenuItems,
            users,
            subscription.Plan.MaxUsers,
            features);
    }

    public async Task<bool> HasFeatureAsync(
        string featureKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(featureKey))
            return false;

        var entitlement = await GetCurrentAsync(cancellationToken);
        return entitlement is { IsActive: true } && entitlement.HasFeature(featureKey.Trim());
    }

    public Task EnsureCanCreateBranchAsync(CancellationToken cancellationToken = default) =>
        EnsureWithinLimitAsync("branches", x => x.BranchesUsed, x => x.MaxBranches, cancellationToken);

    public Task EnsureCanCreateMenuItemAsync(CancellationToken cancellationToken = default) =>
        EnsureWithinLimitAsync("menu-items", x => x.MenuItemsUsed, x => x.MaxMenuItems, cancellationToken);

    public Task EnsureCanAddUserAsync(CancellationToken cancellationToken = default) =>
        EnsureWithinLimitAsync("users", x => x.UsersUsed, x => x.MaxUsers, cancellationToken);

    private async Task EnsureWithinLimitAsync(
        string entitlementKey,
        Func<EntitlementDto, int> usage,
        Func<EntitlementDto, int> limit,
        CancellationToken cancellationToken)
    {
        var entitlement = await GetCurrentAsync(cancellationToken);
        if (entitlement is null)
            throw new EntitlementViolationException(
                "This restaurant does not have an active subscription.",
                "subscription");

        if (!entitlement.IsActive)
            throw new EntitlementViolationException(
                "This subscription is not active. Update billing before adding more resources.",
                "subscription",
                usage: usage(entitlement));

        var currentUsage = usage(entitlement);
        var maxAllowed = limit(entitlement);
        if (maxAllowed > 0 && currentUsage >= maxAllowed)
            throw new EntitlementViolationException(
                $"Your {entitlement.PlanName} plan allows up to {maxAllowed} {entitlementKey}.",
                entitlementKey,
                maxAllowed,
                currentUsage);
    }

    private Guid RequireTenant() => _tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context is required.");
}

using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record PlanDto(
    Guid Id,
    string Name,
    decimal MonthlyPrice,
    string Currency,
    int MaxBranches,
    int MaxMenuItems,
    int MaxUsers,
    bool AdvancedAnalytics,
    bool CustomBranding,
    bool IsActive,
    IReadOnlyList<PlanFeatureDto> Features);

public sealed record PlanFeatureInput(string FeatureKey, bool Enabled, int? LimitValue);

public sealed record PlanManagementInput(
    string Name,
    decimal MonthlyPrice,
    string Currency,
    int MaxBranches,
    int MaxMenuItems,
    int MaxUsers,
    bool AdvancedAnalytics,
    bool CustomBranding,
    bool IsActive,
    IReadOnlyList<PlanFeatureInput> Features);

public sealed record PlanFeatureDto(string FeatureKey, bool Enabled, int? LimitValue);

public sealed record SubscriptionDto(
    Guid Id,
    Guid PlanId,
    string PlanName,
    SubscriptionStatus Status,
    DateTime StartsAtUtc,
    DateTime? EndsAtUtc,
    string? PaymentProvider,
    string? ExternalSubscriptionId);

public sealed record BillingOverviewDto(
    SubscriptionDto? CurrentSubscription,
    IReadOnlyList<PlanDto> Plans,
    IReadOnlyList<SubscriptionDto> SubscriptionHistory,
    IReadOnlyList<PaymentTransactionDto> Payments,
    EntitlementDto? CurrentEntitlement = null);

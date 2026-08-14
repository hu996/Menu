using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record PaymentInitiationRequest(
    Guid PlanId,
    Guid? SubscriptionId = null);

public sealed record PaymentGatewayRequest(
    Guid TransactionId,
    decimal Amount,
    string Currency);

public sealed record PaymentGatewayInitiation(
    string Provider,
    string ProviderReference,
    string? CheckoutUrl);

public sealed record PaymentWebhookRequest(
    string Provider,
    string ProviderReference,
    PaymentStatus Status);

public sealed record PaymentTransactionDto(
    Guid Id,
    decimal Amount,
    string Currency,
    string Provider,
    string ProviderReference,
    PaymentStatus Status,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);

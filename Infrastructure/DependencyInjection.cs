using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Identity;
using RestaurantMenuPlatform.Infrastructure.Persistence;
using RestaurantMenuPlatform.Infrastructure.Services;
using RestaurantMenuPlatform.Infrastructure.Storage;
using RestaurantMenuPlatform.Infrastructure.Payments;

namespace RestaurantMenuPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddSingleton<PasswordService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();
        services.AddScoped<IMembershipAuthorizationService, MembershipAuthorizationService>();
        services.AddScoped<IEntitlementService, EntitlementService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        var paymentProvider = configuration["Payments:Provider"]?.Trim();
        if (string.Equals(paymentProvider, "Sandbox", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IPaymentGateway, SandboxPaymentGateway>();
        else
            services.AddSingleton<IPaymentGateway, UnconfiguredPaymentGateway>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<IPlanManagementService, PlanManagementService>();
        services.AddScoped<IIngredientService, IngredientService>();
        services.AddScoped<IAllergenService, AllergenService>();
        services.AddScoped<IModifierService, ModifierService>();
        services.AddScoped<IRestaurantService, RestaurantService>();
        services.AddScoped<IPlatformRestaurantService, PlatformRestaurantService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IProductCatalogService, ProductCatalogService>();
        services.AddScoped<IBranchMenuService, BranchMenuService>();
        services.AddScoped<IPublicMenuService, PublicMenuService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<ITableService, TableService>();
        services.AddSingleton<HttpClient>();
        services.AddSingleton<S3CompatibleImageStorage>();
        services.AddSingleton<IImageStorage>(serviceProvider =>
        {
            var provider = configuration["Storage:Provider"]?.Trim();
            if (string.Equals(provider, "ObjectStorage", StringComparison.OrdinalIgnoreCase))
                return serviceProvider.GetRequiredService<S3CompatibleImageStorage>();

            var imageRoot = configuration["Storage:RootPath"];
            if (string.IsNullOrWhiteSpace(imageRoot))
                throw new InvalidOperationException("Storage:RootPath must be configured for local image storage.");

            return new LocalImageStorage(imageRoot, TryGetLong(configuration["Storage:MaxUploadBytes"]));
        });
        services.AddScoped<IImageManagementService, ImageManagementService>();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions
                    .CommandTimeout(TryGetInt(configuration["Database:CommandTimeoutSeconds"], 30))
                    .EnableRetryOnFailure(
                        maxRetryCount: TryGetInt(configuration["Database:MaxRetryCount"], 5),
                        maxRetryDelay: TimeSpan.FromSeconds(TryGetInt(configuration["Database:MaxRetryDelaySeconds"], 10)),
                        errorNumbersToAdd: null)));

        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }

    private static int TryGetInt(string? value, int fallback) =>
        int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;

    private static long? TryGetLong(string? value) =>
        long.TryParse(value, out var parsed) && parsed > 0 ? parsed : null;
}

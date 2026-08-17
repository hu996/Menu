using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Web.Services;

public sealed class DataRetentionService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DataRetentionService> _logger;

    public DataRetentionService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<DataRetentionService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_configuration.GetValue("Maintenance:Enabled", true))
            return;

        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        var interval = TimeSpan.FromHours(Math.Clamp(
            _configuration.GetValue("Maintenance:RunEveryHours", 24),
            1,
            168));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Scheduled data-retention cleanup failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        var resetRetentionDays = Math.Clamp(
            _configuration.GetValue("Maintenance:PasswordResetRetentionDays", 7),
            1,
            90);
        var analyticsRetentionDays = Math.Clamp(
            _configuration.GetValue("Maintenance:AnalyticsRetentionDays", 400),
            30,
            3650);
        var resetCutoff = DateTime.UtcNow.AddDays(-resetRetentionDays);
        var analyticsCutoff = DateTime.UtcNow.AddDays(-analyticsRetentionDays);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var resetTokens = await db.PasswordResetTokens
            .Where(x => x.ExpiresAtUtc < resetCutoff)
            .ExecuteDeleteAsync(cancellationToken);
        var analyticsEvents = await db.AnalyticsEvents
            .IgnoreQueryFilters()
            .Where(x => x.CreatedAtUtc < analyticsCutoff)
            .ExecuteDeleteAsync(cancellationToken);

        if (resetTokens > 0 || analyticsEvents > 0)
        {
            _logger.LogInformation(
                "Data-retention cleanup removed {ResetTokens} password-reset tokens and {AnalyticsEvents} analytics events.",
                resetTokens,
                analyticsEvents);
        }
    }
}

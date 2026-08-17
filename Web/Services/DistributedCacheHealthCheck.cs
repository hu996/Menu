using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RestaurantMenuPlatform.Web.Services;

public sealed class DistributedCacheHealthCheck : IHealthCheck
{
    private readonly IDistributedCache _cache;

    public DistributedCacheHealthCheck(IDistributedCache cache) => _cache = cache;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var key = $"health:{Guid.NewGuid():N}";
        try
        {
            var expected = Guid.NewGuid().ToByteArray();
            await _cache.SetAsync(
                key,
                expected,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) },
                cancellationToken);
            var actual = await _cache.GetAsync(key, cancellationToken);
            await _cache.RemoveAsync(key, cancellationToken);
            return actual is not null && actual.AsSpan().SequenceEqual(expected)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(exception: exception);
        }
    }
}

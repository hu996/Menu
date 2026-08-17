using System.Diagnostics;
using System.Security.Cryptography;

namespace RestaurantMenuPlatform.Web.Extensions;

public sealed class SecurityHeadersMiddleware
{
    public const string CspNonceItemKey = "CspNonce";
    private readonly RequestDelegate _next;
    private readonly bool _requireHttps;
    private readonly long _slowRequestMilliseconds;

    public SecurityHeadersMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _requireHttps = configuration.GetValue("Security:RequireHttps", false);
        _slowRequestMilliseconds = Math.Clamp(
            configuration.GetValue("Performance:SlowRequestMilliseconds", 2000),
            250,
            60_000);
    }

    public async Task InvokeAsync(HttpContext context, ILogger<SecurityHeadersMiddleware> logger)
    {
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(18));
        context.Items[CspNonceItemKey] = nonce;
        var requestId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers.TryAdd("X-Content-Type-Options", "nosniff");
            headers.TryAdd("X-Frame-Options", "DENY");
            headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
            headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=(), usb=()");
            headers.TryAdd("Cross-Origin-Opener-Policy", "same-origin");
            headers.TryAdd("X-Permitted-Cross-Domain-Policies", "none");
            headers.TryAdd("X-Request-ID", requestId);

            var upgrade = _requireHttps ? "; upgrade-insecure-requests" : string.Empty;
            headers.TryAdd(
                "Content-Security-Policy",
                $"default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'none'; " +
                $"form-action 'self'; script-src 'self' 'nonce-{nonce}'; style-src 'self' 'unsafe-inline'; " +
                $"img-src 'self' data: blob:; font-src 'self'; connect-src 'self'{upgrade}");

            if ((context.User.Identity?.IsAuthenticated == true ||
                 context.Request.Path.StartsWithSegments("/Account")) &&
                !headers.ContainsKey("Cache-Control"))
            {
                headers.CacheControl = "no-store, no-cache, must-revalidate";
                headers.Pragma = "no-cache";
            }

            return Task.CompletedTask;
        });

        var started = Stopwatch.GetTimestamp();
        using (logger.BeginScope(new Dictionary<string, object> { ["RequestId"] = requestId }))
        {
            try
            {
                await _next(context);
            }
            finally
            {
                var elapsed = Stopwatch.GetElapsedTime(started);
                if (elapsed.TotalMilliseconds >= _slowRequestMilliseconds)
                {
                    logger.LogWarning(
                        "Slow HTTP request {Method} {Path} returned {StatusCode} in {ElapsedMilliseconds} ms.",
                        context.Request.Method,
                        context.Request.Path.Value,
                        context.Response.StatusCode,
                        Math.Round(elapsed.TotalMilliseconds, 1));
                }
            }
        }
    }
}

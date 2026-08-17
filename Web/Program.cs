using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Globalization;
using System.IO.Compression;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using RestaurantMenuPlatform.Application;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure;
using RestaurantMenuPlatform.Infrastructure.Persistence;
using RestaurantMenuPlatform.Web.Extensions;
using RestaurantMenuPlatform.Web.Services;

var builder = WebApplication.CreateBuilder(args);
var initializeReferenceDataOnly = args.Any(x =>
    string.Equals(x, "--initialize-reference-data", StringComparison.OrdinalIgnoreCase));

if (!string.Equals(Environment.GetEnvironmentVariable("EF_DESIGN_TIME"), "true", StringComparison.OrdinalIgnoreCase))
    ProductionConfigurationValidator.Validate(builder.Configuration, builder.Environment);
var requireHttps = builder.Configuration.GetValue(
    "Security:RequireHttps",
    !builder.Environment.IsDevelopment());

builder.Logging.ClearProviders();
builder.Logging.Configure(options =>
    options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId |
                                      ActivityTrackingOptions.SpanId |
                                      ActivityTrackingOptions.ParentId);
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss ";
    });
}
else
{
    builder.Logging.AddJsonConsole(options =>
    {
        options.IncludeScopes = true;
        options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        options.UseUtcTimestamp = true;
    });
}

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.MaxRequestBodySize = builder.Configuration.GetValue<long?>("Server:MaxRequestBodyBytes") ?? 26_214_400;
    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(builder.Configuration.GetValue("Server:KeepAliveSeconds", 120));
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(builder.Configuration.GetValue("Server:RequestHeadersTimeoutSeconds", 30));
});

var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName("RestaurantMenuPlatform");
var dataProtectionPath = builder.Configuration["Security:DataProtectionKeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionPath))
{
    var absoluteDataProtectionPath = Path.GetFullPath(dataProtectionPath);
    Directory.CreateDirectory(absoluteDataProtectionPath);
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(absoluteDataProtectionPath));
}

var sessionProvider = builder.Configuration["Session:Provider"]?.Trim();
if (string.Equals(sessionProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDistributedSqlServerCache(options =>
    {
        options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
        options.SchemaName = "dbo";
        options.TableName = "DistributedCache";
        options.DefaultSlidingExpiration = TimeSpan.FromMinutes(builder.Configuration.GetValue("Session:IdleMinutes", 30));
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = builder.Configuration.GetValue<long?>("Server:MaxRequestBodyBytes") ?? 26_214_400;
    options.ValueCountLimit = builder.Configuration.GetValue("Server:FormValueCountLimit", 2048);
});
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            PartitionKey(context),
            _ => FixedWindow(600, TimeSpan.FromMinutes(1))));
    options.AddPolicy("authentication", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            $"auth:{PartitionKey(context)}",
            _ => FixedWindow(30, TimeSpan.FromMinutes(15))));
    options.AddPolicy("public-ordering", context =>
        RateLimitPartition.GetTokenBucketLimiter(
            $"ordering:{PartitionKey(context)}",
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 80,
                TokensPerPeriod = 40,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                AutoReplenishment = true,
                QueueLimit = 0
            }));
    options.AddPolicy("payment-webhook", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            $"webhook:{PartitionKey(context)}",
            _ => FixedWindow(240, TimeSpan.FromMinutes(1))));
    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { error = "Too many requests. Please try again shortly." },
            cancellationToken);
    };
});
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ar") };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = new IRequestCultureProvider[]
    {
        new QueryStringRequestCultureProvider(),
        new LanguageQueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider()
    };
});
builder.Services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue("Session:IdleMinutes", 30));
    options.Cookie.Name = requireHttps ? "__Host-RMP.Session" : "RMP.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = requireHttps
        ? CookieSecurePolicy.Always
        : CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = requireHttps ? "__Host-RMP.Antiforgery" : "RMP.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = requireHttps
        ? CookieSecurePolicy.Always
        : CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" })
    .AddCheck<DistributedCacheHealthCheck>("distributed-cache", tags: new[] { "ready" });
builder.Services.AddHostedService<DataRetentionService>();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    foreach (var value in builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [])
    {
        if (System.Net.IPAddress.TryParse(value, out var address))
            options.KnownProxies.Add(address);
    }
});

builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    })
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = requireHttps ? "__Host-RMP.Auth" : "RMP.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = requireHttps
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Events.OnValidatePrincipal = async validationContext =>
        {
            if (!Guid.TryParse(
                    validationContext.Principal?.FindFirstValue(ClaimTypes.NameIdentifier),
                    out var userId))
            {
                validationContext.RejectPrincipal();
                await validationContext.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            if (!Guid.TryParse(
                    validationContext.Principal?.FindFirstValue("tenant_id"),
                    out var tenantId))
            {
                validationContext.RejectPrincipal();
                await validationContext.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var services = validationContext.HttpContext.RequestServices;
            var cache = services.GetRequiredService<IMemoryCache>();
            var cacheSeconds = Math.Clamp(
                builder.Configuration.GetValue("Security:PrincipalValidationCacheSeconds", 15),
                5,
                60);
            var sessionState = await cache.GetOrCreateAsync(
                $"principal-session:{userId:N}:{tenantId:N}",
                async cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheSeconds);
                    cacheEntry.Size = 1;
                    var db = services.GetRequiredService<AppDbContext>();
                    return await (
                            from membership in db.Memberships.IgnoreQueryFilters().AsNoTracking()
                            join user in db.Users.AsNoTracking() on membership.UserId equals user.Id
                            join tenant in db.Tenants.IgnoreQueryFilters().AsNoTracking() on membership.TenantId equals tenant.Id
                            where membership.UserId == userId &&
                                  membership.TenantId == tenantId &&
                                  membership.IsActive &&
                                  user.IsActive &&
                                  tenant.IsActive
                            select new PrincipalSessionState(
                                user.SecurityStamp,
                                membership.Role.ToString(),
                                membership.BranchId,
                                tenant.Slug))
                        .SingleOrDefaultAsync(validationContext.HttpContext.RequestAborted);
                });
            var stamp = validationContext.Principal?.FindFirstValue("security_stamp");
            var permissionsLoaded = validationContext.Principal?.FindFirstValue("permissions_loaded");
            var claimRole = validationContext.Principal?.FindFirstValue(ClaimTypes.Role);
            var claimBranch = validationContext.Principal?.FindFirstValue("branch_id");
            var claimTenantSlug = validationContext.Principal?.FindFirstValue("tenant_slug");
            var branchMatches = sessionState?.BranchId is null
                ? string.IsNullOrWhiteSpace(claimBranch)
                : Guid.TryParse(claimBranch, out var branchId) && branchId == sessionState.BranchId.Value;
            if (sessionState is null ||
                !string.Equals(permissionsLoaded, "1", StringComparison.Ordinal) ||
                !string.Equals(sessionState.SecurityStamp, stamp, StringComparison.Ordinal) ||
                !string.Equals(claimRole, sessionState.Role, StringComparison.Ordinal) ||
                !string.Equals(claimTenantSlug, sessionState.TenantSlug, StringComparison.OrdinalIgnoreCase) ||
                !branchMatches)
            {
                validationContext.RejectPrincipal();
                await validationContext.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
        options.Events.OnRedirectToLogin = redirectContext =>
        {
            var tenantSlug = redirectContext.Request.RouteValues["tenantSlug"]?.ToString();
            var loginPath = string.IsNullOrWhiteSpace(tenantSlug)
                ? options.LoginPath.ToString()
                : $"/r/{tenantSlug}/Account/Login";
            var returnUrl = redirectContext.Request.PathBase + redirectContext.Request.Path + redirectContext.Request.QueryString;
            redirectContext.Response.Redirect($"{loginPath}?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = redirectContext =>
        {
            var tenantSlug = redirectContext.Request.RouteValues["tenantSlug"]?.ToString();
            var deniedPath = string.IsNullOrWhiteSpace(tenantSlug)
                ? options.AccessDeniedPath.ToString()
                : $"/r/{tenantSlug}/Account/AccessDenied";
            redirectContext.Response.Redirect(deniedPath);
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TenantAdmin", policy =>
        policy.RequireRole("PlatformAdmin", "TenantOwner", "TenantAdmin")
            .RequireAssertion(context => !context.User.Claims.Any(x => x.Type == "branch_id")));
    options.AddPolicy("BranchMenuEditor", policy =>
        policy.RequireRole("PlatformAdmin", "TenantOwner", "TenantAdmin", "MenuEditor", "BranchManager"));
    options.AddPolicy("MenuEditor", policy =>
        policy.RequireRole("PlatformAdmin", "TenantOwner", "TenantAdmin", "MenuEditor")
            .RequireAssertion(context => !context.User.Claims.Any(x => x.Type == "branch_id")));
    foreach (var permission in PermissionCatalog.AllCodes)
    {
        options.AddPolicy(permission, policy => policy.RequireClaim("permission", permission));
    }
});
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/StatusCodePage", "?code={0}");

if (!app.Environment.IsDevelopment() &&
    app.Configuration.GetValue("Security:RequireHttps", true))
{
    app.UseHsts();
}

if (app.Configuration.GetValue("Security:RequireHttps", !app.Environment.IsDevelopment()))
    app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.CacheControl = context.Context.Request.Query.ContainsKey("v")
            ? "public,max-age=31536000,immutable"
            : "public,max-age=3600";
    }
});
app.UseRequestLocalization();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseRateLimiter();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();
app.UseMiddleware<LocalizationResponseMiddleware>();

app.MapControllers();

app.MapControllerRoute(
    name: "public-menu",
    pattern: "menu/{restaurantSlug}/{branchSlug}",
    defaults: new { controller = "PublicMenu", action = "Index" });

app.MapControllerRoute(
    name: "media",
    pattern: "media/{mediaTenantId:guid}/menu-items/{fileName}",
    defaults: new { controller = "Media", action = "MenuItem" });

app.MapControllerRoute(
    name: "branding-media",
    pattern: "media/{mediaTenantId:guid}/branding/{fileName}",
    defaults: new { controller = "Media", action = "Branding" });

app.MapControllerRoute(
    name: "public-media",
    pattern: "media/{restaurantSlug}/menu-items/{fileName}",
    defaults: new { controller = "Media", action = "PublicMenuItem" });

app.MapControllerRoute(
    name: "public-branding-media",
    pattern: "media/{restaurantSlug}/branding/{fileName}",
    defaults: new { controller = "Media", action = "PublicBranding" });

app.MapControllerRoute(
    name: "tenant",
    pattern: "r/{tenantSlug}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = WriteHealthResponse
}).DisableRateLimiting();
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponse
}).DisableRateLimiting();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
    if (app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", false))
    {
        await db.Database.MigrateAsync();
    }
    else
    {
        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
            throw new InvalidOperationException("The database has pending EF Core migrations. Apply them during deployment before starting the application.");
    }

    var developmentLoginDefaults = app.Environment.IsDevelopment()
        ? new DevelopmentLoginSeed(
            builder.Configuration["DevelopmentLoginDefaults:Email"],
            builder.Configuration["DevelopmentLoginDefaults:Password"],
            builder.Configuration["DevelopmentLoginDefaults:TenantSlug"])
        : null;
    var developmentPlatformAdmin = app.Environment.IsDevelopment()
        ? new DevelopmentPlatformAdminSeed(
            builder.Configuration["DevelopmentPlatformAdmin:Email"],
            builder.Configuration["DevelopmentPlatformAdmin:Password"])
        : null;
    if (app.Environment.IsDevelopment() ||
        initializeReferenceDataOnly ||
        app.Configuration.GetValue("Database:InitializeReferenceDataOnStartup", false))
    {
        await DbInitializer.InitializeAsync(
            db,
            tenantContext,
            developmentLoginDefaults,
            developmentPlatformAdmin,
            allowDevelopmentSeed: app.Environment.IsDevelopment());
    }
}

if (initializeReferenceDataOnly)
    return;

static Task WriteHealthResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";
    var status = report.Status == HealthStatus.Healthy ? "Healthy" : "Unhealthy";
    return context.Response.WriteAsJsonAsync(new { status });
}

app.Run();

static string PartitionKey(HttpContext context) =>
    context.User.FindFirstValue(ClaimTypes.NameIdentifier)
    ?? context.Connection.RemoteIpAddress?.ToString()
    ?? "unknown";

static FixedWindowRateLimiterOptions FixedWindow(int permitLimit, TimeSpan window) => new()
{
    PermitLimit = permitLimit,
    Window = window,
    QueueLimit = 0,
    AutoReplenishment = true
};

internal sealed record PrincipalSessionState(
    string SecurityStamp,
    string Role,
    Guid? BranchId,
    string TenantSlug);

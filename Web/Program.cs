using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json.Serialization;
using RestaurantMenuPlatform.Application;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure;
using RestaurantMenuPlatform.Infrastructure.Persistence;
using RestaurantMenuPlatform.Web.Extensions;
using RestaurantMenuPlatform.Web.Services;

var builder = WebApplication.CreateBuilder(args);

if (!string.Equals(Environment.GetEnvironmentVariable("EF_DESIGN_TIME"), "true", StringComparison.OrdinalIgnoreCase))
    ProductionConfigurationValidator.Validate(builder.Configuration, builder.Environment);
var requireHttps = builder.Configuration.GetValue(
    "Security:RequireHttps",
    !builder.Environment.IsDevelopment());

builder.Services.AddHttpContextAccessor();
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
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = requireHttps
        ? CookieSecurePolicy.Always
        : CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = requireHttps
        ? CookieSecurePolicy.Always
        : CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" });
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    foreach (var value in builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [])
    {
        if (System.Net.IPAddress.TryParse(value, out var address))
            options.KnownProxies.Add(address);
    }
});

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "RestaurantMenuPlatform.Auth";
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
                return;
            }

            var db = validationContext.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId);
            if (!Guid.TryParse(
                    validationContext.Principal?.FindFirstValue("tenant_id"),
                    out var tenantId))
            {
                validationContext.RejectPrincipal();
                return;
            }

            var membership = await db.Memberships
                .IgnoreQueryFilters()
                .AsNoTracking()
                .SingleOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.TenantId == tenantId &&
                    x.IsActive);
            var tenant = await db.Tenants
                .IgnoreQueryFilters()
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == tenantId && x.IsActive);
            var stamp = validationContext.Principal?.FindFirstValue("security_stamp");
            var permissionsLoaded = validationContext.Principal?.FindFirstValue("permissions_loaded");
            var claimRole = validationContext.Principal?.FindFirstValue(ClaimTypes.Role);
            var claimBranch = validationContext.Principal?.FindFirstValue("branch_id");
            var branchMatches = membership?.BranchId is null
                ? string.IsNullOrWhiteSpace(claimBranch)
                : Guid.TryParse(claimBranch, out var branchId) && branchId == membership.BranchId.Value;
            if (user is null || !user.IsActive || tenant is null || membership is null ||
                !string.Equals(permissionsLoaded, "1", StringComparison.Ordinal) ||
                !string.Equals(user.SecurityStamp, stamp, StringComparison.Ordinal) ||
                !string.Equals(claimRole, membership.Role.ToString(), StringComparison.Ordinal) ||
                !branchMatches)
                validationContext.RejectPrincipal();
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


app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/StatusCodePage", "?code={0}");
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment() &&
    app.Configuration.GetValue("Security:RequireHttps", true))
{
    app.UseHsts();
}

if (app.Configuration.GetValue("Security:RequireHttps", !app.Environment.IsDevelopment()))
    app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
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
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponse
});

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
    await DbInitializer.InitializeAsync(
        db,
        tenantContext,
        developmentLoginDefaults,
        developmentPlatformAdmin,
        allowDevelopmentSeed: app.Environment.IsDevelopment());
}

static Task WriteHealthResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";
    var status = report.Status == HealthStatus.Healthy ? "Healthy" : "Unhealthy";
    return context.Response.WriteAsJsonAsync(new { status });
}

app.Run();

using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Identity;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Web.Extensions;

public sealed class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        AppDbContext db)
    {
        var requestedSlug = context.Request.RouteValues["tenantSlug"]?.ToString();
        var publicRestaurantSlug = context.Request.RouteValues["restaurantSlug"]?.ToString()
            ?? context.Request.RouteValues["mediaRestaurantSlug"]?.ToString();
        var mediaTenantIdValue = context.Request.RouteValues["mediaTenantId"]?.ToString();

        if (!string.IsNullOrWhiteSpace(mediaTenantIdValue))
        {
            if (!Guid.TryParse(mediaTenantIdValue, out var mediaTenantId))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            if (context.User.Identity?.IsAuthenticated == true)
            {
                if (!Guid.TryParse(context.User.FindFirstValue("tenant_id"), out var authenticatedTenantId) ||
                    authenticatedTenantId != mediaTenantId)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                tenantContext.SetTenant(mediaTenantId);
            }
            else
            {
                if (!await db.Tenants.IgnoreQueryFilters().AnyAsync(x => x.Id == mediaTenantId && x.IsActive))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
                tenantContext.SetPublicTenant(mediaTenantId);
            }

            await _next(context);
            return;
        }

        if (!string.IsNullOrWhiteSpace(publicRestaurantSlug))
        {
            var publicTenant = await db.Tenants
                .IgnoreQueryFilters()
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Slug == publicRestaurantSlug && x.IsActive);

            if (publicTenant is null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            tenantContext.SetPublicTenant(publicTenant.Id);
            await _next(context);
            return;
        }

        // Tenant-scoped login/access pages are anonymous before authentication.
        // Resolve the slug from SQL so the subsequent credential check can prove
        // membership in this tenant; never use it as an authenticated identity.
        if (context.User.Identity?.IsAuthenticated != true &&
            !string.IsNullOrWhiteSpace(requestedSlug))
        {
            var requestedTenant = await db.Tenants
                .IgnoreQueryFilters()
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Slug == requestedSlug && x.IsActive);

            if (requestedTenant is null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            tenantContext.SetTenant(requestedTenant.Id);
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirstValue("tenant_id");
            if (!Guid.TryParse(tenantIdClaim, out var authenticatedTenantId))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            if (!string.IsNullOrWhiteSpace(requestedSlug))
            {
                var authenticatedSlug = context.User.FindFirstValue("tenant_slug");
                if (!string.Equals(requestedSlug, authenticatedSlug, StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
            }

            tenantContext.SetTenant(authenticatedTenantId);
            await _next(context);
            return;
        }

        await _next(context);
    }
}

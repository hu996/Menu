using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Interfaces;

namespace RestaurantMenuPlatform.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly ITenantContext _tenantContext;

    public DashboardController(
        IDashboardService dashboardService,
        ITenantContext tenantContext)
    {
        _dashboardService = dashboardService;
        _tenantContext = tenantContext;
    }

    [Authorize(Policy = "Restaurant.View")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!_tenantContext.HasTenant)
            return NotFound();

        var model = await _dashboardService.GetAsync(cancellationToken);
        return View(model);
    }
}

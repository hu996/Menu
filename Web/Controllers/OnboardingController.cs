using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantMenuPlatform.Web.Controllers;

/// <summary>
/// Compatibility entry point for the former public provisioning route.
/// Restaurant creation now belongs to the authenticated admin workspace.
/// </summary>
[Authorize(Policy = "TenantAdmin")]
public sealed class OnboardingController : Controller
{
    [HttpGet]
    public IActionResult Register() => User.IsInRole("PlatformAdmin")
        ? RedirectToAction("Index", "PlatformRestaurants")
        : RedirectToAction("Create", "Restaurant");

}

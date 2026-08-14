using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "TenantAdmin")]
public sealed class AdministrationController : Controller
{
    public IActionResult Index() => View();
}

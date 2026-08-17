using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Error() =>
        View(new ErrorViewModel(
            500,
            "Something went wrong",
            "We could not complete that request. Please try again.",
            HttpContext.TraceIdentifier));

    public IActionResult StatusCodePage(int code)
    {
        var model = code switch
        {
            401 => new ErrorViewModel(code, "Sign in required", "Please sign in to continue.", HttpContext.TraceIdentifier),
            403 => new ErrorViewModel(code, "Access denied", "You do not have permission to view this page.", HttpContext.TraceIdentifier),
            404 => new ErrorViewModel(code, "Page not found", "That page or resource is not available.", HttpContext.TraceIdentifier),
            _ => new ErrorViewModel(code, "Request could not be completed", "Please check the address and try again.", HttpContext.TraceIdentifier)
        };
        Response.StatusCode = code;
        var originalPath = HttpContext.Features.Get<IStatusCodeReExecuteFeature>()?.OriginalPath;
        var isPublicMenuError = Request.Path.Value?.StartsWith("/menu/", StringComparison.OrdinalIgnoreCase) == true ||
                                originalPath?.StartsWith("/menu/", StringComparison.OrdinalIgnoreCase) == true;
        if (isPublicMenuError)
            return View("PublicStatusCode", model);
        return View("StatusCode", model);
    }
}

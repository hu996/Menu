using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantMenuPlatform.Web.Controllers;

public sealed class LanguageController : Controller
{
    [HttpGet]
    public IActionResult Set(string culture = "en", string? returnUrl = null)
    {
        culture = string.Equals(culture, "ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                SameSite = SameSiteMode.Lax,
                HttpOnly = false
            });

        return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : Redirect("/");
    }
}

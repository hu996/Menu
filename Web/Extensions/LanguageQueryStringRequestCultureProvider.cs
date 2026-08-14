using Microsoft.AspNetCore.Localization;

namespace RestaurantMenuPlatform.Web.Extensions;

public sealed class LanguageQueryStringRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var language = httpContext.Request.Query["lang"].ToString();
        if (!string.Equals(language, "ar", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(language, "en", StringComparison.OrdinalIgnoreCase))
            return NullProviderCultureResult;

        return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(language, language));
    }
}

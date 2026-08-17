using System.Text;
using RestaurantMenuPlatform.Web.Services;

namespace RestaurantMenuPlatform.Web.Extensions;

public sealed class LocalizationResponseMiddleware
{
    private readonly RequestDelegate _next;

    public LocalizationResponseMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Public ordering pages localize their copy and business data explicitly.
        // English responses need no translation either. Avoid buffering these hot
        // paths so the server can stream the response directly to the client.
        if (!UiText.IsArabic || context.Request.Path.StartsWithSegments("/menu"))
        {
            await _next(context);
            return;
        }

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);
            if (context.Response.ContentType?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) == true)
            {
                buffer.Position = 0;
                using var reader = new StreamReader(buffer, Encoding.UTF8, leaveOpen: true);
                var html = await reader.ReadToEndAsync(context.RequestAborted);
                var translated = UiText.TranslateHtml(html);
                var bytes = Encoding.UTF8.GetBytes(translated);
                context.Response.ContentLength = bytes.Length;
                await originalBody.WriteAsync(bytes, context.RequestAborted);
            }
            else
            {
                buffer.Position = 0;
                await buffer.CopyToAsync(originalBody, context.RequestAborted);
            }
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }
}

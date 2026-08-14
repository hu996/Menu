using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

public sealed class PublicMenuController : Controller
{
    private readonly IPublicMenuService _publicMenuService;
    private readonly IOrderService _orderService;
    private readonly IAnalyticsService _analyticsService;
    private readonly IQrCodeService _qrCodeService;

    public PublicMenuController(
        IPublicMenuService publicMenuService,
        IOrderService orderService,
        IAnalyticsService analyticsService,
        IQrCodeService qrCodeService)
    {
        _publicMenuService = publicMenuService;
        _orderService = orderService;
        _analyticsService = analyticsService;
        _qrCodeService = qrCodeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string restaurantSlug,
        string branchSlug,
        CancellationToken cancellationToken)
    {
        var menu = await _publicMenuService.GetAsync(
            restaurantSlug,
            branchSlug,
            Request.Query["lang"].ToString(),
            cancellationToken);
        if (menu is null)
            return NotFound();

        var analyticsContext = await _publicMenuService.GetAnalyticsContextAsync(restaurantSlug, branchSlug, cancellationToken);
        if (analyticsContext is null)
            return NotFound();

        PublicOrderingContextDto? publicContext = null;
        var sourceIsQr = string.Equals(Request.Query["source"], "qr", StringComparison.OrdinalIgnoreCase);
        var code = Request.Query["code"].ToString();
        if (sourceIsQr)
        {
            publicContext = await _qrCodeService.ResolvePublicContextAsync(restaurantSlug, branchSlug, code, cancellationToken);
            if (publicContext is null)
                return NotFound();
        }
        else
        {
            var stateCode = PublicCartSession.Read(HttpContext.Session).QrCodeCode;
            if (!string.IsNullOrWhiteSpace(stateCode))
                publicContext = await _qrCodeService.ResolvePublicContextAsync(restaurantSlug, branchSlug, stateCode, cancellationToken);
        }

        if (publicContext is not null)
        {
            var state = PublicCartSession.Read(HttpContext.Session);
            if (state.BranchId != Guid.Empty && (state.BranchId != publicContext.BranchId || state.TableId != publicContext.TableId))
                state = new PublicCartState();
            state.BranchId = publicContext.BranchId;
            state.TableId = publicContext.TableId;
            state.QrCodeId = publicContext.QrCodeId;
            state.QrCodeCode = publicContext.QrCodeCode;
            state.TableName = publicContext.TableName;
            state.TableNameAr = publicContext.TableNameAr;
            PublicCartSession.Write(HttpContext.Session, state);
            menu = menu with { TableName = publicContext.TableName, TableNameAr = publicContext.TableNameAr, QrCodeCode = publicContext.QrCodeCode };
        }

        var userAgent = Request.Headers.UserAgent.ToString();
        foreach (var trackedMenu in analyticsContext.Menus)
        {
            await _analyticsService.TrackMenuViewAsync(analyticsContext.BranchId, trackedMenu.MenuId, userAgent, cancellationToken);
            await _analyticsService.TrackMenuItemViewsAsync(analyticsContext.BranchId, trackedMenu.MenuId, trackedMenu.MenuItemIds, userAgent, cancellationToken);
        }
        if (sourceIsQr)
            await _analyticsService.TrackQrScanAsync(analyticsContext.BranchId, userAgent, cancellationToken);

        var basket = await BuildBasketAsync(analyticsContext.BranchId, restaurantSlug, branchSlug, publicContext, cancellationToken);
        return View(new PublicMenuPageViewModel(menu, basket));
    }

    private async Task<CartDto> BuildBasketAsync(
        Guid branchId,
        string restaurantSlug,
        string branchSlug,
        PublicOrderingContextDto? publicContext,
        CancellationToken cancellationToken)
    {
        var state = PublicCartSession.Read(HttpContext.Session);
        if (state.BranchId != Guid.Empty && state.BranchId != branchId)
        {
            HttpContext.Session.Remove(PublicCartSession.Key);
            return new CartDto(restaurantSlug, branchSlug, branchId, [], 0, string.Empty, publicContext?.TableId, publicContext?.TableName, publicContext?.TableNameAr, publicContext?.QrCodeId, publicContext?.QrCodeCode);
        }

        try
        {
            return await _orderService.RecalculateCartAsync(
                branchId,
                restaurantSlug,
                branchSlug,
                state.Lines,
                cancellationToken: cancellationToken,
                publicContext: publicContext) ?? new CartDto(restaurantSlug, branchSlug, branchId, [], 0, string.Empty, publicContext?.TableId, publicContext?.TableName, publicContext?.TableNameAr, publicContext?.QrCodeId, publicContext?.QrCodeCode);
        }
        catch (ArgumentException exception)
        {
            HttpContext.Session.Remove(PublicCartSession.Key);
            TempData["Error"] = exception.Message;
            return new CartDto(restaurantSlug, branchSlug, branchId, [], 0, string.Empty, publicContext?.TableId, publicContext?.TableName, publicContext?.TableNameAr, publicContext?.QrCodeId, publicContext?.QrCodeCode);
        }
    }
}

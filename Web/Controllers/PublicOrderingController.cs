using Microsoft.AspNetCore.Mvc;
using System.Text;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Route("menu/{restaurantSlug}/{branchSlug}")]
public sealed class PublicOrderingController : Controller
{
    private const string LastCheckoutKey = "public-last-checkout-key";
    private readonly IPublicMenuService _publicMenuService;
    private readonly IOrderService _orderService;
    private readonly IQrCodeService _qrCodeService;

    public PublicOrderingController(IPublicMenuService publicMenuService, IOrderService orderService, IQrCodeService qrCodeService)
    {
        _publicMenuService = publicMenuService;
        _orderService = orderService;
        _qrCodeService = qrCodeService;
    }

    [HttpGet("product/{itemId:guid}")]
    public async Task<IActionResult> Product(
        string restaurantSlug,
        string branchSlug,
        Guid itemId,
        string? editKey,
        CancellationToken cancellationToken)
    {
        var context = await GetContextAsync(restaurantSlug, branchSlug, cancellationToken);
        if (context is null)
            return NotFound();

        var item = await _orderService.GetPublicItemAsync(
            context.BranchId,
            itemId,
            Language(),
            cancellationToken);
        if (item is null)
            return NotFound();

        var basket = await BuildCartAsync(restaurantSlug, branchSlug, cancellationToken)
            ?? new CartDto(restaurantSlug, branchSlug, context.BranchId, [], 0, string.Empty);
        var selectedModifierOptionIds = new List<Guid>();
        var initialQuantity = 1;
        var state = PublicCartSession.Read(HttpContext.Session);
        var existingLine = string.IsNullOrWhiteSpace(editKey)
            ? null
            : state.Lines.SingleOrDefault(x =>
                PublicCartSession.BuildKey(x.MenuItemId, x.ModifierOptionIds) == editKey &&
                x.MenuItemId == itemId);
        if (existingLine is not null)
        {
            selectedModifierOptionIds = existingLine.ModifierOptionIds.ToList();
            initialQuantity = existingLine.Quantity;
        }

        return View(new PublicProductPageViewModel(
            restaurantSlug,
            branchSlug,
            item,
            basket,
            selectedModifierOptionIds,
            initialQuantity,
            existingLine is null ? null : editKey));
    }

    [HttpPost("cart/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(
        string restaurantSlug,
        string branchSlug,
        Guid itemId,
        int quantity,
        Guid[]? modifierOptionIds,
        bool stayOnMenu = false,
        string? replaceKey = null,
        CancellationToken cancellationToken = default)
    {
        var context = await GetContextAsync(restaurantSlug, branchSlug, cancellationToken);
        if (context is null)
            return NotFound();

        var publicContext = await GetTableContextAsync(restaurantSlug, branchSlug, cancellationToken);
        if (publicContext is null)
            return MutationError("Scan a table QR code before adding items.", "امسح رمز QR الخاص بالطاولة قبل إضافة الأصناف.", 409, restaurantSlug, branchSlug, itemId, stayOnMenu);

        var item = await _orderService.GetPublicItemAsync(context.BranchId, itemId, Language(), cancellationToken);
        if (item is null)
            return MutationError("This product is not available in this menu.", "هذا المنتج غير متاح في هذه القائمة.", 404, restaurantSlug, branchSlug, itemId, stayOnMenu);
        if (!item.IsAvailable)
            return MutationError("This product is currently unavailable.", "هذا المنتج غير متاح حالياً.", 409, restaurantSlug, branchSlug, itemId, stayOnMenu);
        if (quantity is < 1 or > 20)
            return MutationError("Quantity must be between 1 and 20.", "يجب أن تكون الكمية بين 1 و20.", 422, restaurantSlug, branchSlug, itemId, stayOnMenu);

        var state = PublicCartSession.Read(HttpContext.Session);
        if (state.BranchId != Guid.Empty && state.BranchId != context.BranchId)
            state = new PublicCartState();
        if (state.MenuId.HasValue && state.MenuId.Value != item.MenuId)
            return MutationError("Keep basket items within one menu context.", "يرجى إبقاء عناصر السلة ضمن قائمة واحدة.", 409, restaurantSlug, branchSlug, itemId, stayOnMenu);

        var options = (modifierOptionIds ?? []).Distinct().OrderBy(x => x).ToList();
        var lines = state.Lines.ToList();
        if (!string.IsNullOrWhiteSpace(replaceKey))
        {
            var lineToReplace = lines.SingleOrDefault(x =>
                PublicCartSession.BuildKey(x.MenuItemId, x.ModifierOptionIds) == replaceKey &&
                x.MenuItemId == itemId);
            if (lineToReplace is null)
                return MutationError("This basket item is no longer available to edit.", "لم يعد هذا العنصر متاحاً للتعديل.", 409, restaurantSlug, branchSlug, itemId, stayOnMenu);
            lines.Remove(lineToReplace);
        }

        var key = PublicCartSession.BuildKey(itemId, options);
        var existing = lines.SingleOrDefault(x => PublicCartSession.BuildKey(x.MenuItemId, x.ModifierOptionIds) == key);
        if (existing is null)
            lines.Add(new CartLineInput(itemId, quantity, options));
        else
            lines[lines.IndexOf(existing)] = existing with { Quantity = Math.Min(20, existing.Quantity + quantity) };

        if (lines.Count > 20)
            return MutationError("Your basket can contain up to 20 different items.", "يمكن أن تحتوي السلة على 20 صنفاً مختلفاً كحد أقصى.", 422, restaurantSlug, branchSlug, itemId, stayOnMenu);

        CartDto cart;
        try
        {
            // This is the authoritative mutation boundary: product availability,
            // modifier membership/rules, current prices, currency and totals are
            // all recalculated before anything is written to the session basket.
            cart = await _orderService.RecalculateCartAsync(
                context.BranchId,
                restaurantSlug,
                branchSlug,
                lines,
                cancellationToken: cancellationToken,
                publicContext: publicContext) ?? throw new ArgumentException("The basket could not be recalculated.");
        }
        catch (ArgumentException exception)
        {
            return MutationError(exception.Message, "تعذر إضافة المنتج. يرجى مراجعة الاختيارات والمحاولة مرة أخرى.", 422, restaurantSlug, branchSlug, itemId, stayOnMenu);
        }

        state.BranchId = context.BranchId;
        state.TableId = publicContext.TableId;
        state.QrCodeId = publicContext.QrCodeId;
        state.QrCodeCode = publicContext.QrCodeCode;
        state.TableName = publicContext.TableName;
        state.TableNameAr = publicContext.TableNameAr;
        state.MenuId = item.MenuId;
        state.Lines = lines;
        PublicCartSession.Write(HttpContext.Session, state);
        var addedLine = cart.Lines.SingleOrDefault(x => x.Key == key);
        return MutationSuccess(
            cart,
            $"{item.Name} added to basket.",
            $"تمت إضافة {item.Name} إلى السلة.",
            restaurantSlug,
            branchSlug,
            itemId,
            stayOnMenu,
            addedLine);
    }

    [HttpGet("cart")]
    public async Task<IActionResult> Cart(string restaurantSlug, string branchSlug, CancellationToken cancellationToken)
    {
        var cart = await BuildCartAsync(restaurantSlug, branchSlug, cancellationToken);
        return cart is null ? NotFound() : View(cart);
    }

    [HttpPost("cart/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        string restaurantSlug,
        string branchSlug,
        string key,
        int quantity,
        CancellationToken cancellationToken)
    {
        var context = await GetContextAsync(restaurantSlug, branchSlug, cancellationToken);
        if (context is null)
            return NotFound();
        var publicContext = await GetTableContextAsync(restaurantSlug, branchSlug, cancellationToken);
        if (publicContext is null)
            return CartMutationError("Scan a table QR code before updating the basket.", "امسح رمز QR الخاص بالطاولة قبل تحديث السلة.", restaurantSlug, branchSlug, 409);
        if (quantity is < 0 or > 20)
            return CartMutationError("Quantity must be between 0 and 20.", "يجب أن تكون الكمية بين 0 و20.", restaurantSlug, branchSlug);

        var state = PublicCartSession.Read(HttpContext.Session);
        if (state.BranchId != Guid.Empty && state.BranchId != context.BranchId)
            return CartMutationError("This basket belongs to another branch.", "هذه السلة تخص فرعاً آخر.", restaurantSlug, branchSlug, 409);
        var line = state.Lines.SingleOrDefault(x => PublicCartSession.BuildKey(x.MenuItemId, x.ModifierOptionIds) == key);
        if (line is null)
            return CartMutationError("This basket item could not be found.", "تعذر العثور على عنصر السلة.", restaurantSlug, branchSlug, 404);

        var lines = state.Lines.ToList();
        if (quantity == 0)
            lines.Remove(line);
        else
            lines[lines.IndexOf(line)] = line with { Quantity = quantity };

        CartDto cart;
        try
        {
            cart = await _orderService.RecalculateCartAsync(context.BranchId, restaurantSlug, branchSlug, lines, cancellationToken: cancellationToken, publicContext: publicContext)
                ?? throw new ArgumentException("The basket could not be recalculated.");
        }
        catch (ArgumentException exception)
        {
            return CartMutationError(exception.Message, "تعذر تحديث السلة. يرجى المحاولة مرة أخرى.", restaurantSlug, branchSlug, 422);
        }

        state.Lines = lines;
        PublicCartSession.Write(HttpContext.Session, state);
        return CartMutationSuccess(cart, restaurantSlug, branchSlug);
    }

    [HttpPost("cart/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(
        string restaurantSlug,
        string branchSlug,
        string key,
        CancellationToken cancellationToken)
    {
        var context = await GetContextAsync(restaurantSlug, branchSlug, cancellationToken);
        if (context is null)
            return NotFound();
        var publicContext = await GetTableContextAsync(restaurantSlug, branchSlug, cancellationToken);
        if (publicContext is null)
            return CartMutationError("Scan a table QR code before updating the basket.", "امسح رمز QR الخاص بالطاولة قبل تحديث السلة.", restaurantSlug, branchSlug, 409);

        var state = PublicCartSession.Read(HttpContext.Session);
        var line = state.Lines.SingleOrDefault(x => PublicCartSession.BuildKey(x.MenuItemId, x.ModifierOptionIds) == key);
        if (line is null)
            return CartMutationError("This basket item could not be found.", "تعذر العثور على عنصر السلة.", restaurantSlug, branchSlug, 404);

        var lines = state.Lines.Where(x => !ReferenceEquals(x, line)).ToList();
        CartDto cart;
        try
        {
            cart = await _orderService.RecalculateCartAsync(context.BranchId, restaurantSlug, branchSlug, lines, cancellationToken: cancellationToken, publicContext: publicContext)
                ?? throw new ArgumentException("The basket could not be recalculated.");
        }
        catch (ArgumentException exception)
        {
            return CartMutationError(exception.Message, "تعذر تحديث السلة. يرجى المحاولة مرة أخرى.", restaurantSlug, branchSlug, 422);
        }

        state.Lines = lines;
        PublicCartSession.Write(HttpContext.Session, state);
        return CartMutationSuccess(cart, restaurantSlug, branchSlug);
    }

    [HttpGet("checkout")]
    public async Task<IActionResult> Checkout(string restaurantSlug, string branchSlug, CancellationToken cancellationToken)
    {
        var cart = await BuildCartAsync(restaurantSlug, branchSlug, cancellationToken);
        if (cart is null)
            return NotFound();
        if (cart.Lines.Count == 0)
        {
            TempData["Error"] = Local("Add at least one item before checkout.", "أضف صنفاً واحداً على الأقل قبل تأكيد الطلب.");
            return RedirectToAction(nameof(Cart), new { restaurantSlug, branchSlug, lang = Language() });
        }
        if (await GetTableContextAsync(restaurantSlug, branchSlug, cancellationToken) is null)
        {
            TempData["Error"] = Local("Scan a table QR code before checkout.", "امسح رمز QR الخاص بالطاولة قبل تأكيد الطلب.");
            return RedirectToAction(nameof(Cart), new { restaurantSlug, branchSlug, lang = Language() });
        }
        return View(new CheckoutViewModel { RestaurantSlug = restaurantSlug, BranchSlug = branchSlug, Cart = cart });
    }

    [HttpPost("checkout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(
        string restaurantSlug,
        string branchSlug,
        CheckoutViewModel model,
        CancellationToken cancellationToken)
    {
        var cart = await BuildCartAsync(restaurantSlug, branchSlug, cancellationToken);
        if (cart is null)
            return NotFound();
        model.RestaurantSlug = restaurantSlug;
        model.BranchSlug = branchSlug;
        model.Cart = cart;
        var isIdempotentRetry = string.Equals(HttpContext.Session.GetString(LastCheckoutKey), model.IdempotencyKey, StringComparison.Ordinal);
        if (!ModelState.IsValid || (cart.Lines.Count == 0 && !isIdempotentRetry))
        {
            if (cart.Lines.Count == 0)
                ModelState.AddModelError(string.Empty, Local("Your basket is empty or no longer available.", "السلة فارغة أو لم تعد المنتجات متاحة."));
            return View(model);
        }

        try
        {
            var state = PublicCartSession.Read(HttpContext.Session);
            var publicContext = await GetTableContextAsync(restaurantSlug, branchSlug, cancellationToken);
            if (publicContext is null)
            {
                ModelState.AddModelError(string.Empty, Local("Scan a table QR code before placing an order.", "امسح رمز QR الخاص بالطاولة قبل تأكيد الطلب."));
                return View(model);
            }
            var receipt = await _orderService.CreateAsync(
                new CheckoutInput(cart.BranchId, state.MenuId, model.IdempotencyKey, model.CustomerName, model.CustomerPhone, model.Notes, isIdempotentRetry ? [] : state.Lines, publicContext.TableId, publicContext.QrCodeId, publicContext.QrCodeCode),
                cancellationToken);
            if (receipt is null)
                return NotFound();
            HttpContext.Session.Remove(PublicCartSession.Key);
            HttpContext.Session.SetString(LastCheckoutKey, model.IdempotencyKey);
            return RedirectToAction(nameof(Order), new { restaurantSlug, branchSlug, orderNumber = receipt.OrderNumber, lang = Language() });
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [HttpGet("order/{orderNumber}")]
    public async Task<IActionResult> Order(string restaurantSlug, string branchSlug, string orderNumber, CancellationToken cancellationToken)
    {
        var context = await GetContextAsync(restaurantSlug, branchSlug, cancellationToken);
        if (context is null)
            return NotFound();
        var receipt = await _orderService.GetPublicOrderAsync(orderNumber, context.BranchId, cancellationToken);
        return receipt is null ? NotFound() : View(receipt);
    }

    private async Task<CartDto?> BuildCartAsync(string restaurantSlug, string branchSlug, CancellationToken cancellationToken)
    {
        var context = await GetContextAsync(restaurantSlug, branchSlug, cancellationToken);
        if (context is null)
            return null;
        var state = PublicCartSession.Read(HttpContext.Session);
        var publicContext = await GetTableContextAsync(restaurantSlug, branchSlug, cancellationToken);
        if (state.BranchId != Guid.Empty && state.BranchId != context.BranchId)
        {
            state = new PublicCartState();
            PublicCartSession.Write(HttpContext.Session, state);
        }
        try
        {
            return await _orderService.RecalculateCartAsync(context.BranchId, restaurantSlug, branchSlug, state.Lines, cancellationToken: cancellationToken, publicContext: publicContext);
        }
        catch (ArgumentException exception)
        {
            PublicCartSession.Write(HttpContext.Session, new PublicCartState());
            TempData["Error"] = exception.Message;
            return new CartDto(restaurantSlug, branchSlug, context.BranchId, [], 0, string.Empty, publicContext?.TableId, publicContext?.TableName, publicContext?.TableNameAr, publicContext?.QrCodeId, publicContext?.QrCodeCode);
        }
    }

    private async Task<PublicOrderingContextDto?> GetTableContextAsync(string restaurantSlug, string branchSlug, CancellationToken cancellationToken)
    {
        var state = PublicCartSession.Read(HttpContext.Session);
        var code = Request.Query["code"].ToString();
        if (string.IsNullOrWhiteSpace(code))
            code = state.QrCodeCode;
        if (string.IsNullOrWhiteSpace(code))
            return null;
        var context = await _qrCodeService.ResolvePublicContextAsync(restaurantSlug, branchSlug, code, cancellationToken);
        if (context is null && !string.IsNullOrWhiteSpace(state.QrCodeCode))
            PublicCartSession.Write(HttpContext.Session, new PublicCartState());
        return context;
    }

    private async Task<PublicMenuAnalyticsContext?> GetContextAsync(string restaurantSlug, string branchSlug, CancellationToken cancellationToken) =>
        await _publicMenuService.GetAnalyticsContextAsync(restaurantSlug, branchSlug, cancellationToken);

    private IActionResult MutationSuccess(
        CartDto cart,
        string englishMessage,
        string arabicMessage,
        string restaurantSlug,
        string branchSlug,
        Guid itemId,
        bool stayOnMenu,
        CartLineDto? addedLine)
    {
        var message = Local(englishMessage, arabicMessage);
        if (WantsJson())
        {
            return Json(new
            {
                ok = true,
                message,
                itemCount = cart.Lines.Sum(x => x.Quantity),
                total = cart.Total,
                subtotal = cart.Total,
                currency = cart.Currency,
                lineKey = addedLine?.Key,
                lineQuantity = addedLine?.Quantity,
                lineTotal = addedLine?.LineTotal
            });
        }

        TempData["Success"] = message;
        return stayOnMenu
            ? RedirectToAction("Index", "PublicMenu", new { restaurantSlug, branchSlug, lang = Language() })
            : RedirectToAction(nameof(Product), new { restaurantSlug, branchSlug, itemId, lang = Language() });
    }

    private IActionResult MutationError(
        string englishMessage,
        string arabicMessage,
        int statusCode,
        string restaurantSlug,
        string branchSlug,
        Guid itemId,
        bool stayOnMenu)
    {
        var message = Local(englishMessage, arabicMessage);
        if (WantsJson())
            return StatusCode(statusCode, new { ok = false, error = message });

        TempData["Error"] = message;
        return stayOnMenu
            ? RedirectToAction("Index", "PublicMenu", new { restaurantSlug, branchSlug, lang = Language() })
            : RedirectToAction(nameof(Product), new { restaurantSlug, branchSlug, itemId, lang = Language() });
    }

    private IActionResult CartMutationSuccess(CartDto cart, string restaurantSlug, string branchSlug)
    {
        if (WantsJson())
        {
            return Json(new
            {
                ok = true,
                itemCount = cart.Lines.Sum(x => x.Quantity),
                total = cart.Total,
                subtotal = cart.Total,
                currency = cart.Currency
            });
        }
        return RedirectToAction(nameof(Cart), new { restaurantSlug, branchSlug, lang = Language() });
    }

    private IActionResult CartMutationError(string englishMessage, string arabicMessage, string restaurantSlug, string branchSlug, int statusCode = 422)
    {
        var message = Local(englishMessage, arabicMessage);
        if (WantsJson())
            return StatusCode(statusCode, new { ok = false, error = message });
        TempData["Error"] = message;
        return RedirectToAction(nameof(Cart), new { restaurantSlug, branchSlug, lang = Language() });
    }

    private string Language() => string.Equals(Request.Query["lang"].ToString(), "ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";
    private bool IsArabic => Language() == "ar";
    private string Local(string english, string arabic) => IsArabic ? DecodeLegacyArabic(arabic) : english;
    private static string DecodeLegacyArabic(string value) =>
        value.IndexOf('\u00D8') >= 0 || value.IndexOf('\u00D9') >= 0 || value.IndexOf('\u00E2') >= 0
            ? Encoding.UTF8.GetString(Encoding.Latin1.GetBytes(value))
            : value;
    private bool WantsJson() =>
        string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase) ||
        Request.Headers["Accept"].ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Exceptions;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Domain.Enums;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "Menu.View")]
public sealed class MenusController : Controller
{
    private readonly IMenuService _menuService;
    private readonly IImageManagementService _imageService;
    private readonly ILookupService _lookupService;
    private readonly IModifierService _modifierService;
    private readonly IIngredientService _ingredientService;
    private readonly IAllergenService _allergenService;
    private readonly IBranchService _branchService;
    private readonly IPublicMenuService _publicMenuService;
    private readonly IEntitlementService _entitlementService;

    public MenusController(
        IMenuService menuService,
        IImageManagementService imageService,
        ILookupService lookupService,
        IModifierService modifierService,
        IIngredientService ingredientService,
        IAllergenService allergenService,
        IBranchService branchService,
        IPublicMenuService publicMenuService,
        IEntitlementService entitlementService)
    {
        _menuService = menuService;
        _imageService = imageService;
        _lookupService = lookupService;
        _modifierService = modifierService;
        _ingredientService = ingredientService;
        _allergenService = allergenService;
        _branchService = branchService;
        _publicMenuService = publicMenuService;
        _entitlementService = entitlementService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await _menuService.GetAllAsync(cancellationToken));

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var menu = await _menuService.GetAsync(id, cancellationToken);
        return menu is null ? NotFound() : View(menu);
    }

    [Authorize(Policy = "Menu.View")]
    [HttpGet]
    public async Task<IActionResult> Preview(
        Guid id,
        Guid branchId,
        string? lang,
        CancellationToken cancellationToken)
    {
        if (await _menuService.GetAsync(id, cancellationToken) is null)
            return NotFound();

        var preview = await _publicMenuService.GetPreviewAsync(
            id,
            branchId,
            lang,
            cancellationToken);
        if (preview is null)
            return NotFound();

        ViewData["IsAdminPreview"] = true;
        ViewData["PreviewBackUrl"] = Url.Action(nameof(Details), new { id });
        return View("~/Views/PublicMenu/Index.cshtml", preview);
    }

    [Authorize(Policy = "Menu.Create")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new MenuViewModel { IsGlobal = true };
        await PopulateMenuOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [Authorize(Policy = "Menu.Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateMenuOptionsAsync(model, cancellationToken);
            return View(model);
        }
        try
        {
            var menu = await _menuService.CreateAsync(new(model.Name, model.IsGlobal, model.NameAr, model.MenuTypeCode, model.ScopeCode, model.BranchIds, model.Description, model.DescriptionAr, model.BrandPrimaryColor, model.BrandAccentColor, model.SortOrder), cancellationToken);
            TempData["Success"] = "Menu created as a draft.";
            return RedirectToAction(nameof(Details), new { id = menu.Id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateMenuOptionsAsync(model, cancellationToken);
            return View(model);
        }
        catch (EntitlementViolationException ex)
        {
            ViewData["EntitlementError"] = ex.Message;
            await PopulateMenuOptionsAsync(model, cancellationToken);
            return View(model);
        }
    }

    [Authorize(Policy = "Menu.Edit")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var menu = await _menuService.GetAsync(id, cancellationToken);
        if (menu is null)
            return NotFound();
        var model = new MenuViewModel
        {
            Id = menu.Id,
            Name = menu.Name,
            NameAr = menu.NameAr,
            Slug = menu.Slug,
            IsGlobal = menu.IsGlobal,
            MenuTypeCode = menu.MenuTypeCode,
            ScopeCode = menu.ScopeCode,
            Description = menu.Description,
            DescriptionAr = menu.DescriptionAr,
            SortOrder = menu.SortOrder,
            BrandPrimaryColor = menu.BrandPrimaryColor,
            BrandAccentColor = menu.BrandAccentColor
        };
        model.BranchIds = menu.BranchIds?.ToList() ?? [];
        await PopulateMenuOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [Authorize(Policy = "Menu.Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MenuViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateMenuOptionsAsync(model, cancellationToken);
            return View(model);
        }
        try
        {
            var menu = await _menuService.UpdateAsync(id, new(model.Name, model.IsGlobal, model.NameAr, model.MenuTypeCode, model.ScopeCode, model.BranchIds, model.Description, model.DescriptionAr, model.BrandPrimaryColor, model.BrandAccentColor, model.SortOrder), cancellationToken);
            if (menu is null)
                return NotFound();
            TempData["Success"] = "Menu updated.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateMenuOptionsAsync(model, cancellationToken);
            return View(model);
        }
        catch (EntitlementViolationException ex)
        {
            ViewData["EntitlementError"] = ex.Message;
            await PopulateMenuOptionsAsync(model, cancellationToken);
            return View(model);
        }
    }

    [Authorize(Policy = "Menu.Publish")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(Guid id, MenuStatus status, CancellationToken cancellationToken)
    {
        if (status is not (MenuStatus.Draft or MenuStatus.Published or MenuStatus.Archived))
            return BadRequest();

        try
        {
            var menu = await _menuService.SetStatusAsync(id, status, cancellationToken);
            if (menu is null)
                return NotFound();

            TempData["Success"] = status switch
            {
                MenuStatus.Published => "Menu published.",
                MenuStatus.Archived => "Menu archived.",
                _ => "Menu moved to draft."
            };
        }
        catch (ArgumentException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Policy = "Category.Create")]
    [HttpGet]
    public async Task<IActionResult> CreateCategory(Guid menuId, CancellationToken cancellationToken)
    {
        var model = new MenuCategoryViewModel { MenuId = menuId };
        await PopulateCategoryOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [Authorize(Policy = "Category.Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(MenuCategoryViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoryOptionsAsync(model, cancellationToken);
            return View(model);
        }
        try
        {
            var category = await _menuService.CreateCategoryAsync(model.MenuId, new(model.Name, model.SortOrder, model.NameAr, model.Description, model.DescriptionAr, model.ClassificationCode, model.ParentCategoryId), cancellationToken);
            if (category is null) return NotFound();
            TempData["Success"] = "Category created.";
            return RedirectToAction(nameof(Details), new { id = model.MenuId });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCategoryOptionsAsync(model, cancellationToken);
            return View(model);
        }
    }

    [Authorize(Policy = "Category.Edit")]
    [HttpGet]
    public async Task<IActionResult> EditCategory(Guid id, CancellationToken cancellationToken)
    {
        var category = (await FindCategoryAsync(id, cancellationToken));
        if (category is null) return NotFound();
        var model = new MenuCategoryViewModel { MenuId = category.MenuId!.Value, Name = category.Name, NameAr = category.NameAr, Description = category.Description, DescriptionAr = category.DescriptionAr, ClassificationCode = category.ClassificationCode, ParentCategoryId = category.ParentCategoryId, SortOrder = category.SortOrder };
        await PopulateCategoryOptionsAsync(model, cancellationToken, id);
        return View(model);
    }

    [Authorize(Policy = "Category.Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(Guid id, MenuCategoryViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoryOptionsAsync(model, cancellationToken, id);
            return View(model);
        }
        try
        {
            var category = await _menuService.UpdateCategoryAsync(id, new(model.Name, model.SortOrder, model.NameAr, model.Description, model.DescriptionAr, model.ClassificationCode, model.ParentCategoryId), cancellationToken);
            if (category is null) return NotFound();
            TempData["Success"] = "Category updated.";
            return RedirectToAction(nameof(Details), new { id = model.MenuId });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCategoryOptionsAsync(model, cancellationToken, id);
            return View(model);
        }
    }

    [Authorize(Policy = "Category.Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveCategory(Guid id, bool moveUp, Guid menuId, CancellationToken cancellationToken)
    {
        if (!await _menuService.MoveCategoryAsync(id, moveUp, cancellationToken)) return NotFound();
        TempData["Success"] = "Category order updated.";
        return RedirectToAction(nameof(Details), new { id = menuId });
    }

    [Authorize(Policy = "Product.Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveItem(Guid id, bool moveUp, Guid menuId, CancellationToken cancellationToken)
    {
        if (!await _menuService.MoveItemAsync(id, moveUp, cancellationToken)) return NotFound();
        TempData["Success"] = "Item order updated.";
        return RedirectToAction(nameof(Details), new { id = menuId });
    }

    [Authorize(Policy = "Product.Create")]
    [HttpGet]
    public async Task<IActionResult> CreateItem(Guid categoryId, CancellationToken cancellationToken)
    {
        var model = new MenuItemViewModel { CategoryId = categoryId };
        await PopulateItemOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [Authorize(Policy = "Product.Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateItem(MenuItemViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateItemOptionsAsync(model, cancellationToken);
            return View(model);
        }
        try
        {
            var item = await _menuService.CreateItemAsync(model.CategoryId, ToInput(model), cancellationToken);
            if (item is null)
                return NotFound();
            TempData["Success"] = "Item created.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateItemOptionsAsync(model, cancellationToken);
            return View(model);
        }
        catch (EntitlementViolationException ex)
        {
            ViewData["EntitlementError"] = ex.Message;
            await PopulateItemOptionsAsync(model, cancellationToken);
            return View(model);
        }
    }

    [Authorize(Policy = "Product.Edit")]
    [HttpGet]
    public async Task<IActionResult> EditItem(Guid id, CancellationToken cancellationToken)
    {
        var item = await _menuService.GetItemAsync(id, cancellationToken);
        if (item is null)
            return NotFound();
        var model = ToViewModel(item);
        await PopulateItemOptionsAsync(model, cancellationToken);
        model.Images = await _imageService.GetForItemAsync(id, cancellationToken) ?? [];
        model.CategoryOptions = await GetCategoryOptionsAsync(cancellationToken);
        return View(model);
    }

    [Authorize(Policy = "Product.Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditItem(Guid id, MenuItemViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateItemOptionsAsync(model, cancellationToken);
            model.CategoryOptions = await GetCategoryOptionsAsync(cancellationToken);
            return View(model);
        }
        try
        {
            var item = await _menuService.UpdateItemAsync(id, ToInput(model), cancellationToken);
            if (item is null)
                return NotFound();
            TempData["Success"] = "Item updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateItemOptionsAsync(model, cancellationToken);
            model.CategoryOptions = await GetCategoryOptionsAsync(cancellationToken);
            return View(model);
        }
    }

    [Authorize(Policy = "Product.Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetItemAvailability(Guid id, bool isAvailable, CancellationToken cancellationToken)
    {
        if (!await _menuService.SetItemAvailabilityAsync(id, isAvailable, cancellationToken))
            return NotFound();
        TempData["Success"] = isAvailable ? "Item marked available." : "Item marked unavailable.";
        return RedirectToAction(nameof(Index));
    }

    private static MenuItemInput ToInput(MenuItemViewModel model) =>
            new(model.Name, model.Description, model.Price, model.Currency, model.SortOrder, model.IngredientIds, model.AllergenIds, model.NameAr, model.DescriptionAr, model.ProductTypeCode, model.ModifierIds, model.CategoryId);

    private async Task<IReadOnlyList<MenuCategoryDto>> GetCategoryOptionsAsync(CancellationToken cancellationToken)
    {
        var categories = new List<MenuCategoryDto>();
        foreach (var menu in await _menuService.GetAllAsync(cancellationToken))
        {
            var details = await _menuService.GetAsync(menu.Id, cancellationToken);
            if (details is not null)
                categories.AddRange(details.Categories);
        }

        return categories
            .OrderBy(x => x.MenuId)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToList();
    }

    private static MenuItemViewModel ToViewModel(MenuItemDto item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        NameAr = item.NameAr,
        Description = item.Description,
        DescriptionAr = item.DescriptionAr,
        Price = item.Price,
        Currency = item.Currency,
        SortOrder = item.SortOrder,
        IngredientIds = item.IngredientIds?.ToList() ?? [],
        AllergenIds = item.AllergenIds?.ToList() ?? [],
        IsAvailable = item.IsAvailable
        ,ProductTypeCode = item.ProductTypeCode,
        ModifierIds = item.ModifierIds?.ToList() ?? [],
        CategoryName = item.CategoryName,
        MenuName = item.MenuName,
        BranchAvailability = item.BranchAvailability?.ToList() ?? []
    };

    private async Task PopulateItemOptionsAsync(
        MenuItemViewModel model,
        CancellationToken cancellationToken)
    {
        model.CurrencyOptions = await _lookupService.GetActiveAsync(
            LookupTypes.Currency,
            cancellationToken);
        if (string.IsNullOrWhiteSpace(model.Currency))
            model.Currency = model.CurrencyOptions.FirstOrDefault()?.Code ?? string.Empty;
        model.ProductTypes = await _lookupService.GetActiveAsync(LookupTypes.ProductType, cancellationToken);
        model.Modifiers = await _modifierService.GetActiveAsync(cancellationToken);
        model.IngredientOptions = await _ingredientService.GetActiveAsync(cancellationToken);
        model.AllergenOptions = await _allergenService.GetActiveAsync(cancellationToken);
    }

    private async Task PopulateMenuOptionsAsync(MenuViewModel model, CancellationToken cancellationToken)
    {
        model.MenuTypes = await _lookupService.GetActiveAsync(LookupTypes.MenuType, cancellationToken);
        model.MenuScopes = await _lookupService.GetActiveAsync(LookupTypes.MenuScope, cancellationToken);
        model.Branches = await _branchService.GetAllAsync(cancellationToken: cancellationToken);
        model.CanCustomizeBranding = await _entitlementService.HasFeatureAsync(FeatureKeys.CustomBranding, cancellationToken);
        if (string.IsNullOrWhiteSpace(model.MenuTypeCode))
            model.MenuTypeCode = model.MenuTypes.FirstOrDefault()?.Code;
        if (string.IsNullOrWhiteSpace(model.ScopeCode))
            model.ScopeCode = model.MenuScopes.FirstOrDefault(x => x.Code == MenuLookupCodes.AllBranches)?.Code
                ?? model.MenuScopes.FirstOrDefault()?.Code;
    }

    private async Task PopulateCategoryOptionsAsync(MenuCategoryViewModel model, CancellationToken cancellationToken, Guid? excludeCategoryId = null)
    {
        model.Classifications = await _lookupService.GetActiveAsync(LookupTypes.CategoryType, cancellationToken);
        var menu = await _menuService.GetAsync(model.MenuId, cancellationToken);
        model.ParentCategories = menu?.Categories.Where(x => x.Id != excludeCategoryId).OrderBy(x => x.SortOrder).ToList() ?? [];
    }

    private async Task<MenuCategoryDto?> FindCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var menus = await _menuService.GetAllAsync(cancellationToken);
        foreach (var menu in menus)
        {
            var details = await _menuService.GetAsync(menu.Id, cancellationToken);
            var category = details?.Categories.SingleOrDefault(x => x.Id == categoryId);
            if (category is not null) return category;
        }
        return null;
    }
}

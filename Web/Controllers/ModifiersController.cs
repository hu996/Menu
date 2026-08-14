using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "Modifier.View")]
public sealed class ModifiersController : Controller
{
    private readonly IModifierService _modifierService;

    public ModifiersController(IModifierService modifierService) => _modifierService = modifierService;

    [HttpGet]
    public async Task<IActionResult> Index(string? search, bool? isActive, string sortBy = "name", bool descending = false, int page = 1, CancellationToken cancellationToken = default) =>
        View(new ModifierIndexViewModel { Page = await _modifierService.GetPageAsync(search, isActive, sortBy, descending, page, 25, cancellationToken) });

    [HttpGet]
    [Authorize(Policy = "Modifier.Manage")]
    public IActionResult Create() => View(new ModifierViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Modifier.Manage")]
    public async Task<IActionResult> Create(ModifierViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _modifierService.CreateAsync(ToInput(model), cancellationToken);
            TempData["Success"] = "Modifier created.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    [Authorize(Policy = "Modifier.Manage")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var modifier = await _modifierService.GetAsync(id, cancellationToken);
        return modifier is null ? NotFound() : View(ToViewModel(modifier));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Modifier.Manage")]
    public async Task<IActionResult> Edit(Guid id, ModifierViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            var modifier = await _modifierService.UpdateAsync(id, ToInput(model), cancellationToken);
            if (modifier is null)
                return NotFound();
            TempData["Success"] = "Modifier updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Modifier.Manage")]
    public async Task<IActionResult> SetActive(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        if (!await _modifierService.SetActiveAsync(id, isActive, cancellationToken))
            return NotFound();
        TempData["Success"] = isActive ? "Modifier activated." : "Modifier deactivated.";
        return RedirectToAction(nameof(Index));
    }

    private static ModifierInput ToInput(ModifierViewModel model) => new(
        model.Name,
        model.IsRequired,
        model.MinSelections,
        model.MaxSelections,
        model.Options.Select((x, index) => new ModifierOptionInput(x.Name, x.PriceAdjustment, x.SortOrder == 0 ? index + 1 : x.SortOrder, x.NameAr, x.IsActive)).ToList(),
        model.NameAr,
        model.IsActive);

    private static ModifierViewModel ToViewModel(ModifierDto modifier) => new()
    {
        Id = modifier.Id,
        Name = modifier.Name,
        NameAr = modifier.NameAr,
        IsRequired = modifier.IsRequired,
        MinSelections = modifier.MinSelections,
        MaxSelections = modifier.MaxSelections,
        IsActive = modifier.IsActive,
        Options = modifier.Options.Select(x => new ModifierOptionViewModel { Name = x.Name, NameAr = x.NameAr, PriceAdjustment = x.PriceAdjustment, SortOrder = x.SortOrder, IsActive = x.IsActive }).ToList()
    };
}

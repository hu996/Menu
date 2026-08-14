using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "TenantAdmin")]
public sealed class LookupsController : Controller
{
    private readonly ILookupService _lookupService;

    public LookupsController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? type,
        string? search,
        bool? isActive = null,
        string sortBy = "type",
        bool descending = false,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var valuePage = await _lookupService.GetPageAsync(
            type, search, isActive, sortBy, descending, page, 25, cancellationToken);
        return View(new LookupIndexViewModel
        {
            Page = valuePage,
            Types = await _lookupService.GetTypesAsync(false, cancellationToken)
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new LookupValueViewModel();
        await PopulateTypesAsync(model, true, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LookupValueViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateTypesAsync(model, true, cancellationToken);
            return View(model);
        }

        try
        {
            await _lookupService.CreateAsync(ToInput(model), cancellationToken);
            TempData["Success"] = "Lookup value created.";
            return RedirectToAction(nameof(Index), new { type = model.Type });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        await PopulateTypesAsync(model, true, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var value = await _lookupService.GetAsync(id, cancellationToken);
        if (value is null)
            return NotFound();

        var model = ToViewModel(value);
        await PopulateTypesAsync(model, false, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, LookupValueViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateTypesAsync(model, false, cancellationToken);
            return View(model);
        }

        try
        {
            var value = await _lookupService.UpdateAsync(id, ToInput(model), cancellationToken);
            if (value is null)
                return NotFound();
            TempData["Success"] = "Lookup value updated.";
            return RedirectToAction(nameof(Index), new { type = value.Type });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        await PopulateTypesAsync(model, false, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        if (!await _lookupService.SetActiveAsync(id, isActive, cancellationToken))
            return NotFound();
        TempData["Success"] = isActive ? "Lookup value activated." : "Lookup value deactivated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Move(Guid id, bool moveUp, CancellationToken cancellationToken)
    {
        if (!await _lookupService.MoveValueAsync(id, moveUp, cancellationToken))
            return NotFound();
        TempData["Success"] = "Lookup value order updated.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateTypesAsync(
        LookupValueViewModel model,
        bool activeOnly,
        CancellationToken cancellationToken)
    {
        model.Types = User.IsInRole("PlatformAdmin")
            ? await _lookupService.GetTypesAsync(activeOnly, cancellationToken)
            : await _lookupService.GetTenantManagedTypesAsync(activeOnly, cancellationToken);
    }

    private static LookupValueInput ToInput(LookupValueViewModel model) =>
        new(model.Type, model.Code, model.NameEn, model.NameAr, model.Description, model.SortOrder);

    private static LookupValueViewModel ToViewModel(LookupValueDto value) => new()
    {
        Id = value.Id,
        Type = value.Type,
        Code = value.Code,
        NameEn = value.NameEn,
        NameAr = value.NameAr,
        Description = value.Description,
        SortOrder = value.SortOrder,
        IsActive = value.IsActive,
        IsGlobal = value.IsGlobal
    };
}

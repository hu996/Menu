using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "TenantAdmin")]
public sealed class LookupTypesController : Controller
{
    private readonly ILookupService _lookupService;

    public LookupTypesController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        bool? isActive = null,
        string sortBy = "code",
        bool descending = false,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        return View(new LookupTypeIndexViewModel
        {
            Page = await _lookupService.GetTypePageAsync(search, isActive, sortBy, descending, page, 25, cancellationToken)
        });
    }

    [HttpGet]
    public IActionResult Create() => View(new LookupTypeViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LookupTypeViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _lookupService.CreateTypeAsync(ToInput(model), cancellationToken);
            TempData["Success"] = "Lookup type created.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var type = await _lookupService.GetTypeAsync(id, cancellationToken);
        return type is null ? NotFound() : View(ToViewModel(type));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, LookupTypeViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var type = await _lookupService.UpdateTypeAsync(id, ToInput(model), cancellationToken);
            if (type is null)
                return NotFound();
            TempData["Success"] = "Lookup type updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        if (!await _lookupService.SetTypeActiveAsync(id, isActive, cancellationToken))
            return NotFound();
        TempData["Success"] = isActive ? "Lookup type activated." : "Lookup type deactivated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Move(Guid id, bool moveUp, CancellationToken cancellationToken)
    {
        if (!await _lookupService.MoveTypeAsync(id, moveUp, cancellationToken))
            return NotFound();
        TempData["Success"] = "Lookup type order updated.";
        return RedirectToAction(nameof(Index));
    }

    private static LookupTypeInput ToInput(LookupTypeViewModel model) =>
        new(model.Code, model.NameEn, model.NameAr, model.Description, model.SortOrder);

    private static LookupTypeViewModel ToViewModel(LookupTypeDto type) => new()
    {
        Id = type.Id,
        Code = type.Code,
        NameEn = type.NameEn,
        NameAr = type.NameAr,
        Description = type.Description,
        SortOrder = type.SortOrder,
        IsActive = type.IsActive,
        IsGlobal = type.IsGlobal,
        ValueCount = type.ValueCount
    };
}

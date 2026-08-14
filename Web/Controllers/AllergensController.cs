using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "Allergen.View")]
public sealed class AllergensController : Controller
{
    private readonly IAllergenService _allergenService;

    public AllergensController(IAllergenService allergenService) => _allergenService = allergenService;

    [HttpGet]
    public async Task<IActionResult> Index(string? search, bool? isActive, string sortBy = "name", bool descending = false, int page = 1, CancellationToken cancellationToken = default) =>
        View(new AllergenIndexViewModel { Page = await _allergenService.GetPageAsync(search, isActive, sortBy, descending, page, 25, cancellationToken) });

    [HttpGet]
    [Authorize(Policy = "Allergen.Manage")]
    public IActionResult Create() => View(new AllergenViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Allergen.Manage")]
    public async Task<IActionResult> Create(AllergenViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _allergenService.CreateAsync(new(model.Name, model.NameAr), cancellationToken);
            TempData["Success"] = "Allergen created.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    [Authorize(Policy = "Allergen.Manage")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var allergen = await _allergenService.GetAsync(id, cancellationToken);
        return allergen is null ? NotFound() : View(ToViewModel(allergen));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Allergen.Manage")]
    public async Task<IActionResult> Edit(Guid id, AllergenViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            var allergen = await _allergenService.UpdateAsync(id, new(model.Name, model.NameAr), cancellationToken);
            if (allergen is null)
                return NotFound();
            TempData["Success"] = "Allergen updated.";
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
    [Authorize(Policy = "Allergen.Manage")]
    public async Task<IActionResult> SetActive(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        if (!await _allergenService.SetActiveAsync(id, isActive, cancellationToken))
            return NotFound();
        TempData["Success"] = isActive ? "Allergen activated." : "Allergen deactivated.";
        return RedirectToAction(nameof(Index));
    }

    private static AllergenViewModel ToViewModel(AllergenDto value) => new() { Id = value.Id, Name = value.Name, NameAr = value.NameAr, IsActive = value.IsActive };
}

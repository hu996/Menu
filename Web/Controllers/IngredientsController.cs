using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "Ingredient.View")]
public sealed class IngredientsController : Controller
{
    private readonly IIngredientService _ingredientService;

    public IngredientsController(IIngredientService ingredientService) => _ingredientService = ingredientService;

    [HttpGet]
    public async Task<IActionResult> Index(string? search, bool? isActive, string sortBy = "name", bool descending = false, int page = 1, CancellationToken cancellationToken = default) =>
        View(new IngredientIndexViewModel { Page = await _ingredientService.GetPageAsync(search, isActive, sortBy, descending, page, 25, cancellationToken) });

    [HttpGet]
    [Authorize(Policy = "Ingredient.Manage")]
    public IActionResult Create() => View(new IngredientViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Ingredient.Manage")]
    public async Task<IActionResult> Create(IngredientViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _ingredientService.CreateAsync(new(model.Name, model.NameAr), cancellationToken);
            TempData["Success"] = "Ingredient created.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    [Authorize(Policy = "Ingredient.Manage")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var ingredient = await _ingredientService.GetAsync(id, cancellationToken);
        return ingredient is null ? NotFound() : View(ToViewModel(ingredient));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Ingredient.Manage")]
    public async Task<IActionResult> Edit(Guid id, IngredientViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            var ingredient = await _ingredientService.UpdateAsync(id, new(model.Name, model.NameAr), cancellationToken);
            if (ingredient is null)
                return NotFound();
            TempData["Success"] = "Ingredient updated.";
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
    [Authorize(Policy = "Ingredient.Manage")]
    public async Task<IActionResult> SetActive(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        if (!await _ingredientService.SetActiveAsync(id, isActive, cancellationToken))
            return NotFound();
        TempData["Success"] = isActive ? "Ingredient activated." : "Ingredient deactivated.";
        return RedirectToAction(nameof(Index));
    }

    private static IngredientViewModel ToViewModel(IngredientDto item) => new() { Id = item.Id, Name = item.Name, NameAr = item.NameAr, IsActive = item.IsActive };
}

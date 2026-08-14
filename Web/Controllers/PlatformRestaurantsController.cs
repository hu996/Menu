using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Exceptions;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Roles = "PlatformAdmin")]
[Route("PlatformRestaurants")]
public sealed class PlatformRestaurantsController : Controller
{
    private readonly IPlatformRestaurantService _restaurantService;
    private readonly ILookupService _lookupService;

    public PlatformRestaurantsController(
        IPlatformRestaurantService restaurantService,
        ILookupService lookupService)
    {
        _restaurantService = restaurantService;
        _lookupService = lookupService;
    }

    [HttpGet("Index")]
    public async Task<IActionResult> Index(string? search, bool? isActive, CancellationToken cancellationToken)
    {
        return View(new PlatformRestaurantIndexViewModel
        {
            Items = await _restaurantService.GetAllAsync(search, isActive, cancellationToken),
            Search = search,
            IsActive = isActive
        });
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken) =>
        View(await PopulateAsync(new PlatformRestaurantCreateViewModel(), cancellationToken));

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PlatformRestaurantCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(await PopulateAsync(model, cancellationToken));

        try
        {
            var result = await _restaurantService.ProvisionAsync(
                new(
                    model.NameEn,
                    model.NameAr,
                    model.Slug,
                    model.Phone,
                    model.Email,
                    model.Address,
                    model.Currency,
                    model.DefaultLanguage,
                    model.PlanId,
                    model.OwnerName,
                    model.OwnerEmail,
                    model.OwnerPassword),
                cancellationToken);
            TempData["Success"] = result.OwnerWasExistingUser
                ? "Restaurant created and the existing owner was assigned."
                : "Restaurant created and the owner account is ready.";
            return RedirectToAction(nameof(Details), new { id = result.TenantId });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (EntitlementViolationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return View(await PopulateAsync(model, cancellationToken));
    }

    [HttpGet("Details/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var details = await _restaurantService.GetAsync(id, cancellationToken);
        return details is null
            ? NotFound()
            : View(new PlatformRestaurantDetailsViewModel { Details = details });
    }

    [HttpPost("SetActive")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        if (!await _restaurantService.SetActiveAsync(id, isActive, cancellationToken))
            return NotFound();
        TempData["Success"] = isActive ? "Restaurant activated." : "Restaurant deactivated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<PlatformRestaurantCreateViewModel> PopulateAsync(
        PlatformRestaurantCreateViewModel model,
        CancellationToken cancellationToken)
    {
        model.Currencies = await _lookupService.GetActiveAsync(LookupTypes.Currency, cancellationToken);
        model.Languages = await _lookupService.GetActiveAsync(LookupTypes.Language, cancellationToken);
        model.Plans = await _restaurantService.GetActivePlansAsync(cancellationToken);
        if (model.PlanId == Guid.Empty)
            model.PlanId = model.Plans.FirstOrDefault()?.Id ?? Guid.Empty;
        return model;
    }
}

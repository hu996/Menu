using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "Pricing.View")]
public sealed class PricingController : Controller
{
    private readonly IPricingService _pricingService;

    public PricingController(IPricingService pricingService)
    {
        _pricingService = pricingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new PricingViewModel();
        await PopulateAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preview(PricingViewModel model, CancellationToken cancellationToken)
    {
        await PopulateAsync(model, cancellationToken);
        if (!ModelState.IsValid)
            return View("Index", model);

        try
        {
            model.Preview = await _pricingService.PreviewAsync(ToRequest(model), cancellationToken);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Pricing.Edit")]
    public async Task<IActionResult> Apply(PricingViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAsync(model, cancellationToken);
            return View("Index", model);
        }

        try
        {
            var applied = await _pricingService.ApplyAsync(ToRequest(model), cancellationToken);
            TempData["Success"] = $"Applied pricing to {applied.Lines.Count} product(s).";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateAsync(model, cancellationToken);
            return View("Index", model);
        }
    }

    private async Task PopulateAsync(PricingViewModel model, CancellationToken cancellationToken)
    {
        var catalog = await _pricingService.GetCatalogAsync(cancellationToken);
        model.Items = catalog.Items;
        model.Categories = catalog.Categories;
        model.Branches = catalog.Branches;
        model.Operations = catalog.Operations;
        model.Scopes = catalog.Scopes;
        model.History = await _pricingService.GetHistoryAsync(100, cancellationToken);
    }

    private static PricingPreviewRequest ToRequest(PricingViewModel model) =>
        new(model.ScopeCode, model.OperationCode, model.CategoryId, model.BranchId, model.MenuItemIds, model.Value, model.Reason);
}

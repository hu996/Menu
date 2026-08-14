using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Roles = "PlatformAdmin")]
[Route("PlatformPlans")]
public sealed class PlatformPlansController : Controller
{
    private readonly IPlanManagementService _planService;
    private readonly ILookupService _lookupService;

    public PlatformPlansController(IPlanManagementService planService, ILookupService lookupService)
    {
        _planService = planService;
        _lookupService = lookupService;
    }

    [HttpGet("Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(new PlanManagementIndexViewModel { Plans = await _planService.GetAllAsync(cancellationToken) });

    [HttpGet("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken) =>
        View(await WithCurrenciesAsync(new PlanFormViewModel(), cancellationToken));

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PlanFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(await WithCurrenciesAsync(model, cancellationToken));
        try
        {
            await _planService.CreateAsync(ToInput(model), cancellationToken);
            TempData["Success"] = "Plan created.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await WithCurrenciesAsync(model, cancellationToken));
        }
    }

    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var plan = await _planService.GetAsync(id, cancellationToken);
        return plan is null ? NotFound() : View(await WithCurrenciesAsync(ToViewModel(plan), cancellationToken));
    }

    [HttpPost("Update/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, PlanFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(await WithCurrenciesAsync(model, cancellationToken));
        try
        {
            if (await _planService.UpdateAsync(id, ToInput(model), cancellationToken) is null)
                return NotFound();
            TempData["Success"] = "Plan updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await WithCurrenciesAsync(model, cancellationToken));
        }
    }

    [HttpPost("SetActive")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        if (!await _planService.SetActiveAsync(id, isActive, cancellationToken))
            return NotFound();
        TempData["Success"] = isActive ? "Plan activated." : "Plan deactivated.";
        return RedirectToAction(nameof(Index));
    }

    private static PlanManagementInput ToInput(PlanFormViewModel model) => new(
        model.Name,
        model.MonthlyPrice,
        model.Currency,
        model.MaxBranches,
        model.MaxMenuItems,
        model.MaxUsers,
        model.AdvancedAnalytics,
        model.CustomBranding,
        model.IsActive,
        ParseFeatures(model.FeaturesText));

    private static IReadOnlyList<PlanFeatureInput> ParseFeatures(string? text)
    {
        var features = new List<PlanFeatureInput>();
        foreach (var line in (text ?? string.Empty).Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = line.Split('|', StringSplitOptions.TrimEntries);
            if (parts.Length is < 1 or > 3)
                throw new ArgumentException("Each feature must use: key|enabled|optional numeric limit.");
            var enabled = true;
            if (parts.Length >= 2 && !bool.TryParse(parts[1], out enabled))
                throw new ArgumentException("Feature enabled values must be true or false.");
            int? limit = null;
            if (parts.Length == 3 && !string.IsNullOrWhiteSpace(parts[2]))
            {
                if (!int.TryParse(parts[2], out var parsedLimit))
                    throw new ArgumentException("Feature limits must be whole numbers.");
                limit = parsedLimit;
            }
            features.Add(new PlanFeatureInput(parts[0], enabled, limit));
        }
        return features;
    }

    private static PlanFormViewModel ToViewModel(PlanDto plan) => new()
    {
        Id = plan.Id,
        Name = plan.Name,
        MonthlyPrice = plan.MonthlyPrice,
        Currency = plan.Currency,
        MaxBranches = plan.MaxBranches,
        MaxMenuItems = plan.MaxMenuItems,
        MaxUsers = plan.MaxUsers,
        AdvancedAnalytics = plan.AdvancedAnalytics,
        CustomBranding = plan.CustomBranding,
        IsActive = plan.IsActive,
        FeaturesText = string.Join(Environment.NewLine, plan.Features.Select(x => $"{x.FeatureKey}|{x.Enabled}|{(x.LimitValue?.ToString() ?? string.Empty)}"))
    };

    private async Task<PlanFormViewModel> WithCurrenciesAsync(
        PlanFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.CurrencyOptions = await _lookupService.GetActiveAsync(LookupTypes.Currency, cancellationToken);
        return model;
    }
}

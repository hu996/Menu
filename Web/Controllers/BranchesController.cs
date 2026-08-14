using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Exceptions;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "Branch.View")]
public sealed class BranchesController : Controller
{
    private readonly IBranchService _branchService;
    private readonly IMembershipAuthorizationService _membershipAuthorization;
    private readonly IEntitlementService _entitlementService;

    public BranchesController(
        IBranchService branchService,
        IMembershipAuthorizationService membershipAuthorization,
        IEntitlementService entitlementService)
    {
        _branchService = branchService;
        _membershipAuthorization = membershipAuthorization;
        _entitlementService = entitlementService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(BranchQuery query, CancellationToken cancellationToken)
    {
        var restrictedBranchId = !IsTenantWideAdmin()
            ? GetClaimGuid("branch_id")
            : null;
        var page = await _branchService.GetPageAsync(query, restrictedBranchId, cancellationToken);
        return View(new BranchIndexViewModel
        {
            Items = page.Items.Select(ToViewModel).ToList(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
            TotalPages = page.TotalPages,
            Search = page.Search,
            IsActive = page.IsActive,
            SortBy = page.SortBy,
            Descending = page.Descending
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        if (!await CanAccessAsync(id, cancellationToken))
            return NotFound();

        var branch = await _branchService.GetAsync(id, cancellationToken);
        return branch is null ? NotFound() : View(ToViewModel(branch));
    }

    [Authorize(Policy = "Branch.Create")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken) =>
        View(await PopulateAsync(new BranchViewModel(), cancellationToken));

    [Authorize(Policy = "Branch.Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BranchViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(await PopulateAsync(model, cancellationToken));

        try
        {
            var branch = await _branchService.CreateAsync(
                new BranchInput(model.Name, model.Address, model.Phone, model.NameAr, model.Latitude, model.Longitude, model.OpeningHours, model.BrandPrimaryColorOverride, model.BrandAccentColorOverride),
                cancellationToken);
            TempData["Success"] = "Branch created.";
            return RedirectToAction(nameof(Details), new { id = branch.Id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await PopulateAsync(model, cancellationToken));
        }
        catch (EntitlementViolationException ex)
        {
            ViewData["EntitlementError"] = ex.Message;
            return View(await PopulateAsync(model, cancellationToken));
        }
    }

    [Authorize(Policy = "Branch.Edit")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var branch = await _branchService.GetAsync(id, cancellationToken);
        return branch is null
            ? NotFound()
            : View(await PopulateAsync(ToViewModel(branch), cancellationToken));
    }

    [Authorize(Policy = "Branch.Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, BranchViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(await PopulateAsync(model, cancellationToken));

        try
        {
            var branch = await _branchService.UpdateAsync(
                id,
                new BranchInput(model.Name, model.Address, model.Phone, model.NameAr, model.Latitude, model.Longitude, model.OpeningHours, model.BrandPrimaryColorOverride, model.BrandAccentColorOverride),
                cancellationToken);
            if (branch is null)
                return NotFound();

            TempData["Success"] = "Branch updated.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await PopulateAsync(model, cancellationToken));
        }
    }

    [Authorize(Policy = "Branch.Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        if (!await _branchService.SetActiveAsync(id, isActive, cancellationToken))
            return NotFound();

        TempData["Success"] = isActive ? "Branch activated." : "Branch deactivated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<bool> CanAccessAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var userId = GetClaimGuid(ClaimTypes.NameIdentifier);
        return userId.HasValue && await _membershipAuthorization
            .CanAccessBranchAsync(userId.Value, branchId, cancellationToken);
    }

    private Guid? GetClaimGuid(string claimType) =>
        Guid.TryParse(User.FindFirstValue(claimType), out var id) ? id : null;

    private bool IsTenantWideAdmin() =>
        User.IsInRole("PlatformAdmin") ||
        User.IsInRole("TenantOwner") ||
        User.IsInRole("TenantAdmin");

    private static BranchViewModel ToViewModel(BranchDto branch) => new()
    {
        Id = branch.Id,
        Name = branch.Name,
        Slug = branch.Slug,
        Address = branch.Address,
        Phone = branch.Phone,
        NameAr = branch.NameAr,
        Latitude = branch.Latitude,
        Longitude = branch.Longitude,
        OpeningHours = branch.OpeningHours,
        IsActive = branch.IsActive,
        BrandPrimaryColorOverride = branch.BrandPrimaryColorOverride,
        BrandAccentColorOverride = branch.BrandAccentColorOverride
    };

    private async Task<BranchViewModel> PopulateAsync(
        BranchViewModel model,
        CancellationToken cancellationToken)
    {
        model.CanCustomizeBranding = await _entitlementService.HasFeatureAsync(
            FeatureKeys.CustomBranding,
            cancellationToken);
        return model;
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "BranchMenuEditor")]
public sealed class BranchMenusController : Controller
{
    private readonly IBranchMenuService _branchMenuService;
    private readonly AppDbContext _db;
    private readonly IMembershipAuthorizationService _membershipAuthorization;

    public BranchMenusController(
        IBranchMenuService branchMenuService,
        AppDbContext db,
        IMembershipAuthorizationService membershipAuthorization)
    {
        _branchMenuService = branchMenuService;
        _db = db;
        _membershipAuthorization = membershipAuthorization;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid branchId, CancellationToken cancellationToken)
    {
        if (!await CanAccessBranchAsync(branchId, cancellationToken))
            return NotFound();
        var branch = await _db.Branches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == branchId, cancellationToken);
        if (branch is null)
            return NotFound();

        var overrides = await _branchMenuService.GetOverridesAsync(branchId, cancellationToken);
        var items = await _db.MenuItems.AsNoTracking().Where(x => x.TenantId == branch.TenantId).ToListAsync(cancellationToken);
        var model = overrides.Select(x =>
        {
            var item = items.Single(i => i.Id == x.MenuItemId);
            return ToViewModel(branch.Name, item.Name, item.Price, x);
        }).ToList();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid branchId, Guid menuItemId, CancellationToken cancellationToken)
    {
        if (!await CanAccessBranchAsync(branchId, cancellationToken))
            return NotFound();
        var branch = await _db.Branches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == branchId, cancellationToken);
        var item = await _db.MenuItems.AsNoTracking().SingleOrDefaultAsync(x => x.Id == menuItemId, cancellationToken);
        if (branch is null || item is null)
            return NotFound();

        var existing = (await _branchMenuService.GetOverridesAsync(branchId, cancellationToken))
            .SingleOrDefault(x => x.MenuItemId == menuItemId);
        return View(new BranchOverrideEditViewModel
        {
            BranchId = branchId,
            MenuItemId = menuItemId,
            BranchName = branch.Name,
            ItemName = item.Name,
            GlobalPrice = item.Price,
            PriceOverride = existing?.PriceOverride,
            IsAvailableOverride = existing?.IsAvailableOverride,
            IsVisibleOverride = existing?.IsVisibleOverride
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BranchOverrideEditViewModel model, CancellationToken cancellationToken)
    {
        if (!await CanAccessBranchAsync(model.BranchId, cancellationToken))
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _branchMenuService.UpsertOverrideAsync(
                new(model.BranchId, model.MenuItemId, model.PriceOverride, model.IsAvailableOverride, model.IsVisibleOverride),
                cancellationToken);
            TempData["Success"] = "Branch override saved.";
            return RedirectToAction(nameof(Index), new { branchId = model.BranchId });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    private static BranchOverrideViewModel ToViewModel(string branchName, string itemName, decimal globalPrice, BranchMenuItemOverrideDto value) => new()
    {
        BranchId = value.BranchId,
        MenuItemId = value.MenuItemId,
        BranchName = branchName,
        ItemName = itemName,
        GlobalPrice = globalPrice,
        PriceOverride = value.PriceOverride,
        IsAvailableOverride = value.IsAvailableOverride,
        IsVisibleOverride = value.IsVisibleOverride,
        EffectivePrice = value.EffectivePrice,
        IsAvailable = value.IsAvailable,
        IsVisible = value.IsVisible
    };

    private async Task<bool> CanAccessBranchAsync(Guid branchId, CancellationToken cancellationToken)
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            && await _membershipAuthorization.CanAccessBranchAsync(userId, branchId, cancellationToken);
    }
}

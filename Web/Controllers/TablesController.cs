using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "Branch.View")]
public sealed class TablesController : Controller
{
    private readonly ITableService _tableService;
    private readonly IBranchService _branchService;
    private readonly IMembershipAuthorizationService _membershipAuthorization;

    public TablesController(
        ITableService tableService,
        IBranchService branchService,
        IMembershipAuthorizationService membershipAuthorization)
    {
        _tableService = tableService;
        _branchService = branchService;
        _membershipAuthorization = membershipAuthorization;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? branchId, CancellationToken cancellationToken) =>
        View(await BuildModelAsync(branchId, cancellationToken));

    [Authorize(Policy = "Branch.Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TableInputViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return await RenderWithCreateFormAsync(model, cancellationToken);

        if (!await CanAccessBranchAsync(model.BranchId, cancellationToken))
            return NotFound();

        try
        {
            var created = await _tableService.CreateAsync(model.BranchId, new RestaurantTableInput(model.Name, null), cancellationToken);
            if (created is null)
                return NotFound();

            TempData["Success"] = "Table created.";
            return RedirectToAction(nameof(Index), new { branchId = created.BranchId });
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return await RenderWithCreateFormAsync(model, cancellationToken);
        }
    }

    [Authorize(Policy = "Branch.Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(TableInputViewModel model, CancellationToken cancellationToken)
    {
        if (!model.Id.HasValue)
            return BadRequest();

        var existing = await _tableService.GetAsync(model.Id.Value, cancellationToken);
        if (existing is null || !await CanAccessBranchAsync(existing.BranchId, cancellationToken))
            return NotFound();

        if (!ModelState.IsValid)
            return await RenderWithEditFormAsync(existing.BranchId, model, cancellationToken);

        try
        {
            var updated = await _tableService.UpdateAsync(model.Id.Value, new RestaurantTableInput(model.Name, null), cancellationToken);
            if (updated is null)
                return NotFound();

            TempData["Success"] = "Table updated.";
            return RedirectToAction(nameof(Index), new { branchId = existing.BranchId });
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return await RenderWithEditFormAsync(existing.BranchId, model, cancellationToken);
        }
    }

    [Authorize(Policy = "Branch.Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(Guid id, Guid branchId, bool isActive, CancellationToken cancellationToken)
    {
        var existing = await _tableService.GetAsync(id, cancellationToken);
        if (existing is null || existing.BranchId != branchId || !await CanAccessBranchAsync(existing.BranchId, cancellationToken))
            return NotFound();

        if (!await _tableService.SetActiveAsync(id, isActive, cancellationToken))
            return NotFound();
        TempData["Success"] = isActive ? "Table activated." : "Table deactivated.";
        return RedirectToAction(nameof(Index), new { branchId = existing.BranchId });
    }

    private async Task<TableManagementViewModel> BuildModelAsync(Guid? branchId, CancellationToken cancellationToken)
    {
        var restrictedBranchId = IsTenantWideAdmin() ? null : GetClaimGuid("branch_id");
        var branches = await _branchService.GetAllAsync(restrictedBranchId, cancellationToken);
        var selected = branches.FirstOrDefault(x => x.Id == branchId) ?? branches.FirstOrDefault();
        return new TableManagementViewModel
        {
            BranchId = selected?.Id,
            Branches = branches,
            SelectedBranch = selected,
            Tables = selected is null ? [] : await _tableService.GetForBranchAsync(selected.Id, cancellationToken),
            CreateForm = new TableInputViewModel { BranchId = selected?.Id ?? Guid.Empty }
        };
    }

    private async Task<IActionResult> RenderWithCreateFormAsync(TableInputViewModel form, CancellationToken cancellationToken)
    {
        var viewModel = await BuildModelAsync(form.BranchId, cancellationToken);
        viewModel.CreateForm = form;
        return View(nameof(Index), viewModel);
    }

    private async Task<IActionResult> RenderWithEditFormAsync(Guid branchId, TableInputViewModel form, CancellationToken cancellationToken)
    {
        var viewModel = await BuildModelAsync(branchId, cancellationToken);
        viewModel.EditForm = form;
        viewModel.EditErrorTableId = form.Id;
        return View(nameof(Index), viewModel);
    }

    private async Task<bool> CanAccessBranchAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var userId = GetClaimGuid(ClaimTypes.NameIdentifier);
        return userId.HasValue && await _membershipAuthorization.CanAccessBranchAsync(userId.Value, branchId, cancellationToken);
    }

    private Guid? GetClaimGuid(string claimType) =>
        Guid.TryParse(User.FindFirstValue(claimType), out var id) ? id : null;

    private bool IsTenantWideAdmin() =>
        User.IsInRole("PlatformAdmin") ||
        User.IsInRole("TenantOwner") ||
        User.IsInRole("TenantAdmin");
}

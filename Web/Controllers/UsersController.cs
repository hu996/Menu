using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Enums;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "User.View")]
public sealed class UsersController : Controller
{
    private readonly IUserManagementService _userService;

    public UsersController(IUserManagementService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1, CancellationToken cancellationToken = default) =>
        View(new UserIndexViewModel { Page = await _userService.GetPageAsync(search, page, 25, cancellationToken) });

    [HttpGet]
    [Authorize(Policy = "User.Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken) =>
        View(new UserCreateViewModel
        {
            Branches = await _userService.GetBranchesAsync(cancellationToken),
            PermissionOptions = await _userService.GetPermissionOptionsAsync(cancellationToken)
        });

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "User.Create")]
    public async Task<IActionResult> Create(UserCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model, cancellationToken);
            return View(model);
        }
        try
        {
            await _userService.CreateAsync(new(model.DisplayName, model.Email, model.Password, model.Role, model.BranchId, model.PermissionCodes), cancellationToken);
            TempData["Success"] = "User membership created.";
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
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        await PopulateOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    [Authorize(Policy = "User.Edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetAsync(id, cancellationToken);
        if (user is null)
            return NotFound();
        return View(new UserEditViewModel
        {
            MembershipId = user.MembershipId,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role,
            BranchId = user.BranchId,
            IsActive = user.IsActive,
            PermissionCodes = user.PermissionCodes?.ToList() ?? [],
            Branches = await _userService.GetBranchesAsync(cancellationToken),
            PermissionOptions = await _userService.GetPermissionOptionsAsync(cancellationToken)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "User.Edit")]
    public async Task<IActionResult> Edit(UserEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model, cancellationToken);
            return View(model);
        }
        try
        {
            if (await _userService.UpdateAsync(
                    model.MembershipId,
                    new(model.DisplayName, model.Email, model.Role, model.BranchId, model.PermissionCodes),
                    cancellationToken) is null)
                return NotFound();
            TempData["Success"] = "User membership updated.";
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
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        await PopulateOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "User.Deactivate")]
    public async Task<IActionResult> SetActive(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _userService.SetActiveAsync(id, isActive, cancellationToken))
                return NotFound();
            TempData["Success"] = isActive ? "Membership activated." : "Membership deactivated.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private async Task PopulateOptionsAsync(UserCreateViewModel model, CancellationToken cancellationToken)
    {
        model.Branches = await _userService.GetBranchesAsync(cancellationToken);
        model.PermissionOptions = await _userService.GetPermissionOptionsAsync(cancellationToken);
    }

    private async Task PopulateOptionsAsync(UserEditViewModel model, CancellationToken cancellationToken)
    {
        model.Branches = await _userService.GetBranchesAsync(cancellationToken);
        model.PermissionOptions = await _userService.GetPermissionOptionsAsync(cancellationToken);
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "Product.View")]
public sealed class ProductsController : Controller
{
    private readonly IProductCatalogService _productCatalog;

    public ProductsController(IProductCatalogService productCatalog)
    {
        _productCatalog = productCatalog;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        Guid? menuId,
        Guid? categoryId,
        Guid? branchId,
        bool? isAvailable,
        int page = 1,
        int pageSize = 25,
        string sortBy = "name",
        bool descending = false,
        CancellationToken cancellationToken = default)
    {
        var restrictedBranchId = IsTenantWideAdmin() ? null : GetClaimGuid("branch_id");
        var query = new ProductQuery(search, menuId, categoryId, branchId, isAvailable, page, pageSize, sortBy, descending);
        var options = await _productCatalog.GetFilterOptionsAsync(restrictedBranchId, cancellationToken);
        var result = await _productCatalog.GetPageAsync(query, restrictedBranchId, cancellationToken);
        return View(new ProductIndexViewModel
        {
            Page = result,
            Options = options,
            CanBulkEdit = User.HasClaim("permission", "Product.Edit")
        });
    }

    [Authorize(Policy = "Product.Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkAvailability(
        List<Guid>? itemIds,
        bool isAvailable,
        CancellationToken cancellationToken)
    {
        var changed = await _productCatalog.SetAvailabilityAsync(itemIds ?? [], isAvailable, cancellationToken);
        TempData["Success"] = $"{changed} product(s) updated.";
        return RedirectToAction(nameof(Index));
    }

    private Guid? GetClaimGuid(string claimType) =>
        Guid.TryParse(User.FindFirstValue(claimType), out var id) ? id : null;

    private bool IsTenantWideAdmin() =>
        User.IsInRole("PlatformAdmin") ||
        User.IsInRole("TenantOwner") ||
        User.IsInRole("TenantAdmin");
}

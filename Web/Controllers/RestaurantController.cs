using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Exceptions;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Domain.Enums;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "Restaurant.View")]
public sealed class RestaurantController : Controller
{
    private readonly IRestaurantService _restaurantService;
    private readonly IDashboardService _dashboardService;

    public RestaurantController(
        IRestaurantService restaurantService,
        IDashboardService dashboardService)
    {
        _restaurantService = restaurantService;
        _dashboardService = dashboardService;
    }

    [HttpGet]
    [Authorize(Policy = "TenantAdmin")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        if (User.IsInRole("PlatformAdmin"))
            return RedirectToAction("Index", "PlatformRestaurants");

        var current = await _restaurantService.GetAsync(cancellationToken);
        if (current is null)
            return NotFound();

        return View(ToCreateViewModel(current));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "TenantAdmin")]
    public async Task<IActionResult> Create(RestaurantCreateViewModel model, CancellationToken cancellationToken)
    {
        if (User.IsInRole("PlatformAdmin"))
            return RedirectToAction("Index", "PlatformRestaurants");

        if (!ModelState.IsValid)
        {
            await PopulateCreateOptionsAsync(model, cancellationToken);
            return View(model);
        }

        try
        {
            var result = await _restaurantService.CreateAsync(
                new(
                    model.NameEn,
                    model.NameAr,
                    model.Slug,
                    model.Phone,
                    model.Email,
                    model.Address,
                    model.Currency,
                    model.DefaultLanguage,
                    model.BrandPrimaryColor,
                    model.BrandAccentColor),
                cancellationToken);

            var brandingError = await SaveBrandingFilesAsync(model.LogoFile, model.CoverFile, cancellationToken);
            await SignInForTenantAsync(result);
            if (brandingError is not null)
            {
                ModelState.AddModelError(brandingError.Value.Field, brandingError.Value.Message);
                await PopulateCreateOptionsAsync(model, cancellationToken);
                return View(model);
            }
            return Redirect($"/r/{result.TenantSlug}/Restaurant/Workspace");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCreateOptionsAsync(model, cancellationToken);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCreateOptionsAsync(model, cancellationToken);
            return View(model);
        }
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCreateOptionsAsync(model, cancellationToken);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Workspace(CancellationToken cancellationToken)
    {
        var model = await _dashboardService.GetAsync(cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var settings = await _restaurantService.GetAsync(cancellationToken);
        return settings is null ? NotFound() : View(ToViewModel(settings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Restaurant.Edit")]
    public async Task<IActionResult> Index(RestaurantViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSettingsOptionsAsync(model, cancellationToken);
            return View(model);
        }
        try
        {
            var settings = await _restaurantService.UpdateAsync(
                new(model.NameEn, model.NameAr, model.Phone, model.Email, model.Address, model.Currency, model.DefaultLanguage, model.BrandPrimaryColor, model.BrandAccentColor),
                cancellationToken);
            if (settings is null)
                return NotFound();
            var brandingError = await SaveBrandingFilesAsync(model.LogoFile, model.CoverFile, cancellationToken);
            TempData["Success"] = brandingError is null ? "Restaurant settings updated." : "Restaurant settings saved; image changes need attention.";
            if (brandingError is not null)
            {
                ModelState.AddModelError(brandingError.Value.Field, brandingError.Value.Message);
                await PopulateSettingsOptionsAsync(model, cancellationToken);
                return View(model);
            }
            var refreshed = await _restaurantService.GetAsync(cancellationToken) ?? settings;
            return View(ToViewModel(refreshed));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateSettingsOptionsAsync(model, cancellationToken);
            return View(model);
        }
        catch (EntitlementViolationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateSettingsOptionsAsync(model, cancellationToken);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Restaurant.Edit")]
    public async Task<IActionResult> DeleteBranding(TenantBrandingKind kind, CancellationToken cancellationToken)
    {
        if (!await _restaurantService.DeleteBrandingAsync(kind, cancellationToken))
            return NotFound();
        TempData["Success"] = kind == TenantBrandingKind.Logo ? "Restaurant logo deleted." : "Restaurant cover deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<(string Field, string Message)?> SaveBrandingFilesAsync(
        IFormFile? logo,
        IFormFile? cover,
        CancellationToken cancellationToken)
    {
        foreach (var upload in new[]
        {
            (Kind: TenantBrandingKind.Logo, Field: "LogoFile", File: logo),
            (Kind: TenantBrandingKind.Cover, Field: "CoverFile", File: cover)
        })
        {
            if (upload.File is null)
                continue;
            if (upload.File.Length == 0)
                return (upload.Field, "Choose an image file.");
            try
            {
                await using var stream = upload.File.OpenReadStream();
                await _restaurantService.UploadBrandingAsync(upload.Kind, stream, upload.File.FileName, upload.File.ContentType, upload.File.Length, cancellationToken);
            }
            catch (ArgumentException ex)
            {
                return (upload.Field, ex.Message);
            }
        }
        return null;
    }

    private async Task PopulateCreateOptionsAsync(RestaurantCreateViewModel model, CancellationToken cancellationToken)
    {
        var current = await _restaurantService.GetAsync(cancellationToken);
        model.Currencies = current?.Currencies ?? [];
        model.Languages = current?.Languages ?? [];
    }

    private async Task PopulateSettingsOptionsAsync(RestaurantViewModel model, CancellationToken cancellationToken)
    {
        var current = await _restaurantService.GetAsync(cancellationToken);
        model.Currencies = current?.Currencies ?? [];
        model.Languages = current?.Languages ?? [];
    }

    private static RestaurantCreateViewModel ToCreateViewModel(RestaurantSettingsDto value) => new()
    {
        Currency = value.Currencies.FirstOrDefault(x => string.Equals(x.Code, value.Currency, StringComparison.OrdinalIgnoreCase))?.Code
            ?? value.Currencies.FirstOrDefault()?.Code
            ?? string.Empty,
        DefaultLanguage = value.Languages.FirstOrDefault(x => string.Equals(x.Code, value.DefaultLanguage, StringComparison.OrdinalIgnoreCase))?.Code
            ?? value.Languages.FirstOrDefault()?.Code
            ?? string.Empty,
        Currencies = value.Currencies,
        Languages = value.Languages
    };

    private static RestaurantViewModel ToViewModel(RestaurantSettingsDto value) => new()
    {
        Id = value.Id,
        IsActive = value.IsActive,
        NameEn = value.NameEn ?? value.Name,
        NameAr = value.NameAr,
        LogoUrl = value.LogoUrl,
        CoverImageUrl = value.CoverImageUrl,
        Phone = value.Phone,
        Email = value.Email,
        Address = value.Address,
        Currency = value.Currency ?? value.Currencies.FirstOrDefault()?.Code ?? string.Empty,
        DefaultLanguage = value.DefaultLanguage.ToUpperInvariant(),
        BrandPrimaryColor = value.BrandPrimaryColor,
        BrandAccentColor = value.BrandAccentColor,
        Currencies = value.Currencies,
        Languages = value.Languages
    };

    private async Task SignInForTenantAsync(RestaurantCreationResult result)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty),
            new(ClaimTypes.Name, result.DisplayName),
            new(ClaimTypes.Email, result.Email),
            new(ClaimTypes.Role, "TenantOwner"),
            new("tenant_id", result.TenantId.ToString()),
            new("tenant_slug", result.TenantSlug),
            new("membership_id", result.MembershipId.ToString()),
            new("security_stamp", result.SecurityStamp),
            new("tenant_name", result.DisplayName),
            new("permissions_loaded", "1")
        };
        claims.AddRange(PermissionCatalog.Preset(MembershipRole.TenantOwner).Select(permission => new Claim("permission", permission)));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
    }
}

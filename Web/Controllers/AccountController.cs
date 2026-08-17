using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Enums;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[EnableRateLimiting("authentication")]
public sealed class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IEmailSender _emailSender;
    private readonly ITenantContext _tenantContext;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAuthService authService,
        IPasswordResetService passwordResetService,
        IEmailSender emailSender,
        ITenantContext tenantContext,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<AccountController> logger)
    {
        _authService = authService;
        _passwordResetService = passwordResetService;
        _emailSender = emailSender;
        _tenantContext = tenantContext;
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        var model = new LoginViewModel { ReturnUrl = returnUrl };
        if (_environment.IsDevelopment())
        {
            model.Email = _configuration["DevelopmentLoginDefaults:Email"] ?? string.Empty;
            model.Password = _configuration["DevelopmentLoginDefaults:Password"] ?? string.Empty;
        }

        return View(model);
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var authentication = await _authService.AuthenticateAsync(
            model.Email,
            model.Password,
            cancellationToken);

        if (!authentication.Succeeded)
        {
            var message = authentication.FailureCode switch
            {
                "membership_required" => "This account has no active restaurant membership.",
                "multiple_memberships" => "This account has multiple active restaurant memberships. Ask an administrator to resolve the membership before signing in.",
                "tenant_inactive" => "This restaurant is currently inactive. Contact the platform owner.",
                _ => "The email or password is incorrect."
            };
            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        var authenticatedUser = authentication.User!;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, authenticatedUser.UserId.ToString()),
            new(ClaimTypes.Name, authenticatedUser.DisplayName),
            new(ClaimTypes.Email, authenticatedUser.Email),
            new(ClaimTypes.Role, authenticatedUser.Role.ToString()),
            new("tenant_id", authenticatedUser.TenantId.ToString()),
            new("tenant_slug", authenticatedUser.TenantSlug),
            new("membership_id", authenticatedUser.MembershipId.ToString()),
            new("security_stamp", authenticatedUser.SecurityStamp),
            new("tenant_name", authenticatedUser.TenantName ?? authenticatedUser.TenantSlug),
            new("permissions_loaded", "1")
        };

        if (authenticatedUser.BranchId.HasValue)
            claims.Add(new Claim("branch_id", authenticatedUser.BranchId.Value.ToString()));
        foreach (var permission in authenticatedUser.Permissions ?? [])
            claims.Add(new Claim("permission", permission));

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        if (authenticatedUser.Role == MembershipRole.PlatformAdmin)
            return Redirect("/PlatformRestaurants/Index");

        return Redirect($"/r/{authenticatedUser.TenantSlug}/Dashboard/Index");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied() => View();

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _passwordResetService.RequestAsync(model.Email, cancellationToken);
        string? developmentResetUrl = null;
        if (result.Token is not null && result.RecipientEmail is not null)
        {
            if (_environment.IsDevelopment())
            {
                developmentResetUrl = Url.Action(
                    nameof(ResetPassword),
                    "Account",
                    new { token = result.Token },
                    Request.Scheme);
            }
            else
            {
                var publicBaseUrl = _configuration["Email:PublicBaseUrl"]?.TrimEnd('/');
                if (string.IsNullOrWhiteSpace(publicBaseUrl))
                {
                    _logger.LogError(
                        "Password reset email delivery skipped because Email:PublicBaseUrl is not configured. Request {RequestId}.",
                        HttpContext.TraceIdentifier);
                }
                else
                {
                    var resetUrl = $"{publicBaseUrl}/Account/ResetPassword?token={Uri.EscapeDataString(result.Token)}";
                    try
                    {
                        await _emailSender.SendPasswordResetAsync(
                            result.RecipientEmail,
                            result.RecipientName ?? result.RecipientEmail,
                            resetUrl,
                            cancellationToken);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(
                            exception,
                            "Password reset email delivery failed for request {RequestId}.",
                            HttpContext.TraceIdentifier);
                    }
                }
            }
        }

        return View("ForgotPasswordConfirmation", new ForgotPasswordConfirmationViewModel(developmentResetUrl));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResetPassword(string? token) =>
        View(new ResetPasswordViewModel { Token = token ?? string.Empty });

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            if (!await _passwordResetService.ResetAsync(model.Token, model.NewPassword, cancellationToken))
            {
                ModelState.AddModelError(string.Empty, "This password reset link is invalid or expired.");
                return View(model);
            }

            TempData["Success"] = "Password reset. Sign in with your new password.";
            return RedirectToAction(nameof(Login));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Challenge();
        try
        {
            if (!await _authService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword, cancellationToken))
            {
                ModelState.AddModelError(string.Empty, "The current password is incorrect.");
                return View(model);
            }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Password changed. Sign in again with your new password.";
            return RedirectToAction(nameof(Login));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}

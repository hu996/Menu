using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.Interfaces;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "Subscription.View")]
public sealed class BillingController : Controller
{
    private readonly IBillingService _billingService;

    public BillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await _billingService.GetOverviewAsync(cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Subscription.Manage")]
    public async Task<IActionResult> InitiatePayment(Guid planId, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _billingService.InitiatePlanPaymentAsync(planId, cancellationToken);
            TempData["Success"] = $"Payment started with {payment.Provider}. Reference: {payment.ProviderReference}. Awaiting verified provider confirmation.";
        }
        catch (ArgumentException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Subscription.Manage")]
    public async Task<IActionResult> CancelSubscription(CancellationToken cancellationToken)
    {
        if (await _billingService.CancelCurrentSubscriptionAsync(cancellationToken))
            TempData["Success"] = "The current subscription was cancelled.";
        else
            TempData["Error"] = "No active subscription was found.";
        return RedirectToAction(nameof(Index));
    }
}

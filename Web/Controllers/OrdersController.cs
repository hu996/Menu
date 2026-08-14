using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "Orders.View")]
public sealed class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ITableService _tableService;
    private readonly IBranchService _branchService;

    public OrdersController(IOrderService orderService, ITableService tableService, IBranchService branchService)
    {
        _orderService = orderService;
        _tableService = tableService;
        _branchService = branchService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        Guid? branchId,
        Guid? tableId,
        string? status,
        string? search,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var branchScopeId = GetBranchScope();
        var visibleBranches = await _branchService.GetAllAsync(branchScopeId, cancellationToken);
        var effectiveBranchId = branchId ?? branchScopeId;
        var tables = new List<RestaurantMenuPlatform.Application.DTOs.RestaurantTableDto>();
        var tableBranches = effectiveBranchId.HasValue
            ? visibleBranches.Where(x => x.Id == effectiveBranchId.Value)
            : visibleBranches;
        foreach (var branch in tableBranches)
            tables.AddRange(await _tableService.GetForBranchAsync(branch.Id, cancellationToken));

        var orders = await _orderService.GetStaffOrdersAsync(
            branchScopeId,
            branchId,
            tableId,
            status,
            search,
            ToUtcStart(dateFrom),
            ToUtcEnd(dateTo),
            cancellationToken);
        ViewBag.Branches = visibleBranches;
        ViewBag.Tables = tables;
        ViewBag.BranchId = branchId;
        ViewBag.TableId = tableId;
        ViewBag.Status = status;
        ViewBag.Search = search;
        ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
        ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetStaffOrderAsync(id, GetBranchScope(), cancellationToken);
        return order is null ? NotFound() : View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(Guid id, OrderStatus status, CancellationToken cancellationToken)
    {
        var permission = status switch
        {
            OrderStatus.Accepted => "Orders.Accept",
            OrderStatus.Preparing => "Orders.Prepare",
            OrderStatus.Ready => "Orders.Ready",
            OrderStatus.Completed => "Orders.Complete",
            OrderStatus.Rejected => "Orders.Reject",
            OrderStatus.Cancelled => "Orders.Cancel",
            _ => string.Empty
        };
        if (string.IsNullOrWhiteSpace(permission) || !User.HasClaim("permission", permission))
            return Forbid();

        try
        {
            var order = await _orderService.TransitionAsync(id, status, GetBranchScope(), GetUserId(), User.Identity?.Name, cancellationToken);
            if (order is null)
                return NotFound();
            TempData["Success"] = $"Order {order.OrderNumber} moved to {order.Status}.";
        }
        catch (ArgumentException exception)
        {
            TempData["Error"] = exception.Message;
        }
        var tenantSlug = User.FindFirst("tenant_slug")?.Value;
        return string.IsNullOrWhiteSpace(tenantSlug)
            ? RedirectToAction(nameof(Details), new { id })
            : Redirect($"/r/{Uri.EscapeDataString(tenantSlug)}/Orders/Details/{id}");
    }

    [HttpGet]
    public async Task<IActionResult> Kitchen(CancellationToken cancellationToken)
    {
        var branchScope = GetBranchScope();
        var statuses = new[] { OrderStatus.Pending, OrderStatus.Accepted, OrderStatus.Preparing, OrderStatus.Ready };
        var queue = new List<RestaurantMenuPlatform.Application.DTOs.StaffOrderDto>();
        foreach (var status in statuses)
            queue.AddRange(await _orderService.GetStaffOrdersAsync(branchScope, status: status.ToString(), cancellationToken: cancellationToken));
        return View(queue.OrderBy(x => x.Status).ThenBy(x => x.CreatedAtUtc).ToList());
    }

    [HttpGet]
    public async Task<IActionResult> Print(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetStaffOrderAsync(id, GetBranchScope(), cancellationToken);
        return order is null ? NotFound() : View(order);
    }

    private Guid? GetBranchScope() => Guid.TryParse(User.FindFirstValue("branch_id"), out var id) ? id : null;
    private Guid? GetUserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private static DateTime? ToUtcStart(DateTime? value) => value.HasValue
        ? DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Local).ToUniversalTime()
        : null;

    private static DateTime? ToUtcEnd(DateTime? value) => value.HasValue
        ? DateTime.SpecifyKind(value.Value.Date.AddDays(1), DateTimeKind.Local).ToUniversalTime()
        : null;
}

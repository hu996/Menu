using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.Interfaces;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "Audit.View")]
public sealed class AuditController : Controller
{
    private readonly IAuditLogService _auditLogService;

    public AuditController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await _auditLogService.GetRecentAsync(100, cancellationToken));
}

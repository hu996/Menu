using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Web.Models;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "QR.View")]
public sealed class QrCodesController : Controller
{
    private readonly IQrCodeService _qrCodeService;
    private readonly IBranchService _branchService;
    private readonly ITableService _tableService;

    public QrCodesController(
        IQrCodeService qrCodeService,
        IBranchService branchService,
        ITableService tableService)
    {
        _qrCodeService = qrCodeService;
        _branchService = branchService;
        _tableService = tableService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? branchId, Guid? tableId, CancellationToken cancellationToken)
    {
        var model = await BuildModelAsync(branchId, tableId, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var code = await _qrCodeService.GetAsync(id, $"{Request.Scheme}://{Request.Host}", cancellationToken);
        return code is null ? NotFound() : View(code);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QR.Create")]
    public async Task<IActionResult> GenerateForTable(Guid branchId, Guid tableId, CancellationToken cancellationToken)
    {
        var generated = await _qrCodeService.GetOrCreateForTableAsync(tableId, $"{Request.Scheme}://{Request.Host}", cancellationToken);
        if (generated is null || generated.BranchId != branchId)
            return NotFound();
        TempData["Success"] = generated.IsActive
            ? $"QR code for {generated.TableName ?? "table"} is ready."
            : $"QR code for {generated.TableName ?? "table"} already exists but is inactive. Reactivate it to use the same QR again.";
        return RedirectToAction(nameof(Index), new { branchId = generated.BranchId, tableId = generated.TableId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QR.Create")]
    public IActionResult Generate(
        Guid branchId,
        string? tableLabels,
        CancellationToken cancellationToken)
    {
        return BadRequest("Create or select a real table first, then generate its table-specific QR code.");
    }

    [HttpGet]
    public async Task<IActionResult> Print(Guid branchId, CancellationToken cancellationToken)
    {
        var model = await BuildModelAsync(branchId, null, cancellationToken);
        if (model.SelectedBranch is null)
            return NotFound();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Asset(
        Guid id,
        string format,
        bool download = false,
        CancellationToken cancellationToken = default)
    {
        var asset = await _qrCodeService.RenderAsync(
            id,
            $"{Request.Scheme}://{Request.Host}",
            format,
            cancellationToken);
        if (asset is null)
            return NotFound();
        return download
            ? File(asset.Content, asset.ContentType, asset.FileName)
            : File(asset.Content, asset.ContentType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QR.Deactivate")]
    public async Task<IActionResult> SetActive(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken)
    {
        if (!await _qrCodeService.SetActiveAsync(id, isActive, cancellationToken))
            return NotFound();
        var code = await _qrCodeService.GetAsync(id, $"{Request.Scheme}://{Request.Host}", cancellationToken);
        if (code is null)
            return NotFound();
        TempData["Success"] = isActive ? "QR code reactivated." : "QR code deactivated.";
        return RedirectToAction(nameof(Index), new { branchId = code.BranchId, tableId = code.TableId });
    }

    private async Task<QrCodeManagementViewModel> BuildModelAsync(
        Guid? branchId,
        Guid? tableId,
        CancellationToken cancellationToken)
    {
        var branches = await _branchService.GetAllAsync(cancellationToken: cancellationToken);
        var selected = branches.FirstOrDefault(x => x.Id == branchId) ?? branches.FirstOrDefault();
        var codes = selected is null
            ? []
            : await _qrCodeService.GetForBranchAsync(
                selected.Id,
                $"{Request.Scheme}://{Request.Host}",
                cancellationToken);
        var tables = selected is null ? [] : await _tableService.GetForBranchAsync(selected.Id, cancellationToken);
        return new QrCodeManagementViewModel
        {
            BranchId = selected?.Id,
            TableId = tableId,
            Branches = branches,
            SelectedBranch = selected,
            Codes = codes,
            Tables = tables
        };
    }
}

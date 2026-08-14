using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuPlatform.Application.Interfaces;

namespace RestaurantMenuPlatform.Web.Controllers;

[Authorize(Policy = "MenuEditor")]
public sealed class ImagesController : Controller
{
    private readonly IImageManagementService _imageService;

    public ImagesController(IImageManagementService imageService)
    {
        _imageService = imageService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(Guid menuItemId, IFormFileCollection? files, string? altText, CancellationToken cancellationToken)
    {
        if (files is null || files.Count == 0 || files.All(x => x.Length == 0))
        {
            TempData["ImageFieldError"] = "Choose an image file.";
            return RedirectToAction("EditItem", "Menus", new { id = menuItemId });
        }

        try
        {
            var uploaded = 0;
            foreach (var file in files.Where(x => x.Length > 0))
            {
                await using var stream = file.OpenReadStream();
                var image = await _imageService.UploadAsync(
                    menuItemId,
                    stream,
                    file.FileName,
                    file.ContentType,
                    file.Length,
                    altText,
                    cancellationToken);
                if (image is null)
                    return NotFound();
                uploaded++;
            }

            TempData["Success"] = $"{uploaded} image(s) uploaded.";
            return RedirectToAction("EditItem", "Menus", new { id = menuItemId });
        }
        catch (ArgumentException ex)
        {
            TempData["ImageFieldError"] = ex.Message;
            return RedirectToAction("EditItem", "Menus", new { id = menuItemId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid imageId, Guid menuItemId, CancellationToken cancellationToken)
    {
        if (!await _imageService.DeleteAsync(imageId, cancellationToken))
            return NotFound();
        TempData["Success"] = "Image deleted.";
        return RedirectToAction("EditItem", "Menus", new { id = menuItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Replace(
        Guid imageId,
        Guid menuItemId,
        IFormFile? file,
        string? altText,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            TempData["ImageFieldError"] = "Choose a replacement image file.";
            TempData["ImageErrorImageId"] = imageId.ToString();
            return RedirectToAction("EditItem", "Menus", new { id = menuItemId });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var image = await _imageService.ReplaceAsync(
                imageId,
                stream,
                file.FileName,
                file.ContentType,
                file.Length,
                altText,
                cancellationToken);
            if (image is null)
                return NotFound();
            TempData["Success"] = "Image replaced.";
            return RedirectToAction("EditItem", "Menus", new { id = menuItemId });
        }
        catch (ArgumentException ex)
        {
            TempData["ImageFieldError"] = ex.Message;
            TempData["ImageErrorImageId"] = imageId.ToString();
            return RedirectToAction("EditItem", "Menus", new { id = menuItemId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPrimary(Guid imageId, Guid menuItemId, CancellationToken cancellationToken)
    {
        if (!await _imageService.SetPrimaryAsync(imageId, cancellationToken))
            return NotFound();
        TempData["Success"] = "Primary image updated.";
        return RedirectToAction("EditItem", "Menus", new { id = menuItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Move(Guid imageId, Guid menuItemId, bool moveUp, CancellationToken cancellationToken)
    {
        if (!await _imageService.MoveAsync(imageId, moveUp, cancellationToken))
            return NotFound();
        TempData["Success"] = "Image order updated.";
        return RedirectToAction("EditItem", "Menus", new { id = menuItemId });
    }
}

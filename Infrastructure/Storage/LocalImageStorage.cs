using System.Security.Cryptography;
using RestaurantMenuPlatform.Application.Interfaces;

namespace RestaurantMenuPlatform.Infrastructure.Storage;

public sealed class LocalImageStorage : IImageStorage
{
    private readonly string _rootPath;
    private readonly long? _maxBytes;

    public LocalImageStorage(string rootPath, long? maxBytes = null)
    {
        _rootPath = Path.GetFullPath(rootPath);
        _maxBytes = maxBytes;
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<StoredImage> SaveAsync(
        Guid tenantId,
        Stream content,
        string originalFileName,
        string contentType,
        long length,
        CancellationToken cancellationToken = default)
    {
        return await SaveToAreaAsync(tenantId, "menu-items", content, originalFileName, contentType, length, cancellationToken);
    }

    public async Task<StoredImage> SaveBrandingAsync(
        Guid tenantId,
        Stream content,
        string originalFileName,
        string contentType,
        long length,
        CancellationToken cancellationToken = default)
    {
        return await SaveToAreaAsync(tenantId, "branding", content, originalFileName, contentType, length, cancellationToken);
    }

    private async Task<StoredImage> SaveToAreaAsync(
        Guid tenantId,
        string area,
        Stream content,
        string originalFileName,
        string contentType,
        long length,
        CancellationToken cancellationToken)
    {
        ImageContentValidator.ValidateFileName(originalFileName, contentType);
        await using var buffered = await ImageContentValidator.BufferAndValidateAsync(
            content,
            contentType,
            length,
            _maxBytes,
            cancellationToken);
        var extension = ImageContentValidator.ExtensionFor(contentType);
        var storedFileName = $"{Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant()}{extension}";
        var tenantPath = GetTenantPath(tenantId, area);
        Directory.CreateDirectory(tenantPath);
        var path = Path.Combine(tenantPath, storedFileName);
        var temporaryPath = path + ".uploading";
        try
        {
            await using (var output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, true))
            {
                await buffered.CopyToAsync(output, cancellationToken);
                await output.FlushAsync(cancellationToken);
            }
            File.Move(temporaryPath, path);
            return new StoredImage($"/media/{tenantId:D}/{area}/{storedFileName}", storedFileName);
        }
        catch
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
            throw;
        }
    }

    public Task DeleteAsync(Guid tenantId, string url, CancellationToken cancellationToken = default)
    {
        return DeleteFromAreaAsync(tenantId, "menu-items", url);
    }

    public Task DeleteBrandingAsync(Guid tenantId, string url, CancellationToken cancellationToken = default)
    {
        return DeleteFromAreaAsync(tenantId, "branding", url);
    }

    private Task DeleteFromAreaAsync(Guid tenantId, string area, string url)
    {
        if (!TryGetFileName(tenantId, area, url, out var fileName))
            return Task.CompletedTask;

        var path = Path.Combine(GetTenantPath(tenantId, area), fileName);
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }

    public Task<Stream?> OpenReadAsync(
        Guid tenantId,
        string url,
        CancellationToken cancellationToken = default)
    {
        return OpenReadFromAreaAsync(tenantId, "menu-items", url);
    }

    public Task<Stream?> OpenBrandingReadAsync(
        Guid tenantId,
        string url,
        CancellationToken cancellationToken = default)
    {
        return OpenReadFromAreaAsync(tenantId, "branding", url);
    }

    private Task<Stream?> OpenReadFromAreaAsync(Guid tenantId, string area, string url)
    {
        if (!TryGetFileName(tenantId, area, url, out var fileName))
            return Task.FromResult<Stream?>(null);

        var path = Path.Combine(GetTenantPath(tenantId, area), fileName);
        if (!File.Exists(path))
            return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, true);
        return Task.FromResult<Stream?>(stream);
    }

    private string GetTenantPath(Guid tenantId, string area) => Path.Combine(_rootPath, tenantId.ToString("D"), area);

    private bool TryGetFileName(Guid tenantId, string area, string url, out string fileName)
    {
        fileName = Path.GetFileName(url);
        return tenantId != Guid.Empty
            && !string.IsNullOrWhiteSpace(fileName)
            && string.Equals(url, $"/media/{tenantId:D}/{area}/{fileName}", StringComparison.OrdinalIgnoreCase)
            && string.Equals(fileName, Path.GetFileName(url), StringComparison.Ordinal)
            && !fileName.Contains('\\');
    }

}

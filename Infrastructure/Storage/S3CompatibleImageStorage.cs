using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using RestaurantMenuPlatform.Application.Interfaces;

namespace RestaurantMenuPlatform.Infrastructure.Storage;

/// <summary>
/// S3-compatible object storage adapter. The application only persists and returns
/// its internal media URL; bucket keys and credentials never leave this adapter.
/// </summary>
public sealed class S3CompatibleImageStorage : IImageStorage
{
    private const string ServiceName = "s3";
    private readonly HttpClient _httpClient;
    private readonly Uri _endpoint;
    private readonly string _bucket;
    private readonly string _region;
    private readonly string _accessKey;
    private readonly string _secretKey;
    private readonly bool _usePathStyle;
    private readonly long? _maxBytes;

    public S3CompatibleImageStorage(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _endpoint = new Uri(Required(configuration["Storage:Endpoint"], "Storage:Endpoint"));
        _bucket = Required(configuration["Storage:Bucket"], "Storage:Bucket");
        _region = Required(configuration["Storage:Region"], "Storage:Region");
        _accessKey = Required(configuration["Storage:AccessKey"], "Storage:AccessKey");
        _secretKey = Required(configuration["Storage:SecretKey"], "Storage:SecretKey");
        _usePathStyle = !bool.TryParse(configuration["Storage:UsePathStyle"], out var usePathStyle) || usePathStyle;
        _maxBytes = long.TryParse(configuration["Storage:MaxUploadBytes"], out var maxBytes) && maxBytes > 0
            ? maxBytes
            : null;
        if (!_usePathStyle)
            throw new InvalidOperationException("S3-compatible storage currently requires path-style addressing.");
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
        var key = GetKey(tenantId, area, storedFileName);
        var payload = buffered.ToArray();
        using var request = CreateSignedRequest(HttpMethod.Put, key, payload);
        request.Content = new ByteArrayContent(payload);
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        EnsureSuccess(response);
        return new StoredImage($"/media/{tenantId:D}/{area}/{storedFileName}", key);
    }

    public async Task DeleteAsync(Guid tenantId, string url, CancellationToken cancellationToken = default)
    {
        await DeleteFromAreaAsync(tenantId, "menu-items", url, cancellationToken);
    }

    public async Task DeleteBrandingAsync(Guid tenantId, string url, CancellationToken cancellationToken = default)
    {
        await DeleteFromAreaAsync(tenantId, "branding", url, cancellationToken);
    }

    private async Task DeleteFromAreaAsync(Guid tenantId, string area, string url, CancellationToken cancellationToken)
    {
        if (!TryGetKey(tenantId, area, url, out var key))
            return;

        using var request = CreateSignedRequest(HttpMethod.Delete, key, null);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return;
        EnsureSuccess(response);
    }

    public async Task<Stream?> OpenReadAsync(
        Guid tenantId,
        string url,
        CancellationToken cancellationToken = default)
    {
        return await OpenReadFromAreaAsync(tenantId, "menu-items", url, cancellationToken);
    }

    public async Task<Stream?> OpenBrandingReadAsync(
        Guid tenantId,
        string url,
        CancellationToken cancellationToken = default)
    {
        return await OpenReadFromAreaAsync(tenantId, "branding", url, cancellationToken);
    }

    private async Task<Stream?> OpenReadFromAreaAsync(Guid tenantId, string area, string url, CancellationToken cancellationToken)
    {
        if (!TryGetKey(tenantId, area, url, out var key))
            return null;

        using var request = CreateSignedRequest(HttpMethod.Get, key, null);
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            response.Dispose();
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            response.Dispose();
            throw new InvalidOperationException("Object storage could not serve the requested image.");
        }

        return new ResponseStream(response);
    }

    private HttpRequestMessage CreateSignedRequest(HttpMethod method, string key, byte[]? payload)
    {
        var uri = BuildObjectUri(key);
        var now = DateTimeOffset.UtcNow;
        var amzDate = now.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture);
        var date = now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var payloadHash = Convert.ToHexString(SHA256.HashData(payload ?? Array.Empty<byte>())).ToLowerInvariant();
        var host = uri.IsDefaultPort ? uri.Host : $"{uri.Host}:{uri.Port}";
        const string signedHeaders = "host;x-amz-content-sha256;x-amz-date";
        var canonicalHeaders = $"host:{host}\n" +
                               $"x-amz-content-sha256:{payloadHash}\n" +
                               $"x-amz-date:{amzDate}\n";
        var canonicalRequest = string.Join('\n',
            method.Method,
            uri.AbsolutePath,
            uri.Query.TrimStart('?'),
            canonicalHeaders,
            signedHeaders,
            payloadHash);
        var credentialScope = $"{date}/{_region}/{ServiceName}/aws4_request";
        var stringToSign = $"AWS4-HMAC-SHA256\n{amzDate}\n{credentialScope}\n" +
                           Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest))).ToLowerInvariant();
        var signingKey = DeriveSigningKey(date);
        var signature = Convert.ToHexString(HMACSHA256.HashData(signingKey, Encoding.UTF8.GetBytes(stringToSign))).ToLowerInvariant();
        var authorization = $"AWS4-HMAC-SHA256 Credential={_accessKey}/{credentialScope}, " +
                            $"SignedHeaders={signedHeaders}, Signature={signature}";

        var request = new HttpRequestMessage(method, uri);
        request.Headers.Host = host;
        request.Headers.TryAddWithoutValidation("x-amz-date", amzDate);
        request.Headers.TryAddWithoutValidation("x-amz-content-sha256", payloadHash);
        request.Headers.TryAddWithoutValidation("Authorization", authorization);
        return request;
    }

    private Uri BuildObjectUri(string key)
    {
        var escapedKey = string.Join('/', key.Split('/').Select(Uri.EscapeDataString));
        var baseUri = _endpoint.AbsoluteUri.TrimEnd('/');
        return new Uri($"{baseUri}/{Uri.EscapeDataString(_bucket)}/{escapedKey}");
    }

    private bool TryGetKey(Guid tenantId, string area, string url, out string key)
    {
        key = string.Empty;
        var prefix = $"/media/{tenantId:D}/{area}/";
        if (tenantId == Guid.Empty ||
            string.IsNullOrWhiteSpace(url) ||
            !url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var fileName = url[prefix.Length..];
        if (string.IsNullOrWhiteSpace(fileName) ||
            string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal) == false ||
            fileName.Contains('\\'))
            return false;

        key = GetKey(tenantId, area, fileName);
        return true;
    }

    private static string GetKey(Guid tenantId, string area, string fileName) =>
        $"tenants/{tenantId:D}/{area}/{fileName}";

    private byte[] DeriveSigningKey(string date)
    {
        var dateKey = HMACSHA256.HashData(Encoding.UTF8.GetBytes("AWS4" + _secretKey), Encoding.UTF8.GetBytes(date));
        var regionKey = HMACSHA256.HashData(dateKey, Encoding.UTF8.GetBytes(_region));
        var serviceKey = HMACSHA256.HashData(regionKey, Encoding.UTF8.GetBytes(ServiceName));
        return HMACSHA256.HashData(serviceKey, Encoding.UTF8.GetBytes("aws4_request"));
    }

    private static void EnsureSuccess(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Object storage request failed.");
    }

    private static string Required(string? value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{name} must be supplied through external configuration.")
            : value.Trim();

    private sealed class ResponseStream : Stream
    {
        private readonly HttpResponseMessage _response;
        private readonly Stream _inner;

        public ResponseStream(HttpResponseMessage response)
        {
            _response = response;
            _inner = response.Content.ReadAsStream();
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => _inner.Position = value; }
        public override void Flush() => _inner.Flush();
        public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override int Read(Span<byte> buffer) => _inner.Read(buffer);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _inner.ReadAsync(buffer, offset, count, cancellationToken);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => _inner.ReadAsync(buffer, cancellationToken);
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
                _response.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

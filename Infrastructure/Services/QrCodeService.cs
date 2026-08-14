using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Infrastructure.Persistence;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class QrCodeService : IQrCodeService
{
    private const string BranchMenuTargetType = "branch-menu";
    private readonly AppDbContext _db;

    public QrCodeService(AppDbContext db)
    {
        _db = db;
    }

    [Obsolete("Use GetOrCreateForTableAsync; operational QR codes must be bound to a RestaurantTable.")]
    public Task<QrCodeDto?> GetOrCreateAsync(
        Guid branchId,
        string? tableLabel,
        string baseUrl,
        CancellationToken cancellationToken = default)
    {
        // Kept only for binary compatibility with the original interface. The
        // branch-level QR flow was retired: never create or return a QR without
        // an authoritative RestaurantTable relationship.
        return Task.FromResult<QrCodeDto?>(null);
    }

    public async Task<IReadOnlyList<QrCodeDto>> GetForBranchAsync(
        Guid branchId,
        string baseUrl,
        CancellationToken cancellationToken = default)
    {
        var branch = await _db.Branches
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == branchId && x.IsActive, cancellationToken);
        if (branch is null)
            return [];

        var tenant = await _db.Tenants
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == branch.TenantId, cancellationToken);
        if (tenant is null)
            return [];

        var codes = await _db.QrCodes
            .AsNoTracking()
            .Include(x => x.Table)
            // Operational QR codes are always table-bound. Legacy branch-level
            // rows remain in SQL for history, but must never be issued, shown,
            // printed, or treated as a customer entry point.
            .Where(x => x.BranchId == branchId && x.TableId != null && x.TargetType == BranchMenuTargetType && x.Branch.IsActive)
            .OrderBy(x => x.TableLabel == null ? 0 : 1)
            .ThenBy(x => x.TableLabel)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return codes.Select(x => ToDto(x, branch.Name, branch.NameAr, branch.Slug, tenant.Name, tenant.Slug, baseUrl)).ToList();
    }

    public async Task<QrCodeDto?> GetAsync(
        Guid id,
        string baseUrl,
        CancellationToken cancellationToken = default)
    {
        var qr = await _db.QrCodes
            .AsNoTracking()
            .Include(x => x.Branch)
            .Include(x => x.Table)
            .SingleOrDefaultAsync(x => x.Id == id && x.TableId != null && x.TargetType == BranchMenuTargetType, cancellationToken);
        if (qr is null)
            return null;

        var tenant = await _db.Tenants
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == qr.TenantId && x.IsActive, cancellationToken);
        return tenant is null
            ? null
            : ToDto(qr, qr.Branch.Name, qr.Branch.NameAr, qr.Branch.Slug, tenant.Name, tenant.Slug, baseUrl);
    }

    [Obsolete("Use GetOrCreateForTableAsync; operational QR codes must be bound to a RestaurantTable.")]
    public Task<IReadOnlyList<QrCodeDto>> GetOrCreateBatchAsync(
        Guid branchId,
        IReadOnlyList<string> tableLabels,
        string baseUrl,
        CancellationToken cancellationToken = default)
    {
        // Kept only for binary compatibility with the original interface. Do
        // not recreate the unsafe label-only QR system.
        return Task.FromResult<IReadOnlyList<QrCodeDto>>([]);
    }

    public async Task<QrCodeDto?> GetOrCreateForTableAsync(
        Guid tableId,
        string baseUrl,
        CancellationToken cancellationToken = default)
    {
        var table = await _db.RestaurantTables
            .Include(x => x.Branch)
            .SingleOrDefaultAsync(x => x.Id == tableId, cancellationToken);
        if (table is null || !table.IsActive || !table.Branch.IsActive)
            return null;

        var tenant = await _db.Tenants.AsNoTracking().SingleOrDefaultAsync(x => x.Id == table.TenantId && x.IsActive, cancellationToken);
        if (tenant is null)
            return null;

        var qr = await _db.QrCodes
            .Include(x => x.Table)
            .Where(x => x.TenantId == table.TenantId &&
                        x.BranchId == table.BranchId &&
                        x.TableId == table.Id &&
                        x.TargetType == BranchMenuTargetType)
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        if (qr is null)
        {
            qr = new QrCode
            {
                TenantId = table.TenantId,
                BranchId = table.BranchId,
                TableId = table.Id,
                Code = $"qr-{Guid.NewGuid():N}",
                TargetType = BranchMenuTargetType,
                IsActive = true
            };
            _db.QrCodes.Add(qr);
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                qr.Table = table;
            }
            catch (DbUpdateException)
            {
                _db.Entry(qr).State = EntityState.Detached;
                qr = await _db.QrCodes
                    .Include(x => x.Table)
                    .Where(x => x.TableId == table.Id && x.TargetType == BranchMenuTargetType)
                    .OrderByDescending(x => x.IsActive)
                    .ThenByDescending(x => x.CreatedAtUtc)
                    .FirstOrDefaultAsync(cancellationToken);
                if (qr is null)
                    throw;
            }
        }

        return ToDto(qr, table.Branch.Name, table.Branch.NameAr, table.Branch.Slug, tenant.Name, tenant.Slug, baseUrl);
    }

    public async Task<PublicOrderingContextDto?> ResolvePublicContextAsync(
        string restaurantSlug,
        string branchSlug,
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var qr = await _db.QrCodes
            .AsNoTracking()
            .Include(x => x.Branch).ThenInclude(x => x.Tenant)
            .Include(x => x.Table)
            .SingleOrDefaultAsync(x => x.Code == code.Trim() && x.TargetType == BranchMenuTargetType && x.IsActive, cancellationToken);
        if (qr?.Table is null || qr.Branch is null || qr.Branch.Tenant is null ||
            !qr.Branch.IsActive || !qr.Branch.Tenant.IsActive || !qr.Table.IsActive || qr.Table.BranchId != qr.BranchId ||
            !string.Equals(qr.Branch.Tenant.Slug, restaurantSlug, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(qr.Branch.Slug, branchSlug, StringComparison.OrdinalIgnoreCase))
            return null;

        return new PublicOrderingContextDto(
            qr.TenantId,
            qr.Branch.Tenant.Name,
            qr.Branch.Tenant.Slug,
            qr.BranchId,
            qr.Branch.Name,
            qr.Branch.Slug,
            qr.Table.Id,
            qr.Table.Name,
            qr.Table.NameAr,
            qr.Id,
            qr.Code);
    }

    public async Task<QrCodeAssetDto?> RenderAsync(
        Guid id,
        string baseUrl,
        string format,
        CancellationToken cancellationToken = default)
    {
        var qr = await _db.QrCodes
            .AsNoTracking()
            .Include(x => x.Branch)
            .SingleOrDefaultAsync(x => x.Id == id && x.TableId != null && x.TargetType == BranchMenuTargetType, cancellationToken);
        if (qr is null)
            return null;

        var tenant = await _db.Tenants
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == qr.TenantId, cancellationToken);
        if (tenant is null)
            return null;

        var targetUrl = BuildTargetUrl(baseUrl, tenant.Slug, qr.Branch.Slug, qr.Code);
        var normalizedFormat = format.Trim().ToLowerInvariant();
        if (normalizedFormat is not ("svg" or "png"))
            throw new ArgumentException("QR format must be SVG or PNG.");

        var writer = new BarcodeWriterGeneric
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new EncodingOptions
            {
                Width = 640,
                Height = 640,
                Margin = 2,
                PureBarcode = true
            }
        };
        var matrix = writer.Encode(targetUrl);

        if (normalizedFormat == "svg")
        {
            var svg = new SvgRenderer()
                .Render(matrix, BarcodeFormat.QR_CODE, targetUrl)
                .Content;
            return new QrCodeAssetDto(
                Encoding.UTF8.GetBytes(svg),
                "image/svg+xml",
                $"menu-qr-{qr.Code}.svg");
        }

        return new QrCodeAssetDto(
            PngEncoder.Encode(matrix),
            "image/png",
            $"menu-qr-{qr.Code}.png");
    }

    public Task<bool> IsActiveAsync(
        string code,
        Guid branchId,
        CancellationToken cancellationToken = default) =>
        !string.IsNullOrWhiteSpace(code)
            ? _db.QrCodes.AsNoTracking().AnyAsync(
                x => x.Code == code &&
                     x.BranchId == branchId &&
                     x.TargetType == BranchMenuTargetType &&
                     x.Branch.IsActive &&
                     x.IsActive,
                cancellationToken)
            : Task.FromResult(false);

    public async Task<bool> SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var qr = await _db.QrCodes
            .Include(x => x.Branch)
            .Include(x => x.Table)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (qr is null || qr.TableId is null || qr.Table is null || !qr.Branch.IsActive)
            return false;

        if (qr.IsActive == isActive)
            return true;

        if (isActive && (qr.Table is null || qr.Table.TenantId != qr.TenantId || qr.Table.BranchId != qr.BranchId || !qr.Table.IsActive))
            return false;

        qr.IsActive = isActive;
        qr.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static QrCodeDto ToDto(
        QrCode qr,
        string branchName,
        string? branchNameAr,
        string branchSlug,
        string restaurantName,
        string tenantSlug,
        string baseUrl) =>
        new(
            qr.Id,
            qr.BranchId,
            branchName,
            qr.TableLabel,
            qr.Code,
            BuildTargetUrl(baseUrl, tenantSlug, branchSlug, qr.Code),
            qr.IsActive,
            branchNameAr,
            qr.TableId,
            qr.Table?.Name,
            qr.Table?.NameAr,
            restaurantName,
            qr.CreatedAtUtc,
            qr.UpdatedAtUtc);

    private static string BuildTargetUrl(string baseUrl, string tenantSlug, string branchSlug, string code) =>
        $"{baseUrl.TrimEnd('/')}/menu/{tenantSlug}/{branchSlug}?source=qr&code={Uri.EscapeDataString(code)}";

    private static class PngEncoder
    {
        private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];

        public static byte[] Encode(BitMatrix matrix)
        {
            using var raw = new MemoryStream();
            for (var y = 0; y < matrix.Height; y++)
            {
                raw.WriteByte(0);
                for (var x = 0; x < matrix.Width; x++)
                    raw.WriteByte(matrix[x, y] ? (byte)0 : (byte)255);
            }

            using var compressed = new MemoryStream();
            using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
            {
                raw.Position = 0;
                raw.CopyTo(zlib);
            }

            using var output = new MemoryStream();
            output.Write(Signature);
            WriteChunk(output, "IHDR", BuildHeader(matrix.Width, matrix.Height));
            WriteChunk(output, "IDAT", compressed.ToArray());
            WriteChunk(output, "IEND", []);
            return output.ToArray();
        }

        private static byte[] BuildHeader(int width, int height)
        {
            var header = new byte[13];
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(0, 4), (uint)width);
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4, 4), (uint)height);
            header[8] = 8;
            header[9] = 0;
            header[10] = 0;
            header[11] = 0;
            header[12] = 0;
            return header;
        }

        private static void WriteChunk(Stream stream, string type, byte[] data)
        {
            var typeBytes = Encoding.ASCII.GetBytes(type);
            var length = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(length, (uint)data.Length);
            stream.Write(length);
            stream.Write(typeBytes);
            stream.Write(data);

            var crcInput = new byte[typeBytes.Length + data.Length];
            typeBytes.CopyTo(crcInput, 0);
            data.CopyTo(crcInput, typeBytes.Length);
            var crc = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(crc, Crc32(crcInput));
            stream.Write(crc);
        }

        private static uint Crc32(byte[] bytes)
        {
            var crc = 0xFFFFFFFFu;
            foreach (var value in bytes)
            {
                crc ^= value;
                for (var bit = 0; bit < 8; bit++)
                    crc = (crc & 1) == 1 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
            }
            return ~crc;
        }
    }
}

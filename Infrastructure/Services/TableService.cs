using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class TableService : ITableService
{
    private readonly AppDbContext _db;

    public TableService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<RestaurantTableDto>> GetForBranchAsync(Guid branchId, CancellationToken cancellationToken = default) =>
        (await Query().Where(x => x.BranchId == branchId).OrderBy(x => x.Name).ToListAsync(cancellationToken)).Select(ToDto).ToList();

    public async Task<RestaurantTableDto?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await Query().SingleOrDefaultAsync(x => x.Id == id, cancellationToken)) is { } table ? ToDto(table) : null;

    public async Task<RestaurantTableDto?> CreateAsync(Guid branchId, RestaurantTableInput input, CancellationToken cancellationToken = default)
    {
        var branch = await _db.Branches.SingleOrDefaultAsync(x => x.Id == branchId, cancellationToken);
        if (branch is null)
            return null;

        var name = Normalize(input.Name);
        if (name is null)
            throw new ArgumentException("A table name is required.");
        if (await _db.RestaurantTables.AnyAsync(x => x.BranchId == branchId && x.Name == name, cancellationToken))
            throw new ArgumentException("A table with this name already exists in the branch.");

        var table = new RestaurantTable
        {
            TenantId = branch.TenantId,
            BranchId = branch.Id,
            Name = name,
            NameAr = Normalize(input.NameAr),
            IsActive = true
        };
        _db.RestaurantTables.Add(table);
        await _db.SaveChangesAsync(cancellationToken);
        await _db.Entry(table).Reference(x => x.Branch).LoadAsync(cancellationToken);
        return ToDto(table);
    }

    public async Task<RestaurantTableDto?> UpdateAsync(Guid id, RestaurantTableInput input, CancellationToken cancellationToken = default)
    {
        var table = await _db.RestaurantTables.Include(x => x.Branch).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (table is null)
            return null;
        var name = Normalize(input.Name);
        if (name is null)
            throw new ArgumentException("A table name is required.");
        if (await _db.RestaurantTables.AnyAsync(x => x.BranchId == table.BranchId && x.Id != id && x.Name == name, cancellationToken))
            throw new ArgumentException("A table with this name already exists in the branch.");

        table.Name = name;
        table.NameAr = Normalize(input.NameAr);
        table.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(table);
    }

    public async Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var table = await _db.RestaurantTables.Include(x => x.Branch).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (table is null || !table.Branch.IsActive && isActive)
            return false;
        table.IsActive = isActive;
        table.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private IQueryable<RestaurantTable> Query() => _db.RestaurantTables.AsNoTracking().Include(x => x.Branch).Include(x => x.QrCodes);

    private static RestaurantTableDto ToDto(RestaurantTable table)
    {
        var qr = table.QrCodes.FirstOrDefault(x => x.IsActive && x.TargetType == "branch-menu");
        return new(table.Id, table.BranchId, table.Branch?.Name ?? string.Empty, table.Name, table.NameAr, table.IsActive, qr is not null, qr?.Code);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

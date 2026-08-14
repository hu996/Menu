using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Enums;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Identity;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class UserManagementService : IUserManagementService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly PasswordService _passwordService;
    private readonly IEntitlementService _entitlementService;
    private readonly IAuditLogService? _auditLogService;
    private readonly ICurrentUserContext _currentUser;

    public UserManagementService(
        AppDbContext db,
        ITenantContext tenantContext,
        PasswordService passwordService,
        IEntitlementService entitlementService,
        IAuditLogService? auditLogService = null,
        ICurrentUserContext? currentUser = null)
    {
        _db = db;
        _tenantContext = tenantContext;
        _passwordService = passwordService;
        _entitlementService = entitlementService;
        _auditLogService = auditLogService;
        _currentUser = currentUser ?? throw new InvalidOperationException("Current user context is required.");
    }

    public async Task<UserMembershipPageDto> GetPageAsync(
        string? search,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 10, 100);
        var query = _db.Memberships.AsNoTracking().Include(x => x.User).Include(x => x.Branch).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.User.DisplayName.Contains(term) || x.User.Email.Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(x => x.User.DisplayName)
            .ThenBy(x => x.User.Email)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(x => new UserMembershipDto(
                x.Id,
                x.UserId,
                x.User.DisplayName,
                x.User.Email,
                x.Role,
                x.BranchId,
                x.Branch == null ? null : x.Branch.Name,
                x.IsActive,
                x.User.LastLoginAtUtc,
                _db.UserPermissions.Count(permission => permission.MembershipId == x.Id),
                null))
            .ToListAsync(cancellationToken);
        return new UserMembershipPageDto(rows, safePage, safePageSize, total, search?.Trim());
    }

    public async Task<IReadOnlyList<BranchDto>> GetBranchesAsync(CancellationToken cancellationToken = default) =>
        await _db.Branches
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new BranchDto(x.Id, x.Name, x.Slug, x.Address, x.Phone, x.IsActive, x.NameAr, x.Latitude, x.Longitude, x.OpeningHours, x.BrandPrimaryColorOverride, x.BrandAccentColorOverride))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PermissionOptionDto>> GetPermissionOptionsAsync(
        CancellationToken cancellationToken = default) =>
        await _db.PermissionDefinitions
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.GroupCode)
            .ThenBy(x => x.SortOrder)
            .Select(x => new PermissionOptionDto(x.Code, x.GroupCode, x.NameEn, x.NameAr, x.SortOrder))
            .ToListAsync(cancellationToken);

    public async Task<UserMembershipDto?> GetAsync(
        Guid membershipId,
        CancellationToken cancellationToken = default)
    {
        var membership = await _db.Memberships
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Branch)
            .SingleOrDefaultAsync(x => x.Id == membershipId, cancellationToken);
        if (membership is null)
            return null;

        var permissionCodes = await _db.UserPermissions
            .AsNoTracking()
            .Where(x => x.MembershipId == membership.Id)
            .OrderBy(x => x.PermissionCode)
            .Select(x => x.PermissionCode)
            .ToListAsync(cancellationToken);
        return ToDto(membership, permissionCodes);
    }

    public async Task<UserMembershipDto> CreateAsync(
        UserMembershipInput input,
        CancellationToken cancellationToken = default)
    {
        var displayName = input.DisplayName?.Trim() ?? string.Empty;
        var email = input.Email?.Trim() ?? string.Empty;
        var normalizedEmail = email.ToUpperInvariant();
        if (displayName.Length is < 2 or > 120 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
            throw new ArgumentException("A valid display name and email address are required.");
        PasswordService.ValidateStrength(input.Password);
        if (!Enum.IsDefined(input.Role) || input.Role == MembershipRole.PlatformAdmin)
            throw new ArgumentException("The selected role is not available for tenant users.");
        if (input.Role is MembershipRole.BranchManager or MembershipRole.Kitchen or MembershipRole.Waiter && !input.BranchId.HasValue)
            throw new ArgumentException("Branch-scoped staff must be assigned to a branch.");
        if (input.Role is MembershipRole.PlatformAdmin or MembershipRole.TenantOwner or MembershipRole.TenantAdmin && input.BranchId.HasValue)
            throw new ArgumentException("Tenant-wide administration roles cannot be limited to a branch.");
        if (_currentUser.Role == MembershipRole.TenantAdmin &&
            input.Role is MembershipRole.TenantOwner or MembershipRole.TenantAdmin)
            throw new UnauthorizedAccessException("Tenant admins cannot grant tenant-wide administrator roles.");
        if (input.BranchId.HasValue && !await _db.Branches.AnyAsync(x => x.Id == input.BranchId.Value && x.IsActive, cancellationToken))
            throw new ArgumentException("The selected branch is not available in this tenant.");

        await _entitlementService.EnsureCanAddUserAsync(cancellationToken);

        if (await _db.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken))
            throw new InvalidOperationException("That email address is already registered.");

        var permissionCodes = await ValidatePermissionCodesAsync(input.PermissionCodes, input.Role, cancellationToken);
        var tenantId = RequireTenant();

        var user = new User
        {
            Email = email,
            NormalizedEmail = normalizedEmail,
            DisplayName = displayName,
            PasswordHash = _passwordService.Hash(input.Password)
        };
        var membership = new Membership
        {
            TenantId = tenantId,
            User = user,
            Role = input.Role,
            BranchId = input.BranchId,
            IsActive = true
        };
        _db.Users.Add(user);
        _db.Memberships.Add(membership);
        foreach (var permissionCode in permissionCodes)
        {
            _db.UserPermissions.Add(new UserPermission
            {
                TenantId = tenantId,
                Membership = membership,
                PermissionCode = permissionCode
            });
        }
        await _db.SaveChangesAsync(cancellationToken);
        await (_auditLogService?.WriteAsync(
            "user.created",
            "Membership",
            membership.Id,
            null,
            new { membership.UserId, membership.Role, membership.BranchId, Permissions = permissionCodes },
            cancellationToken) ?? Task.CompletedTask);
        await (_auditLogService?.WriteAsync(
            "role.changed",
            "Membership",
            membership.Id,
            null,
            new { membership.UserId, membership.Role, membership.BranchId },
            cancellationToken) ?? Task.CompletedTask);
        return ToDto(membership, permissionCodes);
    }

    public async Task<UserMembershipDto?> UpdateAsync(
        Guid membershipId,
        UserMembershipUpdateInput input,
        CancellationToken cancellationToken = default)
    {
        var membership = await _db.Memberships
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.Id == membershipId, cancellationToken);
        if (membership is null)
            return null;

        var displayName = input.DisplayName?.Trim() ?? string.Empty;
        var email = input.Email?.Trim() ?? string.Empty;
        var normalizedEmail = email.ToUpperInvariant();
        if (displayName.Length is < 2 or > 120 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
            throw new ArgumentException("A valid display name and email address are required.");
        if (!Enum.IsDefined(input.Role) || input.Role == MembershipRole.PlatformAdmin)
            throw new ArgumentException("The selected role is not available for tenant users.");
        ValidateBranchScope(input.Role, input.BranchId);
        if (_currentUser.Role == MembershipRole.TenantAdmin &&
            input.Role is MembershipRole.TenantOwner or MembershipRole.TenantAdmin && membership.Role != input.Role)
            throw new UnauthorizedAccessException("Tenant admins cannot grant tenant-wide administrator roles.");
        if (input.BranchId.HasValue && !await _db.Branches.AnyAsync(x => x.Id == input.BranchId.Value && x.IsActive, cancellationToken))
            throw new ArgumentException("The selected branch is not available in this tenant.");
        if (await _db.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail && x.Id != membership.UserId, cancellationToken))
            throw new InvalidOperationException("That email address is already registered.");

        var permissionCodes = await ValidatePermissionCodesAsync(input.PermissionCodes, input.Role, cancellationToken);
        var oldPermissionCodes = await _db.UserPermissions
            .Where(x => x.MembershipId == membership.Id)
            .Select(x => x.PermissionCode)
            .ToListAsync(cancellationToken);
        var oldRole = membership.Role;
        var oldBranchId = membership.BranchId;
        var roleChanged = membership.Role != input.Role;
        var branchChanged = membership.BranchId != input.BranchId;
        var identityChanged = !string.Equals(membership.User.Email, email, StringComparison.Ordinal) ||
            !string.Equals(membership.User.DisplayName, displayName, StringComparison.Ordinal);
        var permissionsChanged = !oldPermissionCodes.OrderBy(x => x).SequenceEqual(permissionCodes.OrderBy(x => x), StringComparer.OrdinalIgnoreCase);

        membership.User.DisplayName = displayName;
        membership.User.Email = email;
        membership.User.NormalizedEmail = normalizedEmail;
        membership.Role = input.Role;
        membership.BranchId = input.BranchId;
        if (roleChanged || branchChanged || permissionsChanged || identityChanged)
            membership.User.SecurityStamp = Guid.NewGuid().ToString("N");
        membership.UpdatedAtUtc = DateTime.UtcNow;

        if (permissionsChanged)
        {
            _db.UserPermissions.RemoveRange(_db.UserPermissions.Where(x => x.MembershipId == membership.Id));
            foreach (var permissionCode in permissionCodes)
            {
                _db.UserPermissions.Add(new UserPermission
                {
                    TenantId = membership.TenantId,
                    MembershipId = membership.Id,
                    PermissionCode = permissionCode
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        if (roleChanged)
            await (_auditLogService?.WriteAsync("role.changed", "Membership", membership.Id, new { Role = oldRole }, new { Role = input.Role }, cancellationToken) ?? Task.CompletedTask);
        if (branchChanged)
            await (_auditLogService?.WriteAsync("membership.branch-scope-changed", "Membership", membership.Id, new { BranchId = oldBranchId }, new { BranchId = input.BranchId }, cancellationToken) ?? Task.CompletedTask);
        if (permissionsChanged)
            await (_auditLogService?.WriteAsync("permissions.changed", "Membership", membership.Id, new { Permissions = oldPermissionCodes }, new { Permissions = permissionCodes }, cancellationToken) ?? Task.CompletedTask);
        if (identityChanged)
            await (_auditLogService?.WriteAsync("user.updated", "User", membership.UserId, null, new { membership.User.DisplayName, membership.User.Email }, cancellationToken) ?? Task.CompletedTask);

        return ToDto(membership, permissionCodes);
    }

    public async Task<bool> SetActiveAsync(Guid membershipId, bool isActive, CancellationToken cancellationToken = default)
    {
        var membership = await _db.Memberships.SingleOrDefaultAsync(x => x.Id == membershipId, cancellationToken);
        if (membership is null)
            return false;
        if (membership.IsActive == isActive)
            return true;
        if (membership.UserId == _currentUser.UserId)
            throw new InvalidOperationException("You cannot deactivate your own membership.");
        if (membership.Role == MembershipRole.TenantOwner && !isActive &&
            await _db.Memberships.CountAsync(x => x.Role == MembershipRole.TenantOwner && x.IsActive, cancellationToken) <= 1)
            throw new InvalidOperationException("The tenant must retain an active owner.");
        if (_currentUser.Role == MembershipRole.TenantAdmin &&
            membership.Role is MembershipRole.TenantOwner or MembershipRole.TenantAdmin)
            throw new UnauthorizedAccessException("Tenant admins cannot change tenant-wide administrator memberships.");
        membership.IsActive = isActive;
        membership.User.SecurityStamp = Guid.NewGuid().ToString("N");
        membership.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await (_auditLogService?.WriteAsync(
            "user-membership.status-changed",
            "Membership",
            membership.Id,
            new { IsActive = !isActive },
            new { membership.IsActive },
            cancellationToken) ?? Task.CompletedTask);
        return true;
    }

    private Guid RequireTenant() => _tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context is required.");

    private static void ValidateBranchScope(MembershipRole role, Guid? branchId)
    {
        if (role is MembershipRole.BranchManager or MembershipRole.Kitchen or MembershipRole.Waiter && !branchId.HasValue)
            throw new ArgumentException("Branch-scoped staff must be assigned to a branch.");
        if (role is MembershipRole.TenantOwner or MembershipRole.TenantAdmin && branchId.HasValue)
            throw new ArgumentException("Tenant-wide administration roles cannot be limited to a branch.");
    }

    private async Task<IReadOnlyList<string>> ValidatePermissionCodesAsync(
        IReadOnlyList<string>? requestedCodes,
        MembershipRole role,
        CancellationToken cancellationToken)
    {
        var permissionCodes = requestedCodes is null
            ? PermissionCatalog.Preset(role).ToArray()
            : requestedCodes
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Select(code => code.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        var activeCodes = await _db.PermissionDefinitions
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Select(x => x.Code)
            .ToListAsync(cancellationToken);
        var invalid = permissionCodes
            .Where(code => !activeCodes.Contains(code, StringComparer.OrdinalIgnoreCase))
            .ToArray();
        if (invalid.Length > 0)
            throw new ArgumentException($"Unknown or inactive permissions: {string.Join(", ", invalid)}");
        return permissionCodes;
    }

    private static UserMembershipDto ToDto(Membership membership, IReadOnlyList<string> permissionCodes) =>
        new(
            membership.Id,
            membership.UserId,
            membership.User.DisplayName,
            membership.User.Email,
            membership.Role,
            membership.BranchId,
            membership.Branch?.Name,
            membership.IsActive,
            membership.User.LastLoginAtUtc,
            permissionCodes.Count,
            permissionCodes);
}

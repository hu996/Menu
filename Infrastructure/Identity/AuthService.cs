using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Domain.Enums;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Identity;

internal sealed class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly PasswordService _passwordService;
    private readonly IAuditLogService? _auditLogService;

    public AuthService(
        AppDbContext db,
        ITenantContext tenantContext,
        PasswordService passwordService,
        IAuditLogService? auditLogService = null)
    {
        _db = db;
        _tenantContext = tenantContext;
        _passwordService = passwordService;
        _auditLogService = auditLogService;
    }

    public async Task<AuthenticationResultDto> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 320 ||
            string.IsNullOrWhiteSpace(password) || password.Length > 128)
            return new(null, "invalid_credentials");

        var normalizedEmail = email.Trim().ToUpperInvariant();
        var user = await _db.Users
            .SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            _passwordService.PerformDummyVerification(password);
            return new(null, "invalid_credentials");
        }
        if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > DateTime.UtcNow)
            return new(null, "invalid_credentials");
        if (!_passwordService.Verify(password, user.PasswordHash, out var needsRehash))
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= 5)
                user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(15);
            await _db.SaveChangesAsync(cancellationToken);
            if (_tenantContext.TenantId.HasValue)
                await (_auditLogService?.WriteAsync(
                "login.failed",
                "User",
                user.Id,
                null,
                new { user.FailedLoginCount, user.LockoutEndUtc },
                cancellationToken) ?? Task.CompletedTask);
            return new(null, "invalid_credentials");
        }

        user.FailedLoginCount = 0;
        user.LockoutEndUtc = null;
        user.LastLoginAtUtc = DateTime.UtcNow;
        if (needsRehash)
            user.PasswordHash = _passwordService.Hash(password);
        await _db.SaveChangesAsync(cancellationToken);
        var memberships = await _db.Memberships
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.UserId == user.Id && x.IsActive)
            .ToListAsync(cancellationToken);
        if (_tenantContext.TenantId is Guid requestedTenantId)
            memberships = memberships.Where(x => x.TenantId == requestedTenantId).ToList();

        if (memberships.Count == 0)
            return new(null, "membership_required");
        if (memberships.Count > 1)
            return new(null, "multiple_memberships");

        var membership = memberships[0];
        _tenantContext.SetTenant(membership.TenantId);

        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == membership.TenantId, cancellationToken);
        if (tenant is null || !tenant.IsActive)
            return new(null, "tenant_inactive");

        var rolePermissions = await _db.RolePermissions
            .AsNoTracking()
            .Where(x => x.Role == membership.Role)
            .Select(x => x.PermissionCode)
            .ToListAsync(cancellationToken);
        var userPermissions = await _db.UserPermissions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == membership.TenantId && x.MembershipId == membership.Id)
            .Select(x => x.PermissionCode)
            .ToListAsync(cancellationToken);
        var permissions = rolePermissions
            .Concat(userPermissions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        await (_auditLogService?.WriteAsync(
            "login.succeeded",
            "User",
            user.Id,
            null,
            new { user.LastLoginAtUtc, membership.TenantId, membership.Id },
            cancellationToken) ?? Task.CompletedTask);

        return new AuthenticationResultDto(new AuthenticatedUserDto(
            user.Id,
            tenant.Id,
            membership.Id,
            user.Email,
            user.DisplayName,
            tenant.Slug,
            membership.Role,
            membership.BranchId,
            user.SecurityStamp,
            permissions,
            tenant.Name), null);
    }

    public async Task<bool> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken);
        if (user is null || !_passwordService.Verify(currentPassword, user.PasswordHash))
            return false;

        PasswordService.ValidateStrength(newPassword);
        user.PasswordHash = _passwordService.Hash(newPassword);
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

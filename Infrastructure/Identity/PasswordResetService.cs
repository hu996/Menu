using System.Security.Cryptography;
using System.Text;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace RestaurantMenuPlatform.Infrastructure.Identity;

internal sealed class PasswordResetService : IPasswordResetService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);
    private readonly AppDbContext _db;
    private readonly PasswordService _passwordService;

    public PasswordResetService(
        AppDbContext db,
        PasswordService passwordService)
    {
        _db = db;
        _passwordService = passwordService;
    }

    public async Task<PasswordResetRequestResult> RequestAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();
        var user = await _db.Users
            .SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail && x.IsActive, cancellationToken);

        // Always return the same shape so the endpoint cannot enumerate users.
        if (user is null)
            return new PasswordResetRequestResult(null);

        var activeTokens = await _db.PasswordResetTokens
            .Where(x => x.UserId == user.Id && x.UsedAtUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var activeToken in activeTokens)
            activeToken.UsedAtUtc = DateTime.UtcNow;

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = Hash(rawToken),
            ExpiresAtUtc = DateTime.UtcNow.Add(TokenLifetime)
        });
        await _db.SaveChangesAsync(cancellationToken);

        // A real email provider can be wired here later. Web decides whether a
        // development-only reset link may be rendered; production remains generic.
        return new PasswordResetRequestResult(rawToken);
    }

    public async Task<bool> ResetAsync(
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        PasswordService.ValidateStrength(newPassword);
        var tokenHash = Hash(token);
        var resetToken = await _db.PasswordResetTokens
            .Include(x => x.User)
            .SingleOrDefaultAsync(x =>
                x.TokenHash == tokenHash &&
                x.UsedAtUtc == null &&
                x.ExpiresAtUtc > DateTime.UtcNow,
                cancellationToken);
        if (resetToken is null || !resetToken.User.IsActive)
            return false;

        resetToken.User.PasswordHash = _passwordService.Hash(newPassword);
        resetToken.User.SecurityStamp = Guid.NewGuid().ToString("N");
        resetToken.User.FailedLoginCount = 0;
        resetToken.User.LockoutEndUtc = null;
        resetToken.UsedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}

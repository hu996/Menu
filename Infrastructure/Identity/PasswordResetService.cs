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
        if (string.IsNullOrWhiteSpace(email) || email.Length > 320)
            return new PasswordResetRequestResult(null);

        var normalizedEmail = email.Trim().ToUpperInvariant();
        var user = await _db.Users
            .SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail && x.IsActive, cancellationToken);

        // Always return the same shape so the endpoint cannot enumerate users.
        if (user is null)
            return new PasswordResetRequestResult(null);

        var now = DateTime.UtcNow;
        var activeTokens = await _db.PasswordResetTokens
            .Where(x => x.UserId == user.Id && x.UsedAtUtc == null)
            .ToListAsync(cancellationToken);

        // Do not let repeated anonymous requests flood a real user's inbox.
        // The endpoint still returns the same generic confirmation response.
        if (activeTokens.Any(x => x.CreatedAtUtc >= now.AddMinutes(-5) && x.ExpiresAtUtc > now))
            return new PasswordResetRequestResult(null);

        foreach (var activeToken in activeTokens)
            activeToken.UsedAtUtc = now;

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = Hash(rawToken),
            ExpiresAtUtc = now.Add(TokenLifetime)
        });
        await _db.SaveChangesAsync(cancellationToken);

        return new PasswordResetRequestResult(rawToken, user.Email, user.DisplayName);
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

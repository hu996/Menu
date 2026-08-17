using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RestaurantMenuPlatform.Domain.Entities;

namespace RestaurantMenuPlatform.Infrastructure.Identity;

public sealed class PasswordService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 120_000;
    private readonly PasswordHasher<User> _identityHasher;
    private readonly string _dummyHash;

    public PasswordService() : this(Options.Create(new PasswordHasherOptions { IterationCount = 210_000 }))
    {
    }

    public PasswordService(IOptions<PasswordHasherOptions> options)
    {
        _identityHasher = new PasswordHasher<User>(options);
        _dummyHash = _identityHasher.HashPassword(new User(), "TimingOnly-Password-9!");
    }

    public static void ValidateStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length is < 10 or > 128 ||
            !password.Any(char.IsUpper) || !password.Any(char.IsLower) ||
            !password.Any(char.IsDigit) || password.All(char.IsLetterOrDigit))
            throw new ArgumentException("Password must be 10 to 128 characters and include uppercase, lowercase, number, and symbol.");
    }

    public string Hash(string password)
    {
        ValidateStrength(password);
        return _identityHasher.HashPassword(new User(), password);
    }

    public bool Verify(string password, string encodedHash)
    {
        return Verify(password, encodedHash, out _);
    }

    public bool Verify(string password, string encodedHash, out bool needsRehash)
    {
        if (!encodedHash.StartsWith("AQAAAA", StringComparison.Ordinal))
        {
            var validLegacy = VerifyLegacy(password, encodedHash);
            needsRehash = validLegacy;
            return validLegacy;
        }

        var result = _identityHasher.VerifyHashedPassword(new User(), encodedHash, password);
        needsRehash = result == PasswordVerificationResult.SuccessRehashNeeded;
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }

    public void PerformDummyVerification(string password) =>
        _identityHasher.VerifyHashedPassword(new User(), _dummyHash, password);

    public bool NeedsRehash(string encodedHash) =>
        !encodedHash.StartsWith("AQAAAA", StringComparison.Ordinal);

    private static bool VerifyLegacy(string password, string encodedHash)
    {
        var parts = encodedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4 || !string.Equals(parts[0], "PBKDF2-SHA256", StringComparison.Ordinal))
            return false;

        if (!int.TryParse(parts[1], out var iterations) || iterations < 1)
            return false;

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedKey = Convert.FromBase64String(parts[3]);
            var actualKey = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedKey.Length);

            return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

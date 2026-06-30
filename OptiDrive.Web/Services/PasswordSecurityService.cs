using System.Security.Cryptography;

namespace OptiDrive.Web.Services;

public static class PasswordSecurityService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 120_000;
    private const string Prefix = "PBKDF2-SHA256";

    public static string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return string.Join(
            "$",
            Prefix,
            Iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        var parts = storedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4 || !parts[0].Equals(Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[2]);
        var expectedHash = Convert.FromBase64String(parts[3]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    public static bool NeedsRehash(string storedHash)
    {
        var parts = storedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length != 4
               || !parts[0].Equals(Prefix, StringComparison.Ordinal)
               || !int.TryParse(parts[1], out var iterations)
               || iterations < Iterations;
    }

    public static bool VerifyLegacyPassword(string password, string legacyPassword)
        => !string.IsNullOrWhiteSpace(legacyPassword) && legacyPassword == password;
}

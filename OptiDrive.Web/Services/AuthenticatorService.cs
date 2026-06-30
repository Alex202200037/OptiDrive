using System.Security.Cryptography;
using System.Text;
using QRCoder;

namespace OptiDrive.Web.Services;

public sealed class AuthenticatorService
{
    private const string Issuer = "OptiDrive";
    private const int SecretByteLength = 20;
    private static readonly char[] Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

    public string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(SecretByteLength);
        return Base32Encode(bytes);
    }

    public string BuildOtpAuthUri(string email, string secret)
    {
        var label = Uri.EscapeDataString($"{Issuer}:{email}");
        var issuer = Uri.EscapeDataString(Issuer);
        return $"otpauth://totp/{label}?secret={secret}&issuer={issuer}&digits=6&period=30";
    }

    public string BuildQrCodeDataUri(string otpAuthUri)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(otpAuthUri, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(data);
        var bytes = qrCode.GetGraphic(10);
        return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
    }

    public bool ValidateCode(string secret, string code, DateTimeOffset? now = null)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        var normalizedCode = NormalizeCode(code);
        if (normalizedCode.Length != 6 || normalizedCode.Any(character => !char.IsDigit(character)))
        {
            return false;
        }

        var timestamp = now ?? DateTimeOffset.UtcNow;
        for (var drift = -1; drift <= 1; drift++)
        {
            if (GenerateCode(secret, timestamp.AddSeconds(drift * 30)) == normalizedCode)
            {
                return true;
            }
        }

        return false;
    }

    public string GenerateCode(string secret, DateTimeOffset? now = null)
    {
        var timestamp = now ?? DateTimeOffset.UtcNow;
        var timestep = timestamp.ToUnixTimeSeconds() / 30;
        var timestepBytes = BitConverter.GetBytes(timestep);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timestepBytes);
        }

        using var hmac = new HMACSHA1(Base32Decode(secret));
        var hash = hmac.ComputeHash(timestepBytes);
        var offset = hash[^1] & 0x0F;
        var binaryCode =
            ((hash[offset] & 0x7F) << 24)
            | ((hash[offset + 1] & 0xFF) << 16)
            | ((hash[offset + 2] & 0xFF) << 8)
            | (hash[offset + 3] & 0xFF);

        return (binaryCode % 1_000_000).ToString("D6");
    }

    public IReadOnlyList<string> GenerateRecoveryCodes(int count = 8)
        => Enumerable.Range(0, count)
            .Select(_ => Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).Insert(4, "-"))
            .ToList();

    public string HashRecoveryCode(string code)
    {
        var normalized = NormalizeRecoveryCode(code);
        var salt = RandomNumberGenerator.GetBytes(12);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{Convert.ToBase64String(salt)}:{normalized}"));
        return $"SHA256${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool VerifyRecoveryCode(string code, string storedHash)
    {
        var parts = storedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 || parts[0] != "SHA256")
        {
            return false;
        }

        var normalized = NormalizeRecoveryCode(code);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{parts[1]}:{normalized}"));
        var expectedHash = Convert.FromBase64String(parts[2]);
        return CryptographicOperations.FixedTimeEquals(hash, expectedHash);
    }

    public static string FormatSecretForDisplay(string secret)
        => string.Join(" ", Enumerable.Range(0, (secret.Length + 3) / 4)
            .Select(index => secret.Substring(index * 4, Math.Min(4, secret.Length - index * 4))));

    private static string NormalizeCode(string code)
        => code.Trim().Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal);

    private static string NormalizeRecoveryCode(string code)
        => code.Trim().Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

    private static string Base32Encode(byte[] bytes)
    {
        var result = new StringBuilder();
        var buffer = 0;
        var bitsLeft = 0;

        foreach (var value in bytes)
        {
            buffer = (buffer << 8) | value;
            bitsLeft += 8;
            while (bitsLeft >= 5)
            {
                result.Append(Base32Alphabet[(buffer >> (bitsLeft - 5)) & 31]);
                bitsLeft -= 5;
            }
        }

        if (bitsLeft > 0)
        {
            result.Append(Base32Alphabet[(buffer << (5 - bitsLeft)) & 31]);
        }

        return result.ToString();
    }

    private static byte[] Base32Decode(string secret)
    {
        var normalized = secret.Trim().Replace(" ", string.Empty, StringComparison.Ordinal).TrimEnd('=').ToUpperInvariant();
        var bytes = new List<byte>();
        var buffer = 0;
        var bitsLeft = 0;

        foreach (var character in normalized)
        {
            var value = Array.IndexOf(Base32Alphabet, character);
            if (value < 0)
            {
                throw new FormatException("Invalid Base32 character.");
            }

            buffer = (buffer << 5) | value;
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                bytes.Add((byte)((buffer >> (bitsLeft - 8)) & 255));
                bitsLeft -= 8;
            }
        }

        return bytes.ToArray();
    }
}

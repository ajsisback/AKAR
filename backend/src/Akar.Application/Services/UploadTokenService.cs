using System.Security.Cryptography;

namespace Akar.Application.Services;

/// <summary>
/// Generates and hashes upload tokens.
/// Token: 32-byte cryptographically random, base64url-encoded.
/// Hash: SHA256 hex string for storage.
/// </summary>
public static class UploadTokenService
{
    /// <summary>Generates a cryptographically strong random token.</summary>
    public static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <summary>Computes SHA256 hash of the token for storage.</summary>
    public static string HashToken(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }

    /// <summary>Returns a short preview of the token (first 8 chars + "...").</summary>
    public static string GetPreview(string token)
    {
        return token.Length > 8 ? token[..8] + "..." : token;
    }
}

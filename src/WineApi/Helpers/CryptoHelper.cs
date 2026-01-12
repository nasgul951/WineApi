using System.Security.Cryptography;
using System.Text;

namespace WineApi.Helpers;

/// <summary>
/// Helper class for cryptographic operations like password hashing.
/// </summary>
public static class CryptoHelper
{
    /// <summary>
    /// Hashes a password using SHA256 with a salt.
    /// </summary>
    /// <param name="password">The plain text password to hash</param>
    /// <param name="salt">The salt to use for hashing</param>
    /// <returns>Lowercase hexadecimal string representation of the hash</returns>
    public static string HashPassword(string password, string salt)
    {
        var saltedPassword = salt + password;
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

        // Return the hash as a lowercase hexadecimal string
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Generates a cryptographically secure random salt.
    /// </summary>
    /// <param name="size">The size of the salt in bytes</param>
    public static string GenerateSalt(int size)
    {
        var salt = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return Convert.ToBase64String(salt);
    }
}
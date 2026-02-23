/*
 * File: Services/PasswordHasher.cs
 * Purpose: Utility for hashing and verifying passwords using PBKDF2 (Rfc2898DeriveBytes).
 * Security notes:
 *  - Uses SHA256 and a high iteration count for key derivation to slow brute-force attempts.
 *  - Output format: "{iterations}.{base64Salt}.{base64Key}". Keep storage and comparisons secure.
 */

using System;
using System.Security.Cryptography;

namespace ExpenseTracker.Services
{
    /// Simple static helper for hashing and verifying passwords.
    /// Use `HashPassword` to persist a password and `VerifyPassword` to validate an input password.
       public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100000;


        /// Hashes a plaintext password using PBKDF2 and returns a concatenated string containing
        /// the iteration count, salt and derived key.
               /// <param name="password">Plaintext password to hash.</param>
        /// <returns>String containing iterations, salt and key suitable for storage.</returns>
        public static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize);

            return string.Join('.', Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(key));
        }


        /// Verifies a plaintext password against the stored hash produced by <see cref="HashPassword"/>.
               /// <param name="password">Plaintext password to verify.</param>
        /// <param name="storedHash">Stored hash string in the format produced by <see cref="HashPassword"/>.</param>
        /// <returns>True if the password matches; otherwise false.</returns>
        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.', 3);
            if (parts.Length != 3)
            {
                return false;
            }

            if (!int.TryParse(parts[0], out var iterations))
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            var computedKey = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                key.Length);

            return CryptographicOperations.FixedTimeEquals(computedKey, key);
        }
    }
}

using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Identity.Client;
using System.Security.Cryptography;

namespace WijkMeld.API.Utils
{
    public static class PasswordHelper
    {

        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password, salt, KeyDerivationPrf.HMACSHA256, 10000, 256 / 8));

            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 2 ) { return false ; }

            byte[] salt = Convert.FromBase64String(parts[0]);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password, salt, KeyDerivationPrf.HMACSHA256, 10000, 256 / 8));

            return hashed == parts[1];
        }
    }
}

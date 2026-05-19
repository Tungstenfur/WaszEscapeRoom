using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Konscious.Security.Cryptography;
namespace WaszEscapeRoom;

internal static class Auth
{
    public static string Login(string username, string password)
    {
        throw new NotImplementedException();
    }
    public static string HashPassword(string password)
    {
        // Generate random salt
        byte[] salt = RandomNumberGenerator.GetBytes(16);

        // Configure Argon2id
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,

            // Memory usage in KB (64 MB)
            MemorySize = 65536,

            // Iterations
            Iterations = 4,

            // Degree of parallelism
            DegreeOfParallelism = Environment.ProcessorCount
        };

        byte[] hash = argon2.GetBytes(32);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string stored)
    {
        string[] parts = stored.Split(':');

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] expectedHash = Convert.FromBase64String(parts[1]);

        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = 65536,
            Iterations = 4,
            DegreeOfParallelism = Environment.ProcessorCount
        };

        byte[] actualHash = argon2.GetBytes(32);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
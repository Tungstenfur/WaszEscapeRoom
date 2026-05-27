using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Konscious.Security.Cryptography;
namespace WaszEscapeRoom;

internal static class Auth
{
    public static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = 65536,
            Iterations = 4,
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
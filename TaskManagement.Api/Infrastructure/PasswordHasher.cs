// Утилита для хэширования и проверки паролей (SHA256, упрощено для теста).
using System.Security.Cryptography;
using System.Text;

namespace TaskManagement.Api.Infrastructure;

public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    public static bool VerifyPassword(string password, string hash) =>
        HashPassword(password) == hash;
}


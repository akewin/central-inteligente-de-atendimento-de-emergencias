using System;
using System.Security.Cryptography;
using System.Text;

namespace Api.Security.RefreshTokens;

public static class HashToken
{
    public static string Compute(string token)
    {
        if (token == null) throw new ArgumentNullException(nameof(token));
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public static bool Verify(string token, string hash)
        => Compute(token) == hash;
}

﻿using System.Security.Cryptography;
using System.Text;
using EcommerceProject.Application.Common.Interfaces;

namespace EcommerceProject.Infrastructure.Helpers;

public class PasswordHelper : IPasswordHelper
{
    private const int SaltSize = 64;
    private const int HashSize = 128;
    private const int HashingIterations = 350000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA512;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            HashingIterations,
            HashAlgorithm,
            HashSize);

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool VerifyPassword(string hashedPassword, string password)
    {
        var parts = hashedPassword.Split('-');
        if (parts.Length != 2)
            return false;

        try
        {
            var hash = Convert.FromHexString(parts[0]);
            var salt = Convert.FromHexString(parts[1]);

            var inputHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                HashingIterations,
                HashAlgorithm,
                HashSize);

            return CryptographicOperations.FixedTimeEquals(inputHash, hash);
        }
        catch
        {
            return false;
        }
    }
}
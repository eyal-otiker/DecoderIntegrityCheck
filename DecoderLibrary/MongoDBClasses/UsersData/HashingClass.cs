using System;
using System.Security.Cryptography;

namespace DecoderLibrary
{
    public class HashingClass
    {
        const int SALT_SIZE = 16;
        const int HASH_SIZE = 20;
        const int ITERATIONS = 100000;

        public HashingClass() { }

        public static string Hash(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[SALT_SIZE]);

            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, ITERATIONS);
            byte[] hash = rfc2898DeriveBytes.GetBytes(HASH_SIZE);

            byte[] hashBytes = new byte[SALT_SIZE + HASH_SIZE];
            Array.Copy(salt, 0, hashBytes, 0, SALT_SIZE);
            Array.Copy(hash, 0, hashBytes, SALT_SIZE, HASH_SIZE);

            string base64Hash = Convert.ToBase64String(hashBytes);
            return base64Hash;
        }

        public static bool Veritify(string enterdPassword, string savedPassword)
        {
            string base64Hash = savedPassword;
            byte[] hashBytes = Convert.FromBase64String(base64Hash);

            byte[] salt = new byte[SALT_SIZE];
            Array.Copy(hashBytes, 0, salt, 0, SALT_SIZE);

            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(enterdPassword, salt, ITERATIONS);
            byte[] hash = rfc2898DeriveBytes.GetBytes(HASH_SIZE);

            for (int i = 0; i < HASH_SIZE; i++)
            {
                if (hashBytes[i + SALT_SIZE] != hash[i])
                    return false;
            }
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Concepts.Ring5 {
    public class SystemUserPassword {


        public static void GeneratePasswordHash(string userId, string password, out string hashedPassword) {

            //byte[] saltb = CreateSalt(32);
            byte[] saltb = GetBytes(userId + ":" + password);

            hashedPassword = Convert.ToBase64String(GenerateSaltedHash(GetBytes(password), saltb));
            //salt = Convert.ToBase64String(saltb);
        }

        //public static bool CheckPassword(string userId, string password, string hashedPassword) {

        //    string newHashedPassword;

        //    HashPassword(password, userId + ":" + password, out newHashedPassword);
        //    return hashedPassword == newHashedPassword;
        //}

        //private static void HashPassword(string password, string salt, out string hashedPassword) {
        //    hashedPassword = Convert.ToBase64String(GenerateSaltedHash(GetBytes(password), Convert.FromBase64String(salt)));
        //}

        private static byte[] CreateSalt(int size) {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);
            return buff;
        }

        private static byte[] GenerateSaltedHash(byte[] plainText, byte[] salt) {
            HashAlgorithm algorithm = new SHA256Managed();

            byte[] plainTextWithSaltBytes =
              new byte[plainText.Length + salt.Length];

            for (int i = 0; i < plainText.Length; i++) {
                plainTextWithSaltBytes[i] = plainText[i];
            }
            for (int i = 0; i < salt.Length; i++) {
                plainTextWithSaltBytes[plainText.Length + i] = salt[i];
            }

            return algorithm.ComputeHash(plainTextWithSaltBytes);
        }

        private static byte[] GetBytes(string str) {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static string GetString(byte[] bytes) {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}

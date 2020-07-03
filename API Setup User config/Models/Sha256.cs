using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API_Setup_User_config.Models
{
    public class Sha256
    {
        public static string salting;
        public static Pass_Model NewSha256Hash(string input, byte[] saltB)
        {
            //checks if the salt is already given (saved in the database) or if a new one is needed ( new user)
            if (saltB == null)
            {
                int minSize = 16;
                int maxSize = 20;
                Random random = new Random();
                int saltSize = random.Next(minSize, maxSize);
                saltB = new byte[saltSize];
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetNonZeroBytes(saltB);
            }
            //Converts input into Bytes
            byte[] InputByte;
            InputByte = Encoding.UTF8.GetBytes(input);

            //byte with input and salt bytes
            byte[] InputSaltBytes = new byte[InputByte.Length + saltB.Length];

            // Copy plain text bytes into resulting array.
            for (int i = 0; i < InputByte.Length; i++)
                InputSaltBytes[i] = InputByte[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < saltB.Length; i++)
                InputSaltBytes[InputByte.Length + i] = saltB[i];

            HashAlgorithm hash = new SHA256Managed();

            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hash.ComputeHash(InputSaltBytes);

            // Create array which will hold hash and original salt bytes.
            byte[] hashWithSaltBytes = new byte[hashBytes.Length + saltB.Length];

            // Copy hash bytes into resulting array.
            for (int i = 0; i < hashBytes.Length; i++)
                hashWithSaltBytes[i] = hashBytes[i];

            // Append salt bytes to the result.
            for (int i = 0; i < saltB.Length; i++)
                hashWithSaltBytes[hashBytes.Length + i] = saltB[i];

            //returns in base64String to be readable characters which will then be saved on the DB, instead of saving it as a Byte[]
            return new Pass_Model(Convert.ToBase64String(hashWithSaltBytes), Convert.ToBase64String(saltB));
        }
    }
}

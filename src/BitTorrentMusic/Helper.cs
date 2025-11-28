using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace BitTorrentMusic
{
    /// <summary>
    /// Helper class for SHA256 hashing.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Compute SHA256 hash of a file from its path.
        /// </summary>
        public static string HashFile(string path)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] hashBytes = sha256Hash.ComputeHash(File.ReadAllBytes(path));
                string hashString = BitConverter.ToString(hashBytes).Replace("-", "");
                return hashString;
            }
        }
        /// <summary>
        /// Compute SHA256 hash from a byte array.
        /// </summary>
        public static string HashFileBytes(byte[] data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(data);
                return BitConverter.ToString(hashBytes).Replace("-", "");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace BitTorrentMusic
{
    public static class Helper
    {
        public static string HashFile(string path)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] hashBytes = sha256Hash.ComputeHash(File.ReadAllBytes(path));
                string hashString = BitConverter.ToString(hashBytes).Replace("-", "");
                return hashString;
            }
        }
    }
}

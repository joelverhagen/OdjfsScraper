using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace OdjfsScraper.Fetcher.Support
{
    public static class ExtensionMethods
    {
        public static string GetSha256Hash(this byte[] bytes)
        {
            var sha = new SHA256Managed();
            byte[] hashBytes = sha.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        public static async Task<byte[]> ReadAsByteArrayAsync(this Stream stream)
        {
            var outputStream = new MemoryStream();
            await stream.CopyToAsync(outputStream);
            return outputStream.ToArray();
        }

        public static async Task<string> ReadAsStringAsync(this Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
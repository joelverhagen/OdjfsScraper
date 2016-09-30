using System.IO;
using System.Threading.Tasks;

namespace OdjfsScraper.Fetch
{
    public static class ExtensionMethods
    {
        public static byte[] ReadAsByteArray(this Stream stream)
        {
            var outputStream = new MemoryStream();
            stream.CopyTo(outputStream);
            return outputStream.ToArray();
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
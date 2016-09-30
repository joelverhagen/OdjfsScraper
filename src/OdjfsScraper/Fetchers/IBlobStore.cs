using System.IO;
using System.Threading.Tasks;

namespace OdjfsScraper.Fetchers
{
    public interface IBlobStore
    {
        Task<Stream> WriteAsync(string name, string tag, Stream stream);
    }
}
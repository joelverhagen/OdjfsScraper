using System.IO;

namespace OdjfsScraper.Fetcher.Support
{
    public interface IHashAlgorithm
    {
        string ComputeHashToString(Stream stream);
    }
}
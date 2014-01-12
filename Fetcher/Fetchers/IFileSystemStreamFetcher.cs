namespace OdjfsScraper.Fetcher.Fetchers
{
    public interface IFileSystemStreamFetcher : IStreamFetcher
    {
        void SetDirectory(string directory);
    }
}
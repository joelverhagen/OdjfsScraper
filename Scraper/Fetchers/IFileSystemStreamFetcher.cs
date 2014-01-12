namespace OdjfsScraper.Scraper.Fetchers
{
    public interface IFileSystemStreamFetcher : IStreamFetcher
    {
        void SetDirectory(string directory);
    }
}
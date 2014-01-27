namespace OdjfsScraper.Fetcher.Fetchers
{
    public interface IFileSystemStreamFetcher : IStreamFetcher
    {
        string Directory { get; set; }
    }
}
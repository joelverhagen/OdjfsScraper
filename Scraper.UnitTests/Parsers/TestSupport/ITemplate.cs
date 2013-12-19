namespace OdjfsScraper.Scraper.UnitTests.Parsers.TestSupport
{
    public interface ITemplate<out T>
    {
        T Model { get; }
        byte[] GetDocument();
    }
}
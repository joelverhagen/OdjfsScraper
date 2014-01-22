using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OdjfsScraper.Fetcher.Fetchers;

namespace OdjfsScraper.Fetcher.UnitTests.Fetchers
{
    [TestClass]
    public class HttpStreamFetcherTest : BaseHttpStreamFetcherTest<HttpStreamFetcher>
    {
        protected override HttpStreamFetcher GetFetcherForHttpStreamFetcherTests(HttpMessageHandler handler, string userAgent)
        {
            return new HttpStreamFetcher(handler, userAgent);
        }
    }
}
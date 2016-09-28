using System.Net.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OdjfsScraper.Fetcher.Fetchers;
using OdjfsScraper.Fetcher.UnitTests.Fetchers.TestSupport;

namespace OdjfsScraper.Fetcher.UnitTests.Fetchers
{
    public class HttpStreamFetcherTest : BaseHttpStreamFetcherTest<HttpStreamFetcher>
    {
        protected override HttpStreamFetcher GetFetcherForHttpStreamFetcherTests(HttpMessageHandler handler, string userAgent)
        {
            return new HttpStreamFetcher(
                new Mock<ILogger<HttpStreamFetcher>>().Object,
                handler,
                userAgent);
        }
    }
}
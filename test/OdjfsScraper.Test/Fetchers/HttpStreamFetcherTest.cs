using System.Net.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OdjfsScraper.Fetchers;
using OdjfsScraper.Test.Fetchers;

namespace OdjfsScraper.Test.Fetchers
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
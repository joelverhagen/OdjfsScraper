using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OdjfsScraper.Fetcher.Fetchers;
using OdjfsScraper.Fetcher.Support;

namespace OdjfsScraper.Fetcher.UnitTests.Fetchers
{
    [TestClass]
    public class DownloadingHttpStreamFetcherTest : BaseHttpStreamFetcherTest<DownloadingHttpStreamFetcher>
    {
        protected override DownloadingHttpStreamFetcher GetFetcherForHttpStreamFetcherTests(HttpMessageHandler handler, string userAgent)
        {
            var mock = new Mock<IFileSystemBlobStore>();

            Stream lastStream = null;
            mock
                .SetupProperty(f => f.Directory);
            mock
                .Setup(f => f.Read(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(() => Task.FromResult(lastStream));
            mock
                .Setup(f => f.Write(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .Returns(Task.FromResult(0))
                .Callback<string, string, Stream>((name, tag, stream) => lastStream = new MemoryStream(stream.ReadAsByteArrayAsync().Result));
            mock.Object.Directory = @"Z:\HTML";
            return new DownloadingHttpStreamFetcher(handler, userAgent, mock.Object);
        }
    }
}
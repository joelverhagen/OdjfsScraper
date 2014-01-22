using System.IO;
using System.Linq;
using System.Net.Http;
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
            var mock = new Mock<IFileSystem>(MockBehavior.Strict);

            mock
                .Setup(f => f.DirectoryExists(It.IsAny<string>()))
                .Returns(false);

            mock
                .Setup(f => f.DirectoryCreateDirectory(It.IsAny<string>()))
                .Returns(default(DirectoryInfo));

            mock
                .Setup(f => f.FileExists(It.IsAny<string>()))
                .Returns(false);

            mock
                .Setup(f => f.FileOpen(It.IsAny<string>(), It.IsAny<FileMode>()))
                .Returns(() => new MemoryStream());

            mock
                .Setup(f => f.DirectoryEnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), SearchOption.TopDirectoryOnly))
                .Returns(Enumerable.Empty<string>);

            mock
                .Setup(f => f.FileMove(It.IsAny<string>(), It.IsAny<string>()));

            return new DownloadingHttpStreamFetcher(handler, userAgent, mock.Object, @"Z:\HTML");
        }
    }
}
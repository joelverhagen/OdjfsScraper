using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.SessionState;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using OdjfsScraper.Fetcher.Fetchers;
using OdjfsScraper.Fetcher.Support;
using OdjfsScraper.Model;

namespace OdjfsScraper.Fetcher.UnitTests.Fetchers
{
    [TestClass]
    public class DownloadingHttpStreamFetcherTest : BaseHttpStreamFetcherTest<DownloadingHttpStreamFetcher>
    {
        [TestMethod]
        public void GetChildCareStubListDocument_EmptyFileSystem()
        {
            
        }
        [TestMethod]
        public void Sandbox()
        {
            County county = new County {Name = "FRANKLIN"};
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            byte[] expectedBytes = Encoding.UTF8.GetBytes("This is the contents of a file.");

            string directory = @"Z:\HTML";
            string prefix = string.Format("County_{0}", county.Name);
            
            string fileName = string.Join("_", new[]
            {
                prefix,
                "Current",
                httpStatusCode.ToString(),
                expectedBytes.GetSha256Hash() + ".html"
            });
            string filePath = Path.Combine(directory, fileName);

            MemoryStream destinationStream = new MemoryStream();
            MemoryStream expectedStream = new MemoryStream();

            var handler = GetHandler(httpStatusCode, expectedBytes);
            var fileSystem = GetEmptyFileSystem(directory, filePath, prefix, destinationStream, expectedStream);

            var fetcher = new DownloadingHttpStreamFetcher(handler, "Foo user agent", fileSystem, directory);

            // ACT
            var task = fetcher.GetChildCareStubListDocument(county);

            // ASSERT
            Stream actualStream = task.Result;
            byte[] actualBytes = destinationStream.ToArray();
            Assert.AreSame(expectedStream, actualStream);
            Assert.IsTrue(expectedBytes.SequenceEqual(actualBytes));
        }

        private static IFileSystem GetEmptyFileSystem(string directory, string filePath, string prefix, Stream destinationStream, Stream expectedStream)
        {
            var fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);
            fileSystemMock
                .Setup(f => f.DirectoryExists(directory))
                .Returns(true);
            fileSystemMock
                .Setup(f => f.FileExists(filePath))
                .Returns(false);
            fileSystemMock
                .Setup(f => f.DirectoryEnumerateFiles(directory, string.Format("{0}*.html", prefix), SearchOption.TopDirectoryOnly))
                .Returns(Enumerable.Empty<string>());
            fileSystemMock
                .Setup(f => f.FileOpen(filePath, FileMode.Create))
                .Returns(destinationStream);
            fileSystemMock
                .Setup(f => f.FileOpen(filePath, FileMode.Open))
                .Returns(expectedStream);

            return fileSystemMock.Object;
        }

        private static HttpMessageHandler GetHandler(HttpStatusCode httpStatusCode, byte[] content)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(GetHttpResponseMessage(httpStatusCode, content));

            return handlerMock.Object;
        }

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
                .Setup(f => f.DirectoryEnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(Enumerable.Empty<string>);

            mock
                .Setup(f => f.FileMove(It.IsAny<string>(), It.IsAny<string>()));

            return new DownloadingHttpStreamFetcher(handler, userAgent, mock.Object, @"Z:\HTML");
        }
    }
}
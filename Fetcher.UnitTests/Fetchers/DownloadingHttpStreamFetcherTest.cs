using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using OdjfsScraper.Fetcher.Fetchers;
using OdjfsScraper.Fetcher.Support;
using OdjfsScraper.Fetcher.UnitTests.Fetchers.TestSupport;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Fetcher.UnitTests.Fetchers
{
    [TestClass]
    public class DownloadingHttpStreamFetcherTest : BaseHttpStreamFetcherTest<DownloadingHttpStreamFetcher>
    {
        [TestMethod]
        public void GetChildCareDocument_ChildCare_CorrectStoreCallsWithInvalidResponse()
        {
            VerifyCorrectCalls(
                HttpStatusCode.InternalServerError,
                "ChildCare",
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new ChildCare {ExternalUrlId = externalUrlId}).Wait());
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCareStub_CorrectStoreCallsWithInvalidResponse()
        {
            VerifyCorrectCalls(
                HttpStatusCode.InternalServerError,
                "ChildCare",
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new ChildCareStub { ExternalUrlId = externalUrlId }).Wait());
        }

        [TestMethod]
        public void GetChildCareStubListDocument_CorrectStoreCallsWithInvalidResponse()
        {
            VerifyCorrectCalls(
                HttpStatusCode.InternalServerError,
                "County",
                (fetcher, countyName) => fetcher.GetChildCareStubListDocument(new County {Name = countyName}).Wait());
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCare_CorrectStoreCallsWithValidResponse()
        {
            VerifyCorrectCalls(
                HttpStatusCode.OK,
                "ChildCare",
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new ChildCare {ExternalUrlId = externalUrlId}).Wait());
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCareStub_CorrectStoreCallsWithValidResponse()
        {
            VerifyCorrectCalls(
                HttpStatusCode.OK,
                "ChildCare",
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new ChildCareStub { ExternalUrlId = externalUrlId }).Wait());
        }

        [TestMethod]
        public void GetChildCareStubListDocument_CorrectStoreCallsWithValidResponse()
        {
            VerifyCorrectCalls(
                HttpStatusCode.OK,
                "County",
                (fetcher, countyName) => fetcher.GetChildCareStubListDocument(new County {Name = countyName}).Wait());
        }

        private void VerifyCorrectCalls(HttpStatusCode httpStatusCode, string prefix, Action<DownloadingHttpStreamFetcher, string> action)
        {
            // ARRANGE
            HttpMessageHandler handler = GetHandler(httpStatusCode, new byte[0]);
            Mock<IFileSystemBlobStore> storeMock = GetFileSystemBlobStoreMock();
            var fetcher = new DownloadingHttpStreamFetcher(handler, null, storeMock.Object);

            // ACT
            try
            {
                action(fetcher, "Foo");
            }
            catch (Exception)
            {
            }


            // ASSERT
            string name = string.Format("{0}-{1}", prefix, "Foo");
            storeMock.Verify(s => s.Write(name, httpStatusCode.ToString(), It.IsAny<Stream>()), Times.Once);
            storeMock.Verify(s => s.Read(name, -1), Times.Once);
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

        private static Mock<IFileSystemBlobStore> GetFileSystemBlobStoreMock()
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

            return mock;
        }

        protected override DownloadingHttpStreamFetcher GetFetcherForHttpStreamFetcherTests(HttpMessageHandler handler, string userAgent)
        {
            return new DownloadingHttpStreamFetcher(handler, userAgent, GetFileSystemBlobStoreMock().Object);
        }
    }
}
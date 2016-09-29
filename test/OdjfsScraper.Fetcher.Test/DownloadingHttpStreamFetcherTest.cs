using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using OdjfsScraper.Fetchers.Test.TestSupport;
using OdjfsScraper.Models;
using Xunit;

namespace OdjfsScraper.Fetchers.Test
{
    public class DownloadingHttpStreamFetcherTest : BaseHttpStreamFetcherTest<DownloadingHttpStreamFetcher>
    {
        [Fact]
        public void GetChildCareDocument_ChildCare_CorrectStoreCallsWithInvalidResponse()
        {
            VerifyCorrectCalls(
                HttpStatusCode.InternalServerError,
                "ChildCare",
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new ChildCare {ExternalUrlId = externalUrlId}).Wait());
        }

        [Fact]
        public void GetChildCareDocument_ChildCareStub_CorrectStoreCallsWithInvalidResponse()
        {
            VerifyCorrectCalls(
                HttpStatusCode.InternalServerError,
                "ChildCare",
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new ChildCareStub { ExternalUrlId = externalUrlId }).Wait());
        }

        [Fact]
        public void GetChildCareStubListDocument_CorrectStoreCallsWithInvalidResponse()
        {
            VerifyCorrectCalls(
                HttpStatusCode.InternalServerError,
                "County",
                (fetcher, countyName) => fetcher.GetChildCareStubListDocument(new County {Name = countyName}).Wait());
        }

        [Fact]
        public void GetChildCareDocument_ChildCare_CorrectStoreCallsWithValidResponse()
        {
            VerifyCorrectCalls(
                HttpStatusCode.OK,
                "ChildCare",
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new ChildCare {ExternalUrlId = externalUrlId}).Wait());
        }

        [Fact]
        public void GetChildCareDocument_ChildCareStub_CorrectStoreCallsWithValidResponse()
        {
            VerifyCorrectCalls(
                HttpStatusCode.OK,
                "ChildCare",
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new ChildCareStub { ExternalUrlId = externalUrlId }).Wait());
        }

        [Fact]
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
            HttpMessageHandler handler = GetHandler(httpStatusCode, null, new byte[0]);
            Mock<IBlobStore> storeMock = GetFileSystemBlobStoreMock();
            var fetcher = new DownloadingHttpStreamFetcher(GetLogger(), handler, null, storeMock.Object);

            // ACT
            try
            {
                action(fetcher, "Foo");
            }
            catch (Exception)
            {
            }

            // ASSERT
            string name = string.Format("{0}/{1}", prefix, "Foo");
            storeMock.Verify(s => s.WriteAsync(name, httpStatusCode.ToString(), It.IsAny<Stream>()), Times.Once);
        }

        private static HttpMessageHandler GetHandler(HttpStatusCode httpStatusCode, IDictionary<string, string> headers, byte[] content)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(GetHttpResponseMessage(httpStatusCode, headers, content));

            return handlerMock.Object;
        }

        private static Mock<IBlobStore> GetFileSystemBlobStoreMock()
        {
            var mock = new Mock<IBlobStore>();
            
            mock
                .Setup(f => f.WriteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .Returns<string, string, Stream>((_, __, stream) => Task.FromResult<Stream>(new MemoryStream(stream.ReadAsByteArray())));

            return mock;
        }

        protected override DownloadingHttpStreamFetcher GetFetcherForHttpStreamFetcherTests(HttpMessageHandler handler, string userAgent)
        {
            return new DownloadingHttpStreamFetcher(
                GetLogger(),
                handler,
                userAgent,
                GetFileSystemBlobStoreMock().Object);
        }

        private static ILogger<DownloadingHttpStreamFetcher> GetLogger()
        {
            return new Mock<ILogger<DownloadingHttpStreamFetcher>>().Object;
        }
    }
}
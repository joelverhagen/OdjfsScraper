﻿using System;
using System.Collections.Generic;
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
            HttpMessageHandler handler = GetHandler(httpStatusCode, null, new byte[0]);
            Mock<IBlobStore> storeMock = GetFileSystemBlobStoreMock();
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
            return new DownloadingHttpStreamFetcher(handler, userAgent, GetFileSystemBlobStoreMock().Object);
        }
    }
}
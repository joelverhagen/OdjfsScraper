using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using OdjfsScraper.Fetcher.Fetchers;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Fetcher.UnitTests.Fetchers
{
    [TestClass]
    public class HttpStreamFetcherTest
    {
        [TestMethod]
        public void Constructor_HttpClientHandler()
        {
            VerifyHttpMessageHandler(new HttpClientHandler());
        }

        [TestMethod]
        public void Constructor_WebRequestHandler()
        {
            VerifyHttpMessageHandler(new WebRequestHandler());
        }

        [TestMethod]
        public void GetChildCareStubListDocument_HappyPath()
        {
            var county = new County {Name = "FRANKLIN"};
            VerifyHappyPathRequest(
                fetcher => fetcher.GetChildCareStubListDocument(county),
                (request, userAgent) => VerifyCountyRequest(request, county.Name, userAgent));
        }

        [TestMethod]
        public void GetChildCareStubListDocument_NullCounty()
        {
            VerifyException<ArgumentNullException>(
                HttpStatusCode.OK,
                string.Empty,
                f => f.GetChildCareStubListDocument(null),
                e => Assert.AreEqual(e.ParamName, "county"));
        }

        [TestMethod]
        public void GetChildCareStubListDocument_NullCountyName()
        {
            VerifyException<ArgumentNullException>(
                HttpStatusCode.OK,
                string.Empty,
                f => f.GetChildCareStubListDocument(new County {Name = null}),
                e => Assert.AreEqual(e.ParamName, "county.Name"));
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCare_NullExternalUrlId()
        {
            VerifyException<ArgumentNullException>(
                HttpStatusCode.OK,
                string.Empty,
                f => f.GetChildCareDocument(new LicensedCenter {ExternalUrlId = null}),
                e => Assert.AreEqual(e.ParamName, "childCare.ExternalUrlId"));
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCareStub_NullExternalUrlId()
        {
            VerifyException<ArgumentNullException>(
                HttpStatusCode.OK,
                string.Empty,
                f => f.GetChildCareDocument(new LicensedCenterStub {ExternalUrlId = null}),
                e => Assert.AreEqual(e.ParamName, "childCareStub.ExternalUrlId"));
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCare_NullChildCare()
        {
            VerifyException<ArgumentNullException>(
                HttpStatusCode.OK,
                string.Empty,
                f => f.GetChildCareDocument((ChildCare) null),
                e => Assert.AreEqual(e.ParamName, "childCare"));
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCareStub_NullChildCareStub()
        {
            VerifyException<ArgumentNullException>(
                HttpStatusCode.OK,
                string.Empty,
                f => f.GetChildCareDocument((ChildCareStub) null),
                e => Assert.AreEqual(e.ParamName, "childCareStub"));
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCare_HappyPath()
        {
            var childCare = new LicensedCenter {ExternalUrlId = "CCCCCCCCCCCCCCCCCC"};
            VerifyHappyPathRequest(
                fetcher => fetcher.GetChildCareDocument(childCare),
                (request, userAgent) => VerifyChildCareRequest(request, childCare.ExternalUrlId, userAgent));
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCareStub_HappyPath()
        {
            var childCare = new LicensedCenterStub {ExternalUrlId = "CCCCCCCCCCCCCCCCCC"};
            VerifyHappyPathRequest(
                fetcher => fetcher.GetChildCareDocument(childCare),
                (request, userAgent) => VerifyChildCareRequest(request, childCare.ExternalUrlId, userAgent));
        }

        private static void VerifyException<T>(HttpStatusCode httpStatusCode, string content, Action<HttpStreamFetcher> act, Action<T> verify) where T : Exception
        {
            // ARRANGE
            var mock = new Mock<HttpMessageHandler>();
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(GetHttpResponseMessage(httpStatusCode, content));

            var fetcher = new HttpStreamFetcher(mock.Object, "Foo user agent");

            // ACT
            try
            {
                act(fetcher);
                Assert.Fail();
            }
            catch (T e)
            {
                // ASSERT
                verify(e);
            }
        }

        private static void VerifyHttpMessageHandler(HttpMessageHandler handler)
        {
            var fetcher = new HttpStreamFetcher(handler, "Foo");
        }

        private static void VerifyHttpMessageHandler(HttpClientHandler handler)
        {
            VerifyHttpMessageHandler((HttpMessageHandler) handler);
            Assert.IsFalse(handler.AllowAutoRedirect);
            Assert.AreEqual(handler.AutomaticDecompression, DecompressionMethods.GZip | DecompressionMethods.Deflate);
            Assert.IsFalse(handler.UseCookies);
        }

        private static void VerifyHttpMessageHandler(WebRequestHandler handler)
        {
            VerifyHttpMessageHandler((HttpClientHandler) handler);
            Assert.IsTrue(handler.AllowPipelining);
        }

        private static void VerifyHappyPathRequest(Func<HttpStreamFetcher, Task<Stream>> getStreamTask, Action<HttpRequestMessage, string> verifyRequest)
        {
            // ARRANGE
            const string userAgent = "Foo user agent";
            var mock = new Mock<HttpMessageHandler>();
            mock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(GetHttpResponseMessage(HttpStatusCode.OK, string.Empty))
                .Callback<HttpRequestMessage, CancellationToken>((request, ct) => verifyRequest(request, userAgent));

            var fetcher = new HttpStreamFetcher(mock.Object, userAgent);

            // ACT
            Task<Stream> task = getStreamTask(fetcher);

            // ASSERT
            Stream result = task.Result;
            Assert.IsNotNull(result);
        }

        private static Task<HttpResponseMessage> GetHttpResponseMessage(HttpStatusCode httpStatusCode, string content)
        {
            var response = new HttpResponseMessage(httpStatusCode);
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            response.Content = new ByteArrayContent(bytes);
            return Task.FromResult(response);
        }

        private static void VerifyCountyRequest(HttpRequestMessage request, string countyName, string userAgent)
        {
            VerifyRequest(request, userAgent);

            // parse the query
            NameValueCollection query = HttpUtility.ParseQueryString(request.RequestUri.Query);

            Assert.AreEqual(request.RequestUri.AbsolutePath, "/cdc/results1.asp");
            Assert.AreEqual(query["county"], countyName);
            Assert.AreEqual(query["rating"], "ALL");
            Assert.AreEqual(query["Printable"], "Y");
            Assert.AreEqual(query["ShowAllPages"], "Y");
        }

        private static void VerifyChildCareRequest(HttpRequestMessage request, string externalUrlId, string userAgent)
        {
            VerifyRequest(request, userAgent);

            // parse the query
            NameValueCollection query = HttpUtility.ParseQueryString(request.RequestUri.Query);

            Assert.AreEqual(request.RequestUri.AbsolutePath, "/cdc/results2.asp");
            Assert.AreEqual(query["provider_number"], externalUrlId);
            Assert.AreEqual(query["Printable"], "Y");
        }

        private static void VerifyRequest(HttpRequestMessage request, string userAgent)
        {
            Assert.AreEqual(request.RequestUri.Scheme, "http");
            Assert.AreEqual(request.RequestUri.Host, "www.odjfs.state.oh.us");
            Assert.AreEqual(request.Headers.UserAgent.ToString(), userAgent);
            Assert.AreEqual(request.Method, HttpMethod.Get);
        }
    }
}
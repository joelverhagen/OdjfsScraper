using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using OdjfsScraper.Fetcher.Support;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Fetcher.Fetchers
{
    public class HttpStreamFetcher : IStreamFetcher
    {
        private static readonly string[] NonNumericPattern = {"OraOLEDB", "error '80040e14'", "ORA-01858: a non-numeric character was found where a numeric was expected"};
        private static readonly string[] InvalidHourPattern = {"OraOLEDB", "error '80040e14'", "ORA-01850: hour must be between 0 and 23"};
        private static readonly string[] DeletedRecordPattern = {"ADODB.Field", "error '800a0bcd'", "Either BOF or EOF is True, or the current record has been deleted. Requested operation requires a current record."};

        private static readonly string[] UnspecifiedError = {"Provider", "error '80004005'", "Unspecified error"};
        private static readonly string[] OracleNotConnectedError = {"OraOLEDB", "error '80040e14'", "ORA-03114: not connected to ORACLE"};
        private static readonly string[] OracleNotAvailableError = {"OraOLEDB", "error '80004005'", "ORA-01034: ORACLE not available", "ORA-27101: shared memory realm does not exist", "IBM AIX RISC System/6000 Error: 2: No such file or directory"};
        private static readonly string[] ImmediateShutdownError = {"OraOLEDB", "error '80004005'", "ORA-01089: immediate shutdown in progress - no operations are permitted"};
        private static readonly string[] OracleShutdownError = {"OraOLEDB", "error '80004005'", "ORA-01033: ORACLE initialization or shutdown in progress"};

        public static readonly IEnumerable<Func<HttpResponseMessage, string, bool>> PermanentErrorTests = new Func<HttpResponseMessage, string, bool>[]
        {
            (r, s) => MatchesPattern(InvalidHourPattern, s),
            (r, s) => MatchesPattern(NonNumericPattern, s),
            (r, s) => MatchesPattern(DeletedRecordPattern, s)
        }.ToList().AsReadOnly();

        public static readonly IEnumerable<Func<HttpResponseMessage, string, bool>> TemporaryErrorTests = new Func<HttpResponseMessage, string, bool>[]
        {
            (r, s) => MatchesPattern(UnspecifiedError, s),
            (r, s) => MatchesPattern(OracleNotConnectedError, s),
            (r, s) => MatchesPattern(OracleNotAvailableError, s),
            (r, s) => MatchesPattern(ImmediateShutdownError, s),
            (r, s) => MatchesPattern(OracleShutdownError, s),
            (r, s) => r.StatusCode == HttpStatusCode.Redirect && (
                r.Headers.Location == new Uri("http://www.odjfs.state.oh.us/maintenance/") ||
                r.Headers.Location == new Uri("http://videos.jfs.ohio.gov/maintenance/"))
        }.ToList().AsReadOnly();

        private readonly HttpClient _httpClient;
        private readonly string _userAgent;
        private readonly ILogger<HttpStreamFetcher> _logger;

        public HttpStreamFetcher(ILogger<HttpStreamFetcher> logger, HttpMessageHandler httpMessageHandler, string userAgent)
        {
            // configure HttpClientHandler
            var httpClientHandler = httpMessageHandler as HttpClientHandler;
            if (httpClientHandler != null)
            {
                httpClientHandler.AllowAutoRedirect = false;
                httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                httpClientHandler.UseCookies = false;
            }

            // configure WebRequestHandler
            var webRequestHandler = httpMessageHandler as WebRequestHandler;
            if (webRequestHandler != null)
            {
                webRequestHandler.AllowPipelining = true;
            }

            _httpClient = new HttpClient(httpMessageHandler);
            _userAgent = userAgent;
            _logger = logger;
        }

        public Task<Stream> GetChildCareDocument(ChildCareStub childCareStub)
        {
            if (childCareStub == null)
            {
                throw new ArgumentNullException("childCareStub");
            }
            if (childCareStub.ExternalUrlId == null)
            {
                throw new ArgumentNullException("childCareStub.ExternalUrlId");
            }
            return GetChildCareDocumentWithoutValidation(childCareStub.ExternalUrlId, r => GetChildCareDocumentStream(r, childCareStub));
        }

        public Task<Stream> GetChildCareDocument(ChildCare childCare)
        {
            if (childCare == null)
            {
                throw new ArgumentNullException("childCare");
            }
            if (childCare.ExternalUrlId == null)
            {
                throw new ArgumentNullException("childCare.ExternalUrlId");
            }
            return GetChildCareDocumentWithoutValidation(childCare.ExternalUrlId, r => GetChildCareDocumentStream(r, childCare));
        }

        public Task<Stream> GetChildCareStubListDocument(County county)
        {
            if (county == null)
            {
                throw new ArgumentNullException("county");
            }
            if (county.Name == null)
            {
                throw new ArgumentNullException("county.Name");
            }
            return GetChildCareStubListDocumentWithoutValidation(county);
        }

        private async Task<Stream> GetChildCareStubListDocumentWithoutValidation(County county)
        {
            // create the URL
            string encodedCountyName = HttpUtility.UrlEncode(county.Name);
            var requestUri = new Uri(string.Format("http://www.odjfs.state.oh.us/cdc/results1.asp?county={0}&rating=ALL&Printable=Y&ShowAllPages=Y", encodedCountyName));

            // get the response
            HttpResponseMessage response = await GetHttpResponseMessage(requestUri);

            // get the stream
            Stream responseStream = await GetChildCareStubListDocumentStream(response, county);

            // search to body from some error strings, indicating specific errors
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string responseString = await responseStream.ReadAsStringAsync();

                if (HasTemporaryError(response, responseString))
                {
                    ThrowTemporaryErrorException(requestUri, response);
                }

                ThrowScraperException(requestUri, response);
            }

            return responseStream;
        }

        private async Task<Stream> GetChildCareDocumentWithoutValidation(string externalUrlId, Func<HttpResponseMessage, Task<Stream>> getStream)
        {
            // create the URL
            string encodedExternalUrlId = HttpUtility.UrlEncode(externalUrlId);
            var requestUri = new Uri(string.Format("http://www.odjfs.state.oh.us/cdc/results2.asp?provider_number={0}&Printable=Y", encodedExternalUrlId));

            // get the response
            HttpResponseMessage response = await GetHttpResponseMessage(requestUri);

            // get the stream
            Stream responseStream = await getStream(response);

            // search to body from some error strings, indicating specific errors
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string responseString = await responseStream.ReadAsStringAsync();

                if (HasPermanentError(response, responseString))
                {
                    return null;
                }

                if (HasTemporaryError(response, responseString))
                {
                    ThrowTemporaryErrorException(requestUri, response);
                }

                ThrowScraperException(requestUri, response);
            }

            return responseStream;
        }

        private static bool HasPermanentError(HttpResponseMessage response, string responseString)
        {
            return MatchesAnyTest(PermanentErrorTests, response, responseString);
        }

        private static bool HasTemporaryError(HttpResponseMessage response, string responseString)
        {
            return MatchesAnyTest(TemporaryErrorTests, response, responseString);
        }

        private static bool MatchesAnyTest(IEnumerable<Func<HttpResponseMessage, string, bool>> tests, HttpResponseMessage response, string responseString)
        {
            return tests.Any(t => t(response, responseString));
        }

        private static bool MatchesPattern(IEnumerable<string> pattern, string responseString)
        {
            return pattern.All(responseString.Contains);
        }

        protected virtual Task<Stream> GetChildCareDocumentStream(HttpResponseMessage response, ChildCare childCare)
        {
            return response.Content.ReadAsStreamAsync();
        }

        protected virtual Task<Stream> GetChildCareDocumentStream(HttpResponseMessage response, ChildCareStub childCareStub)
        {
            return response.Content.ReadAsStreamAsync();
        }

        protected virtual Task<Stream> GetChildCareStubListDocumentStream(HttpResponseMessage response, County county)
        {
            return response.Content.ReadAsStreamAsync();
        }

        private void ThrowTemporaryErrorException(Uri requestUri, HttpResponseMessage response)
        {
            var waitException = new HttpRequestException("The response body has indicated that the document is temporarily unavailable.");
            _logger.LogInformation(
                waitException.Message + " RequestUri: '{requestUri}', StatusCode: '{statusCode}'",
                requestUri,
                response.StatusCode);
            throw waitException;
        }

        private void ThrowScraperException(Uri requestUri, HttpResponseMessage response)
        {
            var exception = new ScraperException("The response body or status code was unexpected.");
            _logger.LogError(
                exception.Message + " RequestUri: '{requestUri}', StatusCode: '{statusCode}', Headers: {headers}",
                requestUri,
                response.StatusCode,
                response.Headers);
            throw exception;
        }

        private async Task<HttpResponseMessage> GetHttpResponseMessage(Uri requestUri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            if (_userAgent != null)
            {
                request.Headers.Add("User-Agent", _userAgent);
            }

            _logger.LogInformation("{method} {requestUri}", request.Method, request.RequestUri);
            HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            _logger.LogInformation("{statusCode} {requestUri}", response.StatusCode, request.RequestUri);

            return response;
        }
    }
}
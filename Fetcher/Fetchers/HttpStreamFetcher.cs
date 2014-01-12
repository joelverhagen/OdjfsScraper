using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NLog;
using OdjfsScraper.Fetcher.Support;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Fetcher.Fetchers
{
    public class HttpStreamFetcher : IHttpStreamFetcher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly IEnumerable<IEnumerable<string>> PermanentErrorPatterns = new[]
        {
            new[] {"OraOLEDB", "error '80040e14'", "ORA-01850: hour must be between 0 and 23"},
            new[] {"OraOLEDB", "error '80040e14'", "ORA-01858: a non-numeric character was found where a numeric was expected"},
            new[] {"ADODB.Field", "error '800a0bcd'", "Either BOF or EOF is True, or the current record has been deleted. Requested operation requires a current record."}
        };

        private static readonly IEnumerable<IEnumerable<string>> TemporaryErrorPatterns = new[]
        {
            new[] {"Provider", "error '80004005'", "Unspecified error"},
            new[] {"OraOLEDB", "error '80040e14'", "ORA-03114: not connected to ORACLE"},
            new[] {"OraOLEDB", "error '80004005'", "ORA-01034: ORACLE not available", "ORA-27101: shared memory realm does not exist", "IBM AIX RISC System/6000 Error: 2: No such file or directory"},
            new[] {"OraOLEDB", "error '80004005'", "ORA-01089: immediate shutdown in progress - no operations are permitted"},
            new[] {"OraOLEDB", "error '80004005'", "ORA-01033: ORACLE initialization or shutdown in progress"}
        };

        private readonly ScraperClient _scraperClient;

        public HttpStreamFetcher()
        {
            _scraperClient = new ScraperClient();
        }

        public Task<Stream> GetChildCareDocument(ChildCareStub childCareStub)
        {
            if (childCareStub == null)
            {
                throw new ArgumentNullException("childCareStub");
            }

            return GetChildCareDocument(childCareStub.ExternalUrlId, r => GetChildCareDocumentStream(r, childCareStub));
        }

        public Task<Stream> GetChildCareDocument(ChildCare childCare)
        {
            if (childCare == null)
            {
                throw new ArgumentNullException("childCare");
            }

            return GetChildCareDocument(childCare.ExternalUrlId, r => GetChildCareDocumentStream(r, childCare));
        }

        public async Task<Stream> GetChildCareStubListDocument(County county)
        {
            if (county == null)
            {
                throw new ArgumentNullException("county");
            }

            // create the URL
            var requestUri = new Uri(string.Format("http://www.odjfs.state.oh.us/cdc/results1.asp?county={0}&rating=ALL&Printable=Y&ShowAllPages=Y", county.Name));

            // get the response
            HttpResponseMessage response = await GetHttpResponseMessage(requestUri);

            // get the stream
            Stream stream = await GetChildCareStubListDocumentStream(response, county);

            // search to body from some error strings, indicating specific errors
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string responseString = await stream.ReadAsStringAsync();

                if (HasTemporaryError(response, responseString))
                {
                    ThrowTemporaryErrorException(requestUri, response);
                }

                ThrowScraperException(requestUri, response);
            }

            return await response.Content.ReadAsStreamAsync();
        }

        private async Task<Stream> GetChildCareDocument(string externalUrlId, Func<HttpResponseMessage, Task<Stream>> getStream)
        {
            // create the URL
            var requestUri = new Uri(string.Format("http://www.odjfs.state.oh.us/cdc/results2.asp?provider_number={0}&Printable=Y", externalUrlId));

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
            return MatchesAnyPattern(PermanentErrorPatterns, responseString);
        }

        private static bool HasTemporaryError(HttpResponseMessage response, string responseString)
        {
            return MatchesAnyPattern(TemporaryErrorPatterns, responseString);
        }

        private static bool MatchesAnyPattern(IEnumerable<IEnumerable<string>> patterns, string responseString)
        {
            return patterns.Any(p => p.All(responseString.Contains));
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

        private static void ThrowTemporaryErrorException(Uri requestUri, HttpResponseMessage response)
        {
            var waitException = new HttpRequestException("The response body has indicated that the document is temporarily unavailable.");
            Logger.DebugException(string.Format("RequestUri: '{0}', StatusCode: '{1}'", requestUri, response.StatusCode), waitException);
            throw waitException;
        }

        private static void ThrowScraperException(Uri requestUri, HttpResponseMessage response)
        {
            var exception = new ScraperException("The response body or status code was expected.");
            Logger.ErrorException(string.Format("RequestUri: '{0}', StatusCode: '{1}', Headers: {2}",
                requestUri,
                response.StatusCode,
                response.Headers), exception);
            throw exception;
        }

        private async Task<HttpResponseMessage> GetHttpResponseMessage(Uri requestUri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await _scraperClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            return response;
        }
    }
}
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NLog;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;
using OdjfsScraper.Model.Support;
using OdjfsScraper.Scraper.Parsers;
using OdjfsScraper.Scraper.Support;

namespace OdjfsScraper.Scraper.Scrapers
{
    public class ChildCareScraper : IChildCareFetcher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IHttpReader _httpReader;
        private readonly IChildCareParser _parser;

        public ChildCareScraper(IHttpReader httpReader, IChildCareParser parser)
        {
            _httpReader = httpReader;
            _parser = parser;
        }

        public async Task<ChildCare> Fetch(ChildCareStub childCareStub)
        {
            // make sure we have a URL ID
            if (string.IsNullOrWhiteSpace(childCareStub.ExternalUrlId))
            {
                var exception = new ArgumentNullException("childCareStub", "The provided child care stub has a null external URL ID.");
                Logger.ErrorException(string.Format(
                    "Type: '{0}', ExternalUrlId: '{1}'",
                    childCareStub.GetType(),
                    childCareStub.ExternalUrlId), exception);
                throw exception;
            }

            // fetch the contents
            ClientResponse response = await _httpReader.GetChildCareDocument(childCareStub);
            ValidateClientResponse(response);
            if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.InternalServerError)
            {
                return null;
            }

            // extract the child care information
            return _parser.Parse(childCareStub, response.Content);
        }

        public async Task<ChildCare> Fetch(ChildCare childCare)
        {
            // make sure we have a URL ID
            if (string.IsNullOrWhiteSpace(childCare.ExternalUrlId))
            {
                var exception = new ArgumentNullException("childCare", "The provided child care has a null external URL ID.");
                Logger.ErrorException(string.Format(
                    "Type: '{0}', ExternalUrlId: '{1}'",
                    childCare.GetType(),
                    childCare.ExternalUrlId), exception);
                throw exception;
            }

            // fetch the contents
            ClientResponse response = await _httpReader.GetChildCareDocument(childCare);
            ValidateClientResponse(response);
            if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.InternalServerError)
            {
                return null;
            }

            // extract the child care information
            return _parser.Parse(childCare, response.Content);
        }

        private void ValidateClientResponse(ClientResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK && // the record will be updated (with new parse)
                response.StatusCode != HttpStatusCode.NotFound && // the record will be deleted
                response.StatusCode != HttpStatusCode.InternalServerError) // the record will be deleted
            {
                var exception = new HttpRequestException("A status code that is not 200 or 404 was returned when getting a child care document.");
                Logger.DebugException(string.Format(
                    "RequestUri: '{0}', StatusCode: '{1}'",
                    response.RequestUri,
                    response.StatusCode), exception);
                throw exception;
            }
        }
    }
}
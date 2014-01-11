using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NLog;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCareStubs;
using OdjfsScraper.Model.Support;
using OdjfsScraper.Scraper.Parsers;
using OdjfsScraper.Scraper.Support;

namespace OdjfsScraper.Scraper.Scrapers
{
    public class ChildCareStubListScraper : IChildCareStubListFetcher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IHttpReader _httpReader;
        private readonly IChildCareStubListParser _parser;

        public ChildCareStubListScraper(IHttpReader httpReader, IChildCareStubListParser parser)
        {
            _httpReader = httpReader;
            _parser = parser;
        }

        public async Task<IEnumerable<ChildCareStub>> Fetch(County county)
        {
            // fetch the contents
            HttpResponse response = await _httpReader.GetListDocument(county);
            ValidateClientResponse(response);

            // extract the information from the HTML
            return _parser.Parse(response.Content, county);
        }

        private void ValidateClientResponse(HttpResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var exception = new HttpRequestException("A status code that is not 200 was returned when getting the list document.");
                Logger.DebugException(string.Format("RequestUri: '{0}', StatusCode: '{1}'", response.RequestUri, response.StatusCode), exception);
                throw exception;
            }
        }
    }
}
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NLog;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCareStubs;
using OdjfsScraper.Scraper.Parsers;
using OdjfsScraper.Scraper.Support;

namespace OdjfsScraper.Scraper.Scrapers
{
    public class ChildCareStubListScraper : IChildCareStubListScraper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IOdjfsClient _odjfsClient;
        private readonly IListParser _parser;

        public ChildCareStubListScraper(IOdjfsClient odjfsClient, IListParser parser)
        {
            _odjfsClient = odjfsClient;
            _parser = parser;
        }

        public async Task<IEnumerable<ChildCareStub>> Scrape()
        {
            // fetch the contents
            ClientResponse response = await _odjfsClient.GetListDocument();
            ValidateClientResponse(response);

            // extract the information from the HTML
            return _parser.Parse(response.Content);
        }

        public async Task<IEnumerable<ChildCareStub>> Scrape(County county)
        {
            // fetch the contents
            ClientResponse response = await _odjfsClient.GetListDocument(county);
            ValidateClientResponse(response);

            // extract the information from the HTML
            return _parser.Parse(response.Content, county);
        }

        private void ValidateClientResponse(ClientResponse response)
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
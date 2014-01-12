using System.IO;
using System.Threading.Tasks;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;
using OdjfsScraper.Scraper.Parsers;
using OdjfsScraper.Scraper.Support;

namespace OdjfsScraper.Scraper.Fetchers
{
    public class ChildCareFetcher : IChildCareFetcher
    {
        private readonly IChildCareParser _parser;
        private readonly IStreamFetcher _streamFetcher;

        public ChildCareFetcher(IStreamFetcher streamFetcher, IChildCareParser parser)
        {
            _streamFetcher = streamFetcher;
            _parser = parser;
        }

        public async Task<ChildCare> Fetch(ChildCareStub childCareStub)
        {
            // fetch the stream
            Stream stream = await _streamFetcher.GetChildCareDocument(childCareStub);
            if (stream == null)
            {
                return null;
            }

            // extract the child care information
            return _parser.Parse(childCareStub, await stream.ReadAsByteArrayAsync());
        }

        public async Task<ChildCare> Fetch(ChildCare childCare)
        {
            // fetch the stream
            Stream stream = await _streamFetcher.GetChildCareDocument(childCare);
            if (stream == null)
            {
                return null;
            }

            // extract the child care information
            return _parser.Parse(childCare, await stream.ReadAsByteArrayAsync());
        }
    }
}
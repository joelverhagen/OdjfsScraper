using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OdjfsScraper.Fetcher.Support;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCareStubs;
using OdjfsScraper.Model.Fetchers;
using OdjfsScraper.Parser.Parsers;

namespace OdjfsScraper.Fetcher.Fetchers
{
    public class ChildCareStubListFetcher : IChildCareStubListFetcher
    {
        private readonly IChildCareStubListParser _parser;
        private readonly IStreamFetcher _streamFetcher;

        public ChildCareStubListFetcher(IStreamFetcher streamFetcher, IChildCareStubListParser parser)
        {
            _streamFetcher = streamFetcher;
            _parser = parser;
        }

        public async Task<IEnumerable<ChildCareStub>> Fetch(County county)
        {
            // fetch the stream
            using (Stream stream = await _streamFetcher.GetChildCareStubListDocument(county))
            {
                if (stream == null)
                {
                    return null;
                }

                // extract the information from the HTML
                return _parser.Parse(county, await stream.ReadAsByteArrayAsync());
            }
        }

        public Task<IEnumerable<County>> GetAvailableCounties()
        {
            return _streamFetcher.GetAvailableCounties();
        }
    }
}
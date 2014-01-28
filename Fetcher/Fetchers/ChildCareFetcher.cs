using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OdjfsScraper.Fetcher.Support;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;
using OdjfsScraper.Model.Fetchers;
using OdjfsScraper.Parser.Parsers;

namespace OdjfsScraper.Fetcher.Fetchers
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
            using (Stream stream = await _streamFetcher.GetChildCareDocument(childCareStub))
            {
                if (stream == null)
                {
                    return null;
                }

                // extract the child care information
                return _parser.Parse(childCareStub, await stream.ReadAsByteArrayAsync());
            }
        }

        public Task<IEnumerable<ChildCare>> GetAvailableChildCares()
        {
            return _streamFetcher.GetAvailableChildCares();
        }

        public async Task<ChildCare> Fetch(ChildCare childCare)
        {
            // fetch the stream
            using (Stream stream = await _streamFetcher.GetChildCareDocument(childCare))
            {
                if (stream == null)
                {
                    return null;
                }

                // extract the child care information
                return _parser.Parse(childCare, await stream.ReadAsByteArrayAsync());
            }
        }
    }
}
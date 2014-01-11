using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCareStubs;
using OdjfsScraper.Model.Support;
using OdjfsScraper.Scraper.Parsers;

namespace OdjfsScraper.Scraper.Readers
{
    public class ChildCareStubListReader : IChildCareStubListFetcher
    {
        private readonly IListParser _parser;

        public ChildCareStubListReader(IListParser parser)
        {
            _parser = parser;
        }

        public Task<IEnumerable<ChildCareStub>> Fetch(County county)
        {
            throw new NotImplementedException();
        }
    }
}
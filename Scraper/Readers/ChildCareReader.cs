using System;
using System.Threading.Tasks;
using NLog;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;
using OdjfsScraper.Model.Support;
using OdjfsScraper.Scraper.Parsers;

namespace OdjfsScraper.Scraper.Readers
{
    public class ChildCareReader : IChildCareFetcher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IChildCareParser _parser;

        public ChildCareReader(IChildCareParser parser)
        {
            _parser = parser;
        }

        public Task<ChildCare> Fetch(ChildCare childCare)
        {
            throw new NotImplementedException();
        }

        public Task<ChildCare> Fetch(ChildCareStub childCareStub)
        {
            throw new NotImplementedException();
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Scraper.Scrapers
{
    public interface IChildCareStubListScraper
    {
        Task<IEnumerable<ChildCareStub>> Scrape();
        Task<IEnumerable<ChildCareStub>> Scrape(County county);
    }
}
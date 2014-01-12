using System.Collections.Generic;
using System.Threading.Tasks;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Scraper.Fetchers
{
    public interface IChildCareStubListFetcher
    {
        Task<IEnumerable<ChildCareStub>> Fetch(County county);
    }
}
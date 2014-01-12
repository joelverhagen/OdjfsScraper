using System.Threading.Tasks;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Scraper.Fetchers
{
    public interface IChildCareFetcher
    {
        Task<ChildCare> Fetch(ChildCare childCare);
        Task<ChildCare> Fetch(ChildCareStub childCareStub);
    }
}
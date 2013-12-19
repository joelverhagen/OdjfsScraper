using System.Threading.Tasks;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Scraper.Scrapers
{
    public interface IChildCareScraper
    {
        Task<ChildCare> Scrape(ChildCareStub childCareStub);
        Task<ChildCare> Scrape(ChildCare childCare);
    }
}
using System.Threading.Tasks;
using OdjfsScraper.Models;

namespace OdjfsScraper.Fetchers
{
    public interface IChildCareFetcher
    {
        Task<ChildCare> Fetch(ChildCare childCare);
        Task<ChildCare> Fetch(ChildCareStub childCareStub);
    }
}
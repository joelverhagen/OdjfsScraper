using System.Threading.Tasks;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Model.Support
{
    public interface IChildCareFetcher
    {
        Task<ChildCare> Fetch(ChildCare childCare);
        Task<ChildCare> Fetch(ChildCareStub childCareStub);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Model.Fetchers
{
    public interface IChildCareStubListFetcher
    {
        Task<IEnumerable<ChildCareStub>> Fetch(County county);
        Task<IEnumerable<County>> GetAvailableCounties();
    }
}
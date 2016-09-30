using System.Collections.Generic;
using System.Threading.Tasks;
using OdjfsScraper.Models;

namespace OdjfsScraper.Fetchers
{
    public interface IChildCareStubListFetcher
    {
        Task<IEnumerable<ChildCareStub>> Fetch(County county);
    }
}
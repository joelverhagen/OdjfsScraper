using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;

namespace OdjfsScraper.Fetcher.Fetchers
{
    public interface IFileSystemStreamFetcher : IStreamFetcher
    {
        string Directory { get; set; }
        Task<IEnumerable<County>> GetAvailableCounties();
        Task<IEnumerable<ChildCare>> GetAvailableChildCares();
    }
}
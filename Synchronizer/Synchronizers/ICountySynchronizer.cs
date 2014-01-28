using System.Threading.Tasks;
using OdjfsScraper.Database;

namespace OdjfsScraper.Synchronizer.Synchronizers
{
    public interface ICountySynchronizer
    {
        Task UpdateNextCounty(Entities ctx);
        Task UpdateCounty(Entities ctx, string name);
        Task UpdateAvailableCounties(Entities ctx);
    }
}
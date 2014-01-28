using System.Threading.Tasks;
using OdjfsScraper.Database;

namespace OdjfsScraper.Synchronizer.Synchronizers
{
    public interface IChildCareSynchronizer
    {
        Task UpdateNextChildCare(Entities ctx);
        Task UpdateChildCare(Entities ctx, string externalUrlId);
        Task UpdateAvailableChildCares(Entities ctx);
    }
}
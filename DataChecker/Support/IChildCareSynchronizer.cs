using System.Threading.Tasks;
using OdjfsScraper.Database;

namespace OdjfsScraper.DataChecker.Support
{
    public interface IChildCareSynchronizer
    {
        Task UpdateNextChildCare(Entities ctx);
        Task UpdateChildCare(Entities ctx, string externalUrlId);
    }
}
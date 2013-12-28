using System.Threading.Tasks;
using OdjfsScraper.Database;

namespace OdjfsScraper.DataChecker.Support
{
    public interface IOdjfsSynchronizer
    {
        Task UpdateNextChildCare(Entities ctx);
        Task UpdateChildCare(Entities ctx, string externalUrlId);
        Task UpdateNextCounty(Entities ctx);
        Task UpdateCounty(Entities ctx, string name);
        Task<bool> NeedsGeocoding(Entities ctx);
        Task GeocodeChildCare(Entities ctx, string externalUrlId, string mapQuestKey);
        Task GeocodeNextChildCare(Entities ctx, string mapQuestKey);
    }
}
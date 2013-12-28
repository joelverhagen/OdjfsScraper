using System.Threading.Tasks;
using OdjfsScraper.Database;

namespace OdjfsScraper.DataChecker.Support
{
    public interface IGeocoder
    {
        Task<bool> NeedsGeocoding(Entities ctx);
        Task GeocodeChildCare(Entities ctx, string externalUrlId, string mapQuestKey);
        Task GeocodeNextChildCare(Entities ctx, string mapQuestKey);
    }
}
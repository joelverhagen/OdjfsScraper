using System.Threading.Tasks;
using OdjfsScraper.Database;

namespace OdjfsScraper.Tool.Support
{
    public interface IGeocoder
    {
        Task<bool> NeedsGeocoding(OdjfsContext ctx);
        Task GeocodeChildCare(OdjfsContext ctx, string externalUrlId, string mapQuestKey);
        Task GeocodeNextChildCare(OdjfsContext ctx, string mapQuestKey);
    }
}
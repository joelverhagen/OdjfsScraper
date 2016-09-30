using System.Threading.Tasks;
using OdjfsScraper.Database;

namespace OdjfsScraper.Synchronize
{
    public interface IChildCareSynchronizer
    {
        Task UpdateNextChildCare(OdjfsContext ctx);
        Task UpdateChildCare(OdjfsContext ctx, string externalUrlId);
    }
}
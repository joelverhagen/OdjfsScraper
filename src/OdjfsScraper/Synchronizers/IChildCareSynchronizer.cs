using System.Threading.Tasks;
using OdjfsScraper.Database;

namespace OdjfsScraper.Synchronizers
{
    public interface IChildCareSynchronizer
    {
        Task UpdateNextChildCare(OdjfsContext ctx);
        Task UpdateChildCare(OdjfsContext ctx, string externalUrlId);
    }
}
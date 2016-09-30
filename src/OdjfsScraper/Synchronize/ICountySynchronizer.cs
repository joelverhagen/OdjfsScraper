using System.Threading.Tasks;
using OdjfsScraper.Database;

namespace OdjfsScraper.Synchronize
{
    public interface ICountySynchronizer
    {
        Task UpdateNextCounty(OdjfsContext ctx);
        Task UpdateCounty(OdjfsContext ctx, string name);
    }
}
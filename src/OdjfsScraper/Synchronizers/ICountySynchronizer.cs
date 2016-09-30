using System.Threading.Tasks;
using OdjfsScraper.Database;

namespace OdjfsScraper.Synchronizers
{
    public interface ICountySynchronizer
    {
        Task UpdateNextCounty(OdjfsContext ctx);
        Task UpdateCounty(OdjfsContext ctx, string name);
    }
}
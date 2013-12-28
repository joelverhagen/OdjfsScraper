using System.Threading.Tasks;
using OdjfsScraper.Database;

namespace OdjfsScraper.DataChecker.Support
{
    public interface ICountySynchronizer
    {
        Task UpdateNextCounty(Entities ctx);
        Task UpdateCounty(Entities ctx, string name);
    }
}
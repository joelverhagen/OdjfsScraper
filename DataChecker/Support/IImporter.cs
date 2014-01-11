using System.Threading.Tasks;

namespace OdjfsScraper.DataChecker.Support
{
    public interface IImporter
    {
        Task Import(string path);
    }
}
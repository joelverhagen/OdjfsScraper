using System.Threading.Tasks;

namespace OdjfsScraper.Tool.Support
{
    public interface IImporter
    {
        Task Import(string path);
    }
}
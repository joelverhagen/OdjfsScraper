using System.IO;
using System.Threading.Tasks;

namespace OdjfsScraper.Fetcher.Support
{
    public interface IFileSystemBlobStore
    {
        string FileExtension { get; set; }
        string FieldSeperator { get; set; }
        void SetDirectory(string directory);
        void VerifyDirectory();
        Task Write(string name, string tag, Stream stream);
        Task<Stream> Read(string name, int versionIndex);
    }
}
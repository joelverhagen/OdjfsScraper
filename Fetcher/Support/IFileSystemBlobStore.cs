using System.Collections;
using System.IO;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.UI;

namespace OdjfsScraper.Fetcher.Support
{
    public interface IFileSystemBlobStore
    {
        string FileExtension { get; set; }
        string FieldSeperator { get; set; }
        void SetDirectory(string directory);
        Task Write(string name, string tag, Stream stream);
        Task<Stream> Read(string name, int versionIndex);
    }
}
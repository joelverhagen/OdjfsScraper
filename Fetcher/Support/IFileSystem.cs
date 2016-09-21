using System.Collections.Generic;
using System.IO;

namespace OdjfsScraper.Fetcher.Support
{
    public interface IFileSystem
    {
        IEnumerable<string> DirectoryEnumerateFiles(string path, string searchPattern, SearchOption searchOption);
        bool DirectoryExists(string path);
        DirectoryInfo DirectoryCreateDirectory(string path);
        bool FileExists(string path);
        void FileMove(string sourceFileName, string destFileName);
        Stream FileOpen(string path, FileMode mode);
    }
}
using System.Collections.Generic;
using System.IO;

namespace OdjfsScraper.Fetcher.Support
{
    public class FileSystem : IFileSystem
    {
        public IEnumerable<string> DirectoryEnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.EnumerateFiles(path, searchPattern, searchOption);
        }

        public DirectoryInfo DirectoryCreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public void FileMove(string sourceFileName, string destFileName)
        {
            File.Move(sourceFileName, destFileName);
        }

        public Stream FileOpen(string path, FileMode mode)
        {
            return File.Open(path, mode);
        }
    }
}
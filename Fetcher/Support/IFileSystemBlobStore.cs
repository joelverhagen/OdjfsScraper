using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OdjfsScraper.Fetcher.Support
{
    public interface IFileSystemBlobStore
    {
        string FileExtension { get; set; }
        string FieldSeperator { get; set; }
        string Directory { get; set; }
        void VerifyDirectory();
        Task Write(string name, string tag, Stream stream);
        Task<Stream> Read(string name, int versionIndex);
        Task<IDictionary<string, int>> GetNames();
    }
}
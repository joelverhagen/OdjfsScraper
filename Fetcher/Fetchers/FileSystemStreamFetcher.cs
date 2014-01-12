using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OdjfsScraper.Fetcher.Support;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Fetcher.Fetchers
{
    public class FileSystemStreamFetcher : IFileSystemStreamFetcher
    {
        private readonly IFileSystem _fileSystem;
        private IDictionary<string, string> _childCarePaths;
        private IDictionary<string, string> _countyPaths;
        private string _directory;

        public FileSystemStreamFetcher(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public Task<Stream> GetChildCareDocument(ChildCareStub childCareStub)
        {
            if (childCareStub == null)
            {
                throw new ArgumentNullException("childCareStub");
            }
            return GetChildCareDocument(childCareStub.ExternalUrlId);
        }

        public Task<Stream> GetChildCareDocument(ChildCare childCare)
        {
            if (childCare == null)
            {
                throw new ArgumentNullException("childCare");
            }
            return GetChildCareDocument(childCare.ExternalUrlId);
        }

        public Task<Stream> GetChildCareStubListDocument(County county)
        {
            if (county == null)
            {
                throw new ArgumentNullException("county");
            }
            return GetChildCareStubListDocument(county.Name);
        }

        public void SetDirectory(string directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }

            _directory = directory;
            _childCarePaths = null;
            _countyPaths = null;
        }

        private Task<Stream> GetChildCareDocument(string externalUrlId)
        {
            if (externalUrlId == null)
            {
                throw new ArgumentNullException("externalUrlId");
            }
            ExploreDirectory();

            // get the path
            string path;
            if (!_childCarePaths.TryGetValue(externalUrlId, out path))
            {
                return null;
            }

            return GetFileStream(path);
        }

        private Task<Stream> GetChildCareStubListDocument(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            ExploreDirectory();

            // get the path
            string path;
            if (!_countyPaths.TryGetValue(name, out path))
            {
                return null;
            }

            return GetFileStream(path);
        }

        private Task<Stream> GetFileStream(string path)
        {
            return Task.FromResult(_fileSystem.FileOpen(path, FileMode.Open));
        }

        private void ExploreDirectory()
        {
            if (_childCarePaths != null && _countyPaths != null)
            {
                return;
            }

            if (_directory == null)
            {
                throw new InvalidOperationException("The directory has not been set.");
            }

            IEnumerable<string> paths = _fileSystem.DirectoryEnumerateFiles(_directory, "*.html", SearchOption.TopDirectoryOnly);
            IDictionary<string, string> childCarePaths = new Dictionary<string, string>();
            IDictionary<string, string> countyPaths = new Dictionary<string, string>();

            foreach (string path in paths)
            {
                // parse the file name
                string fileName = Path.GetFileNameWithoutExtension(path);
                if (fileName == null)
                {
                    continue;
                }

                // split the pieces of the file name
                string[] tokens = fileName.Split('_');
                if (tokens.Length != 5)
                {
                    continue;
                }

                // only keeps track of the current version of the document
                if (tokens[2] != "Current")
                {
                    continue;
                }

                // select the dictionary to load
                IDictionary<string, string> dictionary;
                if (tokens[0] == "ChildCare")
                {
                    dictionary = childCarePaths;
                }
                else if (tokens[0] == "County")
                {
                    dictionary = countyPaths;
                }
                else
                {
                    continue;
                }

                // load the path
                dictionary[tokens[1]] = path;
            }

            // we're done
            _childCarePaths = childCarePaths;
            _countyPaths = countyPaths;
        }
    }
}
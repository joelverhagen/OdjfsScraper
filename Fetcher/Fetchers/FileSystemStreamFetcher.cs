using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OdjfsScraper.Fetcher.Support;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Fetcher.Fetchers
{
    public class FileSystemStreamFetcher : IFileSystemStreamFetcher
    {
        private readonly IFileSystemBlobStore _fileSystemBlobStore;

        public FileSystemStreamFetcher(IFileSystemBlobStore fileSystemBlobStore)
        {
            _fileSystemBlobStore = fileSystemBlobStore;
        }

        public Task<Stream> GetChildCareDocument(ChildCareStub childCareStub)
        {
            if (childCareStub == null)
            {
                throw new ArgumentNullException("childCareStub");
            }
            if (childCareStub.ExternalUrlId == null)
            {
                throw new ArgumentNullException("childCareStub.ExternalUrlId");
            }
            return GetChildCareDocument(childCareStub.ExternalUrlId);
        }

        public Task<Stream> GetChildCareDocument(ChildCare childCare)
        {
            if (childCare == null)
            {
                throw new ArgumentNullException("childCare");
            }
            if (childCare.ExternalUrlId == null)
            {
                throw new ArgumentNullException("childCare.ExternalUrlId");
            }
            return GetChildCareDocument(childCare.ExternalUrlId);
        }

        public Task<Stream> GetChildCareStubListDocument(County county)
        {
            if (county == null)
            {
                throw new ArgumentNullException("county");
            }
            if (county.Name == null)
            {
                throw new ArgumentNullException("county.Name");
            }
            VerifyDirectory();
            return _fileSystemBlobStore.Read(string.Format("County-{0}", county.Name.ToUpper()), -1);
        }

        public string Directory
        {
            get { return _fileSystemBlobStore.Directory; }
            set { _fileSystemBlobStore.Directory = value; }
        }

        public Task<IEnumerable<County>> GetAvailableCounties()
        {
            return GetAllEntities("County", countyName => new County {Name = countyName});
        }

        public Task<IEnumerable<ChildCare>> GetAvailableChildCares()
        {
            return GetAllEntities("ChildCare", externalUrlId => new ChildCare {ExternalUrlId = externalUrlId});
        }

        private async Task<IEnumerable<T>> GetAllEntities<T>(string prefix, Func<string, T> createEntity)
        {
            prefix += "-";

            return (await _fileSystemBlobStore.GetNames())
                .Select(p => p.Key)
                .Where(s => s.StartsWith(prefix))
                .Select(s => s.Substring(prefix.Length))
                .Select(createEntity);
        }

        private void VerifyDirectory()
        {
            if (Directory == null)
            {
                throw new InvalidOperationException("The directory has not been set.");
            }
        }

        private Task<Stream> GetChildCareDocument(string externalUrlId)
        {
            VerifyDirectory();
            return _fileSystemBlobStore.Read(string.Format("ChildCare-{0}", externalUrlId.ToUpper()), -1);
        }
    }
}
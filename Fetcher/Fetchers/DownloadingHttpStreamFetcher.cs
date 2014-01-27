using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using OdjfsScraper.Fetcher.Support;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Fetcher.Fetchers
{
    public class DownloadingHttpStreamFetcher : HttpStreamFetcher
    {
        private readonly IFileSystemBlobStore _fileSystemBlobStore;

        public DownloadingHttpStreamFetcher(HttpMessageHandler httpMessageHandler, string userAgent, IFileSystemBlobStore fileSystemBlobStore) : base(httpMessageHandler, userAgent)
        {
            _fileSystemBlobStore = fileSystemBlobStore;
        }

        protected override Task<Stream> GetChildCareDocumentStream(HttpResponseMessage response, ChildCare childCare)
        {
            return GetChildCareDocumentStream(childCare.ExternalUrlId, response.StatusCode, () => base.GetChildCareDocumentStream(response, childCare));
        }

        protected override Task<Stream> GetChildCareDocumentStream(HttpResponseMessage response, ChildCareStub childCareStub)
        {
            return GetChildCareDocumentStream(childCareStub.ExternalUrlId, response.StatusCode, () => base.GetChildCareDocumentStream(response, childCareStub));
        }

        private async Task<Stream> GetChildCareDocumentStream(string externalUrlId, HttpStatusCode httpStatusCode, Func<Task<Stream>> getStream)
        {
            // get the actual stream
            Stream stream = await getStream();

            // write the stream to disk and read then read from disk
            return await WriteAndGetStream(string.Format("ChildCare-{0}", externalUrlId), httpStatusCode, stream);
        }

        protected override async Task<Stream> GetChildCareStubListDocumentStream(HttpResponseMessage response, County county)
        {
            // get the actual stream
            Stream stream = await base.GetChildCareStubListDocumentStream(response, county);

            // write the stream to disk and read then read from disk
            return await WriteAndGetStream(string.Format("County-{0}", county.Name), response.StatusCode, stream);
        }

        private async Task<Stream> WriteAndGetStream(string name, HttpStatusCode httpStatusCode, Stream stream)
        {
            // write the blob
            await _fileSystemBlobStore.Write(name, httpStatusCode.ToString(), stream);

            // get the blob back
            return await _fileSystemBlobStore.Read(name, -1);
        }
    }
}
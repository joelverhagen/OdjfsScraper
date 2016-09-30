using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OdjfsScraper.Models;

namespace OdjfsScraper.Fetchers
{
    public class DownloadingHttpStreamFetcher : HttpStreamFetcher
    {
        private readonly IBlobStore _blobStore;

        public DownloadingHttpStreamFetcher(ILogger<DownloadingHttpStreamFetcher> logger, HttpMessageHandler httpMessageHandler, string userAgent, IBlobStore blobStore)
            : base(logger, httpMessageHandler, userAgent)
        {
            _blobStore = blobStore;
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
            return await WriteAndGetStream(string.Format("ChildCare/{0}", externalUrlId), httpStatusCode, stream);
        }

        protected override async Task<Stream> GetChildCareStubListDocumentStream(HttpResponseMessage response, County county)
        {
            // get the actual stream
            Stream stream = await base.GetChildCareStubListDocumentStream(response, county);

            // write the stream to disk and read then read from disk
            return await WriteAndGetStream(string.Format("County/{0}", county.Name), response.StatusCode, stream);
        }

        private async Task<Stream> WriteAndGetStream(string name, HttpStatusCode httpStatusCode, Stream stream)
        {
            var tag = httpStatusCode.ToString();

            return await _blobStore.WriteAsync(name, tag, stream);
        }
    }
}
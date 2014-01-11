using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Scraper.Support
{
    public class HttpReader : IHttpReader
    {
        private readonly ScraperClient _scraperClient;

        public HttpReader()
        {
            _scraperClient = new ScraperClient();
        }

        public async Task<HttpResponse> GetChildCareDocument(ChildCareStub childCareStub)
        {
            // geth the document
            HttpResponse response = await GetChildCareDocument(childCareStub.ExternalUrlId);

            // execute implementation-specific code
            await HandleChildCareDocumentResponse(childCareStub, response);

            return response;
        }

        public async Task<HttpResponse> GetChildCareDocument(ChildCare childCare)
        {
            // get the document
            HttpResponse response = await GetChildCareDocument(childCare.ExternalUrlId);

            // execute implementation-specific code
            await HandleChildCareDocumentResponse(childCare, response);

            return response;
        }

        public async Task<HttpResponse> GetListDocument()
        {
            return await GetListDocument(null, 0);
        }

        public async Task<HttpResponse> GetListDocument(int zipCode)
        {
            return await GetListDocument(null, zipCode);
        }

        public async Task<HttpResponse> GetListDocument(County county)
        {
            return await GetListDocument(county, 0);
        }

        public async Task<HttpResponse> GetListDocument(County county, int zipCode)
        {
            // create the query parameter
            string countyQueryParameter = county == null ? string.Empty : string.Format("county={0}&", county.Name);
            string zipCodeQueryParameter = zipCode == 0 ? string.Empty : string.Format("Zip={0}&", zipCode);

            // create the URL
            var requestUri = new Uri(string.Format("http://www.odjfs.state.oh.us/cdc/results1.asp?{0}{1}rating=ALL&Printable=Y&ShowAllPages=Y", countyQueryParameter, zipCodeQueryParameter));

            // fetch the bytes
            HttpResponse response = await GetResponse(requestUri);

            // execute the implementation-specific code
            await HandleListDocumentResponse(county, response);

            return response;
        }

        private async Task<HttpResponse> GetChildCareDocument(string externalUrlId)
        {
            // create the URL
            var requestUri = new Uri(string.Format("http://www.odjfs.state.oh.us/cdc/results2.asp?provider_number={0}&Printable=Y", externalUrlId));

            // fetch the bytes
            HttpResponse response = await GetResponse(requestUri);

            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                string responseString = Encoding.UTF8.GetString(response.Content);
                if (responseString.Contains("error '800a0bcd'"))
                {
                    // missing/deleted record
                    response.StatusCode = HttpStatusCode.NotFound;
                }
                else if (responseString.Contains("error '80040e14'"))
                {
                    // an actual error from ODJFS code
                    response.StatusCode = HttpStatusCode.InternalServerError;
                }
            }

            return response;
        }

        private async Task<HttpResponse> GetResponse(Uri requestUri)
        {
            // get the response bytes
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await _scraperClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            HttpResponse httpResponse = await HttpResponse.Create(requestUri, response);

            // 500 returned are usually 503, e.g. the Oracle database is shutting down or is shut down
            if (httpResponse.StatusCode == HttpStatusCode.InternalServerError)
            {
                httpResponse.StatusCode = HttpStatusCode.ServiceUnavailable;
            }

            return httpResponse;
        }

        protected virtual Task HandleChildCareDocumentResponse(ChildCare childCare, HttpResponse response)
        {
            return Task.FromResult(0);
        }

        protected virtual Task HandleChildCareDocumentResponse(ChildCareStub childCareStub, HttpResponse response)
        {
            return Task.FromResult(0);
        }

        protected virtual Task HandleListDocumentResponse(County county, HttpResponse response)
        {
            return Task.FromResult(0);
        }
    }
}
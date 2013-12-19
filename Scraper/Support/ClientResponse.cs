using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OdjfsScraper.Scraper.Support
{
    public class ClientResponse
    {
        public Uri RequestUri { get; set; }
        public ClientResponseHeaders Headers { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public byte[] Content { get; set; }

        public static async Task<ClientResponse> Create(Uri requestUri, HttpResponseMessage response)
        {
            var output = new ClientResponse();

            // download the body
            output.Content = await response.Content.ReadAsByteArrayAsync();

            // copy over the headers
            output.Headers = new ClientResponseHeaders();
            foreach (var group in response.Headers.GroupBy(p => p.Key, p => p.Value))
            {
                output.Headers[group.Key] = group.SelectMany(v => v).ToArray();
            }

            // copy over the rest of the properties
            output.RequestUri = requestUri;
            output.StatusCode = response.StatusCode;

            return output;
        }
    }
}
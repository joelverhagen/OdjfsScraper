using System.IO;
using System.Text;
using System.Threading.Tasks;
using Knapcode.ToStorage.Core.AzureBlobStorage;
using Newtonsoft.Json;

namespace OdjfsScraper.Fetchers
{
    public class ToStorageBlobStore : IBlobStore
    {
        private const string ContentType = "text/plain";
        private const string PathFormatFormat = "{0}/{{0}}.txt";

        private readonly IClient _client;
        private readonly string _connectionString;
        private readonly string _container;
        private readonly IUniqueClient _uniqueClient;

        public ToStorageBlobStore(IUniqueClient uniqueClient, IClient client, string connectionString, string container)
        {
            _connectionString = connectionString;
            _container = container;
            _uniqueClient = uniqueClient;
            _client = client;
        }

        public async Task<Stream> WriteAsync(string name, string tag, Stream stream)
        {
            var header = new BlobHeader { Tag = tag };
            var headerJson = JsonConvert.SerializeObject(header);
            var headerJsonBytes = Encoding.UTF8.GetBytes(headerJson);

            var bufferedStream = new MemoryStream();
            await stream.CopyToAsync(bufferedStream);
            bufferedStream.Seek(0, SeekOrigin.Begin);

            var taggedStream = new MemoryStream();
            taggedStream.Write(headerJsonBytes, 0, headerJsonBytes.Length);
            await bufferedStream.CopyToAsync(taggedStream);

            bufferedStream.Seek(0, SeekOrigin.Begin);
            taggedStream.Seek(0, SeekOrigin.Begin);

            var request = new UniqueUploadRequest
            {
                ConnectionString = _connectionString,
                Container = _container,
                ContentType = "text/plain",
                PathFormat = string.Format(PathFormatFormat, name),
                UploadDirect = true,
                Stream = taggedStream,
                Type = UploadRequestType.Number,
                Trace = TextWriter.Null
            };

            await _uniqueClient.UploadAsync(request);

            return bufferedStream;
        }

        private class BlobHeader
        {
            public string Tag { get; set; }
        }
    }
}

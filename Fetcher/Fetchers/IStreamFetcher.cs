using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Fetcher.Fetchers
{
    public interface IStreamFetcher
    {
        Task<Stream> GetChildCareDocument(ChildCareStub childCareStub);
        Task<Stream> GetChildCareDocument(ChildCare childCare);
        Task<Stream> GetChildCareStubListDocument(County county);
    }
}
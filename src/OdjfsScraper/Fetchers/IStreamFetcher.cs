using System.IO;
using System.Threading.Tasks;
using OdjfsScraper.Models;

namespace OdjfsScraper.Fetchers
{
    public interface IStreamFetcher
    {
        Task<Stream> GetChildCareDocument(ChildCareStub childCareStub);
        Task<Stream> GetChildCareDocument(ChildCare childCare);
        Task<Stream> GetChildCareStubListDocument(County county);
    }
}
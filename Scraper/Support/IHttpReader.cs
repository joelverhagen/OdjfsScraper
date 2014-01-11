using System.Threading.Tasks;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Scraper.Support
{
    public interface IHttpReader
    {
        Task<ClientResponse> GetChildCareDocument(ChildCareStub childCareStub);
        Task<ClientResponse> GetChildCareDocument(ChildCare childCare);
        Task<ClientResponse> GetListDocument();
        Task<ClientResponse> GetListDocument(int zipCode);
        Task<ClientResponse> GetListDocument(County county);
        Task<ClientResponse> GetListDocument(County county, int zipCode);
    }
}
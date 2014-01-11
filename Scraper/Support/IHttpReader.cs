using System.Threading.Tasks;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Scraper.Support
{
    public interface IHttpReader
    {
        Task<HttpResponse> GetChildCareDocument(ChildCareStub childCareStub);
        Task<HttpResponse> GetChildCareDocument(ChildCare childCare);
        Task<HttpResponse> GetListDocument(County county);
    }
}
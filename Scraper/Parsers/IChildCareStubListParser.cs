using System.Collections.Generic;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Scraper.Parsers
{
    public interface IChildCareStubListParser
    {
        IEnumerable<ChildCareStub> Parse(County county, byte[] bytes);
    }
}
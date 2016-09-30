using System.Collections.Generic;
using OdjfsScraper.Models;

namespace OdjfsScraper.Parse
{
    public interface IChildCareStubListParser
    {
        IEnumerable<ChildCareStub> Parse(County county, byte[] bytes);
    }
}
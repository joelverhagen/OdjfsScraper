using System;
using System.Collections.Generic;
using System.Linq;
using OdjfsScraper.Model.ChildCares;

namespace OdjfsScraper.Parser.UnitTests.Parsers.TestSupport
{
    public class TypeAHomeTemplate : DetailedChildCareHomeTemplate<TypeAHome>
    {
        public TypeAHomeTemplate() : base(Enumerable.Empty<KeyValuePair<string, Func<TypeAHome, string>>>())
        {
        }
    }
}
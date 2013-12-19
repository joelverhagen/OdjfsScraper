using System;
using System.Collections.Generic;
using System.Linq;
using OdjfsScraper.Model.ChildCares;

namespace OdjfsScraper.Scraper.UnitTests.Parsers.TestSupport
{
    public class LicensedCenterTemplate : DetailedChildCareHomeTemplate<LicensedCenter>
    {
        public LicensedCenterTemplate() : base(Enumerable.Empty<KeyValuePair<string, Func<LicensedCenter, string>>>())
        {
        }
    }
}
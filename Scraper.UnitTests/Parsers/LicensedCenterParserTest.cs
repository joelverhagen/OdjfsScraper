using Microsoft.VisualStudio.TestTools.UnitTesting;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Scraper.Parsers;
using OdjfsScraper.Scraper.UnitTests.Parsers.TestSupport;

namespace OdjfsScraper.Scraper.UnitTests.Parsers
{
    [TestClass]
    public class LicensedCenterParserTest : BaseDetailedChildCareParserTest<LicensedCenter, LicensedCenterTemplate, LicensedCenterParser>
    {
    }
}
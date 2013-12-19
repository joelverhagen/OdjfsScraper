using Microsoft.VisualStudio.TestTools.UnitTesting;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Scraper.Parsers;
using OdjfsScraper.Scraper.UnitTests.Parsers.TestSupport;

namespace OdjfsScraper.Scraper.UnitTests.Parsers
{
    [TestClass]
    public class DayCampParserTest : BaseChildCareParserTest<DayCamp, DayCampTemplate, DayCampParser>
    {
        protected override void VerifyAreEqual(DayCamp expected, DayCamp actual)
        {
            base.VerifyAreEqual(expected, actual);

            Assert.AreEqual(expected.RegistrationStatus, actual.RegistrationStatus);
            Assert.AreEqual(expected.Owner, actual.Owner);
            Assert.AreEqual(expected.RegistrationBeginDate, actual.RegistrationBeginDate);
            Assert.AreEqual(expected.RegistrationEndDate, actual.RegistrationEndDate);
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Scraper.Parsers;
using OdjfsScraper.Scraper.UnitTests.Parsers.TestSupport;

namespace OdjfsScraper.Scraper.UnitTests.Parsers
{
    [TestClass]
    public class TypeBHomeParserTest : BaseChildCareParserTest<TypeBHome, TypeBHomeTemplate, TypeBHomeParser>
    {
        protected override void VerifyAreEqual(TypeBHome expected, TypeBHome actual)
        {
            base.VerifyAreEqual(expected, actual);

            Assert.AreEqual(expected.CertificationBeginDate, actual.CertificationBeginDate);
            Assert.AreEqual(expected.CertificationExpirationDate, actual.CertificationExpirationDate);
        }
    }
}
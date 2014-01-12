using Microsoft.VisualStudio.TestTools.UnitTesting;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Parser.Parsers;
using OdjfsScraper.Parser.UnitTests.Parsers.TestSupport;

namespace OdjfsScraper.Parser.UnitTests.Parsers
{
    [TestClass]
    public class TypeAHomeParserTest : BaseDetailedChildCareParserTest<TypeAHome, TypeAHomeTemplate, TypeAHomeParser>
    {
    }
}
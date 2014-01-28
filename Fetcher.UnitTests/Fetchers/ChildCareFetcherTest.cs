using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OdjfsScraper.Fetcher.Fetchers;
using OdjfsScraper.Fetcher.UnitTests.Fetchers.TestSupport;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;
using OdjfsScraper.Parser.Parsers;

namespace OdjfsScraper.Fetcher.UnitTests.Fetchers
{
    [TestClass]
    public class ChildCareFetcherTest : BaseEntityFetcherTest
    {
        [TestMethod]
        public void GetChildCare_ChildCare_Null()
        {
            // ARRANGE
            var fetcher = new ChildCareFetcher(GetStreamFetcherReturningNull(), null);

            // ACT
            ChildCare childCare = fetcher.Fetch(new ChildCare()).Result;

            // ASSERT
            Assert.IsNull(childCare);
        }

        [TestMethod]
        public void GetChildCare_ChildCareStub_Null()
        {
            // ARRANGE
            var fetcher = new ChildCareFetcher(GetStreamFetcherReturningNull(), null);

            // ACT
            ChildCare childCare = fetcher.Fetch(new ChildCareStub()).Result;

            // ASSERT
            Assert.IsNull(childCare);
        }

        [TestMethod]
        public void GetChildCare_ChildCare_NotNull()
        {
            // ARRANGE
            var expectedChildCare = new ChildCare();
            byte[] expectedBytes = Encoding.UTF8.GetBytes("This is the document");

            var parserMock = new Mock<IChildCareParser>();
            parserMock
                .Setup(c => c.Parse(It.IsAny<ChildCare>(), It.IsAny<byte[]>()))
                .Returns(expectedChildCare)
                .Callback<ChildCare, byte[]>((stub, actualBytes) => Assert.IsTrue(expectedBytes.SequenceEqual(actualBytes)));

            var fetcher = new ChildCareFetcher(GetStreamFetcherReturningDocument(expectedBytes), parserMock.Object);

            // ACT
            ChildCare actualChildCare = fetcher.Fetch(new ChildCare()).Result;

            // ASSERT
            Assert.AreSame(expectedChildCare, actualChildCare);
        }

        [TestMethod]
        public void GetChildCare_ChildCareStub_NotNull()
        {
            // ARRANGE
            var expectedChildCare = new ChildCare();
            byte[] expectedBytes = Encoding.UTF8.GetBytes("This is the document");

            var parserMock = new Mock<IChildCareParser>();
            parserMock
                .Setup(c => c.Parse(It.IsAny<ChildCareStub>(), It.IsAny<byte[]>()))
                .Returns(expectedChildCare)
                .Callback<ChildCareStub, byte[]>((stub, actualBytes) => Assert.IsTrue(expectedBytes.SequenceEqual(actualBytes)));

            var fetcher = new ChildCareFetcher(GetStreamFetcherReturningDocument(expectedBytes), parserMock.Object);

            // ACT
            ChildCare actualChildCare = fetcher.Fetch(new ChildCareStub()).Result;

            // ASSERT
            Assert.AreSame(expectedChildCare, actualChildCare);
        }
    }
}
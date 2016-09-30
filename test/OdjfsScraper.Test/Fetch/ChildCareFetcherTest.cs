using System.Linq;
using System.Text;
using Moq;
using OdjfsScraper.Fetch;
using OdjfsScraper.Models;
using OdjfsScraper.Parse;
using Xunit;

namespace OdjfsScraper.Test.Fetch
{
    public class ChildCareFetcherTest : BaseEntityFetcherTest
    {
        [Fact]
        public void GetChildCare_ChildCare_Null()
        {
            // ARRANGE
            var fetcher = new ChildCareFetcher(GetStreamFetcherReturningNull(), null);

            // ACT
            ChildCare childCare = fetcher.Fetch(new ChildCare()).Result;

            // ASSERT
            Assert.Null(childCare);
        }

        [Fact]
        public void GetChildCare_ChildCareStub_Null()
        {
            // ARRANGE
            var fetcher = new ChildCareFetcher(GetStreamFetcherReturningNull(), null);

            // ACT
            ChildCare childCare = fetcher.Fetch(new ChildCareStub()).Result;

            // ASSERT
            Assert.Null(childCare);
        }

        [Fact]
        public void GetChildCare_ChildCare_NotNull()
        {
            // ARRANGE
            var expectedChildCare = new ChildCare();
            byte[] expectedBytes = Encoding.UTF8.GetBytes("This is the document");

            var parserMock = new Mock<IChildCareParser>();
            parserMock
                .Setup(c => c.Parse(It.IsAny<ChildCare>(), It.IsAny<byte[]>()))
                .Returns(expectedChildCare)
                .Callback<ChildCare, byte[]>((stub, actualBytes) => Assert.True(expectedBytes.SequenceEqual(actualBytes)));

            var fetcher = new ChildCareFetcher(GetStreamFetcherReturningDocument(expectedBytes), parserMock.Object);

            // ACT
            ChildCare actualChildCare = fetcher.Fetch(new ChildCare()).Result;

            // ASSERT
            Assert.Same(expectedChildCare, actualChildCare);
        }

        [Fact]
        public void GetChildCare_ChildCareStub_NotNull()
        {
            // ARRANGE
            var expectedChildCare = new ChildCare();
            byte[] expectedBytes = Encoding.UTF8.GetBytes("This is the document");

            var parserMock = new Mock<IChildCareParser>();
            parserMock
                .Setup(c => c.Parse(It.IsAny<ChildCareStub>(), It.IsAny<byte[]>()))
                .Returns(expectedChildCare)
                .Callback<ChildCareStub, byte[]>((stub, actualBytes) => Assert.True(expectedBytes.SequenceEqual(actualBytes)));

            var fetcher = new ChildCareFetcher(GetStreamFetcherReturningDocument(expectedBytes), parserMock.Object);

            // ACT
            ChildCare actualChildCare = fetcher.Fetch(new ChildCareStub()).Result;

            // ASSERT
            Assert.Same(expectedChildCare, actualChildCare);
        }
    }
}
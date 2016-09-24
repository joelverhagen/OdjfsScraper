using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using OdjfsScraper.Fetcher.Fetchers;
using OdjfsScraper.Fetcher.UnitTests.Fetchers.TestSupport;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCareStubs;
using OdjfsScraper.Parser.Parsers;
using Xunit;

namespace OdjfsScraper.Fetcher.UnitTests.Fetchers
{
    public class ChildCareStubListFetcherTest : BaseEntityFetcherTest
    {
        [Fact]
        public void GetChildCareStubList_Null()
        {
            // ARRANGE
            var fetcher = new ChildCareStubListFetcher(GetStreamFetcherReturningNull(), null);

            // ACT
            IEnumerable<ChildCareStub> actualStubs = fetcher.Fetch(new County()).Result;

            // ASSERT
            Assert.Null(actualStubs);
        }

        [Fact]
        public void GetChildCareStubList_NotNull()
        {
            // ARRANGE
            var expectedStubs = new List<ChildCareStub>();
            byte[] expectedBytes = Encoding.UTF8.GetBytes("This is the document");

            var parserMock = new Mock<IChildCareStubListParser>();
            parserMock
                .Setup(c => c.Parse(It.IsAny<County>(), It.IsAny<byte[]>()))
                .Returns(expectedStubs)
                .Callback<County, byte[]>((stub, actualBytes) => Assert.True(expectedBytes.SequenceEqual(actualBytes)));

            var fetcher = new ChildCareStubListFetcher(GetStreamFetcherReturningDocument(expectedBytes), parserMock.Object);

            // ACT
            IEnumerable<ChildCareStub> actualStubs = fetcher.Fetch(new County()).Result;

            // ASSERT
            Assert.Same(expectedStubs, actualStubs);
        }
    }
}
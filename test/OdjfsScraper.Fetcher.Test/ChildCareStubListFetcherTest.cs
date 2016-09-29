using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using OdjfsScraper.Fetchers.Test.TestSupport;
using OdjfsScraper.Models;
using OdjfsScraper.Parsers;
using Xunit;

namespace OdjfsScraper.Fetchers.Test
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
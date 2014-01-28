using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using OdjfsScraper.Fetcher.Fetchers;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Fetcher.UnitTests.Fetchers.TestSupport
{
    public abstract class BaseEntityFetcherTest
    {
        protected static IStreamFetcher GetStreamFetcherReturningDocument(byte[] bytes)
        {
            var mock = new Mock<IStreamFetcher>();
            mock
                .Setup(s => s.GetChildCareDocument(It.IsAny<ChildCare>()))
                .Returns(() => Task.FromResult((Stream) new MemoryStream(bytes)));
            mock
                .Setup(s => s.GetChildCareDocument(It.IsAny<ChildCareStub>()))
                .Returns(() => Task.FromResult((Stream) new MemoryStream(bytes)));
            mock
                .Setup(s => s.GetChildCareStubListDocument(It.IsAny<County>()))
                .Returns(() => Task.FromResult((Stream) new MemoryStream(bytes)));
            return mock.Object;
        }

        protected static IStreamFetcher GetStreamFetcherReturningNull()
        {
            var mock = new Mock<IStreamFetcher>();
            mock
                .Setup(s => s.GetChildCareDocument(It.IsAny<ChildCare>()))
                .Returns(Task.FromResult((Stream) null));
            mock
                .Setup(s => s.GetChildCareDocument(It.IsAny<ChildCareStub>()))
                .Returns(Task.FromResult((Stream) null));
            mock
                .Setup(s => s.GetChildCareStubListDocument(It.IsAny<County>()))
                .Returns(Task.FromResult((Stream) null));
            return mock.Object;
        }

        protected static IStreamFetcher GetStreamFetcherReturningCounties(IEnumerable<County> counties)
        {
            var mock = new Mock<IStreamFetcher>();
            mock
                .Setup(s => s.GetAvailableCounties())
                .Returns(Task.FromResult(counties));
            return mock.Object;
        }

        protected static IStreamFetcher GetStreamFetcherReturningChildCares(IEnumerable<ChildCare> childCares)
        {
            var mock = new Mock<IStreamFetcher>();
            mock
                .Setup(s => s.GetAvailableChildCares())
                .Returns(Task.FromResult(childCares));
            return mock.Object;
        }
    }
}
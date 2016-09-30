using System.IO;
using System.Threading.Tasks;
using Moq;
using OdjfsScraper.Fetch;
using OdjfsScraper.Models;

namespace OdjfsScraper.Test.Fetch
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
    }
}
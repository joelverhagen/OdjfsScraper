using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                .Returns(() => Task.FromResult((Stream)new MemoryStream(bytes)));
            mock
                .Setup(s => s.GetChildCareDocument(It.IsAny<ChildCareStub>()))
                .Returns(() => Task.FromResult((Stream)new MemoryStream(bytes)));
            mock
                .Setup(s => s.GetChildCareStubListDocument(It.IsAny<County>()))
                .Returns(() => Task.FromResult((Stream)new MemoryStream(bytes)));
            return mock.Object;
        }

        protected static IStreamFetcher GetStreamFetcherReturningNull()
        {
            var mock = new Mock<IStreamFetcher>();
            mock
                .Setup(s => s.GetChildCareDocument(It.IsAny<ChildCare>()))
                .Returns(Task.FromResult((Stream)null));
            mock
                .Setup(s => s.GetChildCareDocument(It.IsAny<ChildCareStub>()))
                .Returns(Task.FromResult((Stream)null));
            mock
                .Setup(s => s.GetChildCareStubListDocument(It.IsAny<County>()))
                .Returns(Task.FromResult((Stream)null));
            return mock.Object;
        }
    }
}

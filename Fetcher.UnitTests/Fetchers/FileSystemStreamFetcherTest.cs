using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OdjfsScraper.Fetcher.Fetchers;
using OdjfsScraper.Fetcher.Support;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Fetcher.UnitTests.Fetchers
{
    [TestClass]
    public class FileSystemStreamFetcherTest
    {
        [TestMethod]
        public void GetChildCareStubListDocument_CaseInsensitive()
        {
            VerifyGetChildCareStubListDocument(countyName => countyName.ToLower());
        }

        [TestMethod]
        public void GetChildCareStubListDocument_HappyPath()
        {
            VerifyGetChildCareStubListDocument(countyName => countyName.ToUpper());
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCareStub_CaseInsensitive()
        {
            VerifyGetChildCareDocument(
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new ChildCareStub {ExternalUrlId = externalUrlId}),
                externalUrlId => externalUrlId.ToLower());
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCare_CaseInsensitive()
        {
            VerifyGetChildCareDocument((fetcher, externalUrlId) => fetcher.GetChildCareDocument(new ChildCare {ExternalUrlId = externalUrlId}),
                externalUrlId => externalUrlId.ToLower());
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCareStub_HappyPath()
        {
            VerifyGetChildCareDocument(
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new ChildCareStub {ExternalUrlId = externalUrlId}),
                externalUrlId => externalUrlId.ToUpper());
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCareStub_Missing()
        {
            VerifyMissing((fetcher, externalUrlId) => fetcher.GetChildCareDocument(new ChildCareStub {ExternalUrlId = externalUrlId}));
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCare_HappyPath()
        {
            VerifyGetChildCareDocument(
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new ChildCare {ExternalUrlId = externalUrlId}),
                externalUrlId => externalUrlId.ToUpper());
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCare_Missing()
        {
            VerifyMissing((fetcher, externalUrlId) => fetcher.GetChildCareDocument(new ChildCare {ExternalUrlId = externalUrlId}));
        }

        [TestMethod]
        public void GetChildCareStubListDocument_Missing()
        {
            VerifyMissing((fetcher, countyName) => fetcher.GetChildCareStubListDocument(new County {Name = countyName}));
        }

        [TestMethod]
        public void GetChildCareStubListDocument_NullCounty()
        {
            VerifyException<ArgumentNullException>(
                true,
                f => f.GetChildCareStubListDocument(null),
                e => Assert.AreEqual(e.ParamName, "county"));
        }

        [TestMethod]
        public void GetChildCareStubListDocument_NoSetDirectory()
        {
            VerifyException<InvalidOperationException>(
                false,
                f => f.GetChildCareStubListDocument(new County {Name = "FRANKLIN"}),
                e => Assert.AreEqual(e.Message, "The directory has not been set."));
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCare_NoSetDirectory()
        {
            VerifyException<InvalidOperationException>(
                false,
                f => f.GetChildCareDocument(new ChildCare {ExternalUrlId = "CCCCCCCCCCCCCCCCCC"}),
                e => Assert.AreEqual(e.Message, "The directory has not been set."));
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCareStub_NoSetDirectory()
        {
            VerifyException<InvalidOperationException>(
                false,
                f => f.GetChildCareDocument(new ChildCareStub {ExternalUrlId = "CCCCCCCCCCCCCCCCCC"}),
                e => Assert.AreEqual(e.Message, "The directory has not been set."));
        }

        [TestMethod]
        public void GetChildCareStubListDocument_NullCountyName()
        {
            VerifyException<ArgumentNullException>(
                true,
                f => f.GetChildCareStubListDocument(new County {Name = null}),
                e => Assert.AreEqual(e.ParamName, "county.Name"));
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCare_NullExternalUrlId()
        {
            VerifyException<ArgumentNullException>(
                true,
                f => f.GetChildCareDocument(new ChildCare {ExternalUrlId = null}),
                e => Assert.AreEqual(e.ParamName, "childCare.ExternalUrlId"));
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCare_NullChildCare()
        {
            VerifyException<ArgumentNullException>(
                true,
                f => f.GetChildCareDocument((ChildCare) null),
                e => Assert.AreEqual(e.ParamName, "childCare"));
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCareStub_NullChildCareStub()
        {
            VerifyException<ArgumentNullException>(
                true,
                f => f.GetChildCareDocument((ChildCareStub) null),
                e => Assert.AreEqual(e.ParamName, "childCareStub"));
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCareStub_NullExternalUrlId()
        {
            VerifyException<ArgumentNullException>(
                true,
                f => f.GetChildCareDocument(new ChildCareStub {ExternalUrlId = null}),
                e => Assert.AreEqual(e.ParamName, "childCareStub.ExternalUrlId"));
        }

        [TestMethod]
        public void GetAvailableCounties_HappyPath()
        {
            VerifyGetAvailable(
                new Dictionary<string, int> {{"County-FRANKLIN", 3}, {"County-HAMILTON", 2}, {"ChildCare-AAAAA", 4}, {"ChildCare-BBBBB", 5}},
                fetcher => fetcher.GetAvailableCounties(),
                counties =>
                {
                    counties = counties.OrderBy(c => c.Name).ToArray();
                    Assert.AreEqual(2, counties.Length);
                    Assert.AreEqual("FRANKLIN", counties[0].Name);
                    Assert.AreEqual("HAMILTON", counties[1].Name);
                });
        }

        [TestMethod]
        public void GetAvailableChildCares_HappyPath()
        {
            VerifyGetAvailable(
                new Dictionary<string, int> {{"County-FRANKLIN", 3}, {"County-HAMILTON", 2}, {"ChildCare-AAAAA", 4}, {"ChildCare-BBBBB", 5}},
                fetcher => fetcher.GetAvailableChildCares(),
                childCares =>
                {
                    childCares = childCares.OrderBy(c => c.ExternalUrlId).ToArray();
                    Assert.AreEqual(2, childCares.Length);
                    Assert.AreEqual("AAAAA", childCares[0].ExternalUrlId);
                    Assert.AreEqual("BBBBB", childCares[1].ExternalUrlId);
                });
        }

        private static void VerifyException<T>(bool setDirectory, Action<FileSystemStreamFetcher> act, Action<T> verify) where T : Exception
        {
            // ARRANGE
            IFileSystemBlobStore fileSystem = new Mock<IFileSystemBlobStore>().Object;
            var fetcher = new FileSystemStreamFetcher(fileSystem);
            if (setDirectory)
            {
                fetcher.Directory = @"Z:\HTML";
            }

            // ACT
            try
            {
                act(fetcher);
                Assert.Fail();
            }
            catch (T e)
            {
                // ASSERT
                verify(e);
            }
        }

        private static void VerifyGetChildCareDocument(Func<FileSystemStreamFetcher, string, Task<Stream>> getStreamTask, Func<string, string> getInputExternalUrlId)
        {
            Verify(
                getStreamTask,
                "ChildCare",
                "CCCCCCCCCCCCCCCCCC",
                getInputExternalUrlId);
        }

        private static void VerifyGetChildCareStubListDocument(Func<string, string> getInputCountyName)
        {
            Verify(
                (fetcher, countyName) => fetcher.GetChildCareStubListDocument(new County {Name = countyName}),
                "County",
                "FRANKLIN",
                getInputCountyName);
        }

        private static void VerifyGetAvailable<T>(IDictionary<string, int> names, Func<FileSystemStreamFetcher, Task<IEnumerable<T>>> getEntities, Action<T[]> verifyEntities)
        {
            // ARRANGE
            var mock = new Mock<IFileSystemBlobStore>();
            mock
                .SetupProperty(f => f.Directory);
            mock
                .Setup(s => s.GetNames())
                .Returns(Task.FromResult(names));
            IFileSystemBlobStore store = mock.Object;
            var fetcher = new FileSystemStreamFetcher(store);
            fetcher.Directory = @"Z:\HTML";

            // ACT
            T[] entities = getEntities(fetcher).Result.ToArray();

            // ASSERT
            verifyEntities(entities);
        }

        private static void VerifyMissing(Func<FileSystemStreamFetcher, string, Task<Stream>> getStreamTask)
        {
            // ARRANGE
            var mock = new Mock<IFileSystemBlobStore>();
            mock
                .SetupProperty(f => f.Directory);
            IFileSystemBlobStore store = mock.Object;
            var fetcher = new FileSystemStreamFetcher(store);
            fetcher.Directory = @"Z:\HTML";

            // ACT
            Task<Stream> task = getStreamTask(fetcher, "DOESNOTMATTER");

            // ASSERT
            Stream actualStream = task.Result;
            Assert.IsNull(actualStream);
        }

        private static void Verify(Func<FileSystemStreamFetcher, string, Task<Stream>> getStreamTask, string prefix, string identifier, Func<string, string> getInputIdentifer)
        {
            // ARRANGE
            var expectedStream = new MemoryStream();

            var mock = new Mock<IFileSystemBlobStore>();
            mock
                .SetupProperty(f => f.Directory);
            mock
                .Setup(f => f.Read(string.Format("{0}-{1}", prefix, identifier.ToUpper()), It.IsAny<int>()))
                .Returns(Task.FromResult((Stream) expectedStream));
            IFileSystemBlobStore store = mock.Object;
            var fetcher = new FileSystemStreamFetcher(store);
            fetcher.Directory = @"Z:\HTML";

            // ACT
            Task<Stream> task = getStreamTask(fetcher, getInputIdentifer(identifier));

            // ASSERT
            Stream actualStream = task.Result;
            Assert.AreSame(expectedStream, actualStream);
        }
    }
}
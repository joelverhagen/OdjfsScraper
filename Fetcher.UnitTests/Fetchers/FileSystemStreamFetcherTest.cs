using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
            VerifyGetChildCareStubListDocument(
                countyName => countyName.ToUpper(),
                countyName => countyName.ToLower(),
                5,
                Assert.AreSame);
        }

        [TestMethod]
        public void GetChildCareStubListDocument_MultipleVersions()
        {
            VerifyGetChildCareStubListDocument(
                countyName => countyName,
                countyName => countyName,
                5,
                Assert.AreSame);
        }


        [TestMethod]
        public void GetChildCareDocument_ChildCareStub_CaseInsensitive()
        {
            VerifyGetChildCareDocument(
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new LicensedCenterStub {ExternalUrlId = externalUrlId}),
                externalUrlId => externalUrlId.ToUpper(),
                externalUrlId => externalUrlId.ToLower(),
                5,
                Assert.AreSame);
        }


        [TestMethod]
        public void GetChildCareDocument_ChildCare_CaseInsensitive()
        {
            VerifyGetChildCareDocument(
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new LicensedCenter {ExternalUrlId = externalUrlId}),
                externalUrlId => externalUrlId.ToUpper(),
                externalUrlId => externalUrlId.ToLower(),
                5,
                Assert.AreSame);
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCareStub_MultipleVersions()
        {
            VerifyGetChildCareDocument(
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new LicensedCenterStub {ExternalUrlId = externalUrlId}),
                externalUrlId => externalUrlId,
                externalUrlId => externalUrlId,
                5,
                Assert.AreSame);
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCareStub_Missing()
        {
            VerifyGetChildCareDocument(
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new LicensedCenterStub {ExternalUrlId = externalUrlId}),
                externalUrlId => externalUrlId,
                externalUrlId => externalUrlId,
                0,
                (expected, actual) => Assert.IsNull(actual));
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCare_MultipleVersions()
        {
            VerifyGetChildCareDocument(
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new LicensedCenter {ExternalUrlId = externalUrlId}),
                externalUrlId => externalUrlId,
                externalUrlId => externalUrlId,
                5,
                Assert.AreSame);
        }


        [TestMethod]
        public void GetChildCareDocument_ChildCare_Missing()
        {
            VerifyGetChildCareDocument(
                (fetcher, externalUrlId) => fetcher.GetChildCareDocument(new LicensedCenter {ExternalUrlId = externalUrlId}),
                externalUrlId => externalUrlId,
                externalUrlId => externalUrlId,
                0,
                (expected, actual) => Assert.IsNull(actual));
        }

        [TestMethod]
        public void GetChildCareStubListDocument_Missing()
        {
            VerifyGetChildCareStubListDocument(
                countyName => countyName,
                countyName => countyName,
                0,
                (expected, actual) => Assert.IsNull(actual));
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
                f => f.GetChildCareDocument(new LicensedCenter {ExternalUrlId = "CCCCCCCCCCCCCCCCCC"}),
                e => Assert.AreEqual(e.Message, "The directory has not been set."));
        }

        [TestMethod]
        public void GetChildCareDocument_ChildCareStub_NoSetDirectory()
        {
            VerifyException<InvalidOperationException>(
                false,
                f => f.GetChildCareDocument(new LicensedCenterStub {ExternalUrlId = "CCCCCCCCCCCCCCCCCC"}),
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
                f => f.GetChildCareDocument(new LicensedCenter {ExternalUrlId = null}),
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
                f => f.GetChildCareDocument(new LicensedCenterStub {ExternalUrlId = null}),
                e => Assert.AreEqual(e.ParamName, "childCareStub.ExternalUrlId"));
        }

        [TestMethod]
        public void SetDirectory_Null()
        {
            VerifyException<ArgumentNullException>(
                false,
                f => f.SetDirectory(null),
                e => Assert.AreEqual(e.ParamName, "directory"));
        }

        private static IFileSystem GetFileSystemContaining(string directoryPath, string prefix, string identifier, Stream currentStream, int versions)
        {
            var mock = new Mock<IFileSystem>(MockBehavior.Strict);

            var filePaths = new List<string>();
            for (int i = 0; i < versions; i++)
            {
                string version = i == versions - 1 ? "Current" : i.ToString(CultureInfo.InvariantCulture);
                Stream stream = i == versions - 1 ? currentStream : new MemoryStream();

                string fileName = string.Format(
                    @"{0}_{1}_{2}_OK_0000000000000000000000000000000000000000000000000000000000000000.html",
                    prefix,
                    identifier,
                    version);
                string filePath = Path.Combine(directoryPath, fileName);
                filePaths.Add(filePath);

                mock
                    .Setup(f => f.FileOpen(filePath, FileMode.Open))
                    .Returns(() => stream);
            }

            mock
                .Setup(f => f.DirectoryEnumerateFiles(directoryPath, "*.html", SearchOption.TopDirectoryOnly))
                .Returns(() => filePaths);

            return mock.Object;
        }

        private static void VerifyException<T>(bool setDirectory, Action<FileSystemStreamFetcher> act, Action<T> verify) where T : Exception
        {
            // ARRANGE
            IFileSystem fileSystem = new Mock<IFileSystem>().Object;
            var fetcher = new FileSystemStreamFetcher(fileSystem);
            if (setDirectory)
            {
                fetcher.SetDirectory(@"Z:\HTML");
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

        private static void VerifyGetChildCareDocument(Func<FileSystemStreamFetcher, string, Task<Stream>> getStreamTask, Func<string, string> getFileSystemExternalUrlId, Func<string, string> getInputExternalUrlId, int versions, Action<Stream, Stream> verifyStream)
        {
            // ARRANGE
            const string directoryPath = @"Z:\HTML";
            const string externalUrlId = "CCCCCCCCCCCCCCCCCC";

            var expectedStream = new MemoryStream();
            IFileSystem fileSystem = GetFileSystemContaining(directoryPath, "ChildCare", getFileSystemExternalUrlId(externalUrlId), expectedStream, versions);
            var fetcher = new FileSystemStreamFetcher(fileSystem);
            fetcher.SetDirectory(directoryPath);

            // ACT
            Task<Stream> task = getStreamTask(fetcher, getInputExternalUrlId(externalUrlId));

            // ASSERT
            Stream actualStream = task.Result;
            verifyStream(expectedStream, actualStream);
        }

        private static void VerifyGetChildCareStubListDocument(Func<string, string> getFileSystemCountyName, Func<string, string> getInputCountyName, int versions, Action<Stream, Stream> verifyStream)
        {
            // ARRANGE
            const string directoryPath = @"Z:\HTML";
            const string countyName = "FRANKLIN";

            var expectedStream = new MemoryStream();
            IFileSystem fileSystem = GetFileSystemContaining(directoryPath, "County", getFileSystemCountyName(countyName), expectedStream, versions);
            var fetcher = new FileSystemStreamFetcher(fileSystem);
            fetcher.SetDirectory(directoryPath);

            // ACT
            Task<Stream> task = fetcher.GetChildCareStubListDocument(new County {Name = getInputCountyName(countyName)});

            // ASSERT
            Stream actualStream = task.Result;
            verifyStream(expectedStream, actualStream);
        }
    }
}
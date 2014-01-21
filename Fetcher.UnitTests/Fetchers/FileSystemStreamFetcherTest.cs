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

namespace OdjfsScraper.Fetcher.UnitTests.Fetchers
{
    [TestClass]
    public class FileSystemStreamFetcherTest
    {
        [TestMethod]
        public void GetChildCareStubListDocument_CaseInsensitive()
        {
            // ARRANGE
            const string directoryPath = @"Z:\HTML";
            const string countyName = "FRANKLIN";

            var expectedStream = new MemoryStream();
            IFileSystem fileSystem = GetFileSystemContainingCounty(directoryPath, countyName.ToUpper(), expectedStream, 5);
            var fetcher = new FileSystemStreamFetcher(fileSystem);
            fetcher.SetDirectory(directoryPath);

            // ACT
            Task<Stream> task = fetcher.GetChildCareStubListDocument(new County {Name = countyName.ToLower()});

            // ASSERT
            Stream actualStream = task.Result;
            Assert.AreSame(expectedStream, actualStream);
        }

        [TestMethod]
        public void GetChildCareStubListDocument_MultipleVersions()
        {
            // ARRANGE
            const string directoryPath = @"Z:\HTML";
            const string countyName = "FRANKLIN";

            var expectedStream = new MemoryStream();
            IFileSystem fileSystem = GetFileSystemContainingCounty(directoryPath, countyName, expectedStream, 5);
            var fetcher = new FileSystemStreamFetcher(fileSystem);
            fetcher.SetDirectory(directoryPath);

            // ACT
            Task<Stream> task = fetcher.GetChildCareStubListDocument(new County {Name = countyName});

            // ASSERT
            Stream actualStream = task.Result;
            Assert.AreSame(expectedStream, actualStream);
        }

        [TestMethod]
        public void GetChildCareStubListDocument_Missing()
        {
            // ARRANGE
            const string directoryPath = @"Z:\HTML";
            const string countyName = "FRANKLIN";

            var expectedStream = new MemoryStream();
            IFileSystem fileSystem = GetFileSystemContainingCounty(directoryPath, countyName, expectedStream, 0);
            var fetcher = new FileSystemStreamFetcher(fileSystem);
            fetcher.SetDirectory(directoryPath);

            // ACT
            Task<Stream> task = fetcher.GetChildCareStubListDocument(new County {Name = countyName});

            // ASSERT
            Assert.IsNull(task.Result);
        }

        [TestMethod]
        public void GetChildCareStubListDocument_NullCounty()
        {
            // ARRANGE
            IFileSystem fileSystem = GetEmptyFileSystem();
            var fetcher = new FileSystemStreamFetcher(fileSystem);
            fetcher.SetDirectory(@"Z:\HTML");

            // ACT
            try
            {
                fetcher.GetChildCareStubListDocument(null);
                Assert.Fail();
            }
            catch (ArgumentNullException e)
            {
                // ASSERT
                Assert.IsTrue(e.ParamName == "county");
            }
        }

        [TestMethod]
        public void GetChildCareStubListDocument_NoSetDirectory()
        {
            // ARRANGE
            IFileSystem fileSystem = GetEmptyFileSystem();
            var fetcher = new FileSystemStreamFetcher(fileSystem);

            // ACT
            try
            {
                fetcher.GetChildCareStubListDocument(new County {Name = "FRANKLIN"});
                Assert.Fail();
            }
            catch (InvalidOperationException e)
            {
                // ASSERT
                Assert.AreEqual(e.Message, "The directory has not been set.");
            }
        }

        [TestMethod]
        public void SetDirectory_Null()
        {
            // ARRANGE
            IFileSystem fileSystem = GetEmptyFileSystem();
            var fetcher = new FileSystemStreamFetcher(fileSystem);

            // ACT
            try
            {
                fetcher.SetDirectory(null);
                Assert.Fail();
            }
            catch (ArgumentNullException e)
            {
                // ASSERT
                Assert.IsTrue(e.ParamName == "directory");
            }
        }

        [TestMethod]
        public void GetChildCareStubListDocument_NullCountyName()
        {
            // ARRANGE
            IFileSystem fileSystem = GetEmptyFileSystem();
            var fetcher = new FileSystemStreamFetcher(fileSystem);
            fetcher.SetDirectory(@"Z:\HTML");

            // ACT
            try
            {
                fetcher.GetChildCareStubListDocument(new County {Name = null});
                Assert.Fail();
            }
            catch (ArgumentNullException e)
            {
                // ASSERT
                Assert.IsTrue(e.ParamName == "name");
            }
        }

        private static IFileSystem GetEmptyFileSystem()
        {
            return new Mock<IFileSystem>().Object;
        }

        private static IFileSystem GetFileSystemContainingCounty(string directoryPath, string countyName, Stream currentStream, int versions)
        {
            var mock = new Mock<IFileSystem>(MockBehavior.Strict);

            var filePaths = new List<string>();
            for (int i = 0; i < versions; i++)
            {
                string version = i == versions - 1 ? "Current" : i.ToString(CultureInfo.InvariantCulture);
                Stream stream = i == versions - 1 ? currentStream : new MemoryStream();

                string fileName = string.Format(
                    @"County_{0}_{1}_OK_0000000000000000000000000000000000000000000000000000000000000000.html",
                    countyName,
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
    }
}
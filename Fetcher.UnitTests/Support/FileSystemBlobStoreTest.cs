using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OdjfsScraper.Fetcher.Support;
using OdjfsScraper.Fetcher.UnitTests.Support.TestSupport;

namespace OdjfsScraper.Fetcher.UnitTests.Support
{
    [TestClass]
    public class FileSystemBlobStoreTest
    {
        [TestMethod]
        public void FileExtension_Null()
        {
            VerifyException<ArgumentException>(
                s => s.FileExtension = null,
                e => Assert.IsTrue(e.Message.Contains("must not be null or empty")));
        }

        [TestMethod]
        public void FileExtension_EmptyString()
        {
            VerifyException<ArgumentException>(
                s => s.FileExtension = string.Empty,
                e => Assert.IsTrue(e.Message.Contains("must not be null or empty")));
        }

        [TestMethod]
        public void FileExtension_InvalidCharacter()
        {
            VerifyException<ArgumentException>(
                s => s.FileExtension = ".|foo|",
                e => Assert.IsTrue(e.Message.Contains("must not contain the invalid characters")));
        }

        [TestMethod]
        public void FileExtension_ContainsFieldSeperator()
        {
            VerifyException<ArgumentException>(
                s =>
                {
                    s.FieldSeperator = "a";
                    s.FileExtension = ".bat";
                },
                e => Assert.IsTrue(e.Message.Contains("must not contain the field seperator")));
        }

        [TestMethod]
        public void FileExtension_DoesNotStartWithPeriod()
        {
            VerifyException<ArgumentException>(
                s => s.FileExtension = "dat",
                e => Assert.IsTrue(e.Message.Contains("must start with a period")));
        }

        [TestMethod]
        public void FieldSeperator_Null()
        {
            VerifyException<ArgumentException>(
                s => s.FieldSeperator = null,
                e => Assert.IsTrue(e.Message.Contains("must not be null or empty")));
        }

        [TestMethod]
        public void FieldSeperator_EmptyString()
        {
            VerifyException<ArgumentException>(
                s => s.FieldSeperator = string.Empty,
                e => Assert.IsTrue(e.Message.Contains("must not be null or empty")));
        }

        [TestMethod]
        public void FieldSeperator_InvalidCharacter()
        {
            VerifyException<ArgumentException>(
                s => s.FieldSeperator = "|_|",
                e => Assert.IsTrue(e.Message.Contains("must not contain the invalid characters")));
        }

        [TestMethod]
        public void FieldSeperator_IsContainedByFileExtension()
        {
            VerifyException<ArgumentException>(
                s =>
                {
                    s.FileExtension = ".bat";
                    s.FieldSeperator = "a";
                },
                e => Assert.IsTrue(e.Message.Contains("must not contain the field seperator")));
        }

        [TestMethod]
        public void VerifyDirectory_NoneSet()
        {
            // ARRANGE
            var test = new Test();
            test.FileSystemBlobStore.SetDirectory(null);

            // ACT, ASSERT
            VerifyException<InvalidOperationException>(
                test.FileSystemBlobStore.VerifyDirectory,
                e => Assert.IsTrue(e.Message.Contains("The directory must be set before using the blob store.")));
        }

        [TestMethod]
        public void VerifyDirectory_DoesNotExist()
        {
            // ARRANGE
            var test = new Test();
            test.FileSystemMock
                .Setup(f => f.DirectoryExists(It.IsAny<string>()))
                .Returns(false);

            // ACT
            test.FileSystemBlobStore.VerifyDirectory();

            // ASSERT
            test.FileSystemMock
                .Verify(f => f.DirectoryExists(It.IsAny<string>()), Times.Once);
            test.FileSystemMock
                .Verify(f => f.DirectoryCreateDirectory(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void VerifyDirectory_Exists()
        {
            // ARRANGE
            var test = new Test();
            test.FileSystemMock
                .Setup(f => f.DirectoryExists(It.IsAny<string>()))
                .Returns(true);

            // ACT
            test.FileSystemBlobStore.VerifyDirectory();

            // ASSERT
            test.FileSystemMock
                .Verify(f => f.DirectoryExists(It.IsAny<string>()), Times.Once);
            test.FileSystemMock
                .Verify(f => f.DirectoryCreateDirectory(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void Read_EmptyDirectory()
        {
            // ARRANGE
            var test = new Test();
            test.SetupForEmptyDirectory();

            // ACT
            Stream actualStream = test.FileSystemBlobStore.Read("FooName", -1).Result;

            // ASSERT
            Assert.IsNull(actualStream);
            test.FileSystemMock
                .Verify(f => f.FileOpen(It.IsAny<string>(), It.IsAny<FileMode>()), Times.Never);
        }

        [TestMethod]
        public void Read_CorrectBytes()
        {
            // ARRANGE
            const string directory = @"Z:\HTML";
            const string fileName = "FooName_Current_FooHash.blob";
            byte[] expectedBytes = Encoding.UTF8.GetBytes("This content will be read out of a fake file.");
            var test = new Test();
            test.SetupForRead(directory, fileName, new MemoryStream(expectedBytes));
            test.SetupForConstantHash("FooHash");
            test.SetupDirectory(directory, new []
            {
                fileName
            });

            // ACT
            Stream actualStream = test.FileSystemBlobStore.Read("FooName", -1).Result;

            // ASSERT
            Assert.IsNotNull(actualStream);
            byte[] actualBytes = actualStream.ReadAsByteArrayAsync().Result;
            Assert.IsTrue(expectedBytes.SequenceEqual(actualBytes));
            test.FileSystemMock
                .Verify(f => f.FileOpen(It.IsAny<string>(), It.IsAny<FileMode>()), Times.Once);
        }

        [TestMethod]
        public void Read_ValidPositiveIndex()
        {
            // ARRANGE
            const string directory = @"Z:\HTML";
            var test = new Test();
            const string fileName = "FooName_0_FooHash.blob";
            test.SetupDirectory(directory, new[]
            {
                "FooName_Current_FooHash.blob",
                "FooName_1_FooHash.blob",
                fileName
            });
            test.SetupForRead(directory, fileName, new MemoryStream());

            // ACT
            test.FileSystemBlobStore.Read("FooName", 0).Wait();

            // ASSERT
            test.FileSystemMock
                .Verify(f => f.FileOpen(It.IsAny<string>(), It.IsAny<FileMode>()), Times.Once);
        }

        [TestMethod]
        public void Read_InvalidNegativeIndex()
        {
            // ARRANGE
            var test = new Test();
            test.SetupForEmptyDirectory();
            test.SetupDirectory(@"Z:\HTML", new[]
            {
                "FooName_Current_FooHash.blob",
                "FooName_1_FooHash.blob",
                "FooName_0_FooHash.blob"
            });

            // ACT
            Stream actualStream = test.FileSystemBlobStore.Read("FooName", -4).Result;

            // ASSERT
            Assert.IsNull(actualStream);
            test.FileSystemMock
                .Verify(f => f.FileOpen(It.IsAny<string>(), It.IsAny<FileMode>()), Times.Never);
        }

        [TestMethod]
        public void Read_ValidNegativeIndex()
        {
            // ARRANGE
            const string directory = @"Z:\HTML";
            var test = new Test();
            const string fileName = "FooName_0_FooHash.blob";
            test.SetupDirectory(directory, new[]
            {
                "FooName_Current_FooHash.blob",
                "FooName_1_FooHash.blob",
                fileName
            });
            test.SetupForRead(directory, fileName, new MemoryStream());

            // ACT
            test.FileSystemBlobStore.Read("FooName", -3).Wait();

            // ASSERT
            test.FileSystemMock
                .Verify(f => f.FileOpen(It.IsAny<string>(), It.IsAny<FileMode>()), Times.Once);
        }

        [TestMethod]
        public void Read_SupportsTags()
        {
            // ARRANGE
            const string directory = @"Z:\HTML";
            var test = new Test();
            const string fileName = "FooName_Current_BarTag_FooHash.blob";
            test.SetupDirectory(directory, new[]
            {
                fileName
            });
            test.SetupForRead(directory, fileName, new MemoryStream());

            // ACT
            test.FileSystemBlobStore.Read("FooName", 0).Wait();

            // ASSERT
            test.FileSystemMock
                .Verify(f => f.FileOpen(It.IsAny<string>(), It.IsAny<FileMode>()), Times.Once);
        }

        [TestMethod]
        public void Read_InvalidPositiveIndex()
        {
            // ARRANGE
            var test = new Test();
            test.SetupForEmptyDirectory();
            test.SetupDirectory(@"Z:\HTML", new[]
            {
                "FooName_Current_FooHash.blob",
                "FooName_1_FooHash.blob",
                "FooName_0_FooHash.blob"
            });

            // ACT
            Stream actualStream = test.FileSystemBlobStore.Read("FooName", 3).Result;

            // ASSERT
            Assert.IsNull(actualStream);
            test.FileSystemMock
                .Verify(f => f.FileOpen(It.IsAny<string>(), It.IsAny<FileMode>()), Times.Never);
        }

        [TestMethod]
        public void Read_WrongNumbeOfPieces()
        {
            // ARRANGE
            var test = new Test();
            test.SetupForEmptyDirectory();
            test.SetupDirectory(@"Z:\HTML", new[]
            {
                "FooName_FooHash.blob"
            });

            // ACT
            Stream actualStream = test.FileSystemBlobStore.Read("FooName", -1).Result;

            // ASSERT
            Assert.IsNull(actualStream);
            test.FileSystemMock
                .Verify(f => f.FileOpen(It.IsAny<string>(), It.IsAny<FileMode>()), Times.Never);
        }

        [TestMethod]
        public void Read_Duplicate()
        {
            // ARRANGE
            var test = new Test();
            test.SetupForEmptyDirectory();
            test.SetupDirectory(@"Z:\HTML", new[]
            {
                "FooName_0_A_FooHash.blob",
                "FooName_0_B_FooHash.blob"
            });

            // ACT, ASSERT
            VerifyException<ArgumentException>(
                () => test.FileSystemBlobStore.Read("FooName", -1).Wait(),
                e => Assert.IsTrue(e.Message.Contains("There must not be multiple files with name")));
        }

        [TestMethod]
        public void Read_InvalidVersion()
        {
            // ARRANGE
            var test = new Test();
            test.SetupForEmptyDirectory();
            test.SetupDirectory(@"Z:\HTML", new[]
            {
                "FooName_BAD_FooHash.blob"
            });

            // ACT
            Stream actualStream = test.FileSystemBlobStore.Read("FooName", -1).Result;

            // ASSERT
            Assert.IsNull(actualStream);
            test.FileSystemMock
                .Verify(f => f.FileOpen(It.IsAny<string>(), It.IsAny<FileMode>()), Times.Never);
        }

        [TestMethod]
        public void Write_UnseekableStream()
        {
            // ARRANGE
            var outputStream = new MemoryStream();
            var test = new Test();
            test.SetupForWrite(@"Z:\HTML", "FooName_Current_FooHash.blob", outputStream);
            byte[] expectedBytes = Encoding.UTF8.GetBytes("content");

            // ACT
            test.FileSystemBlobStore.Write("FooName", null, new UnseekableStream(expectedBytes)).Wait();

            // ASSERT
            byte[] actualBytes = outputStream.ToArray();
            Assert.IsTrue(expectedBytes.SequenceEqual(actualBytes));
            test.FileSystemMock
                .Verify(f => f.FileOpen(It.IsAny<string>(), It.IsAny<FileMode>()), Times.Once);
        }


        [TestMethod]
        public void Write_CorrectBytes()
        {
            // ARRANGE
            var outputStream = new MemoryStream();
            var test = new Test();
            test.SetupForWrite(@"Z:\HTML", "FooName_Current_FooHash.blob", outputStream);
            byte[] expectedBytes = Encoding.UTF8.GetBytes("content");
            var inputStream = new MemoryStream(expectedBytes);

            // ACT
            test.FileSystemBlobStore.Write("FooName", null, inputStream).Wait();

            // ASSERT
            byte[] actualBytes = outputStream.ToArray();
            Assert.IsTrue(expectedBytes.SequenceEqual(actualBytes));
            test.FileSystemMock
                .Verify(f => f.FileOpen(It.IsAny<string>(), It.IsAny<FileMode>()), Times.Once);
        }

        [TestMethod]
        public void Write_HashContainsSeperator()
        {
            // ARRANGE
            var test = new Test();
            test.SetupForConstantHash("SOME_HASH");
            test.FileSystemBlobStore.FieldSeperator = "_";

            // ACT, ASSERT
            VerifyException<AggregateException>(
                () => test.FileSystemBlobStore.Write("FooName", null, new MemoryStream()).Wait(),
                ae =>
                {
                    Exception e = ae.Flatten().InnerExceptions.FirstOrDefault();
                    Assert.IsTrue(e is InvalidOperationException);
                    Assert.IsTrue(e.Message.Contains("hash"));
                    Assert.IsTrue(e.Message.Contains("must not contain the field seperator"));
                });
        }

        [TestMethod]
        public void Write_NameContainsFieldSeperator()
        {
            VerifyException<ArgumentException>(
                s => s.Write("Foo" + s.FieldSeperator + "Bar", null, null),
                e => Assert.IsTrue(e.Message.Contains("must not contain the field seperator")));
        }

        [TestMethod]
        public void Write_MoveCurrentToZero()
        {
            // ARRANGE
            const string directory = @"Z:\HTML";
            var test = new Test();
            test.SetupDirectory(directory, new[]
            {
                "FooName_Current_OldHash.blob"
            });
            test.SetupForMove(directory, "FooName_Current_OldHash.blob", "FooName_0_OldHash.blob");

            // ACT
            test.FileSystemBlobStore.Write("FooName", null, new MemoryStream()).Wait();

            // ASSERT
            test.FileSystemMock
                .Verify(f => f.FileMove(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void Write_MoveCurrentToNumber()
        {
            // ARRANGE
            const string directory = @"Z:\HTML";
            var test = new Test();
            test.SetupDirectory(directory, new[]
            {
                "FooName_1_B.blob",
                "FooName_Current_D.blob",
                "FooName_0_A.blob",
                "FooName_2_C.blob"
            });
            test.SetupForMove(directory, "FooName_Current_D.blob", "FooName_3_D.blob");

            // ACT
            test.FileSystemBlobStore.Write("FooName", null, new MemoryStream()).Wait();

            // ASSERT
            test.FileSystemMock
                .Verify(f => f.FileMove(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void Write_NoCurrent()
        {
            // ARRANGE
            const string directory = @"Z:\HTML";
            var test = new Test();
            test.SetupDirectory(directory, new[]
            {
                "FooName_1_B.blob",
                "FooName_0_A.blob",
                "FooName_2_C.blob"
            });

            // ACT
            test.FileSystemBlobStore.Write("FooName", null, new MemoryStream()).Wait();

            // ASSERT
            test.FileSystemMock
                .Verify(f => f.FileMove(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void Write_NoChange()
        {
            // ARRANGE
            const string directory = @"Z:\HTML";
            var test = new Test();
            test.SetupDirectory(directory, new[]
            {
                "FooName_Current_BazHash.blob"
            });
            test.SetupForConstantHash("BazHash");

            // ACT
            test.FileSystemBlobStore.Write("FooName", null, new MemoryStream()).Wait();

            // ASSERT
            test.FileSystemMock
                .Verify(f => f.FileOpen(It.IsAny<string>(), It.IsAny<FileMode>()), Times.Never);
            test.FileSystemMock
                .Verify(f => f.FileMove(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void Write_DifferentTagCausesWrite()
        {
            // ARRANGE
            const string directory = @"Z:\HTML";
            var test = new Test();
            test.SetupDirectory(directory, new[]
            {
                "FooName_Current_OldTag_A.blob"
            });
            test.SetupForMove(directory, "FooName_Current_OldTag_A.blob", "FooName_0_OldTag_A.blob");
            test.SetupForWrite(directory, "FooName_Current_NewTag_B.blob", new MemoryStream());
            test.SetupForConstantHash("B");

            // ACT
            test.FileSystemBlobStore.Write("FooName", "NewTag", new MemoryStream()).Wait();

            // ASSERT
            test.FileSystemMock
                .Verify(f => f.FileOpen(It.IsAny<string>(), It.IsAny<FileMode>()), Times.Once);
            test.FileSystemMock
                .Verify(f => f.FileMove(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }


        [TestMethod]
        public void Write_IgnoresDifferentExtension()
        {
            // ARRANGE
            const string directory = @"Z:\HTML";
            var test = new Test();
            test.SetupDirectory(directory, new[]
            {
                "FooName_Current_A.dat"
            });
            test.SetupForWrite(directory, "FooName_Current_B.blob", new MemoryStream());
            test.SetupForConstantHash("B");

            // ACT
            test.FileSystemBlobStore.Write("FooName", null, new MemoryStream()).Wait();

            // ASSERT
            test.FileSystemMock
                .Verify(f => f.FileOpen(It.IsAny<string>(), It.IsAny<FileMode>()), Times.Once);
            test.FileSystemMock
                .Verify(f => f.FileMove(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        private static void VerifyException<T>(Action<FileSystemBlobStore> action, Action<T> verifyException) where T : Exception
        {
            try
            {
                action(new Test().FileSystemBlobStore);
                Assert.Fail();
            }
            catch (T e)
            {
                verifyException(e);
            }
        }

        private static void VerifyException<T>(Action action, Action<T> verifyException) where T : Exception
        {
            try
            {
                action();
                Assert.Fail();
            }
            catch (T e)
            {
                verifyException(e);
            }
        }

        public class Test
        {
            public Mock<IFileSystem> FileSystemMock { get; private set; }
            public Mock<IHashAlgorithm> HashAlgorithMock { get; private set; }
            public FileSystemBlobStore FileSystemBlobStore { get; private set; }

            public Test()
            {
                FileSystemMock = new Mock<IFileSystem>();
                HashAlgorithMock = new Mock<IHashAlgorithm>();
                FileSystemBlobStore = new FileSystemBlobStore(HashAlgorithMock.Object, FileSystemMock.Object);

                SetupDefaults();
            }

            public void SetupDefaults()
            {
                SetupForEmptyDirectory();
                FileSystemMock
                    .Setup(f => f.FileOpen(It.IsAny<string>(), It.IsAny<FileMode>()))
                    .Returns(() => new MemoryStream());
                FileSystemBlobStore.SetDirectory(@"Z:\HTML");
                SetupForConstantHash("FooHash");
            }

            public void SetupForEmptyDirectory()
            {
                FileSystemMock
                    .Setup(f => f.DirectoryEnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                    .Returns(Enumerable.Empty<string>);
            }

            public void SetupForConstantHash(string hash)
            {
                HashAlgorithMock
                    .Setup(h => h.ComputeHashToString(It.IsAny<Stream>()))
                    .Returns(hash);
            }

            public void SetupForMove(string directory, string oldFileName, string newFileName)
            {
                FileSystemMock
                    .Setup(f => f.FileMove(It.IsAny<string>(), It.IsAny<string>()))
                    .Callback<string, string>((fromPath, toPath) =>
                    {
                        Assert.AreEqual(directory, Path.GetDirectoryName(fromPath));
                        Assert.AreEqual(directory, Path.GetDirectoryName(toPath));
                        Assert.AreEqual(oldFileName, Path.GetFileName(fromPath));
                        Assert.AreEqual(newFileName, Path.GetFileName(toPath));
                    });
            }

            public void SetupForWrite(string directory, string fileName, Stream outputStream)
            {
                FileSystemMock
                    .Setup(f => f.FileOpen(It.IsAny<string>(), FileMode.Create))
                    .Returns(outputStream)
                    .Callback<string, FileMode>((filePath, fileMode) =>
                    {
                        Assert.AreEqual(directory, Path.GetDirectoryName(filePath));
                        Assert.AreEqual(fileName, Path.GetFileName(filePath));
                    });
            }

            public void SetupForRead(string directory, string fileName, Stream inputStream)
            {
                FileSystemMock
                    .Setup(f => f.FileOpen(It.IsAny<string>(), FileMode.Open))
                    .Returns(inputStream)
                    .Callback<string, FileMode>((filePath, fileMode) =>
                    {
                        Assert.AreEqual(directory, Path.GetDirectoryName(filePath));
                        Assert.AreEqual(fileName, Path.GetFileName(filePath));
                    });
            }

            public void SetupDirectory(string directory, IEnumerable<string> fileNames)
            {
                var filePaths = fileNames
                    .Select(s => Path.Combine(directory, s))
                    .ToArray();
                FileSystemMock
                    .Setup(f => f.DirectoryEnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                    .Returns(filePaths);
                FileSystemBlobStore.SetDirectory(directory);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using NLog;

namespace OdjfsScraper.Fetcher.Support
{
    public class FileSystemBlobStore : IFileSystemBlobStore
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IFileSystem _fileSystem;
        private string _directory;
        private string _fieldSeperator;
        private string _fileExtension;
        private bool _isDirectoryChecked;

        public FileSystemBlobStore(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _isDirectoryChecked = false;
            FieldSeperator = "_";
        }

        public string FileExtension
        {
            get { return _fileExtension; }
            set
            {
                if (string.IsNullOrEmpty(_fileExtension))
                {
                    _fileExtension = string.Empty;
                    return;
                }

                // make sure all of the characters can be in a file name
                var invalid = new string(Path.GetInvalidFileNameChars().Intersect(value).ToArray());
                if (invalid != string.Empty)
                {
                    throw new ArgumentException(string.Format("The file extension '{0}' must not contain the invalid characters '{1}'.", value, invalid));
                }

                // make sure the file extension does not contain the field seperator
                if (value.Contains(FieldSeperator))
                {
                    throw new ArgumentException(string.Format("The file extension '{0}' must not contain the field seperator '{1}'.", value, FieldSeperator));
                }

                // make sure the file extension starts with a "."
                if (!value.StartsWith("."))
                {
                    throw new ArgumentException(string.Format("The file extension '{0}' must start with a period '.' or be null or empty.", value));
                }

                _fileExtension = value;
            }
        }

        public string FieldSeperator
        {
            get { return _fieldSeperator; }
            set
            {
                if (string.IsNullOrEmpty(_fileExtension))
                {
                    throw new ArgumentException(string.Format("The field seperator '{0}' must not be null or empty.", value));
                }

                // make sure all of the characters can be in a file name
                var invalid = new string(Path.GetInvalidFileNameChars().Intersect(value).ToArray());
                if (invalid != string.Empty)
                {
                    throw new ArgumentException(string.Format("The field seperator '{0}' must not contain the invalid characters '{1}'.", value, invalid));
                }

                // make sure the file extension does not contain the field seperator
                if (FileExtension.Contains(value))
                {
                    throw new ArgumentException(string.Format("The file extension '{0}' must not contain the field seperator '{1}'.", FileExtension, value));
                }

                _fieldSeperator = value;
            }
        }

        public void SetDirectory(string directory)
        {
            if (_directory != directory)
            {
                _isDirectoryChecked = false;
                _directory = directory;
            }
        }

        public async Task Write(string name, string tag, Stream stream)
        {
            if (name.Contains(FieldSeperator))
            {
                throw new ArgumentException(string.Format("The name '{0}' must not contain the field seperator '{1}'.", name, FieldSeperator), "name");
            }
            if (_directory == null)
            {
                throw new InvalidOperationException("The directory must be set before using the blob store.");
            }
            if (!_isDirectoryChecked)
            {
                if (!_fileSystem.DirectoryExists(_directory))
                {
                    _fileSystem.DirectoryCreateDirectory(_directory);
                }
                _isDirectoryChecked = true;
            }

            // fully buffer the stream if it is not seekable
            if (!stream.CanSeek)
            {
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                stream = memoryStream;
            }

            // get the hash, and reset the stream
            HashAlgorithm hashAlgorithm = new SHA256Managed();
            string hash = BitConverter.ToString(hashAlgorithm.ComputeHash(stream)).Replace("-", "");
            if (hash.Contains(FieldSeperator))
            {
                throw new InvalidOperationException(string.Format(
                    "The hash '{0}' must not contain the field seperator '{1}'. Try making the field seperator non-hexadecimal!",
                    hash,
                    FieldSeperator));
            }
            stream.Position = 0;

            // get the two most recent versions
            BlobEntry[] blobEntries = GetBlobEntries(name).Reverse().Take(2).ToArray();

            // have there been any changes?
            if (blobEntries[0].Version == "Current" && blobEntries[0].Hash == hash && blobEntries[0].Tag == tag)
            {
                return;
            }

            if (blobEntries[0].Version == "Current")
            {
                string oldCurrentPath = blobEntries[0].FilePath;
                string newVersionPath = GetPath(
                    blobEntries[1].Name,
                    blobEntries.Length == 2 ? (int.Parse(blobEntries[1].Version) + 1).ToString(CultureInfo.InvariantCulture) : "0",
                    blobEntries[1].Tag,
                    blobEntries[1].Hash);

                _fileSystem.FileMove(oldCurrentPath, newVersionPath);
            }

            // generate the file path
            string newCurrentPath = GetPath(name, "Current", tag, hash);

            // write the stream
            Stream outputStream = _fileSystem.FileOpen(newCurrentPath, FileMode.Create);
            await stream.CopyToAsync(outputStream);
        }

        public Task<Stream> Read(string name, int versionIndex)
        {
            BlobEntry[] blobEntries = GetBlobEntries(name).ToArray();
            if (blobEntries.Length == 0)
            {
                return Task.FromResult((Stream) null);
            }

            // convert the version index to a real index
            versionIndex = versionIndex%blobEntries.Length;
            if (versionIndex < 0 || versionIndex >= blobEntries.Length)
            {
                return Task.FromResult((Stream) null);
            }

            // get the path
            string filePath = blobEntries[versionIndex].FilePath;

            // read the file
            return Task.FromResult(_fileSystem.FileOpen(filePath, FileMode.Open));
        }

        private string GetPath(string name, string version, string tag, string hash)
        {
            return Path.Combine(_directory, string.Join("", new[]
            {
                name,
                FieldSeperator,
                version,
                FieldSeperator,
                tag ?? string.Empty,
                tag == null ? string.Empty : FieldSeperator,
                hash,
                FileExtension
            }));
        }

        /// <summary>
        ///     Returns <see cref="BlobEntry" /> instances matching the given name. The each item in the
        ///     sequence is guaranteed to have a unique version.
        /// </summary>
        /// <param name="searchName">The name of the <see cref="BlobEntry" /> instances to fetch.</param>
        /// <returns>A sequence of <see cref="BlobEntry" /> instances.</returns>
        private IEnumerable<BlobEntry> GetBlobEntries(string searchName)
        {
            string searchPattern = string.Format("{0}{1}*{2}", searchName, FieldSeperator, FileExtension);
            IOrderedEnumerable<string> filePaths = _fileSystem
                .DirectoryEnumerateFiles(_directory, searchPattern, SearchOption.TopDirectoryOnly)
                .OrderBy(s => s);

            ISet<string> versions = new HashSet<string>();
            foreach (string filePath in filePaths)
            {
                // get the file name
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                if (fileName == null)
                {
                    continue;
                }

                // split the fields
                string[] pieces = fileName.Split(new[] {FieldSeperator}, StringSplitOptions.None);
                if (pieces.Length < 3)
                {
                    continue;
                }

                // get the name
                string name = pieces[0];

                // get the version
                string version = pieces[1];
                if (!IsVersion(version))
                {
                    continue;
                }
                if (versions.Contains(version))
                {
                    var exception = new ArgumentException("There must not be files with name '{0}' and version '{1}'.");
                    Logger.ErrorException(string.Format("Name: '{0}', Version: '{1}', FilePath: '{2}'", name, version, filePath), exception);
                    throw exception;
                }
                versions.Add(version);

                // get the tag
                string tag = string.Join(FieldSeperator, pieces.Skip(2).TakeWhile((s, i) => i < pieces.Length - 1));

                // get the hash
                string hash = pieces.Last();
                if (!IsHash(hash))
                {
                    continue;
                }

                yield return new BlobEntry(filePath, name, version, tag, hash);
            }
        }

        private static bool IsHash(string input)
        {
            return input.All(c => ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F')));
        }

        private static bool IsVersion(string input)
        {
            return input.Length > 0 && (
                input == "Version" ||
                (input[0] == '0' && input.Length == 1) ||
                (input[0] != '0' && input.Skip(1).All(c => c >= '0' && c <= '9')));
        }

        private class BlobEntry
        {
            private readonly string _filePath;
            private readonly string _hash;
            private readonly string _name;
            private readonly string _tag;
            private readonly string _version;

            public BlobEntry(string filePath, string name, string version, string tag, string hash)
            {
                _filePath = filePath;
                _name = name;
                _version = version;
                _tag = tag;
                _hash = hash;
            }

            public string Name
            {
                get { return _name; }
            }

            public string Tag
            {
                get { return _tag; }
            }

            public string Hash
            {
                get { return _hash; }
            }

            public string Version
            {
                get { return _version; }
            }

            public string FilePath
            {
                get { return _filePath; }
            }
        }
    }
}
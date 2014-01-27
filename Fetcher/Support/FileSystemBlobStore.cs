using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace OdjfsScraper.Fetcher.Support
{
    public class FileSystemBlobStore : IFileSystemBlobStore
    {
        private const string CurrentBlobKeyword = "Current";
        private const int CurrentBlobValue = int.MaxValue;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IFileSystem _fileSystem;
        private readonly IHashAlgorithm _hashAlgorithm;
        private string _directory;
        private string _fieldSeperator;
        private string _fileExtension;
        private bool _isDirectoryChecked;

        public FileSystemBlobStore(IHashAlgorithm hashAlgorithm, IFileSystem fileSystem)
        {
            _hashAlgorithm = hashAlgorithm;
            _fileSystem = fileSystem;
            _isDirectoryChecked = false;
            FieldSeperator = "_";
            FileExtension = ".blob";
        }

        public string FileExtension
        {
            get { return _fileExtension; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(string.Format("The file extension '{0}' must not be null or empty.", value));
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
                if (string.IsNullOrEmpty(value))
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
                if (FileExtension != null && FileExtension.Contains(value))
                {
                    throw new ArgumentException(string.Format("The file extension '{0}' must not contain the field seperator '{1}'.", FileExtension, value));
                }

                _fieldSeperator = value;
            }
        }

        public string Directory
        {
            get { return _directory; }
            set
            {
                if (value != _directory)
                {
                    _isDirectoryChecked = false;
                    _directory = value;
                }
            }
        }

        public void VerifyDirectory()
        {
            if (Directory == null)
            {
                throw new InvalidOperationException("The directory must be set before using the blob store.");
            }
            if (!_isDirectoryChecked)
            {
                if (!_fileSystem.DirectoryExists(Directory))
                {
                    _fileSystem.DirectoryCreateDirectory(Directory);
                }
                _isDirectoryChecked = true;
            }
        }

        public Task Write(string name, string tag, Stream stream)
        {
            if (name.Contains(FieldSeperator))
            {
                throw new ArgumentException(string.Format("The name '{0}' must not contain the field seperator '{1}'.", name, FieldSeperator), "name");
            }
            VerifyDirectory();

            return WriteWithoutValidation(name, tag, stream);
        }

        public Task<Stream> Read(string name, int versionIndex)
        {
            VerifyDirectory();

            BlobEntry[] blobEntries = GetBlobEntries(name).ToArray();
            if (blobEntries.Length == 0)
            {
                return Task.FromResult((Stream) null);
            }

            // convert the version index to a real index
            if (versionIndex < 0)
            {
                versionIndex += blobEntries.Length;
            }
            if (versionIndex < 0 || versionIndex >= blobEntries.Length)
            {
                return Task.FromResult((Stream) null);
            }

            // get the path
            string filePath = blobEntries[versionIndex].FilePath;

            // read the file
            return Task.FromResult(_fileSystem.FileOpen(filePath, FileMode.Open));
        }

        public Task<IDictionary<string, int>> GetNames()
        {
            IDictionary<string, int> entries = GetBlobEntries(null)
                .GroupBy(b => b.Name)
                .ToDictionary(g => g.Key, g => g.Count());

            return Task.FromResult(entries);
        }

        private async Task WriteWithoutValidation(string name, string tag, Stream stream)
        {
            // fully buffer the stream if it is not seekable
            if (!stream.CanSeek)
            {
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                stream = memoryStream;
                stream.Position = 0;
            }

            // get the hash, and reset the stream
            string hash = _hashAlgorithm.ComputeHashToString(stream);
            if (hash.Contains(FieldSeperator))
            {
                throw new InvalidOperationException(string.Format(
                    "The hash '{0}' must not contain the field seperator '{1}'.",
                    hash,
                    FieldSeperator));
            }
            stream.Position = 0;

            // get the two most recent versions
            BlobEntry[] blobEntries = GetBlobEntries(name)
                .OrderByDescending(b => b.Version)
                .Take(2)
                .ToArray();

            // have there been any changes?
            if (blobEntries.Length > 0 && blobEntries[0].Version == CurrentBlobValue)
            {
                if (blobEntries[0].Hash == hash && blobEntries[0].Tag == tag)
                {
                    return;
                }

                string oldCurrentPath = blobEntries[0].FilePath;
                string newVersionPath = GetPath(
                    blobEntries[0].Name,
                    blobEntries.Length == 2 ? (blobEntries[1].Version + 1).ToString(CultureInfo.InvariantCulture) : "0",
                    blobEntries[0].Tag,
                    blobEntries[0].Hash);

                _fileSystem.FileMove(oldCurrentPath, newVersionPath);
            }

            // generate the file path
            string newCurrentPath = GetPath(name, CurrentBlobKeyword, tag, hash);

            // write the stream
            using (Stream outputStream = _fileSystem.FileOpen(newCurrentPath, FileMode.Create))
            {
                await stream.CopyToAsync(outputStream);
            }
        }

        private string GetPath(string name, string version, string tag, string hash)
        {
            return Path.Combine(Directory, string.Join("", new[]
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
            string searchPattern = searchName == null ? "*" : string.Format("{0}{1}*", searchName, FieldSeperator);
            IOrderedEnumerable<string> filePaths = _fileSystem
                .DirectoryEnumerateFiles(Directory, searchPattern, SearchOption.TopDirectoryOnly)
                .OrderBy(s => s);

            IDictionary<string, ISet<int>> allVersions = new Dictionary<string, ISet<int>>();
            foreach (string filePath in filePaths)
            {
                // make sure the file extension matches
                string fileExtension = Path.GetExtension(filePath);
                if (fileExtension != FileExtension)
                {
                    continue;
                }

                // get the file name
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                // split the fields
                string[] pieces = fileName.Split(new[] {FieldSeperator}, StringSplitOptions.None);
                if (pieces.Length < 3)
                {
                    continue;
                }

                // get the name
                string name = pieces[0];
                ISet<int> versions;
                if (!allVersions.TryGetValue(name, out versions))
                {
                    versions = new HashSet<int>();
                    allVersions[name] = versions;
                }

                // get the version
                string stringVersion = pieces[1];
                int version;
                if (!TryParseVersion(stringVersion, out version))
                {
                    continue;
                }
                if (versions.Contains(version))
                {
                    var exception = new ArgumentException(string.Format("There must not be multiple files with name '{0}' and version '{1}'.",
                        name,
                        version));
                    Logger.ErrorException(string.Format("Name: '{0}', Version: '{1}', FilePath: '{2}'", name, version, filePath), exception);
                    throw exception;
                }
                versions.Add(version);

                // get the tag
                const int skip = 2;
                string[] tagPieces = pieces
                    .Skip(skip)
                    .TakeWhile((s, i) => i < pieces.Length - (skip + 1))
                    .ToArray();
                string tag = tagPieces.Length == 0 ? null : string.Join(FieldSeperator, tagPieces);

                // get the hash
                string hash = pieces.Last();

                yield return new BlobEntry(filePath, name, version, tag, hash);
            }
        }

        private static bool TryParseVersion(string input, out int output)
        {
            if (input == CurrentBlobKeyword)
            {
                output = CurrentBlobValue;
                return true;
            }
            if (int.TryParse(input, out output))
            {
                return true;
            }
            return false;
        }

        private class BlobEntry
        {
            private readonly string _filePath;
            private readonly string _hash;
            private readonly string _name;
            private readonly string _tag;
            private readonly int _version;

            public BlobEntry(string filePath, string name, int version, string tag, string hash)
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

            public int Version
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
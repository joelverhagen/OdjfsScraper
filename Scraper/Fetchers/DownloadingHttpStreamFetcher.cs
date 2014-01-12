using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;
using OdjfsScraper.Scraper.Support;

namespace OdjfsScraper.Scraper.Fetchers
{
    public class DownloadingHttpStreamFetcher : HttpStreamFetcher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _directory;
        private bool _hasDirectoryBeenChecked;

        public DownloadingHttpStreamFetcher(string directory)
        {
            _directory = directory;
            _hasDirectoryBeenChecked = false;
        }

        protected override Task<Stream> GetChildCareDocumentStream(HttpResponseMessage response, ChildCare childCare)
        {
            return GetChildCareDocumentStream(childCare.ExternalUrlId, response.StatusCode, () => base.GetChildCareDocumentStream(response, childCare));
        }

        protected override Task<Stream> GetChildCareDocumentStream(HttpResponseMessage response, ChildCareStub childCareStub)
        {
            return GetChildCareDocumentStream(childCareStub.ExternalUrlId, response.StatusCode, () => base.GetChildCareDocumentStream(response, childCareStub));
        }

        private async Task<Stream> GetChildCareDocumentStream(string externalUrlId, HttpStatusCode httpStatusCode, Func<Task<Stream>> getStream)
        {
            // get the actual stream
            Stream stream = await getStream();

            // write the stream to disk and read then read from disk
            return await WriteAndGetStream(string.Format("ChildCare_{0}", externalUrlId), httpStatusCode, stream);
        }

        protected override async Task<Stream> GetChildCareStubListDocumentStream(HttpResponseMessage response, County county)
        {
            // get the actual stream
            Stream stream = await base.GetChildCareStubListDocumentStream(response, county);

            // write the stream to disk and read then read from disk
            return await WriteAndGetStream(string.Format("County_{0}", county.Name), response.StatusCode, stream);
        }

        private async Task<Stream> WriteAndGetStream(string fileNamePrefix, HttpStatusCode httpStatusCode, Stream stream)
        {
            // buffer the stream
            byte[] bytes = await stream.ReadAsByteArrayAsync();

            // make sure the directory exists before writing to it...
            if (!_hasDirectoryBeenChecked)
            {
                if (!Directory.Exists(_directory))
                {
                    Directory.CreateDirectory(_directory);
                }
                _hasDirectoryBeenChecked = true;
            }

            // generate a path for the child care
            string newCurrentPath = Path.Combine(_directory, string.Format(
                "{0}_Current_{1}_{2}.html",
                fileNamePrefix,
                httpStatusCode,
                bytes.GetSha256Hash()));

            if (!File.Exists(newCurrentPath))
            {
                // make sure the old "Current" file is moved
                MoveOldCurrentVersion(fileNamePrefix);

                // write the new "Current" file
                using (var outputStream = new FileStream(newCurrentPath, FileMode.Create, FileAccess.Write))
                {
                    // write to the file
                    await outputStream.WriteAsync(bytes, 0, bytes.Length);
                }
            }

            return new FileStream(newCurrentPath, FileMode.Open);
        }

        private void MoveOldCurrentVersion(string fileNamePrefix)
        {
            // get all of the file names with the same prefix
            IEnumerable<string> filePaths = Directory.EnumerateFiles(_directory,
                string.Format("{0}*.html", fileNamePrefix), SearchOption.TopDirectoryOnly);

            int largestVersionNumber = -1;
            string oldCurrentFileName = null;
            foreach (string filePath in filePaths)
            {
                // parse the file name
                string fileName = Path.GetFileName(filePath);
                string identifer = fileName.Substring(fileNamePrefix.Length + 1);
                string[] pieces = identifer.Split(new[] {"_"}, 2, StringSplitOptions.None);
                if (pieces.Length != 2)
                {
                    continue;
                }
                string version = pieces[0];
                int versionNumber;
                if (version == "Current")
                {
                    if (oldCurrentFileName != null)
                    {
                        var exception = new ScraperException("There are more than one 'Current' files for a prefix.");
                        Logger.ErrorException(
                            string.Format(
                                "FirstFound: '{0}', SecondFound: '{1}', Directory: '{2}', AbsoluteDirectory: '{3}', FileNamePrefix: '{4}'",
                                oldCurrentFileName,
                                fileName,
                                _directory,
                                Path.GetFullPath(_directory),
                                fileNamePrefix),
                            exception);
                        throw exception;
                    }
                    oldCurrentFileName = fileName;
                }
                else if (int.TryParse(version, out versionNumber))
                {
                    largestVersionNumber = Math.Max(largestVersionNumber, versionNumber);
                }
            }

            // move the old "Current" file to having a version index
            if (oldCurrentFileName != null)
            {
                string newIndexFileName = Regex.Replace(
                    Path.GetFileName(oldCurrentFileName),
                    string.Format("^(?<Prefix>{0}_)(?<Version>[^_]+)(?<Suffix>.+)$", fileNamePrefix),
                    string.Format("${{Prefix}}{0}${{Suffix}}", largestVersionNumber + 1));

                string fromPath = Path.Combine(_directory, oldCurrentFileName);
                string toPath = Path.Combine(_directory, newIndexFileName);

                File.Move(fromPath, toPath);
            }
        }
    }
}
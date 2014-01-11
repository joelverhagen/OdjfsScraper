using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Scraper.Support
{
    public class DownloadingHttpReader : HttpReader
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _directory;
        private bool _hasDirectoryBeenChecked;

        public DownloadingHttpReader(string directory)
        {
            _directory = directory;
            _hasDirectoryBeenChecked = false;
        }

        protected override async Task HandleChildCareDocumentResponse(ChildCare childCare, ClientResponse response)
        {
            await WriteChildCareBytes(childCare.ExternalUrlId, response);
        }

        protected override async Task HandleListDocumentResponse(County county, ClientResponse response)
        {
            string countyName = county == null ? "all" : county.Name;
            await WriteBytes("County_" + countyName, response);
        }

        protected override async Task HandleChildCareDocumentResponse(ChildCareStub childCareStub,
            ClientResponse response)
        {
            await WriteChildCareBytes(childCareStub.ExternalUrlId, response);
        }

        private async Task WriteChildCareBytes(string externalUrlId, ClientResponse response)
        {
            await WriteBytes("ChildCare_" + externalUrlId, response);
        }

        private async Task WriteBytes(string fileNamePrefix, ClientResponse response)
        {
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
                response.StatusCode,
                response.Content.GetSha256Hash()));

            if (File.Exists(newCurrentPath))
            {
                return;
            }

            // make sure the old "Current" file is moved
            MoveOldCurrentVersion(fileNamePrefix, newCurrentPath);

            // write the new "Current" file
            using (var outputStream = new FileStream(newCurrentPath, FileMode.Create, FileAccess.Write))
            {
                // write to the file
                await outputStream.WriteAsync(response.Content, 0, response.Content.Length);
            }
        }

        private void MoveOldCurrentVersion(string fileNamePrefix, string newCurrentPath)
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
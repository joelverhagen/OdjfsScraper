using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using NLog;

namespace OdjfsScraper.DataChecker.Support
{
    public class HtmlDirectoryImporter : IHtmlDirectoryImporter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public async Task Import(string path)
        {
            IEnumerable<string> files = Directory.EnumerateFiles(path, "*.html", SearchOption.TopDirectoryOnly);

            foreach (string file in files)
            {
                HtmlFile htmlFile;
                if (!HtmlFile.TryParse(file, out htmlFile))
                {
                    continue;
                }

                switch (htmlFile.Type)
                {
                    case HtmlFileType.ChildCare:
                        await ParseChildCare(htmlFile);
                        break;
                    case HtmlFileType.County:
                        await ParseCounty(htmlFile);
                        break;
                    default:
                        var exception = new NotImplementedException("The provided file type is not supported.");
                        Logger.ErrorException(string.Format("Path: {0}, HtmlFileType: {1}", file, htmlFile.Type), exception);
                        throw exception;
                }
            }
        }

        private async Task ParseCounty(HtmlFile htmlFile)
        {
            Console.WriteLine("Parsing county: {0}", htmlFile.Identifier);
        }

        private async Task ParseChildCare(HtmlFile htmlFile)
        {
            Console.WriteLine("Parsing child care: {0}", htmlFile.Identifier);
        }

        private struct HtmlFile
        {
            private readonly string _hash;
            private readonly HttpStatusCode _httpStatusCode;
            private readonly string _identifier;
            private readonly string _path;
            private readonly HtmlFileType _type;
            private readonly int _version;

            public HtmlFile(string path, HtmlFileType type, string identifier, int version, HttpStatusCode httpStatusCode, string hash)
            {
                _path = path;
                _type = type;
                _identifier = identifier;
                _version = version;
                _httpStatusCode = httpStatusCode;
                _hash = hash;
            }

            public string Path
            {
                get { return _path; }
            }

            public HtmlFileType Type
            {
                get { return _type; }
            }

            public string Identifier
            {
                get { return _identifier; }
            }

            public int Version
            {
                get { return _version; }
            }

            public HttpStatusCode HttpStatusCode
            {
                get { return _httpStatusCode; }
            }

            public string Hash
            {
                get { return _hash; }
            }

            public static bool TryParse(string path, out HtmlFile htmlFile)
            {
                htmlFile = default(HtmlFile);

                // get the file name
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (fileName == null)
                {
                    return false;
                }

                // get the tokens
                string[] pieces = fileName.Split('_');
                if (pieces.Length != 5)
                {
                    return false;
                }

                // parse the type
                HtmlFileType htmlFileType;
                if (!Enum.TryParse(pieces[0], out htmlFileType))
                {
                    return false;
                }

                // parse the version
                int version;
                if (pieces[2] == "Current")
                {
                    version = int.MaxValue;
                }
                else if (!int.TryParse(pieces[2], out version))
                {
                    return false;
                }

                // parse the HttpStatusCode
                HttpStatusCode httpStatusCode;
                if (!Enum.TryParse(pieces[3], out httpStatusCode))
                {
                    return false;
                }

                htmlFile = new HtmlFile(path, htmlFileType, pieces[1], version, httpStatusCode, pieces[4]);
                return true;
            }
        }

        private enum HtmlFileType
        {
            County,
            ChildCare
        }
    }
}
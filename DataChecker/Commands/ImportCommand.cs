using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManyConsole;
using NLog;
using OdjfsScraper.Database;
using OdjfsScraper.Synchronizer.Synchronizers;

namespace OdjfsScraper.DataChecker.Commands
{
    public enum ImportFormat
    {
        HtmlDir,
        Bak,
    };

    public class ImportCommand : DatabaseCommand, IImportCommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly IDictionary<string, ImportFormat> Formats = Enum.GetValues(typeof (ImportFormat))
            .OfType<ImportFormat>()
            .ToDictionary(e => e.ToString().ToUpper());

        private static readonly string FormatNames = string.Join(", ", Formats.Keys.OrderBy(f => f));

        private static readonly IDictionary<ImportFormat, ISet<string>> FormatFileExtensions = new Dictionary<ImportFormat, ISet<string>>
        {
            {ImportFormat.Bak, new HashSet<string> {".bak"}}
        };

        private readonly IChildCareSynchronizer _childCareSynchronizer;
        private readonly ICountySynchronizer _countySynchronizer;

        public ImportCommand(ICountySynchronizer countySynchronizer, IChildCareSynchronizer childCareSynchronizer)
        {
            _countySynchronizer = countySynchronizer;
            _childCareSynchronizer = childCareSynchronizer;

            IsCommand("import", "import data into the ODJFS database");
            HasRequiredOption("format=", string.Format("the import format; acceptable formats: {0}", FormatNames), v => Format = ParseFormat(v));
            HasRequiredOption("path=", "the path of where the import will be read from", v => Path = v);
            HasOption("truncate", "reset the database before importing", v => Truncate = true);
        }

        public bool Truncate { get; set; }
        public ImportFormat? Format { get; set; }
        public string Path { get; set; }

        private string FormatName
        {
            get
            {
                if (Format == null)
                {
                    return string.Empty;
                }
                return Format.ToString().ToUpper();
            }
        }

        public override int? OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments)
        {
            base.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments);

            // validate format
            if (Format == null)
            {
                throw new ConsoleHelpAsException(string.Format("The provided format is not valid. Acceptable formats: {0}", FormatNames));
            }

            // validate path file extension
            ISet<string> acceptableExtensions;
            if (FormatFileExtensions.TryGetValue(Format.Value, out acceptableExtensions) && !acceptableExtensions.Contains(System.IO.Path.GetExtension(Path)))
            {
                throw new ConsoleHelpAsException(string.Format("When exporting with the '{0}' format, the --path value must have one of the following file extensions: {1}.",
                    FormatName,
                    string.Join(", ", acceptableExtensions)));
            }
            if (Format == ImportFormat.HtmlDir)
            {
                if (File.Exists(Path))
                {
                    throw new ConsoleHelpAsException("The provided path is pointing to a file, not a directory.");
                }
                if (!Directory.Exists(Path))
                {
                    throw new ConsoleHelpAsException("The provided directory path does not exist.");
                }
            }

            return null;
        }

        public override int Run(string[] remainingArguments)
        {
            string fullPath = System.IO.Path.GetFullPath(Path);
            Logger.Trace("An {0} import will be performed from the following location:{1}  {2}", FormatName, Environment.NewLine, fullPath);

            switch (Format)
            {
                case ImportFormat.HtmlDir:
                    using (var ctx = new Entities())
                    {
                        _countySynchronizer.UpdateAvailableCounties(ctx).Wait();
                        _childCareSynchronizer.UpdateAvailableChildCares(ctx).Wait();
                    }
                    break;
                default:
                    throw new NotImplementedException(string.Format("Woops! The '{0}' import format is not yet implemented.", FormatName));
            }

            return 0;
        }

        private static ImportFormat? ParseFormat(string input)
        {
            input = input.Trim().ToUpper();
            ImportFormat output;
            if (Formats.TryGetValue(input, out output))
            {
                return output;
            }
            return output;
        }
    }
}
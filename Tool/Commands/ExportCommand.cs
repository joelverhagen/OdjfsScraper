using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using ManyConsole;
using NLog;
using OdjfsScraper.Database;
using OdjfsScraper.Exporter.Exporters;
using OdjfsScraper.Model.ChildCares;

namespace OdjfsScraper.Tool.Commands
{
    public enum ExportFormat
    {
        Srds,
        Bak,
    };

    public class ExportCommand : DatabaseCommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly IDictionary<string, ExportFormat> Formats = Enum.GetValues(typeof (ExportFormat))
            .OfType<ExportFormat>()
            .ToDictionary(e => e.ToString().ToUpper());

        private static readonly string FormatNames = string.Join(", ", Formats.Keys.OrderBy(f => f));

        private static readonly IDictionary<ExportFormat, ISet<string>> FormatFileExtensions = new Dictionary<ExportFormat, ISet<string>>
        {
            {ExportFormat.Srds, new HashSet<string> {".zip"}},
            {ExportFormat.Bak, new HashSet<string> {".bak"}}
        };

        private readonly ISrdsExporter<DetailedChildCare> _srdsExporter;

        public ExportCommand(ISrdsExporter<DetailedChildCare> srdsExporter)
        {
            _srdsExporter = srdsExporter;
            IsCommand("export", "export ODJFS data in various formats");
            HasRequiredOption("format=", string.Format("the export format; acceptable formats: {0}", FormatNames), v => Format = ParseFormat(v));
            HasRequiredOption("path=", "the file path of where the export will be written", v => Path = v);
        }

        public ExportFormat? Format { get; set; }
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

        private static ExportFormat? ParseFormat(string input)
        {
            input = input.Trim().ToUpper();
            ExportFormat output;
            if (Formats.TryGetValue(input, out output))
            {
                return output;
            }
            return null;
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

            return null;
        }

        public override int Run(string[] remainingArguments)
        {
            string fullPath = System.IO.Path.GetFullPath(Path);
            Logger.Trace("An {0} export will be created at the following location:{1}  {2}", FormatName, Environment.NewLine, fullPath);

            switch (Format)
            {
                case ExportFormat.Srds:
                    using (var ctx = new Entities())
                    {
                        IEnumerable<DetailedChildCare> childCares = ctx
                            .DetailedChildCares.Where(d => d.Latitude.HasValue && d.Longitude.HasValue);

                        using (var fileStream = new FileStream(Path, FileMode.Create, FileAccess.Write))
                        {
                            _srdsExporter.Export(childCares, fileStream);
                        }
                    }
                    break;
                case ExportFormat.Bak:
                    using (var ctx = new Entities())
                    {
                        ctx.Database.ExecuteSqlCommand(
                            TransactionalBehavior.DoNotEnsureTransaction,
                            "BACKUP DATABASE @Database TO DISK = @Path WITH COPY_ONLY, INIT",
                            new SqlParameter("@Database", ctx.Database.Connection.Database),
                            new SqlParameter("@Path", fullPath));
                    }
                    break;
                default:
                    throw new NotImplementedException(string.Format("Woops! The '{0}' export format is not yet implemented.", FormatName));
            }

            return 0;
        }
    }
}
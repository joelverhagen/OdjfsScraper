using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManyConsole;
using OdjfsScraper.Database;
using OdjfsScraper.Exporter.Exporters;
using OdjfsScraper.Model.ChildCares;

namespace OdjfsScraper.DataChecker.Commands
{
    public class ExportCommand : Command
    {
        private static readonly ISet<string> Formats = new HashSet<string> {"srds"};

        public ExportCommand()
        {
            IsCommand("export", "export ODJFS data in various formats");
            HasRequiredOption("format=", string.Format("the export format; acceptable formats are: {0}", string.Join(", ", Formats)), v => Format = v);
            HasRequiredOption("path=", "the file path of where the export will be written to", v => Path = v);
        }

        public string Format { get; set; }
        public string Path { get; set; }

        public override int? OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments)
        {
            base.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments);

            Format = Format.ToLower().Trim();
            if (!Formats.Contains(Format))
            {
                throw new ConsoleHelpAsException(string.Format("The provided format is not valid. Acceptable formats are: {0}", string.Join(", ", Formats)));
            }

            return null;
        }

        public override int Run(string[] remainingArguments)
        {
            if (Format == "srds")
            {
                ISrdsExporter<DetailedChildCare> srdsExporter = this.GetSrdsExporter<DetailedChildCare>();

                using (var ctx = new Entities())
                {
                    IEnumerable<DetailedChildCare> childCares = ctx
                        .DetailedChildCares
                        .Where(d => d.Latitude.HasValue && d.Longitude.HasValue);

                    using (var fileStream = new FileStream(Path, FileMode.Create, FileAccess.Write))
                    {
                        srdsExporter.Export(childCares, fileStream);
                    }
                }
            }
            else
            {
                throw new NotImplementedException(string.Format("Woops! The exporter for format '{0}' is not implemented.", Format));
            }

            return 0;
        }
    }
}
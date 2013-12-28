using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManyConsole;
using Ninject;
using Ninject.Extensions.Conventions;
using NLog;
using OdjfsScraper.DataChecker.Commands;
using OdjfsScraper.Scraper.Support;
using PolyGeocoder.Geocoders;
using PolyGeocoder.Support;

namespace OdjfsScraper.DataChecker
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int Main(string[] args)
        {
            try
            {
                IKernel kernel = new StandardKernel();

                // discover... everything
                kernel.Bind(c => c
                    .From("OdjfsScraper.DataChecker.exe", "OdjfsScraper.Exporter.dll", "OdjfsScraper.Scraper.dll")
                    .SelectAllClasses()
                    .BindAllInterfaces());

                // clear up one ambiguity; we want to save ALL fetched HTML
                kernel.Unbind<IOdjfsClient>();
                kernel.Bind<IOdjfsClient>()
                    .To<DownloadingOdjfsClient>()
                    .WithConstructorArgument("directory", @"Logs\HTML");

                // set a user agent on the geocoder client
                kernel.Unbind<IClient>();
                kernel.Bind<IClient>()
                    .To<Client>()
                    .WithConstructorArgument("userAgent", ScraperClient.GetUserAgent());

                // choose a geocoder
                kernel.Unbind<ISimpleGeocoder>();
                kernel.Bind<ISimpleGeocoder>()
                    .To<MapQuestGeocoder>()
                    .WithConstructorArgument("endpoint", MapQuestGeocoder.LicensedEndpoint)
                    .WithConstructorArgument("key", Settings.MapQuestKey);

                // activate all commands
                IEnumerable<ConsoleCommand> commands = kernel
                    .GetAll<ICommand>()
                    .OfType<ConsoleCommand>();

                return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
            }
            catch (Exception e)
            {
                Logger.ErrorException("An error occurred during the execution of OdjfsScraper.DataChecker.", e);
                var sb = new StringBuilder();
                TraceExceptionMessages(sb, e, 1);
                Logger.Error(sb.ToString().Trim());
                return 1;
            }
        }

        private static void TraceExceptionMessages(StringBuilder sb, Exception e, int level)
        {
            if (e == null)
            {
                return;
            }

            // limit depth of recursion, since this is being run in a catch statement (gotta be careful)
            sb.AppendFormat("{0} {1}", new string('=', level), level > 10 ? "..." : e.Message);
            sb.AppendLine();
            if (level > 10)
            {
                return;
            }

            // recurse on child exceptions
            var ae = e as AggregateException;
            if (ae != null)
            {
                foreach (Exception child in ae.InnerExceptions)
                {
                    TraceExceptionMessages(sb, child, level + 1);
                }
            }
            else
            {
                TraceExceptionMessages(sb, e.InnerException, level + 1);
            }
        }
    }
}
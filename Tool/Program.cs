using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using ManyConsole;
using Ninject;
using Ninject.Extensions.Conventions;
using NLog;
using OdjfsScraper.Exporter.Exporters;
using OdjfsScraper.Fetcher.Fetchers;
using OdjfsScraper.Fetcher.Support;
using OdjfsScraper.Model.Fetchers;
using OdjfsScraper.Parser.Parsers;
using OdjfsScraper.Synchronizer.Synchronizers;
using OdjfsScraper.Tool.Commands;
using PolyGeocoder.Geocoders;
using PolyGeocoder.Support;

namespace OdjfsScraper.Tool
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
                    .FromAssemblyContaining(new[]
                    {
                        typeof (IChildCareFetcher),
                        typeof (ICommand),
                        typeof (SrdsExporter<>),
                        typeof (IChildCareParser),
                        typeof (IHttpStreamFetcher),
                        typeof (IChildCareSynchronizer)
                    })
                    .SelectAllClasses()
                    .BindAllInterfaces());

                // configure the HTTP stream fetcher
                kernel.Unbind<IStreamFetcher>();
                kernel.Bind<IStreamFetcher>()
                    .To<DownloadingHttpStreamFetcher>()
                    .WithConstructorArgument("httpMessageHandler", new WebRequestHandler())
                    .WithConstructorArgument("userAgent", GetUserAgent());

                // configure the file system blob store
                kernel.Unbind<IFileSystemBlobStore>();
                kernel.Bind<IFileSystemBlobStore>()
                    .To<FileSystemBlobStore>()
                    .WithPropertyValue("Directory", Settings.HtmlDirectory)
                    .WithPropertyValue("FileExtension", ".html");

                // specify the logs directory
                GlobalDiagnosticsContext.Set("LogsDirectory", Settings.LogsDirectory);

                // set a user agent on the geocoder client
                kernel.Unbind<IClient>();
                kernel.Bind<IClient>()
                    .To<Client>()
                    .WithConstructorArgument("userAgent", GetUserAgent());

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
                Logger.ErrorException("An error occurred during the execution of OdjfsScraper.Tool.", e);
                var sb = new StringBuilder();
                TraceExceptionMessages(sb, e, 1);
                Logger.Error(sb.ToString().Trim());
                return 1;
            }
        }

        private static string GetUserAgent()
        {
            string version = Assembly.GetExecutingAssembly().GetInformationalVersion();
            return string.Format("OdjfsScraper/{0}", version);
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
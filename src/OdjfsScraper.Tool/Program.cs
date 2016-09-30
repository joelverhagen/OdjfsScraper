using System;
using System.Net.Http;
using System.Reflection;
using Knapcode.PolyGeocoder;
using Knapcode.ToStorage.Core.Abstractions;
using Knapcode.ToStorage.Core.AzureBlobStorage;
using ManyConsole;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OdjfsScraper.Database;
using OdjfsScraper.Exporters;
using OdjfsScraper.Fetchers;
using OdjfsScraper.Geocode;
using OdjfsScraper.Models;
using OdjfsScraper.Parsers;
using OdjfsScraper.Synchronizers;
using OdjfsScraper.Tool.Commands;
using IPolyGeocoderClient = Knapcode.PolyGeocoder.IClient;
using IToStorageClient = Knapcode.ToStorage.Core.AzureBlobStorage.IClient;
using PolyGeocoderClient = Knapcode.PolyGeocoder.Client;
using ToStorageClient = Knapcode.ToStorage.Core.AzureBlobStorage.Client;

namespace OdjfsScraper.Tool
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddLogging();
                
                serviceCollection.AddTransient<ChildCareCommand>();
                serviceCollection.AddTransient<CountyCommand>();
                serviceCollection.AddTransient<DaemonCommand>();
                serviceCollection.AddTransient<ExportCommand>();
                serviceCollection.AddTransient<GeocodeCommand>();
                serviceCollection.AddTransient<IChildCareFetcher, ChildCareFetcher>();
                serviceCollection.AddTransient<IChildCareParser, ChildCareParser>();
                serviceCollection.AddTransient<IChildCareParser<DayCamp>, DayCampParser>();
                serviceCollection.AddTransient<IChildCareParser<LicensedCenter>, LicensedCenterParser>();
                serviceCollection.AddTransient<IChildCareParser<TypeAHome>, TypeAHomeParser>();
                serviceCollection.AddTransient<IChildCareParser<TypeBHome>, TypeBHomeParser>();
                serviceCollection.AddTransient<IChildCareStubListFetcher, ChildCareStubListFetcher>();
                serviceCollection.AddTransient<IChildCareStubListParser, ChildCareStubListParser>();
                serviceCollection.AddTransient<IChildCareSynchronizer, ChildCareSynchronizer>();
                serviceCollection.AddTransient<ICountySynchronizer, CountySynchronizer>();
                serviceCollection.AddTransient<IGeocoder, Geocoder>();
                serviceCollection.AddTransient<IMigrationService, MigrationService>();
                serviceCollection.AddTransient<IPathBuilder, PathBuilder>();
                serviceCollection.AddTransient<ISrdsExporter<DetailedChildCare>, SrdsExporter<DetailedChildCare>>();
                serviceCollection.AddTransient<ISrdsSchema<DetailedChildCare>, DetailedChildCareSrdsSchema>();
                serviceCollection.AddTransient<ISystemTime, SystemTime>();
                serviceCollection.AddTransient<IToStorageClient, ToStorageClient>();
                serviceCollection.AddTransient<IUniqueClient, UniqueClient>();

                serviceCollection.AddTransient<IStreamFetcher, DownloadingHttpStreamFetcher>(provider =>
                {
                    return new DownloadingHttpStreamFetcher(
                        provider.GetRequiredService<ILogger<DownloadingHttpStreamFetcher>>(),
                        new WebRequestHandler(),
                        GetUserAgent(),
                        provider.GetRequiredService<IBlobStore>());
                });

                serviceCollection.AddTransient<IBlobStore, ToStorageBlobStore>(provider =>
                {
                    return new ToStorageBlobStore(
                        provider.GetRequiredService<IUniqueClient>(),
                        provider.GetRequiredService<IToStorageClient>(),
                        Settings.AzureBlobStorageConnectionString,
                        Settings.AzureBlobStorageContainer);
                });

                serviceCollection.AddTransient<IPolyGeocoderClient, PolyGeocoderClient>(provider =>
                {
                    return new PolyGeocoderClient(GetUserAgent());
                });

                serviceCollection.AddTransient<ISimpleGeocoder, MapQuestGeocoder>(provider =>
                {
                    return new MapQuestGeocoder(
                        provider.GetRequiredService<IPolyGeocoderClient>(),
                        MapQuestGeocoder.LicensedEndpoint,
                        Settings.MapQuestKey);
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();

                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                loggerFactory.AddConsole(LogLevel.Debug);

                var commands = new ConsoleCommand[]
                {
                    serviceProvider.GetRequiredService<ChildCareCommand>(),
                    serviceProvider.GetRequiredService<CountyCommand>(),
                    serviceProvider.GetRequiredService<DaemonCommand>(),
                    serviceProvider.GetRequiredService<ExportCommand>(),
                    serviceProvider.GetRequiredService<GeocodeCommand>()
                };

                return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("An error occurred during the execution of OdjfsScraper.Tool.");
                Console.Error.WriteLine(e);
                return 1;
            }
        }

        private static string GetUserAgent()
        {
            string version = Assembly.GetExecutingAssembly().GetInformationalVersion();
            return string.Format("OdjfsScraper/{0}", version);
        }
    }
}
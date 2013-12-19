using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Parameters;
using NLog;
using OdjfsScraper.Database;
using OdjfsScraper.Scraper.Scrapers;
using OdjfsScraper.Scraper.Support;

namespace OdjfsScraper.DataChecker
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static int Main(string[] args)
        {
            // parse the options
            string verb = null;
            object verbOptions = null;
            if (!Parser.Default.ParseArgumentsStrict(args, new Options(), (innerVerb, innerVerbOptions) =>
            {
                verb = innerVerb;
                verbOptions = innerVerbOptions;
            }))
            {
                Console.Error.WriteLine("There was an error when parsing the command line arguments.");
                return Parser.DefaultExitCodeFail;
            }

            // execute the options
            switch (verb)
            {
                case "crawl":
                    var crawlOptions = (CrawlOptions) verbOptions;
                    Crawl(crawlOptions.SleepDuration, crawlOptions.MaximumDuration);
                    break;
                case "childcare":
                    var childCareOptions = (ChildCareOptions) verbOptions;
                    if (childCareOptions.ExternalUrlId != null)
                    {
                        Update((ctx, odjfs) => odjfs.UpdateChildCare(ctx, childCareOptions.ExternalUrlId));
                    }
                    else
                    {
                        Update((ctx, odjfs) => odjfs.UpdateNextChildCare(ctx));
                    }

                    break;
                case "county":
                    var countyOptions = (CountyOptions) verbOptions;
                    if (countyOptions.Name != null)
                    {
                        Update((ctx, odjfs) => odjfs.UpdateCounty(ctx, countyOptions.Name));
                    }
                    else
                    {
                        Update((ctx, odjfs) => odjfs.UpdateNextCounty(ctx));
                    }
                    break;
                case "geocode":
                    var geocodeOptions = (GeocodeOptions) verbOptions;
                    if (geocodeOptions.ExternalUrlId != null)
                    {
                        Update((ctx, odjfs) => odjfs.GeocodeChildCare(ctx, geocodeOptions.ExternalUrlId));
                    }
                    else
                    {
                        for (int i = 0; i < geocodeOptions.Count; i++)
                        {
                            Update((ctx, odjfs) => odjfs.GeocodeNextChildCare(ctx));
                        }
                    }
                    break;
            }

            return 0;
        }

        private static void Crawl(int sleepDuration, int maximumDuration)
        {
            // start timing
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // update a list and sleep (if possible)
            Update((ctx, odjfs) => odjfs.UpdateNextCounty(ctx));
            if (stopwatch.ElapsedMilliseconds + sleepDuration >= maximumDuration)
            {
                return;
            }
            Thread.Sleep(sleepDuration);

            // update individual child cares for the rest of the time
            IList<long> times = new List<long>();
            long expectedEnd = stopwatch.ElapsedMilliseconds;
            while (expectedEnd < maximumDuration)
            {
                long before = stopwatch.ElapsedMilliseconds;

                // update a child care and sleep (if possible)
                Update((ctx, odjfs) => odjfs.UpdateNextChildCare(ctx));
                if (stopwatch.ElapsedMilliseconds + sleepDuration >= maximumDuration)
                {
                    return;
                }
                Thread.Sleep(sleepDuration);

                times.Add(stopwatch.ElapsedMilliseconds - before);
                expectedEnd = (long) times.Average() + stopwatch.ElapsedMilliseconds;
            }
        }

        private static void Update(Func<Entities, Odjfs, Task> odfjsTaskGenerator)
        {
            try
            {
                Logger.Trace("OdjfsDataChecker is now starting.");

                IKernel kernel = new StandardKernel();
                kernel.Bind(c => c
                    .FromAssemblyContaining(typeof (IChildCareStubListScraper))
                    .SelectAllClasses()
                    .BindAllInterfaces());

                var parameter = new ConstructorArgument("odjfsClient", new DownloadingOdjfsClient(@"Logs\HTML"));
                var odjfs = new Odjfs(kernel.Get<IChildCareStubListScraper>(parameter), kernel.Get<IChildCareScraper>(parameter));

                using (var ctx = new Entities())
                {
                    odfjsTaskGenerator(ctx, odjfs).Wait();
                }

                Logger.Trace("OdjfsDataChecker has completed.");
            }
            catch (Exception e)
            {
                Logger.ErrorException("An exception has forced the OdjfsDataChecker to terminate.", e);
            }
        }
    }
}
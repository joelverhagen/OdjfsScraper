using System;
using System.Net.Http;
using System.Threading;
using NLog;
using OdjfsScraper.Database;
using OdjfsScraper.DataChecker.Options;
using OdjfsScraper.DataChecker.Support;

namespace OdjfsScraper.DataChecker.Commands
{
    public class DaemonCommand : OdfjsSleepCommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly SleepOption _geocodeSleepOption;
        private readonly IOdjfsSynchronizer _odjfsSynchronizer;

        public DaemonCommand(IOdjfsSynchronizer odjfsSynchronizer) : base(2000)
        {
            _odjfsSynchronizer = odjfsSynchronizer;
            _geocodeSleepOption = new SleepOption("geocode", 0);
            Geocode = false;

            IsCommand("daemon", "continuously scrape country listing pages and child cares");
            HasOption(_geocodeSleepOption);
            HasOption("geocode", "whether or not to geocode child cares (default: false)", v => Geocode = true);
        }

        public bool Geocode { get; set; }

        public int? GeocodeSleep
        {
            get { return _geocodeSleepOption.Value; }
        }

        public override int? OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments)
        {
            base.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments);

            if (Geocode)
            {
                this.GetMapQuestKey();
            }

            return null;
        }

        public override int Run(string[] remainingArguments)
        {
            string mapQuestKey = null;
            if (Geocode)
            {
                mapQuestKey = this.GetMapQuestKey();
            }

            using (var ctx = new Entities())
            {
                var state = new DaemonEventLoop(ctx);
                var odjfsSleeper = new Sleeper(OdjfsSleep.Value);
                while (state.NextStep())
                {
                    odjfsSleeper.Sleep();
                    if (state.IsCountyStep)
                    {
                        IgnoreHttpRequestException(() => _odjfsSynchronizer.UpdateNextCounty(ctx).Wait());
                    }
                    else
                    {
                        IgnoreHttpRequestException(() => _odjfsSynchronizer.UpdateNextChildCare(ctx).Wait());

                        // geocode everything that needs geocoding...
                        var geocodeSleeper = new Sleeper(GeocodeSleep.Value);
                        while (Geocode && _odjfsSynchronizer.NeedsGeocoding(ctx).Result)
                        {
                            geocodeSleeper.Sleep();
                            _odjfsSynchronizer.GeocodeNextChildCare(ctx, mapQuestKey).Wait();
                        }
                    }
                }
            }

            return 0;
        }

        private void IgnoreHttpRequestException(Action action)
        {
            try
            {
                action();
            }
            catch (AggregateException ae)
            {
                foreach (Exception e in ae.InnerExceptions)
                {
                    var he = e as HttpRequestException;
                    if (he != null)
                    {
                        Logger.Trace("An unexpected HTTP status code was returned by ODJFS.");
                        Logger.Trace("OdjfsScraper.DataChecker will now sleep for 10 minutes.");
                        Thread.Sleep(10*60*1000);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
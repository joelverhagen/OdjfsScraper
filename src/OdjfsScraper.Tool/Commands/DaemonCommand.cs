﻿using System;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using OdjfsScraper.Database;
using OdjfsScraper.Geocode;
using OdjfsScraper.Synchronize;
using OdjfsScraper.Tool.Options;
using OdjfsScraper.Tool.Support;

namespace OdjfsScraper.Tool.Commands
{
    public class DaemonCommand : OdfjsSleepCommand
    {
        private readonly IChildCareSynchronizer _childCareSynchronizer;
        private readonly ICountySynchronizer _countySynchronizer;
        private readonly SleepOption _geocodeSleepOption;
        private readonly IGeocoder _geocoder;
        private readonly ILogger<DaemonCommand> _logger;

        public DaemonCommand(ILogger<DaemonCommand> logger, ICountySynchronizer countySynchronizer, IChildCareSynchronizer childCareSynchronizer, IGeocoder geocoder, IMigrationService migrationService)
            : base(0, 500, migrationService)
        {
            _logger = logger;
            _countySynchronizer = countySynchronizer;
            _childCareSynchronizer = childCareSynchronizer;
            _geocoder = geocoder;
            _geocodeSleepOption = new SleepOption("geocode", 0, 1000);
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

            using (var ctx = new OdjfsContext())
            {
                var state = new DaemonEventLoop(ctx);
                if (state.CountyCount == 0)
                {
                    throw new InvalidOperationException("There are not counties loaded in the database. This should never happen.");
                }

                var odjfsSleeper = new Sleeper(OdjfsSleep.Value);
                while (state.NextStep())
                {
                    odjfsSleeper.Sleep();
                    if (state.IsCountyStep)
                    {
                        IgnoreHttpRequestException(() => _countySynchronizer.UpdateNextCounty(ctx).Wait());
                    }
                    else
                    {
                        IgnoreHttpRequestException(() => _childCareSynchronizer.UpdateNextChildCare(ctx).Wait());

                        // geocode everything that needs geocoding...
                        var geocodeSleeper = new Sleeper(GeocodeSleep.Value);
                        while (Geocode && _geocoder.NeedsGeocoding(ctx).Result)
                        {
                            geocodeSleeper.Sleep();
                            _geocoder.GeocodeNextChildCare(ctx, mapQuestKey).Wait();
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
                        _logger.LogInformation("An unexpected HTTP status code was returned by ODJFS.");
                        _logger.LogInformation("OdjfsScraper.Tool will now sleep for 10 minutes.");
                        Thread.Sleep(10*60*1000);
                    }
                    else
                    {
                        ExceptionDispatchInfo.Capture(ae).Throw();
                    }
                }
            }
        }
    }
}
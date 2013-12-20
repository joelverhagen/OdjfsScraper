using OdjfsScraper.Database;
using OdjfsScraper.DataChecker.Options;
using OdjfsScraper.DataChecker.Support;

namespace OdjfsScraper.DataChecker.Commands
{
    public class DaemonCommand : OdfjsSleepCommand
    {
        private readonly SleepOption _geocodeSleepOption;

        public DaemonCommand() : base(2000)
        {
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
                Odjfs odjfs = this.GetOdjfs();
                var state = new DaemonEventLoop(ctx);
                var odjfsSleeper = new Sleeper(OdjfsSleep.Value);
                while (state.NextStep())
                {
                    odjfsSleeper.Sleep();
                    if (state.IsCountyStep)
                    {
                        odjfs.UpdateNextCounty(ctx).Wait();
                    }
                    else
                    {
                        odjfs.UpdateNextChildCare(ctx).Wait();

                        // geocode everything that needs geocoding...
                        var geocodeSleeper = new Sleeper(GeocodeSleep.Value);
                        while (Geocode && odjfs.NeedsGeocoding(ctx).Result)
                        {
                            geocodeSleeper.Sleep();
                            odjfs.GeocodeNextChildCare(ctx, mapQuestKey).Wait();
                        }
                    }
                }
            }

            return 0;
        }
    }
}
using System.Linq;
using ManyConsole;
using OdjfsScraper.Database;
using OdjfsScraper.Geocode;
using OdjfsScraper.Tool.Options;
using OdjfsScraper.Tool.Support;

namespace OdjfsScraper.Tool.Commands
{
    public class GeocodeCommand : DatabaseCommand
    {
        private readonly SleepOption _geocodeSleepOption;
        private readonly IGeocoder _geocoder;
        private readonly NextOption _nextOption;

        public GeocodeCommand(IGeocoder geocoder, IMigrationService migrationService)
            : base(migrationService)
        {
            _geocoder = geocoder;
            _geocodeSleepOption = new SleepOption("geocode", 0, 1000);
            _nextOption = new NextOption("geocode", "child cares");

            IsCommand("geocode", "geocode child care locations using the MapQuest API");
            HasOption(_geocodeSleepOption);
            HasOption(_nextOption);
            HasOption("url-id=", "geocode the child care with the specified URL ID (e.g. CDCSFJQMQINKNININI)", v => ExternalUrlId = v);
            HasOption("all", "geocode all child cares that have not been geocoded yet", v => All = true);
        }

        public string ExternalUrlId { get; set; }
        public bool All { get; set; }

        public int? GeocodeSleep
        {
            get { return _geocodeSleepOption.Value; }
        }

        public int? Next
        {
            get { return _nextOption.Value; }
        }

        public override int? OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments)
        {
            base.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments);

            int criteriaCount = new[] {ExternalUrlId != null, Next.HasValue, All}.Count(b => b);
            if (criteriaCount > 1)
            {
                throw new ConsoleHelpAsException("The --url-id, --next, and --all options are mutually exclusive.");
            }
            if (criteriaCount == 0)
            {
                throw new ConsoleHelpAsException("You must either use the --url-id, --next, or --all option.");
            }

            this.GetMapQuestKey();

            return null;
        }

        public override int Run(string[] remainingArguments)
        {
            string mapQuestKey = this.GetMapQuestKey();

            // execute the command
            if (ExternalUrlId != null)
            {
                using (var ctx = new OdjfsContext())
                {
                    _geocoder.GeocodeChildCare(ctx, ExternalUrlId, mapQuestKey).Wait();
                }
            }
            else
            {
                using (var ctx = new OdjfsContext())
                {
                    int i = 0;
                    var sleeper = new Sleeper(GeocodeSleep.Value);
                    while (_geocoder.NeedsGeocoding(ctx).Result && (All || i < Next))
                    {
                        sleeper.Sleep();
                        _geocoder.GeocodeNextChildCare(ctx, mapQuestKey).Wait();
                        i++;
                    }
                }
            }

            return 0;
        }
    }
}
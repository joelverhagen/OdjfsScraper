using CommandLine;

namespace OdjfsScraper.DataChecker
{
    public class GeocodeOptions
    {
        [Option('u',
            HelpText = "The URL ID of a specific child care to geocode, e.g. \"CDCSFJQMQINKNININI\".")]
        public string ExternalUrlId { get; set; }

        [Option('n',
            DefaultValue = 1,
            HelpText = "How many child cares to geocode.")]
        public int Count { get; set; }
    }
}
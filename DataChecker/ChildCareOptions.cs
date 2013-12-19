using CommandLine;

namespace OdjfsScraper.DataChecker
{
    public class ChildCareOptions
    {
        [Option('u',
            HelpText = "The URL ID of a specific child care to scrape, e.g. \"CDCSFJQMQINKNININI\".")]
        public string ExternalUrlId { get; set; }
    }
}
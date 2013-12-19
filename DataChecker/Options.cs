using CommandLine;
using CommandLine.Text;

namespace OdjfsScraper.DataChecker
{
    public class Options
    {
        [VerbOption("crawl",
            HelpText = "Automatically select counties and child cares to scrape.")]
        public CrawlOptions CrawlOptions { get; set; }

        [VerbOption("childcare",
            HelpText = "Scrape a specific child care.")]
        public ChildCareOptions ChildCareOptions { get; set; }

        [VerbOption("county",
            HelpText = "Scrape a specific county.")]
        public CountyOptions CountyOptions { get; set; }

        [VerbOption("geocode",
            HelpText = "Geocode a child care.")]
        public GeocodeOptions GeocodeOptions { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            HelpText helpText = HelpText.AutoBuild(this, verb);
            return helpText;
        }
    }
}
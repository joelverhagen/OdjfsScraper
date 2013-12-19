using CommandLine;

namespace OdjfsScraper.DataChecker
{
    public class CrawlOptions
    {
        [Option('s',
            Required = false,
            DefaultValue = 2*1000,
            HelpText = "The time to sleep between each request, in milliseconds.")]
        public int SleepDuration { get; set; }

        [Option('d',
            Required = false,
            DefaultValue = 5*60*1000,
            HelpText = "The maximum duration that the tool will run for. This is not \"best effort\" limit and there are cases where this duration can be exceeded.")]
        public int MaximumDuration { get; set; }
    }
}
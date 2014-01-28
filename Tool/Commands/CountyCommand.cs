using ManyConsole;
using OdjfsScraper.Database;
using OdjfsScraper.Synchronizer.Synchronizers;
using OdjfsScraper.Tool.Support;

namespace OdjfsScraper.Tool.Commands
{
    public class CountyCommand : OdfjsSleepNextCommand
    {
        private readonly ICountySynchronizer _countySynchronizer;

        public CountyCommand(ICountySynchronizer countySynchronizer) : base(0, 500, "scrape", "counties")
        {
            _countySynchronizer = countySynchronizer;
            IsCommand("county", "scrape a county listing page");
            HasOption("name=", "scrape the county with the specified name (e.g. Franklin)", v => Name = v);
        }

        public string Name { get; set; }

        public override int? OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments)
        {
            base.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments);

            if (Name != null && Next.HasValue)
            {
                throw new ConsoleHelpAsException("The --name and --next options are mutually exclusive.");
            }
            if (Name == null && !Next.HasValue)
            {
                throw new ConsoleHelpAsException("You must either use the --name or --next option.");
            }

            return null;
        }

        public override int Run(string[] remainingArguments)
        {
            if (Name != null)
            {
                using (var ctx = new Entities())
                {
                    _countySynchronizer.UpdateCounty(ctx, Name).Wait();
                }
            }
            else
            {
                using (var ctx = new Entities())
                {
                    var sleeper = new Sleeper(OdjfsSleep.Value);
                    for (int i = 0; i < Next; i++)
                    {
                        sleeper.Sleep();
                        _countySynchronizer.UpdateNextCounty(ctx).Wait();
                    }
                }
            }

            return 0;
        }
    }
}
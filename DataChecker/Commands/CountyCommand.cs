using ManyConsole;
using OdjfsScraper.Database;
using OdjfsScraper.DataChecker.Support;

namespace OdjfsScraper.DataChecker.Commands
{
    public class CountyCommand : OdfjsSleepNextCommand
    {
        public CountyCommand() : base(2000, "scrape", "counties")
        {
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
                Odjfs odjfs = this.GetOdjfs();
                using (var ctx = new Entities())
                {
                    odjfs.UpdateCounty(ctx, Name).Wait();
                }
            }
            else
            {
                Odjfs odjfs = this.GetOdjfs();
                using (var ctx = new Entities())
                {
                    var sleeper = new Sleeper(OdjfsSleep.Value);
                    for (int i = 0; i < Next; i++)
                    {
                        sleeper.Sleep();
                        odjfs.UpdateNextCounty(ctx).Wait();
                    }
                }
            }

            return 0;
        }
    }
}
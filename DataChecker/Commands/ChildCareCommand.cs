using ManyConsole;
using OdjfsScraper.Database;
using OdjfsScraper.DataChecker.Support;

namespace OdjfsScraper.DataChecker.Commands
{
    public class ChildCareCommand : OdfjsSleepNextCommand
    {
        public ChildCareCommand() : base(2000, "scrape", "child cares")
        {
            IsCommand("childcare", "scrape a child care page");
            HasOption("url-id=", "scrape the child care with the specified URL ID (e.g. CDCSFJQMQINKNININI)", v => ExternalUrlId = v);
        }

        public string ExternalUrlId { get; set; }

        public override int? OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments)
        {
            base.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments);

            if (ExternalUrlId != null && Next.HasValue)
            {
                throw new ConsoleHelpAsException("The --url-id and --next options are mutually exclusive.");
            }
            if (ExternalUrlId == null && !Next.HasValue)
            {
                throw new ConsoleHelpAsException("You must either use the --url-id or --next option.");
            }

            return null;
        }

        public override int Run(string[] remainingArguments)
        {
            if (ExternalUrlId != null)
            {
                Odjfs odjfs = this.GetOdjfs();
                using (var ctx = new Entities())
                {
                    odjfs.UpdateChildCare(ctx, ExternalUrlId).Wait();
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
                        odjfs.UpdateNextChildCare(ctx).Wait();
                    }
                }
            }

            return 0;
        }
    }
}
using ManyConsole;
using OdjfsScraper.Database;
using OdjfsScraper.Synchronizer.Synchronizers;
using OdjfsScraper.Tool.Support;

namespace OdjfsScraper.Tool.Commands
{
    public class ChildCareCommand : OdfjsSleepNextCommand
    {
        private readonly IChildCareSynchronizer _childCareSynchronizer;

        public ChildCareCommand(IChildCareSynchronizer childCareSynchronizer) : base(0, 1000, "scrape", "child cares")
        {
            _childCareSynchronizer = childCareSynchronizer;
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
                using (var ctx = new Entities())
                {
                    _childCareSynchronizer.UpdateChildCare(ctx, ExternalUrlId).Wait();
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
                        _childCareSynchronizer.UpdateNextChildCare(ctx).Wait();
                    }
                }
            }

            return 0;
        }
    }
}
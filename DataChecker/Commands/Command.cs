using System.Collections.Generic;
using ManyConsole;
using OdjfsScraper.DataChecker.Options;

namespace OdjfsScraper.DataChecker.Commands
{
    public abstract class Command : ConsoleCommand
    {
        private readonly IList<IOption> _options = new List<IOption>();

        protected void HasOption<T>(IValueOption<T> option)
        {
            _options.Add(option);
            HasOption(option.Prototype, option.Description, (T value) => option.Value = value);
        }

        public override int? OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments)
        {
            base.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments);

            foreach (IOption option in _options)
            {
                option.Validate();
            }

            return null;
        }
    }
}
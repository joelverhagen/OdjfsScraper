namespace OdjfsScraper.DataChecker.Commands
{
    public interface ICommand
    {
        int Run(string[] remainingArguments);
    }
}
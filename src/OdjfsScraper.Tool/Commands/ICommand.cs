namespace OdjfsScraper.Tool.Commands
{
    public interface ICommand
    {
        int Run(string[] remainingArguments);
    }
}
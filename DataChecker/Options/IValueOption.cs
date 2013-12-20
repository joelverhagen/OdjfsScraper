namespace OdjfsScraper.DataChecker.Options
{
    public interface IValueOption<T> : IOption
    {
        T Value { get; set; }
    }
}
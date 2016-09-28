namespace OdjfsScraper.Tool.Options
{
    public interface IValueOption<T> : IOption
    {
        T Value { get; set; }
    }
}
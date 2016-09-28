namespace OdjfsScraper.Parser.Parsers
{
    public interface IChildCareParser<T>
    {
        T Parse(T childCare, byte[] bytes);
    }
}
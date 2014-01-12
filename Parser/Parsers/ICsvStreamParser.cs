using System.Collections.Generic;
using System.IO;

namespace OdjfsScraper.Parser.Parsers
{
    public interface ICsvStreamParser<out TOut>
    {
        IEnumerable<TOut> Parse(Stream csvStream);
    }
}
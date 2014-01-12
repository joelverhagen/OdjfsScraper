using System.Collections.Generic;
using System.IO;

namespace OdjfsScraper.Scraper.Parsers
{
    public interface ICsvStreamParser<out TOut>
    {
        IEnumerable<TOut> Parse(Stream csvStream);
    }
}
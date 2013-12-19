using System.Collections.Generic;
using System.IO;

namespace OdjfsScraper.Scraper.Support
{
    public interface ICsvStreamParser<out TOut>
    {
        IEnumerable<TOut> Parse(Stream csvStream);
    }
}
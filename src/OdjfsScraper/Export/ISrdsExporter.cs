using System.Collections.Generic;
using System.IO;

namespace OdjfsScraper.Export
{
    public interface ISrdsExporter<in T>
    {
        void Export(IEnumerable<T> entities, Stream stream);
    }
}
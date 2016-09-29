using System.Collections.Generic;
using System.IO;

namespace OdjfsScraper.Exporters
{
    public interface ISrdsExporter<in T>
    {
        void Export(IEnumerable<T> entities, Stream stream);
    }
}
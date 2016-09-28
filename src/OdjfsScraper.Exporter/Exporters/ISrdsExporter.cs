using System.Collections.Generic;
using System.IO;

namespace OdjfsScraper.Exporter.Exporters
{
    public interface ISrdsExporter<in T>
    {
        void Export(IEnumerable<T> entities, Stream stream);
    }
}
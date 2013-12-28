using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OdjfsScraper.Exporter.Support
{
    public interface ISrdsSchema<T>
    {
        string GetName(T entity);
        double GetLatitude(T entity);
        double GetLongitude(T entity);
        IEnumerable<SrdsAttribute<T>> GetAttributes();
    }
}

using System.Collections.Generic;

namespace OdjfsScraper.Exporters
{
    public interface ISrdsSchema<T>
    {
        string GetName(T entity);
        double GetLatitude(T entity);
        double GetLongitude(T entity);
        IEnumerable<SrdsAttribute<T>> GetAttributes();
    }
}

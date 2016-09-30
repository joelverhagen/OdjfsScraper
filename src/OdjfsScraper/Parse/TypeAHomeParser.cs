using Microsoft.Extensions.Logging;
using OdjfsScraper.Models;

namespace OdjfsScraper.Parse
{
    public class TypeAHomeParser : BaseDetailedChildCareParser<TypeAHome>
    {
        public TypeAHomeParser(ILogger<TypeAHomeParser> logger) : base(logger)
        {
        }
    }
}
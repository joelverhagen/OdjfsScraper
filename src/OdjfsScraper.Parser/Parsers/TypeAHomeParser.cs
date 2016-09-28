using Microsoft.Extensions.Logging;
using OdjfsScraper.Model.ChildCares;

namespace OdjfsScraper.Parser.Parsers
{
    public class TypeAHomeParser : BaseDetailedChildCareParser<TypeAHome>
    {
        public TypeAHomeParser(ILogger<TypeAHomeParser> logger) : base(logger)
        {
        }
    }
}
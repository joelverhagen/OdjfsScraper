using Microsoft.Extensions.Logging;
using OdjfsScraper.Model.ChildCares;

namespace OdjfsScraper.Parser.Parsers
{
    public class LicensedCenterParser : BaseDetailedChildCareParser<LicensedCenter>
    {
        public LicensedCenterParser(ILogger<LicensedCenterParser> logger) : base(logger)
        {
        }
    }
}
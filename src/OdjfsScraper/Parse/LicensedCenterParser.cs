using Microsoft.Extensions.Logging;
using OdjfsScraper.Models;

namespace OdjfsScraper.Parse
{
    public class LicensedCenterParser : BaseDetailedChildCareParser<LicensedCenter>
    {
        public LicensedCenterParser(ILogger<LicensedCenterParser> logger) : base(logger)
        {
        }
    }
}
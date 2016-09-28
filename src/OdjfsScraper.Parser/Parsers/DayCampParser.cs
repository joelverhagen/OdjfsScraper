using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OdjfsScraper.Model.ChildCares;

namespace OdjfsScraper.Parser.Parsers
{
    public class DayCampParser : BaseChildCareParser<DayCamp>
    {
        public DayCampParser(ILogger<DayCampParser> logger) : base(logger)
        {
        }

        protected override void PopulateFields(DayCamp childCare, IDictionary<string, string> details)
        {
            // populate the base fields
            base.PopulateFields(childCare, details);

            childCare.Address = GetDetailString(details, "Address");
            childCare.RegistrationStatus = GetDetailString(details, "Registration Status");
            childCare.RegistrationBeginDate = GetDetailString(details, "Registration Begin Date");
            childCare.RegistrationEndDate = GetDetailString(details, "Registration End Date");

            // TODO: determine if these fields ever appear.
            // childCare.Owner = GetDetailString(details, "Owner");
        }
    }
}
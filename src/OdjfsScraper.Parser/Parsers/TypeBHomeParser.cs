using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Parser.Support;

namespace OdjfsScraper.Parser.Parsers
{
    public class TypeBHomeParser : BaseChildCareParser<TypeBHome>
    {
        public TypeBHomeParser(ILogger<TypeBHomeParser> logger) : base(logger)
        {
        }

        protected override void PopulateFields(TypeBHome childCare, IDictionary<string, string> details)
        {
            // populate the base fields
            base.PopulateFields(childCare, details);

            // type B homes do not have their address exposed
            if (childCare.Address != "Contact County Agency")
            {
                var exception = new ParserException("A type B home does not have the expected address placeholder.");
                _logger.LogError(
                    exception.Message + " Address: '{address}', ExternalUrlId: '{externalUrlId}'",
                    childCare.Address,
                    childCare.ExternalUrlId);
                throw exception;
            }
            childCare.Address = null;

            childCare.CertificationBeginDate = GetDetailString(details, "License Begin Date");
            childCare.CertificationExpirationDate = GetDetailString(details, "License Expiration Date");
        }
    }
}
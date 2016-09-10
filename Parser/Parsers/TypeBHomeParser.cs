using System.Collections.Generic;
using NLog;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Parser.Support;

namespace OdjfsScraper.Parser.Parsers
{
    public class TypeBHomeParser : BaseChildCareParser<TypeBHome>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected override void PopulateFields(TypeBHome childCare, IDictionary<string, string> details)
        {
            // populate the base fields
            base.PopulateFields(childCare, details);

            // type B homes do not have their address exposed
            if (childCare.Address != "Contact County Agency")
            {
                var exception = new ParserException("A type B home does not have the expected address placeholder.");
                Logger.ErrorException(string.Format("Address: '{0}', ExternalUrlId: '{1}'", childCare.Address, childCare.ExternalUrlId), exception);
                throw exception;
            }
            childCare.Address = null;

            childCare.CertificationBeginDate = GetDetailString(details, "License Begin Date");
            childCare.CertificationExpirationDate = GetDetailString(details, "License Expiration Date");
        }
    }
}
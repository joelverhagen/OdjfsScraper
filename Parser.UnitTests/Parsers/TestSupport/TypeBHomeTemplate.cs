using System;
using System.Collections.Generic;
using OdjfsScraper.Model.ChildCares;

namespace OdjfsScraper.Parser.UnitTests.Parsers.TestSupport
{
    public class TypeBHomeTemplate : ChildCareTemplate<TypeBHome>
    {
        private static readonly IDictionary<string, Func<TypeBHome, string>> DefaultDetails = new Dictionary<string, Func<TypeBHome, string>>
        {
            {"Certification Begin Date", c => c.CertificationBeginDate},
            {"Certification Expiration Date", c => c.CertificationExpirationDate}
        };

        public TypeBHomeTemplate() : base(DefaultDetails)
        {
            Model.CertificationBeginDate = "CertificationBeginDate";
            Model.CertificationExpirationDate = "CertificationExpirationDate";

            // Type B homes do not have their addresses exposed
            ReplaceDetails("Address", m => "Contact County CDJFS");
        }
    }
}
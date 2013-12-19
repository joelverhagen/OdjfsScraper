using System;
using System.Collections.Generic;
using System.Linq;
using CsQuery;
using NLog;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Scraper.Support;

namespace OdjfsScraper.Scraper.Parsers
{
    public class BaseChildCareParser<T> : AbstractChildCareParser<T> where T : ChildCare
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly IDictionary<Type, string> TypeNames = new Dictionary<Type, string>
        {
            {typeof (TypeAHome), "Type A Family Child Care Homes"},
            {typeof (TypeBHome), "Type B Family Child Care Homes"},
            {typeof (LicensedCenter), "Licensed Center"},
            {typeof (DayCamp), "Registered Day Camp"},
        };

        static BaseChildCareParser()
        {
            if (!TypeNames.ContainsKey(typeof (T)))
            {
                var exception = new ArgumentException("The type must be a supported ChildCare subclass.");
                Logger.ErrorException(string.Format("Type: '{0}'", typeof (T).Name), exception);
                throw exception;
            }
        }

        protected override IEnumerable<KeyValuePair<string, string>> GetDetailKeyValuePairs(CQ document)
        {
            // get the table
            IDomElement table = document["#PageContent table:first"].FirstElement();
            if (table == null)
            {
                var exception = new ParserException("No Program Details table was found.");
                Logger.ErrorException(string.Format("Type: '{0}'", typeof (T).Name), exception);
                throw exception;
            }

            // replace all of the images with text
            ReplaceImagesWithText(table, new Dictionary<string, string>
            {
                {"smallredstar2.gif", "*"},
                {"http://jfs.ohio.gov/_assets/images/web_graphics/common/spacer.gif", string.Empty},
            });

            // get all of the text fields in the first details table
            return table
                .GetDescendentElements() //                                                    1. get all descendent elements
                .Where(e => e.NodeName == "TR") //                                             2. exclude non-row elements
                .Where(r => r.GetDescendentElements().All(child => child.NodeName != "TR")) // 3. exclude elements that do not themselves have child TR elements
                .Select(r => r.GetCollapsedInnerText()) //                                     4. extract all of the text from the row
                .Select(ParseColonSeperatedString); //                                         5. Parse the colon seperated strings
        }

        protected override void PopulateFields(T childCare, IDictionary<string, string> details)
        {
            // verify the Type detail
            string actualTypeName = GetDetailString(details, "Type");
            string expectedTypeName = TypeNames[typeof (T)];
            if (expectedTypeName != actualTypeName)
            {
                var exception = new ParserException("The type name is not the expected value.");
                Logger.ErrorException(string.Format("Type: '{0}', ExpectedTypeName: '{1}', ActualTypeName: '{2}'",
                    typeof (T).Name,
                    expectedTypeName,
                    actualTypeName), exception);
                throw exception;
            }

            // fill in fields shared by all subclasses
            childCare.ExternalId = GetDetailString(details, "Number");
            childCare.Name = GetDetailString(details, "Name");
            childCare.Address = GetDetailString(details, "Address");
            childCare.City = GetDetailString(details, "City");
            childCare.State = GetDetailString(details, "State");

            string zipCodeString = GetDetailString(details, "Zip");
            int zipCode;
            if (!int.TryParse(zipCodeString, out zipCode))
            {
                var exception = new ParserException("The zip code was not parsable as an integer.");
                Logger.ErrorException(string.Format("Type: '{0}', Zip: '{1}'", typeof (T).Name, zipCodeString), exception);
                throw exception;
            }
            childCare.ZipCode = zipCode;

            childCare.PhoneNumber = GetDetailString(details, "Phone");
            childCare.County = new County {Name = GetDetailString(details, "County")};
        }

        #region Helpers

        private static KeyValuePair<string, string> ParseColonSeperatedString(string input)
        {
            // make sure there is a colon and a key
            input = input.Trim();
            if (!input.Contains(":") || input.StartsWith(":"))
            {
                return default(KeyValuePair<string, string>);
            }

            // split by the colon
            string[] tokens = input.Split(':');
            string key = tokens[0].Trim();
            string value = tokens[1].Trim();

            // coalesce empty strings to null
            value = value == string.Empty ? null : value;

            return new KeyValuePair<string, string>(key, value);
        }

        #endregion
    }
}
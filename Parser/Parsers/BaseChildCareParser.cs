using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsQuery;
using NLog;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Parser.Support;

namespace OdjfsScraper.Parser.Parsers
{
    public class BaseChildCareParser<T> : AbstractChildCareParser<T> where T : ChildCare
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly IDictionary<Type, string> TypeNames = new Dictionary<Type, string>
        {
            {typeof (TypeAHome), "Licensed Type A Family Child Care Home"},
            {typeof (TypeBHome), "Licensed Type B Family Child Care Home"},
            {typeof (LicensedCenter), "Licensed Child Care Center"},
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
            // get the first two divs, which contain the basic information
            var divs = document["#PageContent div"].Elements.Take(2).ToArray();
            if (divs.Length != 2)
            {
                var exception = new ParserException("The basic detail divs were not found.");
                Logger.ErrorException(string.Format("Type: '{0}'", typeof (T).Name), exception);
                throw exception;
            }

            // get all of the text fields in divs
            var pairs = divs
                .SelectMany(GetDetailKeyValuePairs)
                .ToArray();

            return pairs;
        }

        private IEnumerable<KeyValuePair<string, string>> GetDetailKeyValuePairs(IDomElement element)
        {
            // replace all of the images with text
            ReplaceImagesWithText(element, new Dictionary<string, string>
            {
                {"Images/smallredstar2.gif", "*"}
            });

            using (var stringReader = new StringReader(element.InnerText))
            {
                string line;
                while ((line = stringReader.ReadLine()) != null)
                {
                    yield return ParseColonSeperatedString(line);
                }
            }
        }

        protected override void PopulateFields(T childCare, IDictionary<string, string> details)
        {
            // verify the Type detail
            string actualTypeName = GetDetailString(details, "Program Type");
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
            string key = tokens[0].DecodeAndCollapse();
            string value = tokens[1].DecodeAndCollapse().ToNullIfEmpty();

            return new KeyValuePair<string, string>(key, value);
        }

        #endregion
    }
}
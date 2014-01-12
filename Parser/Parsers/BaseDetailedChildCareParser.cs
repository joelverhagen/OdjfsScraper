using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CsQuery;
using NLog;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Parser.Support;

namespace OdjfsScraper.Parser.Parsers
{
    public class BaseDetailedChildCareParser<T> : BaseChildCareParser<T> where T : DetailedChildCare
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected override IEnumerable<KeyValuePair<string, string>> GetDetailKeyValuePairs(CQ document)
        {
            // get the key value pairs from the first table
            IEnumerable<KeyValuePair<string, string>> keyValuePairs = base.GetDetailKeyValuePairs(document);

            // get the table
            IDomElement table = document["#providerinfo"].FirstElement();
            if (table == null)
            {
                return Enumerable.Empty<KeyValuePair<string, string>>();
            }

            // replace all of the images with text
            ReplaceImagesWithText(table, new Dictionary<string, string>
            {
                {"Images/EmptyBox.jpg", "false"},
                {"Images/GreenMark.jpg", "true"},
            });

            // get the useful rows in the table                     
            return keyValuePairs.Concat(table
                .GetDescendentElements() //                                                   1. get all descendent elements
                .Where(e => e.NodeName == "TR") //                                            2. exclude non-row elements
                .Select(row => row.ChildElements.Where(e => e.NodeName == "TD").ToArray()) // 3. get all child cells
                .Where(cells => cells.Length == 6) //                                         4. exclude rows without 6 cells
                .SelectMany(GetTokensFromCells)); //                                          5. re-arrange the text in the cells
        }

        protected override void PopulateFields(T childCare, IDictionary<string, string> details)
        {
            // populate the base fields
            base.PopulateFields(childCare, details);

            // if the address changes, re-geocode
            string oldAddress = childCare.Address;
            string newAddress = GetDetailString(details, "Address");
            childCare.Address = newAddress;
            if (oldAddress != newAddress)
            {
                childCare.Latitude = null;
                childCare.Longitude = null;
                childCare.LastGeocodedOn = null;
            }

            // program details table
            childCare.CenterStatus = GetDetailString(details, "Center Status");
            childCare.Administrators = GetDetailString(details, "Administrator", "Administrators");
            childCare.ProviderAgreement = GetDetailString(details, "Provider Agreement");
            childCare.InitialApplicationDate = GetDetailString(details, "Initial Application Date");
            childCare.LicenseBeginDate = GetDetailString(details, "License Begin Date");
            childCare.LicenseExpirationDate = GetDetailString(details, "License Expiration Date");
            childCare.PhoneNumber = GetDetailString(details, "Phone");

            string sutqRating = GetDetailString(details, "SUTQ Rating");
            if (sutqRating != null && Regex.Match(sutqRating, @"[^\*]").Success)
            {
                var exception = new ParserException("The SUTQ Rating field was not formatted as expected.");
                Logger.ErrorException(string.Format("Value: '{0}'", sutqRating), exception);
                throw exception;
            }
            childCare.SutqRating = sutqRating != null ? sutqRating.Length : (int?) null;

            // children served column
            childCare.Infants = GetDetailBool(details, "Infants");
            childCare.YoungToddlers = GetDetailBool(details, "Younger Toddler");
            childCare.OlderToddlers = GetDetailBool(details, "Older Toddler");
            childCare.Preschoolers = GetDetailBool(details, "Preschool");
            childCare.Gradeschoolers = GetDetailBool(details, "School Age");
            childCare.ChildCareFoodProgram = GetDetailBool(details, "Child Care");

            // accreditations column
            childCare.Naeyc = GetDetailBool(details, "NAEYC");
            childCare.Necpa = GetDetailBool(details, "NECPA");
            childCare.Naccp = GetDetailBool(details, "NACCP");
            childCare.Nafcc = GetDetailBool(details, "NAFCC");
            childCare.Coa = GetDetailBool(details, "COA");
            childCare.Acsi = GetDetailBool(details, "ACSI");

            // Monday
            Tuple<bool, DateTime?, DateTime?> monday = GetHoursOfOperation(details, "Monday:");
            childCare.MondayReported = monday.Item1;
            childCare.MondayBegin = monday.Item2;
            childCare.MondayEnd = monday.Item3;

            // Tuesday
            Tuple<bool, DateTime?, DateTime?> tuesday = GetHoursOfOperation(details, "Tuesday:");
            childCare.TuesdayReported = tuesday.Item1;
            childCare.TuesdayBegin = tuesday.Item2;
            childCare.TuesdayEnd = tuesday.Item3;

            // Wednesday
            Tuple<bool, DateTime?, DateTime?> wednesday = GetHoursOfOperation(details, "Wednesday:");
            childCare.WednesdayReported = wednesday.Item1;
            childCare.WednesdayBegin = wednesday.Item2;
            childCare.WednesdayEnd = wednesday.Item3;

            // Thursday
            Tuple<bool, DateTime?, DateTime?> thursday = GetHoursOfOperation(details, "Thursday:");
            childCare.ThursdayReported = thursday.Item1;
            childCare.ThursdayBegin = thursday.Item2;
            childCare.ThursdayEnd = thursday.Item3;

            // Friday
            Tuple<bool, DateTime?, DateTime?> friday = GetHoursOfOperation(details, "Friday:");
            childCare.FridayReported = friday.Item1;
            childCare.FridayBegin = friday.Item2;
            childCare.FridayEnd = friday.Item3;

            // Saturday
            Tuple<bool, DateTime?, DateTime?> saturday = GetHoursOfOperation(details, "Saturday:");
            childCare.SaturdayReported = saturday.Item1;
            childCare.SaturdayBegin = saturday.Item2;
            childCare.SaturdayEnd = saturday.Item3;

            // Sunday
            Tuple<bool, DateTime?, DateTime?> sunday = GetHoursOfOperation(details, "Sunday:");
            childCare.SundayReported = sunday.Item1;
            childCare.SundayBegin = sunday.Item2;
            childCare.SundayEnd = sunday.Item3;
        }

        #region Helpers

        private static IEnumerable<KeyValuePair<string, string>> GetTokensFromCells(IDomElement[] cells)
        {
            // get the text from each cell
            string[] tokens = cells
                .Select(c => c.GetCollapsedInnerText())
                .Select(s => s.ToNullIfEmpty())
                .ToArray();

            // trim the colon off the hours of operation day label
            tokens[4] = tokens[4].Trim();

            // arrange the tokens as key value pairs
            return new[]
            {
                new KeyValuePair<string, string>(tokens[1], tokens[0]),
                new KeyValuePair<string, string>(tokens[3], tokens[2]),
                new KeyValuePair<string, string>(tokens[4], tokens[5])
            };
        }

        private static Tuple<bool, DateTime?, DateTime?> GetHoursOfOperation(IDictionary<string, string> details, string key)
        {
            string value = GetDetailString(details, key);
            if (value == "NOT REPORTED")
            {
                return new Tuple<bool, DateTime?, DateTime?>(false, null, null);
            }
            if (value == "CLOSED")
            {
                return new Tuple<bool, DateTime?, DateTime?>(true, null, null);
            }

            // parse the string
            Match match = Regex.Match(value, @"^(?<Begin>\d{2}:\d{2} (?:AM|PM)) to (?<End>\d{2}:\d{2} (?:AM|PM))$");
            if (!match.Success)
            {
                var exception = new ParserException("An hours of operation string was not in an expected format.");
                Logger.ErrorException(string.Format("Key: '{0}', Value: '{1}'", key, value), exception);
                throw exception;
            }

            string beginTimeString = match.Groups["Begin"].Value;
            string endTimeString = match.Groups["End"].Value;

            // sometimes "12:00 AM" is listed as the end time... we assume this means midnight of the next day
            string endDayString = "01";
            if (endTimeString == "12:00 AM")
            {
                endDayString = "02";
            }

            DateTime beginTime = DateTime.ParseExact(string.Format("1970-01-01 {0}", beginTimeString), "yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture);
            DateTime endTime = DateTime.ParseExact(string.Format("1970-01-{0} {1}", endDayString, endTimeString), "yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture);

            if (beginTime >= endTime)
            {
                var exception = new ParserException("An hours of operation string has a begin time equal to or after an end time.");
                Logger.ErrorException(string.Format("Key: '{0}', Value: '{1}', BeginTime: '{2}', EndTime: '{3}'", key, value, beginTime, endTime), exception);
                throw exception;
            }

            return new Tuple<bool, DateTime?, DateTime?>(true, beginTime, endTime);
        }

        private static bool GetDetailBool(IDictionary<string, string> details, params string[] keys)
        {
            string value = GetDetailString(details, keys);
            bool result;
            if (bool.TryParse(value, out result))
            {
                return result;
            }

            var exception = new ParserException("A boolean detail could not be parsed.");
            Logger.ErrorException(string.Format("Keys: '{0}', Value: '{1}'", string.Join(", ", keys), value), exception);
            throw exception;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OdjfsScraper.Model.ChildCares;

namespace OdjfsScraper.Scraper.UnitTests.Parsers.TestSupport
{
    public abstract class DetailedChildCareHomeTemplate<T> : ChildCareTemplate<T> where T : DetailedChildCare
    {
        private static readonly ICollection<KeyValuePair<string, Func<T, string>>> DefaultDetails = new Dictionary<string, Func<T, string>>
        {
            {"Center Status", c => c.CenterStatus},
            {"Administrators", c => c.Administrators},
            {"Provider Agreement", c => c.ProviderAgreement},
            {"Initial Application Date", c => c.InitialApplicationDate},
            {"License Begin Date", c => c.LicenseBeginDate},
            {"License Expiration Date", c => c.LicenseExpirationDate},
            {"SUTQ Rating", c => SutqRatingToString(c.SutqRating)},
            {"Infants", c => CheckBooleanToString(c.Infants)},
            {"Younger Toddler", c => CheckBooleanToString(c.YoungToddlers)},
            {"Older Toddler", c => CheckBooleanToString(c.OlderToddlers)},
            {"Preschool", c => CheckBooleanToString(c.Preschoolers)},
            {"School Age", c => CheckBooleanToString(c.Gradeschoolers)},
            {"Child Care", c => CheckBooleanToString(c.ChildCareFoodProgram)},
            {"NAEYC", c => CheckBooleanToString(c.Naeyc)},
            {"NECPA", c => CheckBooleanToString(c.Necpa)},
            {"NACCP", c => CheckBooleanToString(c.Naccp)},
            {"NAFCC", c => CheckBooleanToString(c.Nafcc)},
            {"COA", c => CheckBooleanToString(c.Coa)},
            {"ACSI", c => CheckBooleanToString(c.Acsi)},
            {"Monday:", c => DayTimesToString(c.MondayReported, c.MondayBegin, c.MondayEnd)},
            {"Tuesday:", c => DayTimesToString(c.TuesdayReported, c.TuesdayBegin, c.TuesdayEnd)},
            {"Wednesday:", c => DayTimesToString(c.WednesdayReported, c.WednesdayBegin, c.WednesdayEnd)},
            {"Thursday:", c => DayTimesToString(c.ThursdayReported, c.ThursdayBegin, c.ThursdayEnd)},
            {"Friday:", c => DayTimesToString(c.FridayReported, c.FridayBegin, c.FridayEnd)},
            {"Saturday:", c => DayTimesToString(c.SaturdayReported, c.SaturdayBegin, c.SaturdayEnd)},
            {"Sunday:", c => DayTimesToString(c.SundayReported, c.SundayBegin, c.SundayEnd)}
        };

        private static readonly ISet<string> DefaultProviderInfoDetails = new HashSet<string>
        {
            "Infants", "Younger Toddler", "Older Toddler", "Preschool", "School Age", "Child Care",
            "NAEYC", "NECPA", "NACCP", "NAFCC", "COA", "ACSI",
            "Monday:", "Tuesday:", "Wednesday:", "Thursday:", "Friday:", "Saturday:", "Sunday:"
        };

        private static readonly ISet<string> DefaultKeyFirstProviderInfoDetails = new HashSet<string>
        {
            "Monday:", "Tuesday:", "Wednesday:", "Thursday:", "Friday:", "Saturday:", "Sunday:"
        };

        protected DetailedChildCareHomeTemplate(IEnumerable<KeyValuePair<string, Func<T, string>>> parentDetails) : base(DefaultDetails.Concat(parentDetails))
        {
            // initialize templat properties
            ProviderInfoDetails = DefaultProviderInfoDetails;
            KeyFirstProviderInfoDetails = DefaultKeyFirstProviderInfoDetails;

            // initialze the model
            Model.Address = "Address";

            Model.ProviderAgreement = "ProviderAgreement";
            Model.Administrators = "Adminitrators";
            Model.CenterStatus = "CenterStatus";
            Model.InitialApplicationDate = "InitialApplicationDate";
            Model.LicenseBeginDate = "LicenseBeginDate";
            Model.LicenseExpirationDate = "LicenseExpirationDate";

            Model.SutqRating = 3;

            Model.Infants = true;
            Model.YoungToddlers = false;
            Model.OlderToddlers = true;
            Model.Preschoolers = false;
            Model.Gradeschoolers = true;

            Model.ChildCareFoodProgram = false;

            Model.Naeyc = true;
            Model.Necpa = false;
            Model.Naccp = true;
            Model.Nafcc = false;
            Model.Coa = true;
            Model.Acsi = false;

            Model.MondayReported = true;
            Model.MondayBegin = new DateTime(1970, 1, 1, 1, 0, 0);
            Model.MondayEnd = new DateTime(1970, 1, 1, 15, 0, 0);

            Model.TuesdayReported = false;
            Model.TuesdayBegin = null;
            Model.TuesdayEnd = null;

            Model.WednesdayReported = true;
            Model.WednesdayBegin = null;
            Model.WednesdayEnd = null;

            Model.ThursdayReported = true;
            Model.ThursdayBegin = new DateTime(1970, 1, 1, 0, 0, 0);
            Model.ThursdayEnd = new DateTime(1970, 1, 1, 18, 0, 0);

            Model.FridayReported = true;
            Model.FridayBegin = new DateTime(1970, 1, 1, 5, 0, 0);
            Model.FridayEnd = new DateTime(1970, 1, 2, 0, 0, 0);

            Model.SaturdayReported = true;
            Model.SaturdayBegin = new DateTime(1970, 1, 1, 6, 0, 0);
            Model.SaturdayEnd = new DateTime(1970, 1, 1, 20, 0, 0);

            Model.SundayReported = true;
            Model.SundayBegin = new DateTime(1970, 1, 1, 7, 0, 0);
            Model.SundayEnd = new DateTime(1970, 1, 1, 21, 0, 0);
        }

        public ISet<string> KeyFirstProviderInfoDetails { get; private set; }

        public ISet<string> ProviderInfoDetails { get; private set; }

        public static string CheckBooleanToString(bool value)
        {
            return value ? "<img src=\"Images/GreenMark.jpg\">" : "<img src=\"Images/EmptyBox.jpg\">";
        }

        public static string DayTimesToString(bool reported, DateTime? beginTime, DateTime? endTime)
        {
            if (!reported)
            {
                return "NOT REPORTED";
            }
            if (!beginTime.HasValue || !endTime.HasValue)
            {
                return "CLOSED";
            }
            return string.Format("{0} to {1}", beginTime.Value.ToString("hh:mm tt"), endTime.Value.ToString("hh:mm tt"));
        }

        public static string SutqRatingToString(int? sutqRating)
        {
            if (!sutqRating.HasValue)
            {
                return string.Empty;
            }
            return string.Concat(Enumerable.Repeat("<img src=\"smallredstar2.gif\">", sutqRating.Value));
        }

        public override byte[] GetDocument()
        {
            // fetch and group the details
            IDictionary<bool, IEnumerable<KeyValuePair<string, string>>> groupedDetails = Details
                .Select(p => new KeyValuePair<string, string>(p.Key, p.Value(Model)))
                .GroupBy(p => ProviderInfoDetails.Contains(p.Key))
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var sb = new StringBuilder();

            BuildPageContent(sb, groupedDetails[false]);
            BuildProviderInfo(sb, KeyFirstProviderInfoDetails, groupedDetails[true]);

            return GetBytes(sb.ToString());
        }

        public static void BuildProviderInfo(StringBuilder sb, ISet<string> keyFirstProviderInfoDetails, IEnumerable<KeyValuePair<string, string>> details)
        {
            var valueFirstDetails = new Queue<string>();
            var keyFirstDetails = new Queue<string>();

            foreach (var detail in details)
            {
                string first;
                string second;
                Queue<string> queue;
                if (keyFirstProviderInfoDetails.Contains(detail.Key))
                {
                    first = detail.Key;
                    second = detail.Value;
                    queue = keyFirstDetails;
                }
                else
                {
                    first = detail.Value;
                    second = detail.Key;
                    queue = valueFirstDetails;
                }
                queue.Enqueue(string.Format("<td>{0}</td><td>{1}</td>", first, second));
            }

            sb.AppendLine("<table id='providerinfo'>");
            while (valueFirstDetails.Count > 0 || keyFirstDetails.Count > 0)
            {
                sb.AppendLine("  <tr>");
                for (int i = 0; i < 2; i++)
                {
                    sb.Append("    ");
                    sb.AppendLine(valueFirstDetails.Count > 0 ? valueFirstDetails.Dequeue() : "<td></td><td></td>");
                }
                sb.Append("    ");
                sb.AppendLine(keyFirstDetails.Count > 0 ? keyFirstDetails.Dequeue() : "<td></td><td></td>");
                sb.AppendLine("  </tr>");
            }

            sb.AppendLine("</table>");
        }
    }
}
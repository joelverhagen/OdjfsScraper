using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Scraper.Parsers;
using OdjfsScraper.Scraper.UnitTests.Parsers.TestSupport;

namespace OdjfsScraper.Scraper.UnitTests.Parsers
{
    public class BaseDetailedChildCareParserTest<TModel, TTemplate, TParser> : BaseChildCareParserTest<TModel, TTemplate, TParser>
        where TModel : DetailedChildCare
        where TTemplate : ChildCareTemplate<TModel>
        where TParser : BaseChildCareParser<TModel>
    {
        [TestMethod]
        public virtual void SingularAdministrator()
        {
            TestSuccessfulParseOfTemplate(t =>
            {
                t.Model.Administrators = "John Galt";
                t.RemoveDetails("Administrators");
                t.ReplaceDetails("Administrator", m => m.Administrators);
            });
        }

        [TestMethod]
        public virtual void MissingProviderInfo()
        {
            TestUnsuccessfulParseOfDocument(
                "A parser exception should have been thrown because the #providerinfo <table> element was missing.",
                "<p>Nope, nothing at all.</p>");
        }

        [TestMethod]
        public virtual void StartTimeAfterEndTime()
        {
            TestUnsuccessfulParseOfTemplateHours(
                "A parser exception should have been thrown because {0}'s start time is after the end time.",
                (t, d) => t.ReplaceDetails(d + ":", m => "03:00 PM to 01:00 PM"));
        }

        [TestMethod]
        public virtual void InvalidHours()
        {
            TestUnsuccessfulParseOfTemplateHours(
                "A parser exception should have been thrown because {0}'s hours are not formatted properly.",
                (t, d) => t.ReplaceDetails(d + ":", m => "FOO"));
        }

        [TestMethod]
        public virtual void InvalidSutqRating()
        {
            TestUnsuccessfulParseOfTemplate(
                string.Format("A parser exception should have been thrown because the SUTQ rating was not formatted properly."),
                t => t.ReplaceDetails("SUTQ Rating", m => "+++"));
        }

        [TestMethod]
        public virtual void InvalidBoolean()
        {
            var booleans = new[]
            {
                "Infants", "Younger Toddler", "Older Toddler", "Preschool", "School Age",
                "Child Care", "NAEYC", "NECPA", "NACCP", "NAFCC", "COA", "ACSI"
            };

            foreach (string boolean in booleans)
            {
                string key = boolean;
                TestUnsuccessfulParseOfTemplate(
                    string.Format("A parser exception should have been thrown because {0} was not a valid boolean string.", key),
                    t => t.ReplaceDetails(key, m => "FOO"));
            }
        }

        protected void TestUnsuccessfulParseOfTemplateHours(string messageTemplate, Action<TTemplate, string> mutateTemplateWithDay)
        {
            var days = new[]
            {
                "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
            };

            foreach (string day in days)
            {
                string localDay = day;
                TestUnsuccessfulParseOfTemplate(
                    string.Format(messageTemplate, day),
                    t => mutateTemplateWithDay(t, localDay));
            }
        }

        protected override void VerifyAreEqual(TModel expected, TModel actual)
        {
            base.VerifyAreEqual(expected, actual);

            Assert.AreEqual(expected.CenterStatus, actual.CenterStatus);
            Assert.AreEqual(expected.Administrators, actual.Administrators);
            Assert.AreEqual(expected.ProviderAgreement, actual.ProviderAgreement);
            Assert.AreEqual(expected.InitialApplicationDate, actual.InitialApplicationDate);
            Assert.AreEqual(expected.LicenseBeginDate, actual.LicenseBeginDate);
            Assert.AreEqual(expected.LicenseExpirationDate, actual.LicenseExpirationDate);
            Assert.AreEqual(expected.SutqRating, actual.SutqRating);

            Assert.AreEqual(expected.Infants, actual.Infants);
            Assert.AreEqual(expected.YoungToddlers, actual.YoungToddlers);
            Assert.AreEqual(expected.OlderToddlers, actual.OlderToddlers);
            Assert.AreEqual(expected.Preschoolers, actual.Preschoolers);
            Assert.AreEqual(expected.Gradeschoolers, actual.Gradeschoolers);
            Assert.AreEqual(expected.ChildCareFoodProgram, actual.ChildCareFoodProgram);

            Assert.AreEqual(expected.Naeyc, actual.Naeyc);
            Assert.AreEqual(expected.Necpa, actual.Necpa);
            Assert.AreEqual(expected.Naccp, actual.Naccp);
            Assert.AreEqual(expected.Nafcc, actual.Nafcc);
            Assert.AreEqual(expected.Coa, actual.Coa);
            Assert.AreEqual(expected.Acsi, actual.Acsi);

            Assert.AreEqual(expected.MondayReported, actual.MondayReported);
            Assert.AreEqual(expected.MondayBegin, actual.MondayBegin);
            Assert.AreEqual(expected.MondayEnd, actual.MondayEnd);

            Assert.AreEqual(expected.TuesdayReported, actual.TuesdayReported);
            Assert.AreEqual(expected.TuesdayBegin, actual.TuesdayBegin);
            Assert.AreEqual(expected.TuesdayEnd, actual.TuesdayEnd);

            Assert.AreEqual(expected.WednesdayReported, actual.WednesdayReported);
            Assert.AreEqual(expected.WednesdayBegin, actual.WednesdayBegin);
            Assert.AreEqual(expected.WednesdayEnd, actual.WednesdayEnd);

            Assert.AreEqual(expected.ThursdayReported, actual.ThursdayReported);
            Assert.AreEqual(expected.ThursdayBegin, actual.ThursdayBegin);
            Assert.AreEqual(expected.ThursdayEnd, actual.ThursdayEnd);

            Assert.AreEqual(expected.FridayReported, actual.FridayReported);
            Assert.AreEqual(expected.FridayBegin, actual.FridayBegin);
            Assert.AreEqual(expected.FridayEnd, actual.FridayEnd);

            Assert.AreEqual(expected.SaturdayReported, actual.SaturdayReported);
            Assert.AreEqual(expected.SaturdayBegin, actual.SaturdayBegin);
            Assert.AreEqual(expected.SaturdayEnd, actual.SaturdayEnd);

            Assert.AreEqual(expected.SundayReported, actual.SundayReported);
            Assert.AreEqual(expected.SundayBegin, actual.SundayBegin);
            Assert.AreEqual(expected.SundayEnd, actual.SundayEnd);
        }
    }
}
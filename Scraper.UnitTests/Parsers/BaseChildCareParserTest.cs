using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Scraper.Parsers;
using OdjfsScraper.Scraper.Support;
using OdjfsScraper.Scraper.UnitTests.Parsers.TestSupport;

namespace OdjfsScraper.Scraper.UnitTests.Parsers
{
    public abstract class BaseChildCareParserTest<TModel, TTemplate, TParser>
        where TModel : ChildCare
        where TTemplate : ChildCareTemplate<TModel>
        where TParser : BaseChildCareParser<TModel>
    {
        [TestMethod]
        public virtual void HappyPath()
        {
            TestSuccessfulParseOfTemplate(t => { });
        }

        [TestMethod]
        public virtual void MissingDetail()
        {
            // ARRANGE
            var template = Activator.CreateInstance<TTemplate>();
            var parser = Activator.CreateInstance<TParser>();

            foreach (var detail in template.Details.ToArray())
            {
                template.Details.Remove(detail);
                try
                {
                    // ACT
                    parser.Parse(Activator.CreateInstance<TModel>(), template.GetDocument());

                    // ASSERT
                    Assert.Fail("When detail {0} is missing, an exception should be thrown.", detail.Key);
                }
                catch (ParserException)
                {
                    template.Details.Add(detail);
                }
            }
        }

        [TestMethod]
        public virtual void SpacerImage()
        {
            TestSuccessfulParseOfTemplate(t => t.AddDetail("UNUSED", m => "<img src='http://jfs.ohio.gov/_assets/images/web_graphics/common/spacer.gif'>"));
        }

        [TestMethod]
        public virtual void ValueWithNoKey()
        {
            TestSuccessfulParseOfTemplate(t => t.AddDetail(string.Empty, m => "UNUSED"));
        }

        [TestMethod]
        public virtual void UnusedKeyWithNoValue()
        {
            TestSuccessfulParseOfTemplate(t => t.AddDetail("UNUSED", m => string.Empty));
        }

        [TestMethod]
        public virtual void UnusedDuplicateKeys()
        {
            TestUnsuccessfulParseOfTemplate("An exception should have been thrown because there were duplicate detail keys.",
                t =>
                {
                    t.AddDetail("UNUSED", m => string.Empty);
                    t.AddDetail("UNUSED", m => string.Empty);
                });
        }

        [TestMethod]
        public virtual void NonIntegerZip()
        {
            TestUnsuccessfulParseOfTemplate("An exception should have been thrown because the zip code was not an integer.",
                t => t.ReplaceDetails("Zip", m => "FOO"));
        }

        [TestMethod]
        public virtual void MissingPageContent()
        {
            TestUnsuccessfulParseOfDocument(
                "An exception should have been thrown because the #PageContent <div> was missing.",
                "<p>Nothing here, boss.</p>");
        }

        [TestMethod]
        public virtual void MissingTablePageContent()
        {
            TestUnsuccessfulParseOfDocument(
                "An exception should have been thrown because the table in #PageContent was missing.",
                "<div id='PageContent></div>");
        }

        protected void TestSuccessfulParseOfTemplate(Action<TTemplate> mutateTemplate)
        {
            // ARRANGE
            var template = Activator.CreateInstance<TTemplate>();
            mutateTemplate(template);

            var parser = Activator.CreateInstance<TParser>();
            var actualModel = Activator.CreateInstance<TModel>();

            // ACT
            actualModel = parser.Parse(actualModel, template.GetDocument());

            // ASSERT
            VerifyAreEqual(template.Model, actualModel);
        }

        protected void TestUnsuccessfulParseOfTemplate(string message, Action<TTemplate> mutateTemplate)
        {
            // ARRANGE
            var template = Activator.CreateInstance<TTemplate>();
            mutateTemplate(template);

            // ACT, ASSERT
            TestUnsuccessfulParseOfDocument(message, template.GetDocument());
        }

        protected void TestUnsuccessfulParseOfDocument(string message, string document)
        {
            TestUnsuccessfulParseOfDocument(message, BaseTemplate.GetBytes(document));
        }

        protected void TestUnsuccessfulParseOfDocument(string message, byte[] document)
        {
            // ARRANGE
            var parser = Activator.CreateInstance<TParser>();
            var actualModel = Activator.CreateInstance<TModel>();

            // ACT
            try
            {
                parser.Parse(actualModel, document);

                // ASSERT
                Assert.Fail(message);
            }
            catch (ParserException)
            {
            }
        }

        protected virtual void VerifyAreEqual(TModel expected, TModel actual)
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.ExternalId, actual.ExternalId);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Address, actual.Address);
            Assert.AreEqual(expected.City, actual.City);
            Assert.AreEqual(expected.State, actual.State);
            Assert.AreEqual(expected.ZipCode, actual.ZipCode);
            Assert.AreEqual(expected.County.Name, actual.County.Name);
            Assert.AreEqual(expected.PhoneNumber, actual.PhoneNumber);
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OdjfsScraper.Parser.Support;
using OdjfsScraper.Parser.UnitTests.Parsers.TestSupport;

namespace OdjfsScraper.Parser.UnitTests.Parsers
{
    [TestClass]
    public class CsvStreamParserTest : AbstractCsvStreamParserTest<Person>
    {
        [TestMethod]
        public void OneDataRow()
        {
            // ARRANGE
            var parser = new PersonCsvStreamParser();
            var lines = new[]
            {
                "Name,FavoriteColor",
                "Joel,Orange"
            };
            var expected = new[]
            {
                new Person
                {
                    Name = "Joel",
                    FavoriteColor = "Orange"
                }
            };

            // ACT
            Person[] actual = Parse(parser, lines);

            // ASSERT
            AssertAreEqual(expected, actual);
        }

        [TestMethod]
        public void TrimmableWhitespace()
        {
            // ARRANGE
            var parser = new PersonCsvStreamParser();
            var lines = new[]
            {
                "Name,FavoriteColor",
                "   Joel  ,   Orange   "
            };
            var expected = new[]
            {
                new Person
                {
                    Name = "Joel",
                    FavoriteColor = "Orange"
                }
            };

            // ACT
            Person[] actual = Parse(parser, lines);

            // ASSERT
            AssertAreEqual(expected, actual);
        }

        [TestMethod]
        public void TwoDataRows()
        {
            // ARRANGE
            var parser = new PersonCsvStreamParser();
            var lines = new[]
            {
                "Name,FavoriteColor",
                "Joel,Orange",
                "Anna,Blue"
            };
            var expected = new[]
            {
                new Person
                {
                    Name = "Joel",
                    FavoriteColor = "Orange"
                },
                new Person
                {
                    Name = "Anna",
                    FavoriteColor = "Blue"
                }
            };

            // ACT
            Person[] actual = Parse(parser, lines);

            // ASSERT
            AssertAreEqual(expected, actual);
        }

        [TestMethod]
        public void EmptyInput()
        {
            // ARRANGE
            var parser = new PersonCsvStreamParser();
            var lines = new string[0];
            var expected = new Person[0];

            // ACT
            Person[] actual = Parse(parser, lines);

            // ASSERT
            AssertAreEqual(expected, actual);
        }

        [TestMethod]
        public void OnlyHeaderRow()
        {
            // ARRANGE
            var parser = new PersonCsvStreamParser();
            var lines = new[]
            {
                "Name,FavoriteColor"
            };
            var expected = new Person[0];

            // ACT
            Person[] actual = Parse(parser, lines);

            // ASSERT
            AssertAreEqual(expected, actual);
        }

        [TestMethod]
        public void EmptyFieldIsNull()
        {
            // ARRANGE
            var parser = new PersonCsvStreamParser();
            var lines = new[]
            {
                "Name,FavoriteColor",
                "Joel,"
            };
            var expected = new[]
            {
                new Person
                {
                    Name = "Joel",
                    FavoriteColor = null
                }
            };

            // ACT
            Person[] actual = Parse(parser, lines);

            // ASSERT
            AssertAreEqual(expected, actual);
        }

        [TestMethod]
        public void WhitespaceFieldIsNull()
        {
            // ARRANGE
            var parser = new PersonCsvStreamParser();
            var lines = new[]
            {
                "Name,FavoriteColor",
                "Joel,    "
            };
            var expected = new[]
            {
                new Person
                {
                    Name = "Joel",
                    FavoriteColor = null
                }
            };

            // ACT
            Person[] actual = Parse(parser, lines);

            // ASSERT
            AssertAreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof (ParserException))]
        public void ThrowsIfMissingHeaderField()
        {
            // ARRANGE
            var parser = new PersonCsvStreamParser();
            var lines = new[]
            {
                "FavoriteColor",
                "Joel,Orange"
            };

            // ACT
            Parse(parser, lines);
        }

        [TestMethod]
        [ExpectedException(typeof (ParserException))]
        public void ThrowsIfMissingDataField()
        {
            // ARRANGE
            var parser = new PersonCsvStreamParser();
            var lines = new[]
            {
                "Name,FavoriteColor",
                "Joel"
            };

            // ACT
            Parse(parser, lines);
        }

        [TestMethod]
        [ExpectedException(typeof (ParserException))]
        public void ThrowsIfDuplicateHeading()
        {
            // ARRANGE
            var parser = new PersonCsvStreamParser();
            var lines = new[]
            {
                "Name,Name",
                "Joel,Verhagen"
            };

            // ACT
            Parse(parser, lines);
        }

        protected override void AssertAreEqual(Person expected, Person actual)
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.FavoriteColor, actual.FavoriteColor);
        }
    }
}
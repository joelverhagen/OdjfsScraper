using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OdjfsScraper.Scraper.Parsers;
using OdjfsScraper.Scraper.Support;

namespace OdjfsScraper.Scraper.UnitTests.Support.TestSupport
{
    public abstract class AbstractCsvStreamParserTest<T>
    {
        protected Stream GetCsvStream(IEnumerable<string> lines)
        {
            string csv = string.Join(Environment.NewLine, lines);
            return new MemoryStream(Encoding.UTF8.GetBytes(csv));
        }

        protected T[] Parse(ICsvStreamParser<T> parser, IEnumerable<string> lines)
        {
            return parser.Parse(GetCsvStream(lines)).ToArray();
        }

        protected void AssertAreEqual(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            T[] expectedArray = expected.ToArray();
            T[] actualArray = actual.ToArray();
            Assert.IsNotNull(actual);
            Assert.AreEqual(expectedArray.Length, actualArray.Length);
            for (int i = 0; i < expectedArray.Length; i++)
            {
                AssertAreEqual(expectedArray[i], actualArray[i]);
            }
        }

        protected abstract void AssertAreEqual(T expected, T actual);
    }
}
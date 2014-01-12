using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using NLog;
using OdjfsScraper.Scraper.Support;

namespace OdjfsScraper.Scraper.Parsers
{
    public abstract class CsvStreamParser<TOut> : ICsvStreamParser<TOut>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IEnumerable<TOut> Parse(Stream csvStream)
        {
            // use the .NET library that parses CSVs
            var parser = new TextFieldParser(csvStream);
            parser.HasFieldsEnclosedInQuotes = true;
            parser.TextFieldType = FieldType.Delimited;
            parser.CommentTokens = new string[0];
            parser.Delimiters = new[] {","};

            // get the first line, which is column headings
            string[] keys = parser.ReadFields();
            if (keys == null)
            {
                yield break;
            }

            // verify the headings are unique
            ISet<string> uniqueKeys = new HashSet<string>(keys);
            if (uniqueKeys.Count != keys.Length)
            {
                IEnumerable<string> duplicateKeys = keys
                    .GroupBy(k => k)
                    .ToDictionary(g => g.Key, g => g.Count())
                    .Where(p => p.Value > 1)
                    .Select(p => p.Key);

                var exception = new ParserException("The provided CSV does not have unique column headings.");
                Logger.ErrorException(string.Format("DuplicateHeadings: {0}", string.Join(", ", duplicateKeys)), exception);
                throw exception;
            }

            // parse each line
            string[] values;
            while ((values = parser.ReadFields()) != null)
            {
                // trim and turn empty strings to null
                values = values
                    .Select(v => string.IsNullOrWhiteSpace(v) ? null : v.Trim())
                    .ToArray();

                // verify the correct number of columns in this row
                if (values.Length != keys.Length)
                {
                    var exception = new ParserException("A row in the provided CSV does not have the same number of columns as the heading row.");
                    Logger.ErrorException(string.Format("HeadingCount: {0}, ColumnCount: {1}, Values: {2}",
                        keys.Length,
                        values.Length,
                        string.Join(", ", values)), exception);
                    throw exception;
                }

                // associate values with headings
                IDictionary<string, string> valueDictionary = Enumerable
                    .Range(0, keys.Length)
                    .ToDictionary(i => keys[i], i => values[i]);

                yield return ConstructItem(valueDictionary);
            }
        }

        protected int? ParseNullableInt(string value)
        {
            return value != null ? int.Parse(value) : (int?) null;
        }

        protected double? ParseNullableDouble(string value)
        {
            return value != null ? double.Parse(value) : (double?) null;
        }

        protected abstract TOut ConstructItem(IDictionary<string, string> values);
    }
}
using System.Collections.Generic;
using OdjfsScraper.Parser.Parsers;

namespace OdjfsScraper.Parser.UnitTests.Parsers.TestSupport
{
    public class PersonCsvStreamParser : CsvStreamParser<Person>
    {
        protected override Person ConstructItem(IDictionary<string, string> values)
        {
            return new Person
            {
                Name = values["Name"],
                FavoriteColor = values["FavoriteColor"]
            };
        }
    }
}
using System.Collections.Generic;
using OdjfsScraper.Scraper.Support;

namespace OdjfsScraper.Scraper.UnitTests.Support.TestSupport
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
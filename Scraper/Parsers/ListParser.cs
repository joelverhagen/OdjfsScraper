using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsQuery;
using NLog;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCareStubs;
using OdjfsScraper.Scraper.Support;

namespace OdjfsScraper.Scraper.Parsers
{
    public class ListParser : IListParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IEnumerable<ChildCareStub> Parse(byte[] bytes)
        {
            return Parse(bytes, null);
        }

        public IEnumerable<ChildCareStub> Parse(byte[] bytes, County county)
        {
            // parse the HTML
            CQ document = CQ.Create(new MemoryStream(bytes));

            // select the table
            CQ table = document["table"];
            // TODO: there is not table when the search returns zero results
            if (table.Length != 1)
            {
                var exception = new ParserException("Exactly one table on the search results page is expected.");
                Logger.ErrorException(string.Format("Expected: 1, Actual: {0}", table.Length), exception);
                throw exception;
            }

            // select all of the relevant rows in the table
            // TODO: verify the column headers
            // TODO: verify that every other row is empty
            IEnumerable<IDomElement> rows = table["tr"]
                .Elements
                .Where((e, i) => i%2 == 0) // every other row is empty...
                .Skip(1); // the first two is for the header

            // parse the rows using the child parser
            IEnumerable<ChildCareStub> stubs = rows.Select(r => ParseRow(r, county));
            if (county != null)
            {
                county.LastScrapedOn = DateTime.Now;
            }

            return stubs;
        }

        private ChildCareStub ParseRow(IDomElement element, County county)
        {
            // get all of the cells
            IDomElement[] cells = element.ChildElements.ToArray();
            if (cells.Length != 24)
            {
                var exception = new ParserException("Exactly 24 cells in each search result row is expected.");
                Logger.ErrorException(string.Format("Expected: 24, Actual: {0}, HTML:\n{1}", cells.Length, element.OuterHTML), exception);
                throw exception;
            }

            string typeCode = cells[14].InnerText.Trim();
            ChildCareStub childCareStub;
            switch (typeCode)
            {
                case "A":
                    childCareStub = new TypeAHomeStub();
                    break;
                case "B":
                    childCareStub = new TypeBHomeStub();
                    break;
                case "C":
                    childCareStub = new LicensedCenterStub();
                    break;
                case "D":
                    childCareStub = new DayCampStub();
                    break;
                default:
                    var exception = new ParserException("An unexpected child care type code was found.");
                    Logger.ErrorException(string.Format("TypeCode: '{0}', HTML:\n{1}", typeCode, element.OuterHTML), exception);
                    throw exception;
            }

            // get the link containing URL number
            var nameLink = (IHTMLAnchorElement) cells[2].FirstElementChild;

            // parse the URL number out of the URL
            Match match = Regex.Match(nameLink.Href, @"^results2\.asp\?provider_number=(?<ExternalUrlId>[A-Z]{18})$");
            if (!match.Success)
            {
                var exception = new ParserException("The child care link URL was not in the expected format.");
                Logger.ErrorException(string.Format("HREF: {0}, HTML:\n{1}", nameLink.Href, element.OuterHTML), exception);
                throw exception;
            }
            childCareStub.ExternalUrlId = match.Groups["ExternalUrlId"].Value;

            // parse out the name
            childCareStub.Name = nameLink.GetCollapsedInnerText();

            // parse out the name
            childCareStub.City = cells[10].GetCollapsedInnerText();

            // type B child cares do not have public addresses
            // TODO: verify TypeBHome address placeholder
            if (!(childCareStub is TypeBHomeStub))
            {
                // parse out the address
                childCareStub.Address = cells[6].GetCollapsedInnerText();
            }

            // set the county that was passed down
            if (county != null)
            {
                childCareStub.County = county;
                childCareStub.CountyId = county.Id;
            }

            // TODO: parse out the address and rating

            return childCareStub;
        }
    }
}
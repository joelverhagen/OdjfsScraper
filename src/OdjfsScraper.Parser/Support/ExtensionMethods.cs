using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using CsQuery;

namespace OdjfsScraper.Parser.Support
{
    public static class ExtensionMethods
    {
        public static string ToNullIfEmpty(this string input)
        {
            return input == string.Empty ? null : input;
        }

        public static string DecodeAndCollapse(this string text)
        {
            // decode the entities
            text = WebUtility.HtmlDecode(text);

            // collapse the whitespace
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        public static string GetCollapsedInnerText(this IDomElement e)
        {
            // get the text content
            string text = e.TextContent;

            return text.DecodeAndCollapse();
        }

        public static IEnumerable<IDomElement> GetDescendentElements(this IDomObject e)
        {
            if (e.HasChildren)
            {
                foreach (IDomElement child in e.ChildElements)
                {
                    yield return child;
                    foreach (IDomElement descendent in child.GetDescendentElements())
                    {
                        yield return descendent;
                    }
                }
            }
        }
    }
}
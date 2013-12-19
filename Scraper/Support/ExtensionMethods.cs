using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CsQuery;

namespace OdjfsScraper.Scraper.Support
{
    public static class ExtensionMethods
    {
        public static string ToNullIfEmpty(this string input)
        {
            return input == string.Empty ? null : input;
        }

        public static string GetCollapsedInnerText(this IDomElement e)
        {
            // get the text content
            string text = e.TextContent;

            // decode the entities
            text = WebUtility.HtmlDecode(text);

            // collapse the whitespace
            return Regex.Replace(text, @"\s+", " ").Trim();
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

        public static string GetSha256Hash(this byte[] bytes)
        {
            var sha = new SHA256Managed();
            byte[] hashBytes = sha.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        public static string GetInformationalVersion(this Assembly assembly)
        {
            return ((AssemblyInformationalVersionAttribute) assembly
                .GetCustomAttributes(typeof (AssemblyInformationalVersionAttribute), false)[0])
                .InformationalVersion;
        }
    }
}
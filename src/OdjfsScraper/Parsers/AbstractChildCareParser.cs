using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsQuery;
using CsQuery.Implementation;
using Microsoft.Extensions.Logging;
using OdjfsScraper.Models;

namespace OdjfsScraper.Parsers
{
    public abstract class AbstractChildCareParser<T> : IChildCareParser<T> where T : ChildCare
    {
        protected readonly ILogger<AbstractChildCareParser<T>> _logger;

        public AbstractChildCareParser(ILogger<AbstractChildCareParser<T>> logger)
        {
            _logger = logger;
        }

        public T Parse(T childCare, byte[] bytes)
        {
            // parse the HTML
            CQ document = CQ.Create(new MemoryStream(bytes));

            // get the details
            KeyValuePair<string, string>[] keyValuePairs = GetDetailKeyValuePairs(document)
                .Where(p => p.Key != null)
                .ToArray();

            // make sure the keys are unique
            string[] keys = keyValuePairs.Select(p => p.Key).ToArray();
            string[] uniqueKeys = keys.Distinct().ToArray();
            if (keys.Length != uniqueKeys.Length)
            {
                var exception = new ParserException("All keys in the first details table must be unique.");
                _logger.LogError(exception.Message + " OriginalKeys: {originalKeys}, Exception: {exception}", string.Join(", ", keys), exception);
                throw exception;
            }

            // create the dictionary
            IDictionary<string, string> details = keyValuePairs.ToDictionary(t => t.Key, t => t.Value);

            // generate the concrete object using the child implementation
            PopulateFields(childCare, details);

            // record this execution
            childCare.LastScrapedOn = DateTime.Now;

            return childCare;
        }

        protected abstract IEnumerable<KeyValuePair<string, string>> GetDetailKeyValuePairs(CQ document);
        protected abstract void PopulateFields(T childCare, IDictionary<string, string> details);

        #region Helpers

        protected static void ReplaceImagesWithText(IDomElement parent, IDictionary<string, string> replacements)
        {
            // replace all of the images with text
            IDomElement[] images = parent
                .GetDescendentElements()
                .Where(e => e.NodeName == "IMG")
                .ToArray();
            foreach (IDomElement image in images)
            {
                string text;
                if (!replacements.TryGetValue(image.GetAttribute("src"), out text))
                {
                    text = "unknown";
                }
                image.ParentNode.InsertAfter(new DomText(text), image);
                image.Remove();
            }
        }

        protected static DateTime ParseDate(string date)
        {
            return DateTime.ParseExact(date, "MM/dd/yyyy", CultureInfo.InvariantCulture);
        }

        protected string GetDetailString(IDictionary<string, string> details, params string[] keys)
        {
            // try all of the provided keys to get the value
            foreach (string key in keys)
            {
                string value;
                if (details.TryGetValue(key, out value))
                {
                    return value;
                }
            }

            var exception = new ParserException("An expected key was not found in the child care details.");
            _logger.LogError(
                exception.Message + " AllKeys: '{allKeys}', KeysTried: '{keysTried}'",
                string.Join(", ", details.Keys),
                string.Join(", ", keys));
            throw exception;
        }

        #endregion
    }
}
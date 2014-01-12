using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsQuery;
using CsQuery.Implementation;
using NLog;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Parser.Support;

namespace OdjfsScraper.Parser.Parsers
{
    public abstract class AbstractChildCareParser<T> : IChildCareParser<T> where T : ChildCare
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
                Logger.ErrorException(string.Format("Type: '{0}', OriginalKeys: {1}", typeof (T).Name, string.Join(", ", keys)), exception);
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

        protected static string GetDetailString(IDictionary<string, string> details, params string[] keys)
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
            Logger.ErrorException(string.Format("AllKeys: '{0}', KeysTried: '{1}'", string.Join(", ", details.Keys), string.Join(", ", keys)), exception);
            throw exception;
        }

        #endregion
    }
}
using System.Text;

namespace OdjfsScraper.Parser.UnitTests.Parsers.TestSupport
{
    public abstract class BaseTemplate
    {
        public static byte[] GetBytes(string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }
    }
}
using System;
using NLog;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Parser.Parsers
{
    public class ChildCareParser : IChildCareParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IChildCareParser<TypeAHome> _aParser;
        private readonly IChildCareParser<TypeBHome> _bParser;
        private readonly IChildCareParser<LicensedCenter> _cParser;
        private readonly IChildCareParser<DayCamp> _dParser;

        public ChildCareParser(IChildCareParser<TypeAHome> aParser, IChildCareParser<TypeBHome> bParser, IChildCareParser<LicensedCenter> cParser, IChildCareParser<DayCamp> dParser)
        {
            _aParser = aParser;
            _bParser = bParser;
            _cParser = cParser;
            _dParser = dParser;
        }

        public ChildCare Parse(ChildCareStub childCareStub, byte[] bytes)
        {
            ChildCare childCare;
            if (childCareStub is TypeAHomeStub)
            {
                childCare = new TypeAHome();
            }
            else if (childCareStub is TypeBHomeStub)
            {
                childCare = new TypeBHome();
            }
            else if (childCareStub is LicensedCenterStub)
            {
                childCare = new LicensedCenter();
            }
            else if (childCareStub is DayCampStub)
            {
                childCare = new DayCamp();
            }
            else
            {
                var exception = new ArgumentException("Unknown ChildCareStub type provided.", "childCareStub");
                Logger.ErrorException(string.Format("Type: '{0}'", childCareStub.GetType()), exception);
                throw exception;
            }

            // copy over the external URL ID
            childCare.ExternalUrlId = childCareStub.ExternalUrlId;
            childCare = Parse(childCare, bytes);
            childCareStub.LastScrapedOn = DateTime.Now;

            return childCare;
        }

        public ChildCare Parse(ChildCare childCare, byte[] bytes)
        {
            if (childCare is TypeAHome)
            {
                return _aParser.Parse((TypeAHome) childCare, bytes);
            }
            if (childCare is TypeBHome)
            {
                return _bParser.Parse((TypeBHome) childCare, bytes);
            }
            if (childCare is LicensedCenter)
            {
                return _cParser.Parse((LicensedCenter) childCare, bytes);
            }
            if (childCare is DayCamp)
            {
                return _dParser.Parse((DayCamp) childCare, bytes);
            }

            var exception = new ArgumentException("Unknown ChildCare type provided.", "childCare");
            Logger.ErrorException(string.Format("Type: '{0}'", childCare.GetType()), exception);
            throw exception;
        }
    }
}
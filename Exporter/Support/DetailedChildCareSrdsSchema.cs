using System;
using System.Collections.Generic;
using OdjfsScraper.Model.ChildCares;

namespace OdjfsScraper.Exporter.Support
{
    public class DetailedChildCareSrdsSchema : ISrdsSchema<DetailedChildCare>
    {
        private static readonly IEnumerable<SrdsAttribute<DetailedChildCare>> Properties = new List<SrdsAttribute<DetailedChildCare>>
        {
            // ChildCare
            new SrdsAttribute<DetailedChildCare>("CountyName", "System.String", c => c.County.Name),
            new SrdsAttribute<DetailedChildCare>("ExternalId", "System.String", c => c.ExternalId),
            new SrdsAttribute<DetailedChildCare>("ExternalUrlId", "System.String", c => c.ExternalUrlId),
            new SrdsAttribute<DetailedChildCare>("Address", "System.String", c => c.Address),
            new SrdsAttribute<DetailedChildCare>("City", "System.String", c => c.City),
            new SrdsAttribute<DetailedChildCare>("State", "System.String", c => c.State),
            new SrdsAttribute<DetailedChildCare>("ZipCode", "System.Int32", c => c.ZipCode),
            new SrdsAttribute<DetailedChildCare>("PhoneNumber", "System.String", c => c.PhoneNumber),
            // DetailedChildCare
            new SrdsAttribute<DetailedChildCare>("DetailedChildCareType", "System.String", c => c.DetailedChildCareType),
            new SrdsAttribute<DetailedChildCare>("ProviderAgreement", "System.String", c => c.ProviderAgreement),
            new SrdsAttribute<DetailedChildCare>("Administrators", "System.String", c => c.Administrators),
            new SrdsAttribute<DetailedChildCare>("CenterStatus", "System.String", c => c.CenterStatus),
            new SrdsAttribute<DetailedChildCare>("InitialApplicationDate", "System.String", c => c.InitialApplicationDate),
            new SrdsAttribute<DetailedChildCare>("LicenseBeginDate", "System.String", c => c.LicenseBeginDate),
            new SrdsAttribute<DetailedChildCare>("LicenseExpirationDate", "System.String", c => c.LicenseExpirationDate),
            new SrdsAttribute<DetailedChildCare>("SutqRating", "System.Int32?", c => c.SutqRating),
            new SrdsAttribute<DetailedChildCare>("Infants", "System.Boolean", c => c.Infants),
            new SrdsAttribute<DetailedChildCare>("YoungToddlers", "System.Boolean", c => c.YoungToddlers),
            new SrdsAttribute<DetailedChildCare>("OlderToddlers", "System.Boolean", c => c.OlderToddlers),
            new SrdsAttribute<DetailedChildCare>("Preschoolers", "System.Boolean", c => c.Preschoolers),
            new SrdsAttribute<DetailedChildCare>("Gradeschoolers", "System.Boolean", c => c.Gradeschoolers),
            new SrdsAttribute<DetailedChildCare>("Naeyc", "System.Boolean", c => c.Naeyc),
            new SrdsAttribute<DetailedChildCare>("Necpa", "System.Boolean", c => c.Necpa),
            new SrdsAttribute<DetailedChildCare>("Naccp", "System.Boolean", c => c.Naccp),
            new SrdsAttribute<DetailedChildCare>("Nafcc", "System.Boolean", c => c.Nafcc),
            new SrdsAttribute<DetailedChildCare>("Coa", "System.Boolean", c => c.Coa),
            new SrdsAttribute<DetailedChildCare>("Acsi", "System.Boolean", c => c.Acsi),
            new SrdsAttribute<DetailedChildCare>("MondayReported", "System.Boolean", c => c.MondayReported),
            new SrdsAttribute<DetailedChildCare>("MondayBegin", "System.DateTime?", c => c.MondayBegin),
            new SrdsAttribute<DetailedChildCare>("MondayEnd", "System.DateTime?", c => c.MondayEnd),
            new SrdsAttribute<DetailedChildCare>("TuesdayReported", "System.Boolean", c => c.TuesdayReported),
            new SrdsAttribute<DetailedChildCare>("TuesdayBegin", "System.DateTime?", c => c.TuesdayBegin),
            new SrdsAttribute<DetailedChildCare>("TuesdayEnd", "System.DateTime?", c => c.TuesdayEnd),
            new SrdsAttribute<DetailedChildCare>("WednesdayReported", "System.Boolean", c => c.WednesdayReported),
            new SrdsAttribute<DetailedChildCare>("WednesdayBegin", "System.DateTime?", c => c.WednesdayBegin),
            new SrdsAttribute<DetailedChildCare>("WednesdayEnd", "System.DateTime?", c => c.WednesdayEnd),
            new SrdsAttribute<DetailedChildCare>("ThursdayReported", "System.Boolean", c => c.ThursdayReported),
            new SrdsAttribute<DetailedChildCare>("ThursdayBegin", "System.DateTime?", c => c.ThursdayBegin),
            new SrdsAttribute<DetailedChildCare>("ThursdayEnd", "System.DateTime?", c => c.ThursdayEnd),
            new SrdsAttribute<DetailedChildCare>("FridayReported", "System.Boolean", c => c.FridayReported),
            new SrdsAttribute<DetailedChildCare>("FridayBegin", "System.DateTime?", c => c.FridayBegin),
            new SrdsAttribute<DetailedChildCare>("FridayEnd", "System.DateTime?", c => c.FridayEnd),
            new SrdsAttribute<DetailedChildCare>("SaturdayReported", "System.Boolean", c => c.SaturdayReported),
            new SrdsAttribute<DetailedChildCare>("SaturdayBegin", "System.DateTime?", c => c.SaturdayBegin),
            new SrdsAttribute<DetailedChildCare>("SaturdayEnd", "System.DateTime?", c => c.SaturdayEnd),
            new SrdsAttribute<DetailedChildCare>("SundayReported", "System.Boolean", c => c.SundayReported),
            new SrdsAttribute<DetailedChildCare>("SundayBegin", "System.DateTime?", c => c.SundayBegin),
            new SrdsAttribute<DetailedChildCare>("SundayEnd", "System.DateTime?", c => c.SundayEnd),
        }.AsReadOnly();

        public string GetName(DetailedChildCare entity)
        {
            return entity.Name;
        }

        public double GetLatitude(DetailedChildCare entity)
        {
            if (!entity.Latitude.HasValue)
            {
                throw new ArgumentException("The provided DetailedChildCare does not have a latitude.");
            }
            return entity.Latitude.Value;
        }

        public double GetLongitude(DetailedChildCare entity)
        {
            if (!entity.Longitude.HasValue)
            {
                throw new ArgumentException("The provided DetailedChildCare does not have a longitude.");
            }
            return entity.Longitude.Value;
        }

        public IEnumerable<SrdsAttribute<DetailedChildCare>> GetAttributes()
        {
            return Properties;
        }
    }
}
using System;

namespace OdjfsScraper.Model.ChildCares
{
    public class ChildCare
    {
        public int Id { get; set; }
        public string ChildCareType { get; set; }
        public DateTime LastScrapedOn { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime? LastGeocodedOn { get; set; }

        #region HTML to Entities

        public int CountyId { get; set; }
        public virtual County County { get; set; }

        #endregion

        #region HTML

        public string ExternalUrlId { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int ZipCode { get; set; }
        public string PhoneNumber { get; set; }

        #endregion
    }
}
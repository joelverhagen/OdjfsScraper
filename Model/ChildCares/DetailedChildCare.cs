using System;

namespace OdjfsScraper.Model.ChildCares
{
    public abstract class DetailedChildCare : ChildCare
    {
        #region HTML

        public string ProviderAgreement { get; set; }
        public string Administrators { get; set; }
        public string CenterStatus { get; set; }
        public string InitialApplicationDate { get; set; }
        public string LicenseBeginDate { get; set; }
        public string LicenseExpirationDate { get; set; }

        public int? SutqRating { get; set; }

        public bool Infants { get; set; }
        public bool YoungToddlers { get; set; }
        public bool OlderToddlers { get; set; }
        public bool Preschoolers { get; set; }
        public bool Gradeschoolers { get; set; }

        public bool ChildCareFoodProgram { get; set; }

        public bool Naeyc { get; set; }
        public bool Necpa { get; set; }
        public bool Naccp { get; set; }
        public bool Nafcc { get; set; }
        public bool Coa { get; set; }
        public bool Acsi { get; set; }

        public bool MondayReported { get; set; }
        public DateTime? MondayBegin { get; set; }
        public DateTime? MondayEnd { get; set; }

        public bool TuesdayReported { get; set; }
        public DateTime? TuesdayBegin { get; set; }
        public DateTime? TuesdayEnd { get; set; }

        public bool WednesdayReported { get; set; }
        public DateTime? WednesdayBegin { get; set; }
        public DateTime? WednesdayEnd { get; set; }

        public bool ThursdayReported { get; set; }
        public DateTime? ThursdayBegin { get; set; }
        public DateTime? ThursdayEnd { get; set; }

        public bool FridayReported { get; set; }
        public DateTime? FridayBegin { get; set; }
        public DateTime? FridayEnd { get; set; }

        public bool SaturdayReported { get; set; }
        public DateTime? SaturdayBegin { get; set; }
        public DateTime? SaturdayEnd { get; set; }

        public bool SundayReported { get; set; }
        public DateTime? SundayBegin { get; set; }
        public DateTime? SundayEnd { get; set; }

        #endregion
    }
}
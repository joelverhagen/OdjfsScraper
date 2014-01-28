using System;

namespace OdjfsScraper.Model.ChildCareStubs
{
    public class ChildCareStub
    {
        public int Id { get; set; }

        public virtual string ChildCareType
        {
            get { return null; }
        }

        public virtual int? CountyId { get; set; }
        public virtual County County { get; set; }
        public DateTime? LastScrapedOn { get; set; }

        #region HTML

        public string ExternalUrlId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }

        #endregion
    }
}
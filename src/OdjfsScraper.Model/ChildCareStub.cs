using System;

namespace OdjfsScraper.Models
{
    public class ChildCareStub
    {
        public int Id { get; set; }
        public int? CountyId { get; set; }
        public County County { get; set; }
        public DateTime? LastScrapedOn { get; set; }        
        public string ExternalUrlId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
    }
}
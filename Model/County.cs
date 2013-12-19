using System;

namespace OdjfsScraper.Model
{
    public class County
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? LastScrapedOn { get; set; }
    }
}
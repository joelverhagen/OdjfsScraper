namespace OdjfsScraper.Models
{
    public class DayCamp : ChildCare
    {
        public string Owner { get; set; }
        public string RegistrationStatus { get; set; }
        public string RegistrationBeginDate { get; set; }
        public string RegistrationEndDate { get; set; }
    }
}
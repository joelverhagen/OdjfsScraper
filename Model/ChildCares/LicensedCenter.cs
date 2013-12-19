namespace OdjfsScraper.Model.ChildCares
{
    public class LicensedCenter : DetailedChildCare
    {
        public const string DetailedDiscriminator = "LicensedCenter";

        public LicensedCenter()
        {
            DetailedChildCareType = "LicensedCenter";
        }
    }
}
namespace OdjfsScraper.Model.ChildCares
{
    public class TypeAHome : DetailedChildCare
    {
        public const string DetailedDiscriminator = "TypeAHome";

        public TypeAHome()
        {
            DetailedChildCareType = DetailedDiscriminator;
        }
    }
}
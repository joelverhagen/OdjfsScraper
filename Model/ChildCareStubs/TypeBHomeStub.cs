using OdjfsScraper.Model.ChildCares;

namespace OdjfsScraper.Model.ChildCareStubs
{
    public class TypeBHomeStub : ChildCareStub
    {
        public const string Discriminator = TypeBHome.Discriminator;

        public override string ChildCareType
        {
            get { return Discriminator; }
        }
    }
}
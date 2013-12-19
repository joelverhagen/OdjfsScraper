using OdjfsScraper.Model.ChildCares;

namespace OdjfsScraper.Model.ChildCareStubs
{
    public class DayCampStub : ChildCareStub
    {
        public const string Discriminator = DayCamp.Discriminator;

        public override string ChildCareType
        {
            get { return Discriminator; }
        }
    }
}
using OdjfsScraper.Model.ChildCares;

namespace OdjfsScraper.Model.ChildCareStubs
{
    public class LicensedCenterStub : ChildCareStub
    {
        public const string Discriminator = LicensedCenter.DetailedDiscriminator;

        public override string ChildCareType
        {
            get { return Discriminator; }
        }
    }
}
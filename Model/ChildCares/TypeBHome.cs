namespace OdjfsScraper.Model.ChildCares
{
    public class TypeBHome : ChildCare
    {
        public const string Discriminator = "TypeBHome";

        public TypeBHome()
        {
            ChildCareType = Discriminator;
        }

        #region HTML

        public string CertificationBeginDate { get; set; }
        public string CertificationExpirationDate { get; set; }

        #endregion
    }
}
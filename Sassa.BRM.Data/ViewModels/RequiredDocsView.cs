namespace Sassa.BRM.Models
{
    public class RequiredDocsView
    {
        public string GrantType { get; set; }
        public decimal DOC_ID { get; set; }
        public string DOC_NAME { get; set; }
        public string DOC_SECTION { get; set; }
        public string DOC_CRITICAL { get; set; }
        public bool DOC_PRESENT { get; set; }

        public RequiredDocsView Copy(RequiredDocsView other)
        {
            return new RequiredDocsView() { DOC_ID = other.DOC_ID, DOC_NAME = other.DOC_NAME };
        }
    }
}

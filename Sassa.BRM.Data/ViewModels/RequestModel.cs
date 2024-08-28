namespace Sassa.BRM.Models
{
    public class RequestModel
    {
        public RequestModel()
        {
            GrantType = "";
            Category = "";
            CategoryType = "";
            StakeHolder = "";

        }
        public string IdNo { get; set; }
        public string GrantType { get; set; }
        public string Category { get; set; }
        public string CategoryType { get; set; }
        public string StakeHolder { get; set; }
        public string Description { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Sassa.BRM.Models
{
    public class RequestModel
    {
        public RequestModel()
        {
            GrantType = "";
            Category = 0;
            CategoryType = 0;
            StakeHolder = 0;

        }
        [Required]
        [StringLength(13)]
        public string IdNo { get; set; }
        public string GrantType { get; set; }
        public decimal Category { get; set; }
        [Required]
        public decimal CategoryType { get; set; }
        public decimal StakeHolder { get; set; }
        public string Description { get; set; }

        [Required]
        public string CategoryString 
        {
            get {return Category.ToString(); }
            set
            {
                Category = decimal.Parse(value);
            }
        }
        [Required]
        public string CategoryTypeString
        {
            get { return CategoryType.ToString(); }
            set
            {
                CategoryType = decimal.Parse(value);
            }
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Sassa.eDocs.Data.Models
{
    public class ApplicationType
    {
        [Key]
        public int ApplcationTypeId { get; set; }
        [MaxLength(30)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string Description { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Sassa.eDocs.Data.Models
{
    public class DocImage
    {
        [Key]
        public int DocImageId { get; set; }
        public int DocumentId { get; set; }
        [MaxLength(400000)]
        public byte[] Image { get; set; }
    }
}

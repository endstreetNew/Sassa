using System.ComponentModel.DataAnnotations;

namespace Sassa.eDocs.Data.Models
{
    public class ApplicationDocument
    {
        [Key]
        public int ApplicationDocumentId { get; set; }
        public int DocumentTypeId { get; set; }
        public int ApplicationTypeId { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;


namespace Sassa.eDocs.Data.Models
{
    public class RejectHistory
    {
        [Key]
        public int Id { get; set; }
        public string Reference { get; set; }
        public int LoDocumentTypeId { get; set; }

        public string RejectReason { get; set; }

    }
}

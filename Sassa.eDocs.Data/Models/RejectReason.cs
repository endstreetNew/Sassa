using System.ComponentModel.DataAnnotations;

namespace Sassa.eDocs.Data.Models
{
    public class RejectReason
    {
        [Key]
        public int RejectReasonId { get; set; }
        [MaxLength(100)]
        public string Reason { get; set; }
    }
}

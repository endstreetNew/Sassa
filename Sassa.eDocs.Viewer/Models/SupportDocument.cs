namespace Sassa.eDocs.Models
{
    public class SupportDocument
    {
        public int DocumentId { get; set; }
        public int DocumentTypeId { get; set; }
        public string RejectReasonId { get; set; }
        public string SupportDocumentRef { get; set; }
        public string FileName { get; set; }

    }
}

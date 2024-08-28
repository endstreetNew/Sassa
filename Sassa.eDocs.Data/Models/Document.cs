using System;
using System.ComponentModel.DataAnnotations;

namespace Sassa.eDocs.Data.Models
{
    [Serializable]
    public class Document
    {
        [Key]
        public int DocumentId { get; set; }
        [MaxLength(20)]
        public string Reference { get; set; }
        [MaxLength(20)]
        public string SupportDocument { get; set; }
        [MaxLength(50)]
        public string OtherDocumentType { get; set; }
        [MaxLength(13)]
        public string IdNo { get; set; }
        [MaxLength(13)]
        public string ChildIdNo { get; set; }
        public int ApplicationTypeId { get; set; }
        public int LoDocumentTypeId { get; set; }
        public int DocumentTypeId { get; set; }
        [MaxLength(30)]
        public string User { get; set; }
        [MaxLength(30)]
        public string DateStamp { get; set; }
        public string Status { get; set; }
        [MaxLength(100)]
        public string RejectReason { get; set; }
        [MaxLength(13)]
        public string RegionCode { get; set; }
        [MaxLength(100)]
        public string FileName { get; set; }
        [MaxLength(100)]
        public string CSNode { get; set; }
        /// <summary>
        /// Internal documents will not be displayed to the client.
        /// </summary>
        public bool InternalDocument { get; set; }

    }

    //public enum DocStatus
    //{
    //    New,
    //    Verified,
    //    Rejected
    //}
}

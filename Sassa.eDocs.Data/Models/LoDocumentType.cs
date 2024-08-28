using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sassa.eDocs.Data.Models
{
    public class LoDocumentType
    {
        [Key]
        public int Id { get; set; }
        public int LoDocumentTypeId { get; set; }
        public int DocumentTypeId { get; set; }
        [MaxLength(30)]
        public string DisplayName { get; set; }
        [MaxLength(30)]
        public string DocumentType { get; set; }

    }
}

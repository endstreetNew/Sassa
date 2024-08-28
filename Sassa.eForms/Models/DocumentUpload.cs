using Sassa.eDocs.Data.Models;
using System;
using System.Collections.Generic;

namespace Sassa.eForms.Models
{
    [Serializable]
    public class DocumentUpload
    {
        public DocumentUpload()
        {
            DocList = new List<Document>();
        }
        public IEnumerable<Document> DocList { get; set; }
    }
}

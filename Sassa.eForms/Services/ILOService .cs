using Sassa.eDocs.Data.Models;
using System.Collections.Generic;

namespace Sassa.eForms.Services
{
    public interface ILOService
    {
        public IEnumerable<Document> GetRequiredDocuments(string reference);

        public void SetUploaded(string reference);
    }
}

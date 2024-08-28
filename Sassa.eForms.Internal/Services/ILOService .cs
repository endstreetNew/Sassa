using Sassa.eDocs.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sassa.eForms.Services
{
    public interface ILOService
    {
        public IEnumerable<Document> GetRequiredDocuments(string reference);

        public Task<string> SetUploaded(string reference);
    }
}

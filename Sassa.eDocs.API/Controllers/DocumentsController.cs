using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sassa.eDocs.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sassa.eDocs.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly eDocumentContext _context;

        public DocumentsController(eDocumentContext context)
        {
            _context = context;
        }

        // GET: api/Documents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocuments()
        {
            return await _context.Documents.ToListAsync();
        }
        //Viewer
        [HttpGet("ref/{reference}")]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocumentsByReference(string reference)
        {
            return await _context.Documents.Where(d => d.Reference == reference).ToListAsync();
        }

        [HttpGet("id/{idno}")]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocumentsById(string idno)
        {
            return await _context.Documents.Where(d => d.IdNo == idno).ToListAsync();
        }

        [HttpGet("childid/{childidno}")]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocumentsByChildId(string childidno)
        {
            return await _context.Documents.Where(d => d.ChildIdNo == childidno).ToListAsync();
        }

        // GET: api/Documents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Document>> GetDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            return document;
        }

        // PUT: api/Documents/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocument(int id, Document document)
        {
            if (id != document.DocumentId)
            {
                return BadRequest();
            }

            _context.Entry(document).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // PUT: api/Documents/Status/Uploaded
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("type/{id}/{dtype}")]
        public async Task<IActionResult> PutDocumentType(int id, string dtype)
        {
            var document = await _context.Documents.FindAsync(id);
            document.DocumentTypeId = int.Parse(dtype);
            document.InternalDocument = true;

            _context.Entry(document).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        // PUT: api/Documents/Status/Uploaded
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("status/{id}/{status}")]
        public async Task<IActionResult> PutDocumentStatus(int id, string status)
        {
            var document = await _context.Documents.FindAsync(id);
            string docTypeName = _context.DocumentTypes.Where(d => d.DocumentTypeId == document.DocumentTypeId).First().Name;
            document.FileName = $"{document.IdNo}_{document.Reference}_{document.DocumentTypeId.ToString().PadLeft(2, '0')} - {docTypeName}_{document.RegionCode}{DateTime.Now.ToString("yyyy-MM-dd")}_{document.SupportDocument}.pdf";
            document.Status = status;
            if(status != "Returned")
            {
                document.RejectReason = "";
            }

            _context.Entry(document).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // PUT: api/Documents/Status/Uploaded
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("purge/{reference}")]
        public async Task<IActionResult> PurgeOrphanedDocuments(string reference)
        {
            var docs = await _context.Documents.Where(d => d.Reference == reference).ToListAsync();
            foreach(Document doc in docs)
            {
                if(!_context.DocImage.Where(i => i.DocumentId == doc.DocumentId).Any())
                {
                    _context.Remove(doc);
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("reject/{id}/{rejectReason}")]
        public async Task<IActionResult> PutDocumentReject(int id, string rejectReason)
        {
            var document = await _context.Documents.FindAsync(id);
            document.RejectReason = rejectReason;

            _context.Entry(document).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        //public string GetFilename(Document doc)
        //{
        //    string docTypeName = _context.DocumentTypes.Where(d => d.DocumentTypeId == doc.DocumentTypeId).First().Name;
        //    return $"{doc.IdNo}_{doc.Reference}_{doc.DocumentTypeId.ToString().PadLeft(2, '0')} - {docTypeName}_{doc.RegionCode}{DateTime.Now.ToString("yyyy-MM-dd")}_{doc.SupportDocument}.pdf";
        //}


        // POST: api/Documents
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Document>> PostDocument(Document document)
        {
            if (DocumentExists(document.Reference, document.LoDocumentTypeId,document.ChildIdNo) && document.LoDocumentTypeId != 123)
            {
                document = _context.Documents.Where(e => e.LoDocumentTypeId == document.LoDocumentTypeId && e.Reference == document.Reference && e.ChildIdNo == document.ChildIdNo).First();
            }
            else
            {
                document.Status = "New";
                document.DateStamp = System.DateTime.Now.ToString("dd/MM/yyyy hh:mm");
                if(string.IsNullOrEmpty(document.SupportDocument))
                {
                    document.SupportDocument = await GetNextSDnumber(document);
                }
                _context.Documents.Add(document);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction("GetDocument", new { id = document.DocumentId }, document);
        }

        // DELETE: api/Documents/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Document>> DeleteDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            if(_context.DocImage.Where(e => e.DocumentId == document.DocumentId).Any())
            {
                DocImage image = _context.DocImage.Where(e => e.DocumentId == document.DocumentId).First();
                _context.DocImage.Remove(image);
            }
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            await ReSDnumber(document.Reference);

            return document;
        }

        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.DocumentId == id);
        }

        private bool DocumentExists(string reference, int documentTypeId)
        {
            return _context.Documents.Any(e => e.DocumentTypeId == documentTypeId && e.Reference == reference);
        }
        private bool DocumentExists(string reference, int documentTypeId, string childIdno  )
        {
            return _context.Documents.Any(e => e.LoDocumentTypeId == documentTypeId && e.Reference == reference && e.ChildIdNo == childIdno);
        }

        private async Task<string> GetNextSDnumber(Document document)
        {

            var docs = await _context.Documents.Where(d => d.Reference == document.Reference).ToListAsync();
            int lastSDNo = 0;
            foreach (Document doc in docs)
            {
                if(int.Parse(doc.SupportDocument.Substring(2,2)) > lastSDNo)
                {
                    lastSDNo = int.Parse(doc.SupportDocument.Substring(2, 2));
                }
            }
            lastSDNo++;
            return "SD" + lastSDNo.ToString().PadLeft(2, '0');
        }

        /// <summary>
        /// On Delete we may leave a gap in the sequence
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private async Task ReSDnumber(string reference)
        {

            var docs = await _context.Documents.Where(d => d.Reference == reference && d.InternalDocument).ToListAsync();
            if (!docs.Any()) return;
            var Edocs = await _context.Documents.Where(d => d.Reference == reference && !d.InternalDocument).ToListAsync();
            int lastSDNo = 0;
            foreach (Document doc in Edocs)
            {
                if (int.Parse(doc.SupportDocument.Substring(2, 2)) > lastSDNo)
                {
                    lastSDNo = int.Parse(doc.SupportDocument.Substring(2, 2));
                }
            }
            foreach (Document doc in docs)
            {
                lastSDNo++;
                doc.SupportDocument = "SD" + lastSDNo.ToString().PadLeft(2, '0');
                if (doc.FileName == null)
                {
                    var dt = await _context.DocumentTypes.Where(d => d.DocumentTypeId == doc.DocumentTypeId).FirstAsync();
                    doc.FileName = $"{doc.IdNo}_{doc.Reference}_{doc.DocumentTypeId.ToString().PadLeft(2, '0')} - {dt.Name}_{doc.RegionCode}{DateTime.Now.ToString("yyyy-MM-dd")}_{doc.SupportDocument}.pdf";
                }
                else
                {
                    doc.FileName = doc.FileName.Replace($"{doc.SupportDocument}.pdf", "SD" + lastSDNo.ToString().PadLeft(2, '0') + ".pdf");
                }
                _context.Entry(doc).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();
        }
    }
}

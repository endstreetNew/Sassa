using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sassa.eDocs.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sassa.eDocs.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentTypesController : ControllerBase
    {
        private readonly eDocumentContext _context;

        public DocumentTypesController(eDocumentContext context)
        {
            _context = context;
        }

        // GET: api/DocumentTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentType>>> GetDocumentTypes()
        {
            return await _context.DocumentTypes.ToListAsync();
        }

        // GET: api/DocumentTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentType>> GetDocumentType(int id)
        {
            var documentType = await _context.DocumentTypes.FindAsync(id);

            if (documentType == null)
            {
                return NotFound();
            }

            return documentType;
        }

        // PUT: api/DocumentTypes/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocumentType(int id, DocumentType documentType)
        {
            if (id != documentType.DocumentTypeId)
            {
                return BadRequest();
            }

            _context.Entry(documentType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentTypeExists(id))
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

        // POST: api/DocumentTypes
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<DocumentType>> PostDocumentType(DocumentType documentType)
        {
            _context.DocumentTypes.Add(documentType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDocumentType", new { id = documentType.DocumentTypeId }, documentType);
        }

        // DELETE: api/DocumentTypes/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<DocumentType>> DeleteDocumentType(int id)
        {
            var documentType = await _context.DocumentTypes.FindAsync(id);
            if (documentType == null)
            {
                return NotFound();
            }

            _context.DocumentTypes.Remove(documentType);
            await _context.SaveChangesAsync();

            return documentType;
        }

        private bool DocumentTypeExists(int id)
        {
            return _context.DocumentTypes.Any(e => e.DocumentTypeId == id);
        }
    }
}

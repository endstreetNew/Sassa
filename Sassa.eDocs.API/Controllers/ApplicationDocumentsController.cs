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
    public class ApplicationDocumentsController : ControllerBase
    {
        private readonly eDocumentContext _context;

        public ApplicationDocumentsController(eDocumentContext context)
        {
            _context = context;
        }

        // GET: api/ApplicationDocuments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationDocument>>> GetApplicationDocuments()
        {
            return await _context.ApplicationDocuments.ToListAsync();
        }

        // GET: api/ApplicationDocuments
        [HttpGet("{applicationTypeid}")]
        public async Task<ActionResult<IEnumerable<ApplicationDocument>>> GetApplicationDocuments(int applicationTypeId)
        {
            return await _context.ApplicationDocuments.Where(d => d.ApplicationTypeId == applicationTypeId).ToListAsync();
        }

        //// GET: api/ApplicationDocuments/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<ApplicationDocument>> GetApplicationDocument(int id)
        //{
        //    var applicationDocument = await _context.ApplicationDocuments.FindAsync(id);

        //    if (applicationDocument == null)
        //    {
        //        return NotFound();
        //    }

        //    return applicationDocument;
        //}

        // PUT: api/ApplicationDocuments/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutApplicationDocument(int id, ApplicationDocument applicationDocument)
        {
            if (id != applicationDocument.ApplicationDocumentId)
            {
                return BadRequest();
            }

            _context.Entry(applicationDocument).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicationDocumentExists(id))
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

        // POST: api/ApplicationDocuments
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<ApplicationDocument>> PostApplicationDocument(ApplicationDocument applicationDocument)
        {
            _context.ApplicationDocuments.Add(applicationDocument);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetApplicationDocument", new { id = applicationDocument.ApplicationDocumentId }, applicationDocument);
        }

        // DELETE: api/ApplicationDocuments/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApplicationDocument>> DeleteApplicationDocument(int id)
        {
            var applicationDocument = await _context.ApplicationDocuments.FindAsync(id);
            if (applicationDocument == null)
            {
                return NotFound();
            }

            _context.ApplicationDocuments.Remove(applicationDocument);
            await _context.SaveChangesAsync();

            return applicationDocument;
        }

        private bool ApplicationDocumentExists(int id)
        {
            return _context.ApplicationDocuments.Any(e => e.ApplicationDocumentId == id);
        }
    }
}

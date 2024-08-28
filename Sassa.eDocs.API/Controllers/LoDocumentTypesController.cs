using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sassa.eDocs;
using Sassa.eDocs.Data.Models;

namespace Sassa.eDocs.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoDocumentTypesController : ControllerBase
    {
        private readonly eDocumentContext _context;

        public LoDocumentTypesController(eDocumentContext context)
        {
            _context = context;
        }

        // GET: api/LoDocumentTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoDocumentType>>> GetLoDocumentTypes()
        {
            return await _context.LoDocumentTypes.ToListAsync();
        }

        // GET: api/LoDocumentTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LoDocumentType>> GetDocumentType(int id)
        {
            var loDocumentType = await _context.LoDocumentTypes.FindAsync(id);

            if (loDocumentType == null)
            {
                return NotFound();
            }

            return loDocumentType;
        }


        [HttpGet("lo/{id}")]
        public async Task<ActionResult<LoDocumentType>> GetLoDocumentType(int id)
        {
            var loDocumentType = await _context.LoDocumentTypes.Where(d => d.LoDocumentTypeId == id).FirstAsync();

            if (loDocumentType == null)
            {
                return NotFound();
            }

            return loDocumentType;
        }
        // PUT: api/LoDocumentTypes/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLoDocumentType(int id, LoDocumentType loDocumentType)
        {
            if (id != loDocumentType.Id)
            {
                return BadRequest();
            }

            _context.Entry(loDocumentType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoDocumentTypeExists(id))
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

        // POST: api/LoDocumentTypes
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<LoDocumentType>> PostLoDocumentType(LoDocumentType loDocumentType)
        {
            _context.LoDocumentTypes.Add(loDocumentType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLoDocumentType", new { id = loDocumentType.Id }, loDocumentType);
        }

        // DELETE: api/LoDocumentTypes/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<LoDocumentType>> DeleteLoDocumentType(int id)
        {
            var loDocumentType = await _context.LoDocumentTypes.FindAsync(id);
            if (loDocumentType == null)
            {
                return NotFound();
            }

            _context.LoDocumentTypes.Remove(loDocumentType);
            await _context.SaveChangesAsync();

            return loDocumentType;
        }

        private bool LoDocumentTypeExists(int id)
        {
            return _context.LoDocumentTypes.Any(e => e.Id == id);
        }
    }
}

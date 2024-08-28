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
    public class RejectHistoriesController : ControllerBase
    {
        private readonly eDocumentContext _context;

        public RejectHistoriesController(eDocumentContext context)
        {
            _context = context;
        }

        // GET: api/RejectHistories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RejectHistory>>> GetRejectHistory()
        {
            return await _context.RejectHistory.ToListAsync();
        }

        // GET: api/RejectHistories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RejectHistory>> GetRejectHistory(int id)
        {
            var rejectHistory = await _context.RejectHistory.FindAsync(id);

            if (rejectHistory == null)
            {
                return NotFound();
            }

            return rejectHistory;
        }

        // GET: api/RejectHistories/5
        [HttpGet("ref/{reference}")]
        public async Task<ActionResult<IEnumerable<RejectHistory>>> GetRejectHistory(string  reference)
        {
            var rejectHistory = await _context.RejectHistory.Where(r => r.Reference == reference).ToListAsync();

            if (rejectHistory == null)
            {
                return NotFound();
            }

            return rejectHistory;
        }
        // PUT: api/RejectHistories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRejectHistory(int id, RejectHistory rejectHistory)
        {
            if (id != rejectHistory.Id)
            {
                return BadRequest();
            }

            _context.Entry(rejectHistory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RejectHistoryExists(id))
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

        // POST: api/RejectHistories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<RejectHistory>> PostRejectHistory(RejectHistory rejectHistory)
        {
            _context.RejectHistory.Add(rejectHistory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRejectHistory", new { id = rejectHistory.Id }, rejectHistory);
        }

        // DELETE: api/RejectHistories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRejectHistory(int id)
        {
            var rejectHistory = await _context.RejectHistory.FindAsync(id);
            if (rejectHistory == null)
            {
                return NotFound();
            }

            _context.RejectHistory.Remove(rejectHistory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RejectHistoryExists(int id)
        {
            return _context.RejectHistory.Any(e => e.Id == id);
        }
    }
}

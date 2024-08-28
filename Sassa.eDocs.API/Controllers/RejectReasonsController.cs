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
    public class RejectReasonsController : ControllerBase
    {
        private readonly eDocumentContext _context;

        public RejectReasonsController(eDocumentContext context)
        {
            _context = context;
        }

        // GET: api/RejectReasons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RejectReason>>> GetRejectReasons()
        {
            return await _context.RejectReasons.ToListAsync();
        }

        // GET: api/RejectReasons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RejectReason>> GetRejectReason(int id)
        {
            var rejectReason = await _context.RejectReasons.FindAsync(id);

            if (rejectReason == null)
            {
                return NotFound();
            }

            return rejectReason;
        }

        // PUT: api/RejectReasons/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRejectReason(int id, RejectReason rejectReason)
        {
            if (id != rejectReason.RejectReasonId)
            {
                return BadRequest();
            }

            _context.Entry(rejectReason).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RejectReasonExists(id))
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

        // POST: api/RejectReasons
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<RejectReason>> PostRejectReason(RejectReason rejectReason)
        {
            _context.RejectReasons.Add(rejectReason);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRejectReason", new { id = rejectReason.RejectReasonId }, rejectReason);
        }

        // DELETE: api/RejectReasons/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<RejectReason>> DeleteRejectReason(int id)
        {
            var rejectReason = await _context.RejectReasons.FindAsync(id);
            if (rejectReason == null)
            {
                return NotFound();
            }

            _context.RejectReasons.Remove(rejectReason);
            await _context.SaveChangesAsync();

            return rejectReason;
        }

        private bool RejectReasonExists(int id)
        {
            return _context.RejectReasons.Any(e => e.RejectReasonId == id);
        }
    }
}

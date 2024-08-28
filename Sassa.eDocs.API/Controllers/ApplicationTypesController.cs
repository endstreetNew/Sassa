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
    public class ApplicationTypesController : ControllerBase
    {
        private readonly eDocumentContext _context;

        public ApplicationTypesController(eDocumentContext context)
        {
            _context = context;
        }

        // GET: api/ApplicationTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationType>>> GetApplicationTypes()
        {
            return await _context.ApplicationTypes.ToListAsync();
        }

        // GET: api/ApplicationTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationType>> GetApplicationType(int id)
        {
            var applicationType = await _context.ApplicationTypes.FindAsync(id);

            if (applicationType == null)
            {
                return NotFound();
            }

            return applicationType;
        }

        // PUT: api/ApplicationTypes/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutApplicationType(int id, ApplicationType applicationType)
        {
            if (id != applicationType.ApplcationTypeId)
            {
                return BadRequest();
            }

            _context.Entry(applicationType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicationTypeExists(id))
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

        // POST: api/ApplicationTypes
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<ApplicationType>> PostApplicationType(ApplicationType applicationType)
        {
            _context.ApplicationTypes.Add(applicationType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetApplicationType", new { id = applicationType.ApplcationTypeId }, applicationType);
        }

        // DELETE: api/ApplicationTypes/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApplicationType>> DeleteApplicationType(int id)
        {
            var applicationType = await _context.ApplicationTypes.FindAsync(id);
            if (applicationType == null)
            {
                return NotFound();
            }

            _context.ApplicationTypes.Remove(applicationType);
            await _context.SaveChangesAsync();

            return applicationType;
        }

        private bool ApplicationTypeExists(int id)
        {
            return _context.ApplicationTypes.Any(e => e.ApplcationTypeId == id);
        }
    }
}

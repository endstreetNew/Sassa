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
    public class DocImagesController : ControllerBase
    {
        private readonly eDocumentContext _context;

        public DocImagesController(eDocumentContext context)
        {
            _context = context;
        }

        // GET: api/DocImages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocImage>>> GetDocImage()
        {
            return await _context.DocImage.ToListAsync();
        }

        // GET: api/DocImages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DocImage>> GetImage(int id)
        {
            var docImage = await _context.DocImage.FindAsync(id);

            if (docImage == null)
            {
                return NotFound();
            }

            return docImage;
        }
        // GET: api/DocImages/5
        [HttpGet("doc/{id}")]
        public async Task<ActionResult<DocImage>> GetDocImage(int id)
        {
            var docImage = await _context.DocImage.Where(d => d.DocumentId == id).FirstAsync();

            if (docImage == null)
            {
                return NotFound();
            }

            return docImage;
        }

        //[HttpGet("image/{id}")]
        //public async Task<ActionResult<byte[]>> GetImageBytes(int id)
        //{
        //    var docImage = await _context.DocImage.Where(d => d.DocumentId == id).FirstAsync();

        //    if (docImage == null)
        //    {
        //        return NotFound();
        //    }

        //    return docImage.Image;
        //}

        /// <summary>
        /// MAX 500KB
        /// </summary>
        /// <param name="docImage"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocImage(int id, DocImage docImage)
        {
            if (id != docImage.DocImageId)
            {
                return BadRequest();
            }

            _context.Entry(docImage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocImageExists(id))
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

        /// <summary>
        /// MAX 500KB
        /// </summary>
        /// <param name="docImage"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<DocImage>> PostDocImage(DocImage docImage)
        {
            if (_context.DocImage.Where(d => d.DocumentId == docImage.DocumentId).Any())
            {
                docImage = _context.DocImage.Where(d => d.DocumentId == docImage.DocumentId).First();
                _context.Entry(docImage).State = EntityState.Modified;
            }
            else
            {
                _context.DocImage.Add(docImage);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDocImage", new { id = docImage.DocImageId }, docImage);
        }

        // DELETE: api/DocImages/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<DocImage>> DeleteDocImage(int id)
        {
            var docImage = await _context.DocImage.FindAsync(id);
            if (docImage == null)
            {
                return NotFound();
            }

            _context.DocImage.Remove(docImage);
            await _context.SaveChangesAsync();

            return docImage;
        }

        private bool DocImageExists(int id)
        {
            return _context.DocImage.Any(e => e.DocImageId == id);
        }
    }
}

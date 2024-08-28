using Sassa.eDocs.Data.Models;
using Sassa.eDocs.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sassa.eDocs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Sassa.eForms.Services
{
    public class StoreDbService
    {

        eDocumentContext _context;
        public StoreDbService(eDocumentContext context)
        {
            _context = context;
        }

        public async Task SaveDocumentImage(DocImage docImage)
        {

            DocImage image = docImage;

            if (_context.DocImage.Where(d => d.DocumentId == docImage.DocumentId).Any())
            {
                docImage = _context.DocImage.Where(d => d.DocumentId == docImage.DocumentId).First();
                docImage.Image = image.Image;

                _context.Entry(docImage).State = EntityState.Modified;
            }
            else
            {
                _context.DocImage.Add(docImage);
            }
            await _context.SaveChangesAsync();
        }
    }
}

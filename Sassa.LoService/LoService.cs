using Microsoft.EntityFrameworkCore;
using Sassa.BRM.Models;
using Sassa.Models;

namespace Sassa.Services
{
    public class LoService(IDbContextFactory<LoModelContext> dbContextFactory)
    {
        public async Task<CustCoversheet> GetCoversheetAsync(string reference)
        {
            try
            {
                using (var _context = dbContextFactory.CreateDbContext())
                {
                    var result = await _context.CustCoversheets.Where(c => c.ReferenceNum == reference).ToListAsync();
                    if(result.Count() == 0)throw new Exception($"Reference NUM {reference} not found ");
                    if (result.Count() > 1) throw new Exception($"Multiple records found for Reference NUM {reference} not found ");
                    return result.First();
                }
            }
            catch 
            {
                throw;
            }
        }

        public async Task UpdateValidation(CustCoversheetValidation custCoversheetValidation)
        {
            try
            {
                using (var _context = dbContextFactory.CreateDbContext())
                {
                    var validation = await _context.CustCoversheetValidations.FindAsync(custCoversheetValidation.ReferenceNum);
                    if (validation == null)
                    {
                        _context.CustCoversheetValidations.Add(custCoversheetValidation);
                    }
                    else
                    {
                        validation.ValidationDate = DateTime.Now;
                        validation.Validationresult = custCoversheetValidation.Validationresult;
                    }
                    await _context.SaveChangesAsync(); // Persist changes

                }
            }
            catch 
            {
                throw;
            }
        }

        public async Task UpdateClmNumber(string reference,DcFile file)
        {
            try
            {
                using (var _context = dbContextFactory.CreateDbContext())
                {
                    var result = await _context.CustCoversheets.Where(c => c.ReferenceNum == reference).ToListAsync();
                    if (result.Count() == 0) throw new Exception($"Reference NUM {reference} not found ");
                    if (result.Count() > 1) throw new Exception($"Multiple records found for Reference NUM {reference}");
                    var cover =  result.First();
                    cover.Clmnumber = file.UnqFileNo;
                    cover.BrmNumber = file.BrmBarcode;
                    cover.ScannedDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    await _context.SaveChangesAsync();
                }
            }
            catch 
            {
                throw;
            }

        }
    }
}

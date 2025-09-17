using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sassa.BRM.Models;
using Sassa.Models;

namespace Sassa.Services
{
    public class LoService(IDbContextFactory<LoModelContext> dbContextFactory, ILogger<LoService> logger)
    {

        public async Task<CustCoversheet> GetCoversheetAsync(string reference)
        {
            try
            {
                using (var _context = dbContextFactory.CreateDbContext())
                {
                    return await _context.CustCoversheets.FindAsync(reference);
                    //if(result.Count() == 0)throw new Exception($"Reference NUM {reference} not found.");
                    //if (result.Count() > 1) throw new Exception($"Multiple records found for Reference NUM {reference}.");
                    //return result.First();
                }
            }
            catch (Exception ex)
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
                        validation.Validationresult = custCoversheetValidation.Validationresult?.Length > 254 ? custCoversheetValidation.Validationresult.Substring(0, 254) : custCoversheetValidation.Validationresult;
                    }
                    await _context.SaveChangesAsync(); // Persist changes

                }
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateLOCover(string reference, DcFile file)
        {
            try
            {
                using (var _context = dbContextFactory.CreateDbContext())
                {
                    var cover = await _context.CustCoversheets.FindAsync(reference);
                    if (cover is null) throw new Exception("Could not update cover sheet");
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

        public async Task<bool> ValidationExists(string reference)
        {
            try
            {
                using (var _context = dbContextFactory.CreateDbContext())
                {
                    var result = await _context.CustCoversheetValidations.Where(c => c.ReferenceNum == reference).ToListAsync();
                    return result.Count() > 0;
                }
            }
            catch
            {
                throw new Exception("LO may be offline. (Retry)");
            }
        }

        public async Task<List<CustCoversheetValidation>> GetRepairQueue()
        {
            try
            {
                using (var _context = dbContextFactory.CreateDbContext())
                {
                    return await _context.CustCoversheetValidations.Where(c => c.Validationresult!.ToLower() != "ok").OrderBy(c => c.ValidationDate).Take(100).AsNoTracking().ToListAsync();
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<CustCoversheetValidation> GetValidationRecord(string reference)
        {
            try
            {
                using (var _context = dbContextFactory.CreateDbContext())
                {
                    return await _context.CustCoversheetValidations.FindAsync(reference);
                }
            }
            catch
            {
                logger.LogError($"Error retrieving coversheet for reference {reference}");
                throw;
            }
        }

        public async Task<CustCoversheet> GetEcover(string reference)
        {
            try
            {
                using (var _context = dbContextFactory.CreateDbContext())
                {
                    return await _context.CustCoversheets.FindAsync(reference);
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<CustCoversheet>> GetCoverSheets()
        {
            try
            {
                using (var _context = dbContextFactory.CreateDbContext())
                {

                    return await _context.CustCoversheets.AsNoTracking().Take(100).ToListAsync();

                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<CustCoversheet>> SearchCoverSheets(string idNumber)
        {
            try
            {
                using (var _context = dbContextFactory.CreateDbContext())
                {
                    return await _context.CustCoversheets.Where(c => c.TxtIdNumber == idNumber).AsNoTracking().ToListAsync();
                }
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<CustCoversheet>> GetCoverSheets(string regionName)
        {
            try
            {
                using (var _context = dbContextFactory.CreateDbContext())
                {
                    return await _context.CustCoversheets.Where(c => c.DrpdwnRegionSo == regionName).AsNoTracking().ToListAsync();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}

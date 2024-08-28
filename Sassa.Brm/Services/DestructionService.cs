using Microsoft.EntityFrameworkCore;
using Sassa.Brm.Common.Helpers;
using Sassa.BRM.Models;

namespace Sassa.BRM.Services;

public class DestructionService(IDbContextFactory<ModelContext> dbContextFactory)
{
    public async Task DestroyXlsxFile(string fileName, string columnName = "ID")
    {
        var DestroyList = XlsxHelper.ReadDestroyList(fileName, columnName);
        foreach (var item in DestroyList)
        {
            using var _context = dbContextFactory.CreateDbContext();
            {
                await _context.Database.ExecuteSqlRawAsync($"Update DC_File set Application_Status = 'DESTROY' where Applicant_no = '{item}'");
                await _context.Database.ExecuteSqlRawAsync($"Update dc_socpen set status_code = 'DESTROY' where Beneficiary_id = '{item}'");
            }
        }
    }
}

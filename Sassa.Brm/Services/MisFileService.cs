using Microsoft.EntityFrameworkCore;
using Sassa.BRM.Models;

namespace Sassa.BRM.Services;
public class MisFileService(IDbContextFactory<ModelContext> dbContextFactory)
{

    public async Task<List<MisLivelinkTbl>> GetMisFiles(string idNumber)
    {
        using var _context = dbContextFactory.CreateDbContext();
        {
            return await _context.MisLivelinkTbls.Where(x => x.IdNumber == idNumber).ToListAsync();
        }
    }
}

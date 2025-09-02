using Microsoft.EntityFrameworkCore;
using Sassa.BRM.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sassa.Audit.Services
{
    public class AuditService(IDbContextFactory<ModelContext> dbContextFactory)
    {
        public async Task<List<Inpaymentmonthly>> GetAuditMissing()
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            return await dbContext.Inpaymentsmonthly.AsNoTracking().ToListAsync();
        }

        public async Task<List<InpaymentSummary>> GetInpaymentSummaryAsync()
        {
            using var dbContext = dbContextFactory.CreateDbContext();

            var query = dbContext.InpaymentSummaries.AsNoTracking();

            return await query
                .OrderBy(s => s.RegionId)
                .ThenBy(s => s.FlagName)
                .ToListAsync();
        }

        public async Task<List<InpaymentTotal>> GetInpaymentTotalsAsync()
        {
            using var dbContext = dbContextFactory.CreateDbContext();

            var query = dbContext.InpaymentTotals.AsNoTracking();

            return await query
                .OrderBy(s => s.RegionId)
                .ToListAsync();
        }
    }
}

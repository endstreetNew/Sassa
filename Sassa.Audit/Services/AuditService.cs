using Microsoft.EntityFrameworkCore;
using Sassa.BRM.Models;
using System.Dynamic;

namespace Sassa.Audit.Services
{
    public class AuditService(IDbContextFactory<ModelContext> dbContextFactory)
    {
        public IQueryable<Inpaymentmonthly> GetAuditMissing()
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            return dbContext.Inpaymentsmonthly.AsQueryable();
        }
    }
}

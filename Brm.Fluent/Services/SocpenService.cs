//public class SocpenService(IDbContextFactory<SocpenContext> dbContextFactory)
//{
//    public async Task<List<CustRescode>> GetResCodes(string RegionId)
//    {
//        using (var spctx = dbContextFactory.CreateDbContext())
//        {
//            return await spctx.CustRescodes.Where(r => r.RegionCode == decimal.Parse(RegionId)).ToListAsync();
//        }

//    }

//    public async Task ChangeOfficeStatus(decimal rescode, string status)
//    {
//        using (var spctx = dbContextFactory.CreateDbContext())
//        {
//            CustRescode lo = await spctx.CustRescodes.Where(o => o.ResCode == rescode).FirstAsync();
//            lo.Status = status;
//            await spctx.SaveChangesAsync();
//        }
//    }
//    public async Task ChangeOfficeName(decimal rescode, string name)
//    {
//        using (var spctx = dbContextFactory.CreateDbContext())
//        {
//            CustRescode lo = await spctx.CustRescodes.Where(o => o.ResCode == rescode).FirstAsync();
//            lo.LocalOffice = name;
//            await spctx.SaveChangesAsync();
//        }

//    }
//    public async Task LinkBrmOffice(decimal rescode, decimal officeId)
//    {
//        using (var spctx = dbContextFactory.CreateDbContext())
//        {
//            var result = await spctx.CustRescodes.FindAsync(rescode);
//            if (result == null)
//            {
//                throw new Exception($"Rescode {rescode} not found");
//            }
//            CustRescode lo = result;
//            lo.OfficeId = officeId;
//            lo.Status = "A";
//            await spctx.SaveChangesAsync();
//        }
//    }


//}

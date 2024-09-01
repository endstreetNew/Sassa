using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using razor.Components.Models;
using Sassa.Brm.Common.Models;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;
using Sassa.BRM.ViewModels;

public class QueryableDataService
{

    StaticService _staticService;
    UserSession _userSession;
    BRMDbService _dbService;
    IDbContextFactory<ModelContext> _contextFactory;

    protected int operationCount;
    public QueryableDataService(IDbContextFactory<ModelContext> contextFactory, StaticService staticService, SessionService _sessionService, BRMDbService dbService)
    {
        
        _staticService = staticService;
        _userSession = _sessionService.session;
        _dbService = dbService;
        _contextFactory = contextFactory;
    }
    #region Batching
    public async Task<List<DcBatch>> GetBatches(string status)
    {

        using (var _context = _contextFactory.CreateDbContext())
        {
            //var result = new List<DcBatch>().AsQueryable();
            if (_userSession.IsRmc())
            {
                if (status == "" || status == "RMCBatch")
                {
                    //result.count = _context.DcBatches.Where(b => b.BatchStatus == "RMCBatch" && b.NoOfFiles > 0 && b.OfficeId == _userSession.Office.OfficeId).Count();
                   return await _context.DcBatches.Where(b => b.BatchStatus == "RMCBatch" && b.NoOfFiles > 0 && b.OfficeId == _userSession.Office.OfficeId).OrderByDescending(b => b.UpdatedDate).AsNoTracking().ToListAsync();
                }
                else
                {
                    List<string> regionOffices = _staticService.GetOfficeIds(_userSession.Office.RegionId);
                    //result.count = _context.DcBatches.Where(b => b.BatchStatus == status && b.NoOfFiles > 0 && regionOffices.Contains(b.OfficeId)).Count();
                    return await  _context.DcBatches.Where(b => b.BatchStatus == status && b.NoOfFiles > 0 && regionOffices.Contains(b.OfficeId)).OrderByDescending(b => b.UpdatedDate).AsNoTracking().ToListAsync();
                }
            }
            else
            {
                if (status != "")
                {
                    //result.count = _context.DcBatches.Where(b => b.BatchStatus == status && b.OfficeId == _userSession.Office.OfficeId).Count();
                    return await _context.DcBatches.Where(b => b.BatchStatus == status && b.OfficeId == _userSession.Office.OfficeId).OrderByDescending(b => b.UpdatedDate).AsNoTracking().ToListAsync();
                }
                else
                {
                    //result.count = _context.DcBatches.Where(b => b.OfficeId == _userSession.Office.OfficeId).Count();
                    return await _context.DcBatches.Where(b => b.OfficeId == _userSession.Office.OfficeId).OrderByDescending(b => b.UpdatedDate).AsNoTracking().ToListAsync();
                }
            }
        }

    }
    public async Task<List<DcBatch>> FindBatch(decimal searchBatch)
    {

        using (var _context = _contextFactory.CreateDbContext())
        {
            if (_userSession.IsRmc())
            {
                //result.count = _context.DcBatches.Where(b => b.BatchStatus == "RMCBatch" && b.NoOfFiles > 0 && b.OfficeId == _userSession.Office.OfficeId && b.BatchNo == searchBatch).Count();
                return await _context.DcBatches.Where(b => b.BatchStatus == "RMCBatch" && b.NoOfFiles > 0 && b.OfficeId == _userSession.Office.OfficeId && b.BatchNo == searchBatch).OrderByDescending(b => b.UpdatedDate).AsNoTracking().ToListAsync();
            }
            else
            {
                //result.count = _context.DcBatches.Where(b => b.OfficeId == _userSession.Office.OfficeId).Count();
                return await _context.DcBatches.Where(b => b.OfficeId == _userSession.Office.OfficeId && b.BatchNo == searchBatch).OrderByDescending(b => b.UpdatedDate).AsNoTracking().ToListAsync();
            }
        }
    }
    public async Task<List<DcBatch>> GetMyBatches(bool myBatches)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            if (myBatches)
            {
                // result.count = _context.DcBatches.Where(b => b.UpdatedByAd == _userSession.SamName).Count();
                return await  _context.DcBatches.Where(b => b.UpdatedByAd == _userSession.SamName).OrderByDescending(b => b.UpdatedDate).AsNoTracking().ToListAsync();
            }
            else
            {
                //result.count = _context.DcBatches.Where(b => b.OfficeId == _userSession.Office.OfficeId).Count();
                return await _context.DcBatches.Where(b => b.OfficeId == _userSession.Office.OfficeId).OrderByDescending(b => b.UpdatedDate).AsNoTracking().ToListAsync();
            }
        }
    }
    public async Task<List<DcFile>> GetAllFilesByBatchNoQuery(decimal batchId)
    {


        var result = await _dbService.GetAllFilesByBatchNo(batchId);
        using (var _context = _contextFactory.CreateDbContext())
        {
            //todo: Simplify
            foreach (var file in result)
            {
                var merge = await _context.DcMerges.FirstOrDefaultAsync(m => m.BrmBarcode == file.BrmBarcode);
                if (merge == null) continue;
                file.MergeStatus = merge.BrmBarcode == merge.ParentBrmBarcode ? "Parent" : "Merged";
            }
        }
        return result;
    }
    #endregion
    #region Boxing and Re-Boxing
    public async Task<List<ReboxListItem>> GetAllFilesByBoxNo(string boxNo)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            bool repaired = await _dbService.RepairAltBoxSequence(boxNo);

            var interim = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo).OrderByDescending(f => f.UpdatedDate).AsNoTracking().ToListAsync();
            return interim.OrderBy(f => f.UnqFileNo)
                        .Select(f => new ReboxListItem
                        {
                            ClmNo = f.UnqFileNo,
                            BrmNo = f.BrmBarcode,
                            IdNo = f.ApplicantNo,
                            FullName = f.FullName,
                            GrantType = StaticDataService.GrantTypes[f.GrantType],
                            BoxNo = boxNo,
                            AltBoxNo = f.AltBoxNo,
                            Scanned = f.ScanDatetime != null,
                            MiniBox = (int?)f.MiniBoxno,
                            RegType = f.ApplicationStatus,
                            TdwBatch = (int)f.TdwBatch
                        }).ToList();
        }
    }
    public async Task<List<ReboxListItem>> SearchBox(string boxNo, string searchText)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            return await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo && (bn.ApplicantNo.Contains(searchText) || bn.BrmBarcode.Contains(searchText))).OrderByDescending(f => f.UpdatedDate).AsNoTracking()
                        .Select(f => new ReboxListItem
                        {
                            ClmNo = f.UnqFileNo,
                            BrmNo = f.BrmBarcode,
                            IdNo = f.ApplicantNo,
                            FullName = f.FullName,
                            GrantType = StaticDataService.GrantTypes[f.GrantType],
                            BoxNo = boxNo,
                            AltBoxNo = f.AltBoxNo,
                            Scanned = f.ScanDatetime != null,
                            MiniBox = (int?)f.MiniBoxno,
                            TdwBatch = (int)f.TdwBatch
                        }).ToListAsync();
        }
    }
    #endregion
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using razor.Components.Models;
using Sassa.Brm.Common.Models;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Data.ViewModels;
using Sassa.BRM.Models;
using Sassa.BRM.ViewModels;

public class QueryableDataService(IDbContextFactory<ModelContext> _contextFactory, StaticService _staticService, SessionService _sessionService, BRMDbService _dbService, MailMessages _mail, RawSqlService _raw)
{

    #region Batching
    public async Task<List<DcBatch>> GetBatches(string status)
    {

        using (var _context = _contextFactory.CreateDbContext())
        {
            //var result = new List<DcBatch>().AsQueryable();
            if (_sessionService.session.IsRmc())
            {
                if (status == "" || status == "RMCBatch")
                {
                    //string.IsNullOrEmpty(b.BoxNo) &&
                    return await _context.DcBatches.Where( b =>  b.BatchStatus == "RMCBatch" && b.NoOfFiles > 0 && b.OfficeId == _sessionService.session.Office.OfficeId).AsNoTracking().ToListAsync();
                }
                else
                {
                    List<string> regionOffices = _staticService.GetOfficeIds(_sessionService.session.Office.RegionId);
                    //string.IsNullOrEmpty(b.BoxNo) &&
                    return await  _context.DcBatches.Where(b =>  b.BatchStatus == status && b.NoOfFiles > 0 && regionOffices.Contains(b.OfficeId)).AsNoTracking().ToListAsync();
                }
            }
            else
            {
                if (status != "")
                {
                    //result.count = _context.DcBatches.Where(b => b.BatchStatus == status && b.OfficeId == _sessionService.session.Office.OfficeId).Count();
                    return await _context.DcBatches.Where(b => b.BatchStatus == status && b.OfficeId == _sessionService.session.Office.OfficeId).AsNoTracking().ToListAsync();
                }
                else
                {
                    //result.count = _context.DcBatches.Where(b => b.OfficeId == _sessionService.session.Office.OfficeId).Count();
                    return await _context.DcBatches.Where(b => b.OfficeId == _sessionService.session.Office.OfficeId).AsNoTracking().ToListAsync();
                }
            }
        }

    }
    public async Task<List<DcBatch>> FindBatch(decimal searchBatch)
    {

        using (var _context = _contextFactory.CreateDbContext())
        {
            if (_sessionService.session.IsRmc())
            {
                //result.count = _context.DcBatches.Where(b => b.BatchStatus == "RMCBatch" && b.NoOfFiles > 0 && b.OfficeId == _sessionService.session.Office.OfficeId && b.BatchNo == searchBatch).Count();
                return await _context.DcBatches.Where(b => b.BatchStatus == "RMCBatch" && b.NoOfFiles > 0 && b.OfficeId == _sessionService.session.Office.OfficeId && b.BatchNo == searchBatch).AsNoTracking().ToListAsync();
            }
            else
            {
                //result.count = _context.DcBatches.Where(b => b.OfficeId == _sessionService.session.Office.OfficeId).Count();
                return await _context.DcBatches.Where(b => b.OfficeId == _sessionService.session.Office.OfficeId && b.BatchNo == searchBatch).AsNoTracking().ToListAsync();
            }
        }
    }
    public async Task<List<DcBatch>> GetMyBatches(bool myBatches)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            if (myBatches)
            {
                // result.count = _context.DcBatches.Where(b => b.UpdatedByAd == _sessionService.session.SamName).Count();
                return await  _context.DcBatches.Where(b => b.UpdatedByAd == _sessionService.session.SamName).AsNoTracking().ToListAsync();
            }
            else
            {
                //result.count = _context.DcBatches.Where(b => b.OfficeId == _sessionService.session.Office.OfficeId).Count();
                return await _context.DcBatches.Where(b => b.OfficeId == _sessionService.session.Office.OfficeId).AsNoTracking().ToListAsync();
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
                file.SetMergeStatus(merge.BrmBarcode == merge.ParentBrmBarcode ? "Parent" : "Merged");
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
            //bool repaired = await _dbService.RepairAltBoxSequence(boxNo);

            var interim = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo).AsNoTracking().ToListAsync();//OrderByDescending(f => f.UpdatedDate)
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
                            ScanDate= f.ScanDatetime,
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
            return await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo && (bn.ApplicantNo == searchText) || (bn.BrmBarcode == searchText)).AsNoTracking()
                        .Select(f => new ReboxListItem
                        {
                            ClmNo = f.UnqFileNo,
                            BrmNo = f.BrmBarcode,
                            IdNo = f.ApplicantNo,
                            FullName = f.FullName,
                            GrantType = StaticDataService.GrantTypes[f.GrantType],
                            BoxNo = boxNo,
                            AltBoxNo = f.AltBoxNo,
                            ScanDate = f.ScanDatetime,
                            MiniBox = (int?)f.MiniBoxno,
                            TdwBatch = (int)f.TdwBatch
                        }).ToListAsync();
        }
    }
    #endregion
    #region TdwBatching
    public async Task<List<TdwBatchViewModel>> GetBox(string boxNo)
    {
        try
        {
            using (var _context = _contextFactory.CreateDbContext())
            {

                List<DcFile> allFiles = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo.ToString()).ToListAsync();
                List<TdwBatchViewModel> result = new();
                if (!allFiles.Any()) return result;
                foreach (var box in allFiles.Select(f => f.TdwBoxno).Distinct().ToList())
                {
                    var dcFiles = allFiles.Where(f => f.TdwBoxno == box).ToList();
                    result.Add(
                   new TdwBatchViewModel
                   {
                       BoxNo = box,
                       Region = _staticService.GetRegion(_sessionService.session.Office.RegionId!),
                       MiniBoxes = (int)dcFiles.Sum(f => f.MiniBoxno ?? 1),
                       Files = dcFiles.Count(),
                       User = _sessionService.session.SamName,
                       TdwSendDate = dcFiles.First().TdwBatchDate,
                       IsLocked = dcFiles.First().BoxLocked == 1 ? true : false
                   });
                }
                return result;
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }

    public async Task<List<TdwBatchViewModel>> GetAllBoxes( ReportPeriod period)
    {


        try
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                List<DcFile> allFiles = await _context.DcFiles.Where(bn => bn.TdwBatch == 0 && bn.RegionId == _sessionService.session.Office.RegionId && bn.ApplicationStatus.Contains("LC") && !string.IsNullOrEmpty(bn.TdwBoxno) && (bn.UpdatedDate < period.ToDate && bn.UpdatedDate > period.FromDate)).ToListAsync();
                List<TdwBatchViewModel> result = new();

                if (!allFiles.Any()) return result;


                foreach (var box in allFiles.Select(f => f.TdwBoxno).Distinct().ToList())
                {
                    var dcFiles = allFiles.Where(f => f.TdwBoxno == box).ToList();
                    result.Add(
                   new TdwBatchViewModel
                   {
                       BoxNo = box,
                       Region = _staticService.GetRegion(_sessionService.session.Office.RegionId!),
                       MiniBoxes = (int)dcFiles.Sum(f => f.MiniBoxno ?? 0),
                       Files = dcFiles.Count(),
                       User = _sessionService.session.SamName,
                       TdwSendDate = dcFiles.First().TdwBatchDate,
                       IsLocked = dcFiles.First().BoxLocked == 1 ? true : false
                   });
                }
                return result;
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }

    public async Task<List<TdwBatchViewModel>> GetTdwBatches( ReportPeriod period)
    {
        try
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                List<DcFile> allFiles = await _context.DcFiles.Where(bn => bn.RegionId == _sessionService.session.Office.RegionId && !string.IsNullOrEmpty(bn.TdwBoxno) && bn.TdwBatch != 0 && (bn.UpdatedDate < period.ToDate && bn.UpdatedDate > period.FromDate)).AsNoTracking().ToListAsync();
                List<TdwBatchViewModel> result = new();
                List<DcFile> batchFiles = new List<DcFile>();
                foreach (var batch in allFiles.Select(f => f.TdwBatch).Distinct().ToList())
                {
                    var dcFiles = allFiles.Where(f => f.TdwBatch == batch).ToList();
                    result.Add(
                   new TdwBatchViewModel
                   {
                       TdwBatchNo = (int)batch,
                       Region = _staticService.GetRegion(_sessionService.session.Office!.RegionId!),
                       Boxes = dcFiles.Select(a => a.TdwBoxno).Distinct().Count(),
                       Files = dcFiles.Count(),
                       User = dcFiles.First().UpdatedByAd,
                       TdwSendDate = dcFiles.First().TdwBatchDate
                   });
                }
                return result;
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }


    }
    /// <summary>
    /// Get TDW batch and send mail
    /// </summary>
    /// <param name="boxes"></param>
    /// <returns></returns>
    public async Task<List<TdwBatchViewModel>> GetTdwBatch(int tdwBatchno)
    {
        List<TdwBatchViewModel> boxes = new List<TdwBatchViewModel>();
        using (var _context = _contextFactory.CreateDbContext())
        {
            var dcFiles = await _context.DcFiles
                .Where(bn => bn.TdwBatch == tdwBatchno)
                .AsNoTracking()
                .ToListAsync();

            var boxs = dcFiles
                .GroupBy(t => t.TdwBoxno)
                .Select(grp => grp.First())
                .ToList();

            foreach (var box in boxs)
            {
                var boxFiles = dcFiles
                    .Where(f => f.TdwBoxno == box.TdwBoxno)
                    .ToList();

                boxes.Add(new TdwBatchViewModel
                {
                    BoxNo = box.TdwBoxno,
                    Region = _sessionService.session.Office.RegionName,
                    MiniBoxes = (int)boxFiles.Max(f => f.MiniBoxno ?? 0),
                    Files = boxFiles.Count,
                    User = _sessionService.session.SamName,
                    TdwSendDate = boxFiles.Max(f => f.TdwBatchDate)
                });
            }
        }
        return boxes;
    }
    /// <summary>
    /// Create TDW batch and send mail
    /// </summary>
    /// <param name="boxes"></param>
    /// <returns></returns>
    public async Task<int> CreateTdwBatch(List<TdwBatchViewModel> boxes)
    {
        int tdwBatch = await _raw.GetNextTdwBatch();
        using (var _context = _contextFactory.CreateDbContext())
        {
            foreach (var box in boxes.Where(b => !string.IsNullOrEmpty(b.BoxNo)))
            {
                box.TdwSendDate = DateTime.Now;
                box.TdwBatchNo = tdwBatch;
                box.User = _sessionService.session.SamName;
                await _context.DcFiles.Where(f => f.TdwBoxno == box.BoxNo).ForEachAsync(f => { f.TdwBatch = tdwBatch; f.TdwBatchDate = box.TdwSendDate; f.BoxLocked = 1; });
            }
            await _context.SaveChangesAsync();
            await SendTDWBulkReturnedMail(tdwBatch);
        }
        return tdwBatch;
    }
    /// <summary>
    /// New TDW Batch feature
    /// </summary>
    /// <param name="tdwBatchNo"></param>
    /// <returns></returns>
    public async Task SendTDWBulkReturnedMail(int tdwBatchNo)
    {
        List<TDWRequestMain> tpl = new List<TDWRequestMain>();
        List<DcFile> parentlist;

        TDWRequestMain TdwFormat;
        List<string> files;
        using (var _context = _contextFactory.CreateDbContext())
        {
            foreach (string boxNo in await _context.DcFiles.Where(bn => bn.TdwBatch == tdwBatchNo).Select(b => b.TdwBoxno).Distinct().AsNoTracking().ToListAsync())
            {

                parentlist = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo).AsNoTracking().ToListAsync();
                foreach (DcFile parent in parentlist)
                {
                    TdwFormat = new TDWRequestMain
                    {
                        BRM_No = parent.BrmBarcode,
                        CLM_No = parent.UnqFileNo,
                        Folder_ID = parent.UnqFileNo,
                        Grant_Type = parent.GrantType,
                        Firstname = parent.UserFirstname,
                        Surname = parent.UserLastname,
                        ID_Number = parent.ApplicantNo,
                        Year = (parent.UpdatedDate ?? DateTime.Now).ToString("YYYY"),
                        Location = parent.TdwBoxno,
                        Reg = parent.RegType,
                        //Bin  = parent. ,
                        Box = parent.MiniBoxno.ToString(),
                        //Pos  = parent. ,
                        UserPicked = ""
                    };
                    tpl.Add(TdwFormat);
                }

            }
            string FileName = _sessionService.session.Office.RegionCode + "-" + _sessionService.session.SamName!.ToUpper() + $"-TDW_ReturnedBatch_{tdwBatchNo}-" + DateTime.Now.ToShortDateString().Replace("/", "-") + "-" + DateTime.Now.ToShortTimeString().Replace(":", "-");
            //attachment list
            files = new List<string>();
            //write attachments for manual download/add to mail
            File.WriteAllText(StaticDataService.ReportFolder + $@"{FileName}.csv", tpl.CreateCSV());
            files.Add(StaticDataService.ReportFolder + $@"{FileName}.csv");
        }
        //send mail to TDW
        try
        {
            //if (!Environment.MachineName.ToLower().Contains("prod")) return;
            _mail.SendTDWIncoming(_sessionService.session, tdwBatchNo, files);
        }
        catch
        {
            //ignore confirmation errors
        }
    }
    #endregion
    #region FileRequests
    public async Task<List<DcFileRequest>> GetFileRequests(bool filterUser, bool filterOffice, string statusFilter = "", string reasonFilter = "")
    {
        List<DcFileRequest> result = new List<DcFileRequest>();
        using (var _context = _contextFactory.CreateDbContext())
        {
            var query = _context.DcFileRequests.AsQueryable();

            if (filterUser)
            {
                query = query.Where(r => r.RequestedByAd == _sessionService.session.SamName);
            }
            if (filterOffice)
            {
                if (_sessionService.session.IsRmc())
                {
                    query = query.Where(r => r.RegionId == _sessionService.session.Office.RegionId);
                }
                else
                {
                    query = query.Where(r => r.RequestedOfficeId == _sessionService.session.Office.OfficeId);
                }
            }
            if (!string.IsNullOrEmpty(reasonFilter))
            {
                query = query.Where(r => r.ReqCategoryType.ToString() == reasonFilter);
            }
            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(r => r.Status == statusFilter);
            }

            //var reversed = query.AsEnumerable().Reverse();
            result = await query.Take(200).ToListAsync();
            foreach (var req in result)
            {
                try
                {
                    req.Reason = StaticDataService.RequestCategoryTypes.Where(r => r.TypeId == req.ReqCategoryType).First().TypeDescr;
                }
                catch (Exception ex)
                {
                    var ss = ex.Message;
                }
            }
            return result;
        }
    }

    public async Task<List<DcPicklist>> SearchPickLists(string searchTxt)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            var query = _context.DcPicklists.Where(r => r.UnqPicklist.ToLower().Contains(searchTxt.ToLower())).AsQueryable();
            return await query.Where(r => r.RegionId == _sessionService.session.Office.RegionId).AsNoTracking().ToListAsync();

        }
    }

    public async Task<List<DcPicklist>> GetPickLists(bool filterRequestUser, bool filterInProgress)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            List<DcPicklist> result = new();

            var query = _context.DcPicklists.AsQueryable();

            if (filterRequestUser)
            {
                query = query.Where(r => r.RequestedByAd.ToLower() == _sessionService.session.SamName.ToLower());
            }
            else if (filterInProgress)
            {
                query = query.Where(r => r.Status != "Returned");
            }
            else
            {
                query = query.Where(r => r.RegionId == _sessionService.session.Office.RegionId);
            }


            return await query.Where(r => r.RegionId == _sessionService.session.Office.RegionId).AsNoTracking().ToListAsync();

        }
    }

    internal async Task<List<DcPicklistItem>> GetPicklistItems(string unq_picklist)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            return await _context.DcPicklistItems.Where(p => p.UnqPicklist == unq_picklist).AsNoTracking().ToListAsync();

        }
    }

    public async Task ChangePickListStatus(DcPicklist pi)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            DcPicklist? pl = await _context.DcPicklists.FindAsync(pi.UnqPicklist);
            if (pl == null) throw new Exception($"Picklist {pi.UnqPicklist} not found.");
            pl.Status = pi.nextStatus;
            await _context.SaveChangesAsync();
        }
    }
    #endregion

    #region Destruction
    public async Task<List<DcExclusion>> getExclusions()
    {

        using (var _context = _contextFactory.CreateDbContext())
        {

            return await _context.DcExclusions.Where(p => p.RegionId == decimal.Parse(_sessionService.session.Office.RegionId)).AsNoTracking().ToListAsync();

        }
    }

    public async Task<List<DcExclusionBatch>> GetExclusionBatches(string year)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            return await _context.DcExclusionBatches.OrderByDescending(d => d.BatchId).Where(p => p.ExclusionYear == year).AsNoTracking().ToListAsync();
        }
    }

    public async Task<List<DcExclusionBatch>> GetApprovedBatches(string year)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {

            return await _context.DcExclusionBatches.OrderByDescending(e => e.BatchId).Where(p => p.ExclusionYear == year && !string.IsNullOrEmpty(p.ApprovedBy)).AsNoTracking().ToListAsync();
        }
    }
    #endregion
}

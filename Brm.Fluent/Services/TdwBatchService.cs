using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore;
using razor.Components.Models;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Models;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Data.ViewModels;
using Sassa.BRM.Models;
using Sassa.BRM.ViewModels;
using System.Diagnostics;


public class TdwBatchService(IDbContextFactory<ModelContext> _contextFactory, StaticService _staticService, SessionService session, RawSqlService _raw, MailMessages _mail,LoggingService logger)
{

    UserSession _session = session.session!;//SessionService must be loaded before this service

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
                       Region = _staticService.GetRegion(_session.Office.RegionId!),
                       MiniBoxes = (int)dcFiles.Sum(f => f.MiniBoxno ?? 1),
                       Files = dcFiles.Count(),
                       User = _session.SamName,
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

    public async Task<List<TdwBatchViewModel>> GetAllBoxes(ReportPeriod period)
    {


        try
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var allFiles = await _context.DcFiles.Where(bn => bn.TdwBatch == 0 && bn.RegionId == _session.Office.RegionId && bn.Lctype > 0 && !string.IsNullOrEmpty(bn.TdwBoxno) && (bn.UpdatedDate <= period.ToDate && bn.UpdatedDate >= period.FromDate)).ToListAsync();
                List<TdwBatchViewModel> result = new();
                foreach (var box in allFiles.Select(f => f.TdwBoxno).Distinct().ToList())
                {
                    var dcFiles = allFiles.Where(f => f.TdwBoxno == box).ToList();
                    result.Add(
                   new TdwBatchViewModel
                   {
                       BoxNo = box,
                       Region = _staticService.GetRegion(_session.Office.RegionId!),
                       MiniBoxes = (int)dcFiles.Sum(f => f.MiniBoxno ?? 0),
                       Files = dcFiles.Count(),
                       User = _session.SamName,
                       TdwSendDate = dcFiles.First().TdwBatchDate,
                       IsLocked = dcFiles.First().BoxLocked == 1 ? true : false
                   });
                }
                return result;
            }
        }
        catch (Exception ex)
        {
            await logger.LogToConsole(ex.Message);
            throw new Exception(ex.Message);
        }

    }

    public async Task<List<TdwBatchViewModel>> GetTdwBatches(ReportPeriod period)
    {
        try
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                List<DcFile> allFiles = await _context.DcFiles.Where(bn => bn.RegionId == _session.Office.RegionId && !string.IsNullOrEmpty(bn.TdwBoxno) && bn.TdwBatch != 0 && (bn.UpdatedDate < period.ToDate && bn.UpdatedDate > period.FromDate)).AsNoTracking().ToListAsync();
                List<TdwBatchViewModel> result = new();
                List<DcFile> batchFiles = new List<DcFile>();
                foreach (var batch in allFiles.Select(f => f.TdwBatch).Distinct().ToList())
                {
                    var dcFiles = allFiles.Where(f => f.TdwBatch == batch).ToList();
                    result.Add(
                   new TdwBatchViewModel
                   {
                       TdwBatchNo = (int)batch,
                       Region = _staticService.GetRegion(_session.Office!.RegionId!),
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

    //public async Task<PagedResult<TdwBatchViewModel>> GetBox(string boxNo)
    //{


    //    try
    //    {
    //        using (var _context = _contextFactory.CreateDbContext())
    //        {

    //            List<DcFile> allFiles = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo.ToString()).ToListAsync();
    //            PagedResult<TdwBatchViewModel> result = new PagedResult<TdwBatchViewModel>();

    //            if (!allFiles.Any()) return result;
    //            foreach (var box in allFiles.Select(f => f.TdwBoxno).Distinct().ToList())
    //            {
    //                var dcFiles = allFiles.Where(f => f.TdwBoxno == box).ToList();
    //                result.result.Add(
    //               new TdwBatchViewModel
    //               {
    //                   BoxNo = box,
    //                   Region = _staticService.GetRegion(_session.Office.RegionId!),
    //                   MiniBoxes = (int)dcFiles.Sum(f => f.MiniBoxno ?? 1),
    //                   Files = dcFiles.Count(),
    //                   User = _session.SamName,
    //                   TdwSendDate = dcFiles.First().TdwBatchDate,
    //                   IsLocked = dcFiles.First().BoxLocked == 1 ? true : false
    //               });
    //            }

    //            result.count = allFiles.Select(f => f.TdwBoxno).Distinct().Count();
    //            return result;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        throw new Exception(ex.Message);
    //    }

    //}

    //public async Task<PagedResult<TdwBatchViewModel>> GetAllBoxes(int page, ReportPeriod period)
    //{


    //    try
    //    {
    //        using (var _context = _contextFactory.CreateDbContext())
    //        {
    //            List<DcFile> allFiles = await _context.DcFiles.Where(bn => bn.TdwBatch == 0 && bn.RegionId == _session.Office.RegionId && bn.ApplicationStatus.Contains("LC") && !string.IsNullOrEmpty(bn.TdwBoxno) && (bn.UpdatedDate < period.ToDate && bn.UpdatedDate > period.FromDate)).ToListAsync();
    //            PagedResult<TdwBatchViewModel> result = new PagedResult<TdwBatchViewModel>();

    //            if (!allFiles.Any()) return result;


    //            foreach (var box in allFiles.Select(f => f.TdwBoxno).Distinct().Skip((page - 1) * 20).Take(20).ToList())
    //            {
    //                var dcFiles = allFiles.Where(f => f.TdwBoxno == box).ToList();
    //                result.result.Add(
    //               new TdwBatchViewModel
    //               {
    //                   BoxNo = box,
    //                   Region = _staticService.GetRegion(_session.Office.RegionId!),
    //                   MiniBoxes = (int)dcFiles.Sum(f => f.MiniBoxno ?? 0),
    //                   Files = dcFiles.Count(),
    //                   User = _session.SamName,
    //                   TdwSendDate = dcFiles.First().TdwBatchDate,
    //                   IsLocked = dcFiles.First().BoxLocked == 1 ? true : false
    //               });
    //            }

    //            result.count = allFiles.Select(f => f.TdwBoxno).Distinct().Count();
    //            return result;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        throw new Exception(ex.Message);
    //    }

    //}

    //public async Task<PagedResult<TdwBatchViewModel>> GetTdwBatches(int page, ReportPeriod period)
    //{
    //    try
    //    {
    //        using (var _context = _contextFactory.CreateDbContext())
    //        {
    //            List<DcFile> allFiles = await _context.DcFiles.Where(bn => bn.RegionId == _session.Office.RegionId && !string.IsNullOrEmpty(bn.TdwBoxno) && bn.TdwBatch != 0 && (bn.UpdatedDate < period.ToDate && bn.UpdatedDate > period.FromDate)).AsNoTracking().ToListAsync();
    //            PagedResult<TdwBatchViewModel> result = new PagedResult<TdwBatchViewModel>();
    //            List<DcFile> batchFiles = new List<DcFile>();
    //            foreach (var batch in allFiles.Select(f => f.TdwBatch).Distinct().Skip((page - 1) * 20).Take(20).ToList())
    //            {
    //                var dcFiles = allFiles.Where(f => f.TdwBatch == batch).ToList();
    //                result.result.Add(
    //               new TdwBatchViewModel
    //               {
    //                   TdwBatchNo = (int)batch,
    //                   Region = _staticService.GetRegion(_session.Office!.RegionId!),
    //                   Boxes = dcFiles.Select(a => a.TdwBoxno).Distinct().Count(),
    //                   Files = dcFiles.Count(),
    //                   User = dcFiles.First().UpdatedByAd,
    //                   TdwSendDate = dcFiles.First().TdwBatchDate
    //               });
    //            }

    //            result.count = allFiles.Select(f => f.TdwBatch).Distinct().Count();
    //            return result;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        throw new Exception(ex.Message);
    //    }


    //}
    ///// <summary>
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
                    Region = _session.Office.RegionName,
                    MiniBoxes = (int)boxFiles.Max(f => f.MiniBoxno ?? 0),
                    Files = boxFiles.Count,
                    User = _session.SamName,
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
                box.User = _session.SamName;
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
                        Year = ((DateTime)(parent.UpdatedDate ?? DateTime.Now)).ToString("yyyy"),
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
            string FileName = _session.Office.RegionCode + "-" + _session.SamName!.ToUpper() + $"-TDW_ReturnedBatch_{tdwBatchNo}-" + DateTime.Now.ToShortDateString().Replace("/", "-") + "-" + DateTime.Now.ToShortTimeString().Replace(":", "-");
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
            _mail.SendTDWIncoming(_session, tdwBatchNo, files);
        }
        catch
        {
            //ignore confirmation errors
        }
    }

    //public async Task UnlockBox(string boxNo)
    //{
    //    using (var _context = _contextFactory.CreateDbContext())
    //    {
    //        await _context.DcFiles.Where(f => f.TdwBoxno == boxNo).ForEachAsync(f => { f.TdwBatch = 0; f.TdwBatchDate = null; f.BoxLocked = 0; });
    //        await _context.SaveChangesAsync();
    //    }
    //}

    /// <summary>
    /// TDW Bat submit reboxing change
    /// </summary>
    /// <param name="boxNo"></param>
    /// <param name="IsOpen"></param>
    /// <returns></returns>
    public async Task<bool> OpenCloseBox(string boxNo, bool IsOpen)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            int tdwBatch = IsOpen ? 1 : 0;

            //await _context.DcFiles.Where(b => b.TdwBoxno == boxNo).ForEachAsync(f => f.TdwBatch = tdwBatch);
            await _context.DcFiles.Where(f => f.TdwBoxno == boxNo).ForEachAsync(f => { f.TdwBatch = tdwBatch; f.BoxLocked = IsOpen ? 0 : 1; });

            await _context.SaveChangesAsync();

            return !IsOpen;
        }
    }
    public void SendFile(string fileName)
    {

        string batchPart = fileName.Split('-')[2];
        string tdwBoxNo = batchPart.Substring(batchPart.IndexOf("Batch_") + 6);
        //send mail to TDW
        try
        {
            //if (!Environment.MachineName.ToLower().Contains("prod")) return;
            _mail.SendTDWIncoming(_session, tdwBoxNo, null, StaticDataService.ReportFolder + fileName);
        }
        catch (Exception ex)
        {
            //ignore confirmation errors
            Debug.WriteLine(ex.Message);
        }
    }
}

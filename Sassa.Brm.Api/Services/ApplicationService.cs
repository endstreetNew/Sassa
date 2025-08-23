//using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.EntityFrameworkCore;
using Sassa.Brm.Common.Services;
using Sassa.Brm.Common.Helpers;
using Sassa.BRM.Models;
//using Sassa.BRM.Pages.Components;
using System.Data;
using System.Diagnostics;

namespace Sassa.BRM.Api.Services;


public class ApplicationService(IDbContextFactory<ModelContext> dbContextFactory, StaticService staticService)
{

    public string ValidateApplcation(Application app)
    {
        if (app.Id.Length != 13)
        {
            return "Invalid ID.";
        }
        if (app.AppDate.ToDate("dd/MMM/yy") > DateTime.Now)
        {
            return "Invalid Application Date. Format : dd/MMM/yy.";
        }
        if (string.IsNullOrEmpty(app.LcType.Trim('0')) && app.AppStatus.ToLower().Contains("lc"))
        {
            return "LC status without LcType.";
        }
        if (!string.IsNullOrEmpty(app.LcType.Trim('0')) && !app.AppStatus.ToLower().Contains("lc"))
        {
            return "LcType specified without LC status.";
        }
        if(!"LC-MAIN|LC-ARCHIVE|MAIN|ARCHIVE".Contains(app.AppStatus))
        {
            return "Invalid Application Status.";
        }
        //Ensure insert and update is possible
        if (string.IsNullOrEmpty(app.Clm_No))
        {
            app.Clm_No = "";
        }
        else
        {
            if (app.Clm_No.Length != 12)
            {
                return "Invalid Clm No.";
            }
        }
        if (string.IsNullOrEmpty(app.Brm_BarCode) || app.Brm_BarCode.Length != 8)
        {
            return "Invalid Barcode.";
        }
        //Grant specific validations
        switch (app.GrantType)
        {
            case "C":
            case "9":
            case "5":
                if (string.IsNullOrEmpty(app.ChildId))
                {
                    return "A Child ID is required for this application.";
                }
                if (app.ChildId.Length != 13)
                {
                    return "Invalid Child ID.";
                }
                if (!string.IsNullOrEmpty(app.Srd_No))
                {
                    return "Only Srd Can have Srd No.";
                }
                break;
            case "S":
                if (string.IsNullOrEmpty(app.Srd_No))
                {
                    return "A Srd No is required for this application.";
                }
                if (!app.Srd_No.Substring(1).IsNumeric())
                {
                    return "Invalid Srd No.";
                }
                if (!string.IsNullOrEmpty(app.ChildId))
                {
                    return "Only a child grant can have a child Id.";
                }
                break;
            default:
                if (!"0|1|3|7|8|4|6|S".Contains(app.GrantType))
                {
                    return "Invalid Grant Type.";
                }
                if (!string.IsNullOrEmpty(app.Srd_No))
                {
                    return "Only Srd Can have Srd No.";
                }
                if (!string.IsNullOrEmpty(app.ChildId))
                {
                    return "Only a child grant can have a child Id.";
                }
                break;
        }

        return "";
    }
    #region BRM Records
    public async Task<DcFile> ValidateApiAndInsert(Application application, string reason)
    {
        while (!staticService.IsInitialized) { }

        using (var _context = dbContextFactory.CreateDbContext())
        {
            try
            {


                //Kofax/LO specific validations
                DcLocalOffice office = new DcLocalOffice();
                try
                {
                    if (application.Clm_No.Length == 12)
                    {
                        application.RegionCode = application.Clm_No.Substring(0, 3);
                        application.RegionId = StaticDataService.Regions.Where(r => r.RegionCode == application.RegionCode).First().RegionId;
                        application.OfficeId = StaticDataService.LocalOffices.Where(o => o.RegionId == application.RegionId && o.OfficeType == "RMC").First().OfficeId;
                    }
                    office = StaticDataService.LocalOffices.Where(o => o.OfficeId == application.OfficeId).First();
                    application.RegionId = office.RegionId;

                }
                catch
                {
                    throw new Exception("Invalid Office.");
                }
                if (office.ManualBatch == "A" || application.Clm_No.Length == 12)
                {
                    application.BatchNo = 0;
                }
                else
                {
                    throw new Exception("Manual batching not set for this office.");
                }

                if (application.Clm_No.Length == 12)
                {
                    //BrmRecord Exists
                    return await ScanBRM(application, reason);
                }

                if (await _context.DcFiles.Where(f => f.BrmBarcode == application.Brm_BarCode).CountAsync() > 0)
                {
                    throw new Exception("Duplicate Barcode.");
                }
                return await CreateBRM(application, reason);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
    /// <summary>
    /// The regionID is required 
    /// </summary>
    /// <param name="application"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public async Task<DcFile> CreateBRM(Application application, string reason)
    {

        await RemoveBRM(application.Brm_BarCode, reason);

        DcFile file;
        using (var _context = dbContextFactory.CreateDbContext())
        {
            try
            {

                file = new DcFile()
                {
                    UnqFileNo = application.Clm_No,
                    ApplicantNo = application.Id,
                    BrmBarcode = application.Brm_BarCode,
                    BatchAddDate = DateTime.Now,
                    TransType = application.TRANS_TYPE,
                    BatchNo = application.BatchNo,
                    GrantType = application.GrantType,
                    OfficeId = application.OfficeId,
                    RegionId = application.RegionId,
                    FspId = application.FspId,
                    DocsPresent = application.DocsPresent,
                    UpdatedDate = DateTime.Now,
                    UserFirstname = application.Name,
                    UserLastname = application.SurName,
                    ApplicationStatus = application.AppStatus,
                    TransDate = application.AppDate.ToDate("dd/MMM/yy"),
                    SrdNo = application.Srd_No,
                    ChildIdNo = application.ChildId,
                    Isreview = application.TRANS_TYPE == 2 ? "Y" : "N",
                    Lastreviewdate = application.LastReviewDate.ToDate("dd/MMM/yy"),
                    ArchiveYear = application.AppStatus.Contains("ARCHIVE") ? application.ARCHIVE_YEAR : null,
                    Lctype = string.IsNullOrEmpty(application.LcType.Trim('0')) ? null : (Decimal?)Decimal.Parse(application.LcType),
                    TdwBoxno = application.TDW_BOXNO,
                    MiniBoxno = application.MiniBox,
                    FileComment = reason,
                    UpdatedByAd = application.BrmUserName,
                    TdwBatch = 0
                };
                _context.DcFiles.Add(file);
                await _context.SaveChangesAsync();
            }
            catch 
            {
                throw;
            }
            file = _context.DcFiles.Where(k => k.BrmBarcode == application.Brm_BarCode).FirstOrDefault()!;
            SaveActivity("Capture", file.SrdNo, file.Lctype, "API Insert", file.RegionId, decimal.Parse(file.OfficeId), file.UpdatedByAd, file.UnqFileNo);
            DcSocpen dc_socpen;
            long? srd = null;

            if (application.Srd_No != null && application.Srd_No.IsNumeric())
            {
                srd = long.Parse(application.Srd_No);
            }

            //Remove existing Barcode for this id/grant from dc_socpen
            _context.DcSocpens.Where(s => s.BrmBarcode == application.Brm_BarCode).ToList().ForEach(s => s.BrmBarcode = null);
            await _context.SaveChangesAsync();
            var result = new List<DcSocpen>();
            if ("C95".Contains(application.GrantType) )//child Grant
            {
                result = await _context.DcSocpens.Where(s => s.BeneficiaryId == application.Id && s.GrantType == application.GrantType && s.ChildId == application.ChildId).ToListAsync();
            }
            else
            {
                result = await _context.DcSocpens.Where(s => s.BeneficiaryId == application.Id && s.GrantType == application.GrantType && s.SrdNo == srd).ToListAsync();
            }

            if (result.ToList().Any())
            {
                dc_socpen = result.First();
                dc_socpen.CaptureReference = file.UnqFileNo;
                dc_socpen.BrmBarcode = file.BrmBarcode;
                dc_socpen.CaptureDate = DateTime.Now;
                dc_socpen.RegionId = file.RegionId;
                dc_socpen.LocalofficeId = file.RegionId;
                dc_socpen.StatusCode = application.AppStatus.Contains("MAIN") ? "ACTIVE" : "INACTIVE";
                dc_socpen.ApplicationDate = application.AppDate.ToDate("dd/MMM/yy");
                dc_socpen.SocpenDate = application.AppDate.ToDate("dd/MMM/yy");
            }
            else
            {
                dc_socpen = new DcSocpen();
                dc_socpen.ApplicationDate = application.AppDate.ToDate("dd/MMM/yy");
                dc_socpen.SocpenDate = application.AppDate.ToDate("dd/MMM/yy");
                dc_socpen.StatusCode = application.AppStatus.Contains("MAIN") ? "ACTIVE" : "INACTIVE";
                dc_socpen.BeneficiaryId = application.Id;
                dc_socpen.SrdNo = srd;
                dc_socpen.GrantType = application.GrantType;
                dc_socpen.ChildId = application.ChildId;
                dc_socpen.Name = application.Name;
                dc_socpen.Surname = application.SurName;
                dc_socpen.CaptureReference = file.UnqFileNo;
                dc_socpen.BrmBarcode = file.BrmBarcode;
                dc_socpen.CaptureDate = DateTime.Now;
                dc_socpen.RegionId = file.RegionId;
                dc_socpen.LocalofficeId = file.RegionId;
                dc_socpen.Documents = file.DocsPresent;
                _context.DcSocpens.Add(dc_socpen);
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch 
            {
                //SaveActivity("Capture", file.SrdNo, file.Lctype, "Error:" + ex.Message.Substring(0, 200), file.RegionId, decimal.Parse(file.OfficeId), file.UpdatedByAd, file.UnqFileNo);
                //throw;
            }
        }
        return file;
    }
    #endregion
    public async Task RemoveBRM(string brmNo, string reason)
    {
        using (var _context = dbContextFactory.CreateDbContext())
        {

            var files = await _context.DcFiles.Where(k => k.BrmBarcode == brmNo).ToListAsync();
            //int fileCount = files.Count();
            if (files.Any())
            {
                var dcfile = files.First();
                dcfile.FileComment = reason;
                await BackupDcFileEntry(dcfile.BrmBarcode);
            }
            var merges = await _context.DcMerges.Where(m => m.BrmBarcode == brmNo || m.ParentBrmBarcode == brmNo).ToListAsync();
            if (merges.Any())
            {
                _context.DcMerges.RemoveRange(merges);
            }
            if (files.Any() || merges.Any())
            {
                _context.DcFiles.RemoveRange(files);
                await _context.SaveChangesAsync();
            }

        }
    }
    /// <summary>
    /// Backup DcFile entry for removal
    /// </summary>
    /// <param name="file">Original File</param>
    public async Task BackupDcFileEntry(string BrmBarcode)
    {
        using (var _context = dbContextFactory.CreateDbContext())
        {
            var files = await _context.DcFiles.Where(k => k.BrmBarcode == BrmBarcode).ToListAsync();
            DcFileDeleted removed = new DcFileDeleted();
            foreach (var file in files)
            {
                file.UpdatedByAd = file.UpdatedByAd;
                file.UpdatedDate = System.DateTime.Now;
                try
                {
                    removed.FromDCFile(file);
                    var interim = await _context.DcFileDeleteds.Where(d => d.UnqFileNo == file.UnqFileNo).ToListAsync();
                    if (!interim.Any())
                    {
                        _context.DcFileDeleteds.Add(removed);
                        await _context.SaveChangesAsync();
                    }
                    SaveActivity("Delete", file.SrdNo, file.Lctype, "Delete BRM Record", file.RegionId, decimal.Parse(file.OfficeId), file.UpdatedByAd, file.UnqFileNo);
                }
                catch
                {
                    //throw new Exception("Error backing up file: " + ex.Message);
                }
            }

        }
    }

    /// <summary>
    /// A brm Record exists, update it with the new details.
    /// </summary>
    /// <param name="application"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public async Task<DcFile> ScanBRM(Application application, string reason)
    {
        DcFile? file = null;

        using (var _context = dbContextFactory.CreateDbContext())
        {
            try
            {
                //Get SocpenRecord
                var sprecords = await _context.DcSocpens.Where(d => d.BeneficiaryId == application.Id).ToListAsync();
                if (!sprecords.Any())
                {
                    throw new Exception("Could not lookup Beneficiary detail.");
                }
                DcSocpen sp = sprecords.First();
                //Replace previos record with this one
                await RemoveBRM(application.Brm_BarCode, reason);

                application.DocsPresent = "1";
                application.GrantType = staticService.GetGrantId(application.GrantName);
                application.Name = sp.Name;
                application.SurName = sp.Surname;

                file = _context.DcFiles.Find(application.Clm_No);
                if (file is null)
                {
                    file = new DcFile();
                    _context.DcFiles.Add(file);
                }
                file.UnqFileNo = application.Clm_No;
                file.ApplicantNo = application.Id;
                file.BrmBarcode = application.Brm_BarCode;
                file.BatchAddDate = DateTime.Now;
                file.TransType = application.TRANS_TYPE;
                file.BatchNo = application.BatchNo;
                file.GrantType = application.GrantType;
                file.OfficeId = application.OfficeId;
                file.RegionId = application.RegionId;
                file.FspId = application.FspId;
                file.DocsPresent = application.DocsPresent;
                file.UpdatedDate = DateTime.Now;
                file.UserFirstname = application.Name;
                file.UserLastname = application.SurName;
                file.ApplicationStatus = application.AppStatus;
                file.TransDate = application.AppDate.ToDate("dd/MMM/yy");
                file.SrdNo = application.Srd_No;
                file.ChildIdNo = application.ChildId;
                file.Isreview = application.TRANS_TYPE == 2 ? "Y" : "N";
                file.Lastreviewdate = application.LastReviewDate.ToDate("dd/MMM/yy");
                file.ArchiveYear = application.AppStatus.Contains("ARCHIVE") ? application.ARCHIVE_YEAR : null;
                file.Lctype = string.IsNullOrEmpty(application.LcType) || string.IsNullOrEmpty(application.LcType.Trim('0')) ? null : (Decimal?)Decimal.Parse(application.LcType);
                file.TdwBoxno = application.TDW_BOXNO;
                file.MiniBoxno = application.MiniBox;
                file.FileComment = reason;
                file.UpdatedByAd = application.BrmUserName;
                file.TdwBatch = 0;
                file.ScanDatetime = DateTime.Today;

                await _context.SaveChangesAsync();
            }
            catch 
            {
                throw;
            }
            file = _context.DcFiles.Where(k => k.BrmBarcode == application.Brm_BarCode).FirstOrDefault()!;
            SaveActivity("Capture", file.SrdNo, file.Lctype, "API Insert", file.RegionId, decimal.Parse(file.OfficeId), file.UpdatedByAd, file.UnqFileNo);
            DcSocpen dc_socpen;
            long? srd = null;

            if (application.Srd_No != null && application.Srd_No.IsNumeric())
            {
                srd = long.Parse(application.Srd_No);
            }

            //Remove existing Barcode for this id/grant from dc_socpen
            _context.DcSocpens.Where(s => s.BrmBarcode == application.Brm_BarCode).ToList().ForEach(s => s.BrmBarcode = null);
            await _context.SaveChangesAsync();
            var result = new List<DcSocpen>();
            if (("C95".Contains(application.GrantType) && application.ChildId == application.ChildId))//child Grant
            {
                result = await _context.DcSocpens.Where(s => s.BeneficiaryId == application.Id && s.GrantType == application.GrantType && s.ChildId == application.ChildId).ToListAsync();
            }
            else
            {
                result = await _context.DcSocpens.Where(s => s.BeneficiaryId == application.Id && s.GrantType == application.GrantType && s.SrdNo == srd).ToListAsync();
            }

            if (result.ToList().Any())
            {
                dc_socpen = result.First();
                dc_socpen.CaptureReference = file.UnqFileNo;
                dc_socpen.BrmBarcode = file.BrmBarcode;
                dc_socpen.CaptureDate = DateTime.Now;
                dc_socpen.RegionId = file.RegionId;
                dc_socpen.LocalofficeId = file.RegionId;
                dc_socpen.StatusCode = application.AppStatus.Contains("MAIN") ? "ACTIVE" : "INACTIVE";
                dc_socpen.ApplicationDate = application.AppDate.ToDate("dd/MMM/yy");
                dc_socpen.SocpenDate = application.AppDate.ToDate("dd/MMM/yy");
            }
            else
            {
                dc_socpen = new DcSocpen();
                dc_socpen.ApplicationDate = application.AppDate.ToDate("dd/MMM/yy");
                dc_socpen.SocpenDate = application.AppDate.ToDate("dd/MMM/yy");
                dc_socpen.StatusCode = application.AppStatus.Contains("MAIN") ? "ACTIVE" : "INACTIVE";
                dc_socpen.BeneficiaryId = application.Id;
                dc_socpen.SrdNo = srd;
                dc_socpen.GrantType = application.GrantType;
                dc_socpen.ChildId = application.ChildId;
                dc_socpen.Name = application.Name;
                dc_socpen.Surname = application.SurName;
                dc_socpen.CaptureReference = file.UnqFileNo;
                dc_socpen.BrmBarcode = file.BrmBarcode;
                dc_socpen.CaptureDate = DateTime.Now;
                dc_socpen.RegionId = file.RegionId;
                dc_socpen.LocalofficeId = file.RegionId;
                dc_socpen.Documents = file.DocsPresent;
                _context.DcSocpens.Add(dc_socpen);
            }
            dc_socpen.ScanDate = DateTime.Now;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch 
            {
                //SaveActivity("Capture", file.SrdNo, file.Lctype, "Error:" + ex.Message.Substring(0, 200), file.RegionId, decimal.Parse(file.OfficeId), file.UpdatedByAd, file.UnqFileNo);
                //throw;
            }
        }
        return file;
    }

    #region ActivityUpdate
    public void SaveActivity(string action, string srdNo, decimal? lcType, string Activity, string regionId, decimal officeId, string samName, string UniqueFileNo = "")
    {
        try
        {
            using (var _context = dbContextFactory.CreateDbContext())
            {
                string area = action + GetFileArea(srdNo, lcType);
                DcActivity activity = new DcActivity { ActivityDate = DateTime.Now, RegionId = regionId, OfficeId = officeId, Userid = 0, Username = samName, Area = area, Activity = Activity, Result = "OK", UnqFileNo = UniqueFileNo };
                _context.DcActivities.Add(activity);
                _context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
    public string GetFileArea(string srdNo, decimal? lcType)
    {
        if (!string.IsNullOrEmpty(srdNo)) return "-SRD";
        if (lcType != null) return "-LC";
        return "-File";
    }
    #endregion

}


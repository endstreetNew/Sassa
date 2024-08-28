//using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Sassa.Brm.Common.Models;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Helpers;
using Sassa.BRM.Models;
using System;

//using Sassa.BRM.Pages.Components;
using System.Data;
using System.Diagnostics;

namespace Sassa.BRM.Api.Services;


public class ApplicationService(IDbContextFactory<ModelContext> dbContextFactory)
{


    #region BRM Records
    public async Task<DcFile> ValidateApiAndInsert(Application application, string reason)
    {
        using (var _context = dbContextFactory.CreateDbContext())
        {
            try
            {
                application.ChildId = application.ChildId.Trim();
                if (application.ChildId.Length > 13)
                {
                    throw new Exception("Child ID too long.");
                }
                var office = _context.DcLocalOffices.Where(o => o.OfficeId == application.OfficeId).First();
                if (office.ManualBatch == "A")
                {
                    application.BatchNo = 0;
                }
                else
                {
                    throw new Exception("Manual batch not set for this office.");
                }

                if (StaticDataService.LocalOffices != null && StaticDataService.LocalOffices.Any())
                {
                    application.RegionId = StaticDataService.LocalOffices.Where(o => o.OfficeId == application.OfficeId).FirstOrDefault()!.RegionId;
                }
                else
                {
                    throw new Exception("Office not found");
                }
                if ((await _context.DcFiles.Where(f => f.BrmBarcode == application.Brm_BarCode).ToListAsync()).Any())
                {
                    throw new Exception("Duplicate BRM");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        return await CreateBRM(application, reason);
    }

    public bool checkBRMExists(string brmno)
    {
        using (var _context = dbContextFactory.CreateDbContext())
        {
            return _context.DcFiles.Where(f => f.BrmBarcode == brmno).ToList().Any();
        }
        //return _context.DcFiles.Where(k => k.BrmBarcode.ToLower() == brmno.ToLower()).Any();
    }
    public async Task<DcFile> CreateBRM(Application application, string reason)
    {

        await RemoveBRM(application.Brm_BarCode, reason);
        DcFile file;
        using (var _context = dbContextFactory.CreateDbContext())
        {
            try { 
            file = new DcFile()
            {
                UnqFileNo = "",
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
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating BRM: " + ex.Message);
            }
            _context.ChangeTracker.Clear();
            _context.DcFiles.Add(file);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                SaveActivity("Capture", file.SrdNo, file.Lctype, "Error:" + ex.Message.Substring(0, 200), file.RegionId, decimal.Parse(file.OfficeId), file.UpdatedByAd, file.UnqFileNo);
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
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                SaveActivity("Capture", file.SrdNo, file.Lctype, "Error:" + ex.Message.Substring(0, 200), file.RegionId, decimal.Parse(file.OfficeId), file.UpdatedByAd, file.UnqFileNo);
                //throw;
            }
        }
        return file;
    }
    public async Task RemoveBRM(string brmNo, string reason)
    {
        using (var _context = dbContextFactory.CreateDbContext())
        {
            var files = await _context.DcFiles.Where(k => k.BrmBarcode == brmNo).ToListAsync();
            //int fileCount = files.Count();
            if (files.Any())
            {
                foreach (var dcfile in files)
                {
                    //if (!deleteAll && fileCount-- == 1) continue;//leave one record
                    dcfile.FileComment = reason;
                    await BackupDcFileEntry(dcfile);
                    _context.DcFiles.Remove(dcfile);
                    SaveActivity("Delete", dcfile.SrdNo, dcfile.Lctype, "Delete BRM Record", dcfile.RegionId, decimal.Parse(dcfile.OfficeId), dcfile.UpdatedByAd, dcfile.UnqFileNo);

                }
            }
            var merges = await _context.DcMerges.Where(m => m.BrmBarcode == brmNo || m.ParentBrmBarcode == brmNo).ToListAsync();
            foreach (var merge in merges.ToList())
            {
                _context.DcMerges.Remove(merge);
            }
            if (files.Any() || merges.Any())
            {
                await _context.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// Backup DcFile entry for removal
    /// </summary>
    /// <param name="file">Original File</param>
    public async Task BackupDcFileEntry(DcFile file)
    {
        using (var _context = dbContextFactory.CreateDbContext())
        {
            DcFileDeleted removed = new DcFileDeleted();
            file.UpdatedByAd = file.UpdatedByAd;
            file.UpdatedDate = System.DateTime.Now;
            removed.FromDCFile(file);
            try
            {
                var interim = await _context.DcFileDeleteds.Where(d => d.UnqFileNo == file.UnqFileNo).ToListAsync();
                if (!interim.Any())
                {
                    _context.DcFileDeleteds.Add(removed);
                    await _context.SaveChangesAsync();
                }
            }
            catch
            {
                //throw new Exception("Error backing up file: " + ex.Message);
            }
        }
    }

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
        if (!string.IsNullOrEmpty(srdNo))return "-SRD";
        if (lcType != null) return "-LC";
        return "-File";
    }
    #endregion

}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sassa.Brm.Common.Helpers;
using Sassa.BRM.Api.Services;
using Sassa.BRM.Models;
using Sassa.BRM.ViewModels;

namespace Sassa.BRM.Controller;

[Route("[controller]")]
[ApiController]

public class DcFileController(IDbContextFactory<ModelContext> dbContextFactory, ActivityService activity) : ControllerBase
{


    //private string? lastError;
    // POST: api/Users
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<DcFile>> PostDcFile(DcFile app)
    {

        ApiResponse<DcFile> response = new ApiResponse<DcFile>();
        try
        {
            return await CreateDcFile(app);
        }
        catch (Exception ex)
        {
            response.Data = null;
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return Ok(response);

    }

    private async Task<DcFile> CreateDcFile(DcFile file)
    {
        using (var _context = dbContextFactory.CreateDbContext())
        {
            try
            {
                await RemoveBRM(file.BrmBarcode, "OverWrite selected");
                _context.DcFiles.Add(file);
                await _context.SaveChangesAsync();
                file = _context.DcFiles.Where(k => k.BrmBarcode == file.BrmBarcode).AsNoTracking().FirstOrDefault()!;
            }
            catch (Exception ex)
            {
                throw;
            }

            try
            {
                activity.SaveActivity("Capture", file.SrdNo, file.Lctype, "API Insert", file.RegionId, decimal.Parse(file.OfficeId), file.UpdatedByAd, file.UnqFileNo);
                DcSocpen dc_socpen;
                long srd = 0;
                if (file.SrdNo != null && file.SrdNo.IsNumeric())
                {
                    srd = long.Parse(file.SrdNo);
                }
                //Remove existing Barcode for this id/grant from dc_socpen
                _context.DcSocpens.Where(s => s.BrmBarcode == file.BrmBarcode).ToList().ForEach(s => s.BrmBarcode = null);
                await _context.SaveChangesAsync();
                var result = new List<DcSocpen>();
                if (("C95".Contains(file.GrantType)))//child Grant
                {
                    result = await _context.DcSocpens.Where(s => s.BeneficiaryId == file.ApplicantNo && s.GrantType == file.GrantType && s.ChildId == file.ChildIdNo).ToListAsync();
                }
                else if (srd > 0)
                {
                    result = await _context.DcSocpens.Where(s => s.GrantType == file.GrantType && s.SrdNo == srd).ToListAsync();
                }
                else
                {
                    result = await _context.DcSocpens.Where(s => s.BeneficiaryId == file.ApplicantNo && s.GrantType == file.GrantType).ToListAsync();
                }

                if (result.ToList().Any())
                {
                    dc_socpen = result.First();
                }
                else
                {
                    dc_socpen = new DcSocpen();
                    dc_socpen.BeneficiaryId = file.ApplicantNo;
                    dc_socpen.SrdNo = srd;
                    dc_socpen.GrantType = file.GrantType;
                    dc_socpen.ChildId = file.ChildIdNo ?? "";
                    dc_socpen.Name = file.UserFirstname;
                    dc_socpen.Surname = file.UserLastname;
                    dc_socpen.Documents = file.DocsPresent;
                    _context.DcSocpens.Add(dc_socpen);
                }
                dc_socpen.ApplicationDate = (DateTime)(file.TransDate ?? DateTime.Today);
                dc_socpen.SocpenDate = (DateTime)(file.TransDate ?? DateTime.Today);
                dc_socpen.CaptureReference = file.UnqFileNo;
                dc_socpen.StatusCode = file.ApplicationStatus.Contains("MAIN") ? "ACTIVE" : "INACTIVE";
                dc_socpen.BrmBarcode = file.BrmBarcode;
                dc_socpen.CaptureDate = DateTime.Now;
                dc_socpen.RegionId = file.RegionId;
                dc_socpen.LocalofficeId = file.OfficeId;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //Ignoring failed socpen update for now.
            }
        }
        return file;
    }

    private async Task RemoveBRM(string brmNo, string reason)
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
    private async Task BackupDcFileEntry(string BrmBarcode)
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
                    activity.SaveActivity("Delete", file.SrdNo, file.Lctype, "Delete BRM Record", file.RegionId, decimal.Parse(file.OfficeId), file.UpdatedByAd, file.UnqFileNo);
                }
                catch
                {
                    //throw new Exception("Error backing up file: " + ex.Message);
                }
            }

        }
    }
}

using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Sassa.Brm.Api.Helpers;
using Sassa.Brm.Common.Helpers;
using Sassa.BRM.Api.Services;
using Sassa.BRM.Models;

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
    public async Task<ActionResult<DcFile>> PostApplication(DcFile app)
    {
        DcFile result = new DcFile();

        ApiResponse<DcFile> response = new ApiResponse<DcFile>();
        try
        {
            response.Data = await CreateDcFile(app);
        }
        catch (Exception ex)
        {
            response.Data = null;
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return Ok(response);

    }

    public async Task<DcFile> CreateDcFile(DcFile file)
    {
        using (var _context = dbContextFactory.CreateDbContext())
        {
            try
            {
                _context.DcFiles.Add(file);
                var addedFile = _context.ChangeTracker.Entries<DcFile>().FirstOrDefault(x => x.State == EntityState.Added);
                await _context.SaveChangesAsync();
                if (addedFile == null) throw new Exception("Eror Saving DcFile record");
                file.UnqFileNo = addedFile.CurrentValues.GetValue<string>("UnqFileNo");
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
                    dc_socpen.ChildId = file.ChildIdNo;
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
                dc_socpen.LocalofficeId = file.RegionId;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //Ignoring failed socpen update for now.
            }
        }
        return file;
    }
}

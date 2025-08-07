using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore.Query.Internal;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Data.ViewModels;
using Sassa.BRM.Models;
using Sassa.Models;
using Sassa.Services;

namespace Sassa.Brm.Api.Services
{
    public class FasttrackService(LoService loService,CSService csService, IDbContextFactory<ModelContext> dbContextFactory, StaticService staticService)
    {
        public async Task ProcessLoRecord(FasttrackScan scanModel)
        {
            try
            {
                DcFile dcFile = await GetDcFileFromLoAsync(scanModel);
                string result = Validate(dcFile);
                if (!string.IsNullOrEmpty(result)) throw new Exception(result);
                await CheckForSocpenRecordAsync(dcFile);
                await CreateBrmRecordAsync(dcFile);
                await loService.UpdateClmNumber(scanModel.LoReferece, dcFile.UnqFileNo);
                await loService.UpdateValidation(new CustCoversheetValidation { ReferenceNum = scanModel.LoReferece, ValidationDate = DateTime.Now,Validationresult = "Ok"});
                //Todo: Add the file to contentServer
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<DcFile> GetDcFileFromLoAsync(FasttrackScan scanModel)
        {
            try
            {


                CustCoversheet coverSheet = await loService.GetCoversheetAsync(scanModel.LoReferece);
                if (!decimal.TryParse(coverSheet.DrpdwnTransaction, out decimal decTrnType))
                {
                    throw new Exception($"Invalid Transaction Type {coverSheet.DrpdwnTransaction} , expected 0,1 or 2");
                }
                var localoffice = staticService.GetLocalOfficeFromOfficeName(coverSheet.DrpdwnLocalOfficeSo);
                if (localoffice == null)
                {
                    throw new Exception($"Office {coverSheet.DrpdwnLocalOfficeSo} was not found.");
                }


                DcFile file = new DcFile()
                {
                    UnqFileNo = coverSheet.Clmnumber,
                    ApplicantNo = coverSheet.TxtIdNumber,
                    BrmBarcode = scanModel.BrmBarcode,
                    BatchAddDate = DateTime.Now,
                    TransType = decTrnType,
                    //BatchNo = application.BatchNo, Todo: set to 0 if manual batch or fail
                    GrantType = coverSheet.DrpdwnGrantTypes,
                    OfficeId = localoffice.OfficeId,
                    RegionId = localoffice.RegionId,
                    //FspId = localoffice.,
                    DocsPresent = coverSheet.DocsubmittedBrm,
                    UpdatedDate = coverSheet.ApplicationDate.ToDate("dd/MM/yyyy"),//Todo: tryparse first
                    UserFirstname = coverSheet.TxtName,
                    UserLastname = coverSheet.TxtSurname,
                    ApplicationStatus = AppStatus(coverSheet.DrpdwnLcType, coverSheet.DrpdwnAppStatus),
                    TransDate = coverSheet.ApplicationDate.ToDate("dd/MM/yyyy"),
                    SrdNo = coverSheet.TxtSrdRefNumber,
                    ChildIdNo = coverSheet.TxtIdNumberChild,
                    Isreview = decTrnType == 2 ? "Y" : "N",
                    //Lastreviewdate = application.LastReviewDate.ToDate("dd/MMM/yy"),
                    //ArchiveYear = coverSheet.DrpdwnAppStatus.Contains("ARCHIVE") ? application.ARCHIVE_YEAR : null,
                    Lctype = !coverSheet.DrpdwnLcType.IsNumeric() ? 0 : (Decimal?)Decimal.Parse(coverSheet.DrpdwnLcType),
                    //TdwBoxno = application.TDW_BOXNO,
                    //MiniBoxno = application.MiniBox,
                    FileComment = "Fasttrack API",
                    UpdatedByAd = "SVC_BRM_LO",
                    TdwBatch = 0
                };
                return await Task.FromResult(file);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string AppStatus(string lcType, string AppStatus)
        {
            if (!string.IsNullOrEmpty(lcType.Trim('0')))
            {
                if (!AppStatus.StartsWith("LC-"))
                {
                   return "LC-" + AppStatus;
                }
            }
            return AppStatus;
        }
        private string Validate(DcFile app)
        {
            try
            {
                if (app.ApplicantNo.Length != 13)
                {
                    return "Invalid ID.";
                }
                if (app.UpdatedDate > DateTime.Now)
                {
                    return "Invalid Application Date. Format : dd/MMM/yy.";
                }
                if (app.Lctype == 0 && app.ApplicationStatus.ToLower().Contains("lc"))
                {
                    return "LC status without LcType.";
                }
                if (app.Lctype > 0 && !app.ApplicationStatus.ToLower().Contains("lc"))
                {
                    return "LcType specified without LC status.";
                }
                if (!"LC-MAIN|LC-ARCHIVE|MAIN|ARCHIVE".Contains(app.ApplicationStatus))
                {
                    return "Invalid Application Status.";
                }
                //Ensure insert and update is possible
                if (string.IsNullOrEmpty(app.UnqFileNo))
                {
                    app.UnqFileNo = "";
                }
                else
                {
                    if (app.UnqFileNo.Length != 12)
                    {
                        return "Invalid Clm No.";
                    }
                }
                if (string.IsNullOrEmpty(app.BrmBarcode) || app.BrmBarcode.Length != 8)
                {
                    return "Invalid Barcode.";
                }
                //Grant specific validations
                switch (app.GrantType)
                {
                    case "C":
                    case "9":
                    case "5":
                        if (string.IsNullOrEmpty(app.ChildIdNo))
                        {
                            return "A Child ID is required for this application.";
                        }
                        if (app.ChildIdNo.Length != 13)
                        {
                            return "Invalid Child ID.";
                        }
                        if (!string.IsNullOrEmpty(app.SrdNo))
                        {
                            return "Only Srd Can have Srd No.";
                        }
                        break;
                    case "S":
                        if (string.IsNullOrEmpty(app.SrdNo))
                        {
                            return "A Srd No is required for this application.";
                        }
                        if (!app.SrdNo.Substring(1).IsNumeric())
                        {
                            return "Invalid Srd No.";
                        }
                        if (!string.IsNullOrEmpty(app.ChildIdNo))
                        {
                            return "Only a child grant can have a child Id.";
                        }
                        break;
                    default:
                        if (!"0|1|3|7|8|4|6|S".Contains(app.GrantType))
                        {
                            return "Invalid Grant Type.";
                        }
                        if (!string.IsNullOrEmpty(app.SrdNo))
                        {
                            return "Only Srd Can have Srd No.";
                        }
                        if (!string.IsNullOrEmpty(app.ChildIdNo))
                        {
                            return "Only a child grant can have a child Id.";
                        }
                        break;
                }

                return "";
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task CreateBrmRecordAsync(DcFile file)
        {
            try
            {
                using (var _context = dbContextFactory.CreateDbContext())
                {
                    if(file.UnqFileNo == "")
                    {
                        _context.DcFiles.Add(file);

                    }
                    else
                    {
                        await UpdateBrmRecord(file);
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task UpdateBrmRecord(DcFile file)
        {
            try
            {
                using (var _context = dbContextFactory.CreateDbContext())
                {
                    var existingFile = _context.DcFiles.FirstOrDefault(x => x.UnqFileNo == file.UnqFileNo);
                    if (existingFile != null)
                    {
                        _context.Entry(existingFile).CurrentValues.SetValues(file);
                    }
                    else
                    {
                        _context.DcFiles.Add(file);
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task CheckForSocpenRecordAsync(DcFile file)
        {
            try
            {
                long? srd = null;

                if (file.SrdNo != null && file.SrdNo.IsNumeric())
                {
                    srd = long.Parse(file.SrdNo);
                }
                using (var _context = dbContextFactory.CreateDbContext())
                {
                    var result = new List<DcSocpen>();
                    if ("C95".Contains(file.GrantType) )//child Grant
                    {
                        result = await _context.DcSocpens.Where(s => s.BeneficiaryId == file.ApplicantNo && s.GrantType == file.GrantType && s.ChildId == file.ChildIdNo).ToListAsync();
                    }
                    else
                    {
                        result = await _context.DcSocpens.Where(s => s.BeneficiaryId == file.ApplicantNo && s.GrantType == file.GrantType && s.SrdNo == srd).ToListAsync();
                    }

                    if (!result.Any())
                    {
                        throw new Exception($"No Socpen record found for {file.ApplicantNo} with Grant Type {file.GrantType} and Srd No {file.SrdNo}.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

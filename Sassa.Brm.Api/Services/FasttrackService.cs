using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore.Query.Internal;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Data.ViewModels;
using Sassa.BRM.Models;
using Sassa.Models;
using Sassa.Services;

namespace Sassa.BRM.Api.Services
{
    public class FasttrackService(LoService loService,CSService csService, IDbContextFactory<ModelContext> dbContextFactory, StaticService staticService)
    {
        private string scanFolder = "";
        public void SetScanFolder(string folder)
        {
            scanFolder = folder;
        }
        public async Task ProcessLoRecord(FasttrackScan scanModel)
        {
            try
            {
                var file = $@"{scanFolder}\{scanModel.LoReferece}.pdf";
                if (!File.Exists(file))
                {
                    throw new Exception($"Expected File {file} does not exist (KOFAX).");
                }
                DcFile dcFile = await GetDcFileFromLoAsync(scanModel);
                string result = Validate(dcFile);
                if (!string.IsNullOrEmpty(result)) throw new Exception(result);
                await CheckForSocpenRecordAsync(dcFile);
                await CreateBrmRecordAsync(dcFile);
                await loService.UpdateClmNumber(scanModel.LoReferece, dcFile);
                
                //Rename the file
                var newFileName = GetFilename(dcFile, staticService.GetLocalOffice(dcFile.OfficeId));
                string newFilePath = $@"{scanFolder}\" + newFileName;
                File.Move(file, newFilePath);
                string csNode = dcFile.ApplicantNo + "_" + staticService.GetGrantType(dcFile.GrantType) + "_" + dcFile.UnqFileNo;
               // pretend this one for now!
                //await csService.UploadDoc(csNode, file);
                File.Move(newFilePath, $@"{scanFolder}\Processed\{newFileName}");
            }
            catch 
            {
                throw;
            }
        }
        private string GetFilename(DcFile doc, DcLocalOffice office)
        {
            //0110040430081_Child Support Grant_KZNC31572469_UMZIMKHULU_KZN_LO.pdf
            return $"{doc.ApplicantNo}_{staticService.GetGrantType(doc.GrantType)}_{doc.UnqFileNo}_{office.OfficeName}_{staticService.GetRegionCode(office.RegionId)}_{office.OfficeType}.pdf";
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
                if (!decimal.TryParse(coverSheet.DrpdwnLcType, out decimal decLcType))
                {
                    decLcType = 0;
                }

                // Fix for CS8604: check for null before passing to GetLocalOfficeFromOfficeName
                if (string.IsNullOrEmpty(coverSheet.DrpdwnLocalOfficeSo))
                {
                    throw new Exception("Local Office name is missing.");
                }
                var localoffice = staticService.GetLocalOfficeFromOfficeName(coverSheet.DrpdwnLocalOfficeSo);
                if (localoffice == null)
                {
                    throw new Exception($"Office {coverSheet.DrpdwnLocalOfficeSo} was not found.");
                }
                if(string.IsNullOrEmpty(coverSheet.DrpdwnAppStatus))
                {
                    throw new Exception("Application Status is invalid or missing.");
                }
                if (string.IsNullOrEmpty(coverSheet.ApplicationDate))
                {
                    throw new Exception("Application Status is invalid or missing.");
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
                    UpdatedDate = coverSheet.ApplicationDate.ToDate("mm/MMM/yy"),//Todo: tryparse first
                    UserFirstname = coverSheet.TxtName,
                    UserLastname = coverSheet.TxtSurname,
                    ApplicationStatus = AppStatus(decLcType, coverSheet.DrpdwnAppStatus),
                    TransDate = coverSheet.ApplicationDate.ToDate("mm/MMM/yy"),
                    SrdNo = coverSheet.TxtSrdRefNumber,
                    ChildIdNo = coverSheet.TxtIdNumberChild,
                    Isreview = decTrnType == 2 ? "Y" : "N",
                    ScanDatetime = DateTime.Now,
                    //Lastreviewdate = application.LastReviewDate.ToDate("dd/MMM/yy"),
                    //ArchiveYear = coverSheet.DrpdwnAppStatus.Contains("ARCHIVE") ? application.ARCHIVE_YEAR : null,
                    Lctype = decLcType,
                    //TdwBoxno = application.TDW_BOXNO,
                    //MiniBoxno = application.MiniBox,
                    FileComment = "Fasttrack API",
                    UpdatedByAd = "SVC_BRM_LO",
                    TdwBatch = 0
                };
                return await Task.FromResult(file);
            }
            catch 
            {
                throw;
            }
        }

        private string AppStatus(decimal lcType, string AppStatus)
        {
            if (lcType > 0)
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
            catch 
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
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        await UpdateBrmRecord(file);
                    }
                    await _context.SaveChangesAsync();
                   // file.UnqFileNo = _context.DcFiles.Where(k => k.BrmBarcode == file.BrmBarcode).FirstOrDefault()!.UnqFileNo;
                }
            }
            catch 
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
                        file.UnqFileNo = "";
                        _context.DcFiles.Add(file);
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch 
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
            catch 
            {
                throw;
            }


        }
    }
}

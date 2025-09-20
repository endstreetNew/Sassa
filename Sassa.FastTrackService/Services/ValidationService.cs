using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Data.ViewModels;
using Sassa.BRM.Models;
using Sassa.Models;
using Sassa.Services;

namespace Sassa.Services
{
    public class ValidationService(IDbContextFactory<ModelContext> _brmContextFactory,LoService _loService,StaticService _staticService)
    {
        public bool BrmRecordExists(string barcode)
        {
            try
            {
                using (var _context = _brmContextFactory.CreateDbContext())
                {
                    var existingFile = _context.DcFiles.FirstOrDefault(x => x.BrmBarcode == barcode);
                    if (existingFile != null)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<DcFile> GetDcFileFromLoAsync(CustCoversheet coverSheet, FasttrackScan scanModel, string fileN, string fileName)
        {
            try
            {
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
                var localoffice = _staticService.GetLocalOfficeFromOfficeName(coverSheet.DrpdwnLocalOfficeSo);
                if (localoffice == null)
                {
                    throw new Exception($"Office {coverSheet.DrpdwnLocalOfficeSo} was not found.");
                }
                if (string.IsNullOrEmpty(coverSheet.DrpdwnAppStatus))
                {
                    throw new Exception("Application Status is invalid or missing.");
                }
                if (string.IsNullOrEmpty(coverSheet.ApplicationDate))
                {
                    throw new Exception("ApplicationDate is invalid or missing.");
                }

                //if (localoffice.ManualBatch == "A" || coverSheet.Clmnumber.Length == 12)
                //{
                //    application.BatchNo = 0;
                //}
                //else
                //{
                //    throw new Exception("Manual batching not set for this office.");
                //}

                DcFile file = new DcFile()
                {
                    UnqFileNo = coverSheet.Clmnumber,
                    ApplicantNo = coverSheet.TxtIdNumber,
                    BrmBarcode = scanModel.BrmBarcode,
                    BatchAddDate = DateTime.Now,
                    TransType = decTrnType,
                    BatchNo = 0,//application.BatchNo, Todo: set to 0 if manual batch or fail
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
                return await Task.FromResult(file);//Todo check BRM and clm
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
        public string Validate(DcFile app)
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
        private async Task<DcFile> CreateBrmRecordAsync(DcFile file)
        {
            try
            {
                using (var _context = _brmContextFactory.CreateDbContext())
                {
                    if (file.UnqFileNo == "")
                    {
                        _context.DcFiles.Add(file);
                        await _context.SaveChangesAsync();
                        var files = await _context.DcFiles.Where(k => k.BrmBarcode == file.BrmBarcode).ToListAsync();
                        if (files.Any())
                        {
                            return files.First();
                        }
                    }
                    else
                    {
                        var existingFile = _context.DcFiles.FirstOrDefault(x => x.UnqFileNo == file.UnqFileNo);
                        if (existingFile != null)
                        {
                            _context.Entry(existingFile).CurrentValues.SetValues(file);
                        }
                        else
                        {
                            //Check If the barcode exists
                            _context.DcFiles.Add(file);//Tested
                        }
                        await _context.SaveChangesAsync();
                    }
                    return file;
                }

            }
            catch
            {
                throw;
            }
        }
        public async Task<DcFile> CheckForSocpenRecordAsync(DcFile file, string loReference)
        {
            try
            {
                long? srd = null;

                if (file.SrdNo != null && file.SrdNo.IsNumeric())
                {
                    srd = long.Parse(file.SrdNo);
                }
                using (var _context = _brmContextFactory.CreateDbContext())
                {
                    var result = new List<DcSocpen>();
                    if ("C95".Contains(file.GrantType))//child Grant
                    {
                        result = await _context.DcSocpens.Where(s => s.BeneficiaryId == file.ApplicantNo && s.GrantType == file.GrantType && s.ChildId == file.ChildIdNo).ToListAsync();
                    }
                    else//Other grants
                    {
                        result = await _context.DcSocpens.Where(s => s.BeneficiaryId == file.ApplicantNo && s.GrantType == file.GrantType && s.SrdNo == srd).ToListAsync();
                    }

                    if (!result.Any())
                    {
                        throw new Exception($"No Socpen record found for {file.ApplicantNo} with Grant Type {file.GrantType} and Srd No {file.SrdNo}. (Retry)");
                    }
                    else
                    {
                        //Create BRM record
                        file = await CreateBrmRecordAsync(file);
                        await _loService.UpdateLOCover(loReference, file);
                        foreach (DcSocpen dc_socpen in result)
                        {
                            dc_socpen.CaptureReference = file.UnqFileNo;
                            dc_socpen.BrmBarcode = file.BrmBarcode;
                            dc_socpen.CaptureDate = DateTime.Now;
                            dc_socpen.RegionId = file.RegionId;
                            dc_socpen.LocalofficeId = file.RegionId;
                            //dc_socpen.StatusCode = file.ApplicationStatus.Contains("MAIN") ? "ACTIVE" : "INACTIVE";
                            dc_socpen.ApplicationDate = file.UpdatedDate;
                            dc_socpen.SocpenDate = file.UpdatedDate;
                            dc_socpen.LoReference = loReference;
                            dc_socpen.ScanDate = DateTime.Now;
                            dc_socpen.CsDate = DateTime.Now;

                        }
                        await _context.SaveChangesAsync();
                        return file;
                    }
                }
            }
            catch
            {
                throw;
            }


        }

        public string GetFilename(DcFile doc)
        {
            DcLocalOffice office = _staticService.GetLocalOffice(doc.OfficeId);
            //0110040430081_Child Support Grant_KZNC31572469_UMZIMKHULU_KZN_LO_LC.pdf
            string officeName = office.OfficeName.Replace("(", "_").Replace(")", "").Replace("  ", "").Replace(" ", "").Replace("/", "_").ToUpper();
            string lcTxt = doc.Lctype > 0 ? $"_LC" : "";
            var filename = $"{doc.ApplicantNo}_{_staticService.GetGrantType(doc.GrantType)}_{doc.UnqFileNo}_{officeName}_{_staticService.GetRegionCode(office.RegionId)}_{office.OfficeType}{lcTxt}.pdf";
            return SanitizeFileName(filename);
        }
        private string SanitizeFileName(string text)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                text = text.Replace(c, '_');
            return text;
        }
    }
}

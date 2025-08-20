//using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore.Query.Internal;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Data.ViewModels;
using Sassa.BRM.Models;
using Sassa.Models;
using Sassa.Services;

namespace Sassa.BRM.Services
{
    public class FasttrackService(LoService loService,CoverSheetService coverSheetService,CSService csService, IDbContextFactory<ModelContext> dbContextFactory, StaticService staticService)
    {
        public string scanFolder{ get; set; } = "";
        public async Task ProcessLoRecord(FasttrackScan scanModel,bool addCover = true)
        {
            try
            {
                bool IsRetry = await loService.ValidationExists(scanModel.LoReferece);
                if (IsRetry)
                {
                    if(string.IsNullOrEmpty(scanModel.Result))throw new Exception($"File Already Processed by KOFAX (Retry).");
                    await RetryLoRecord(scanModel, addCover);
                }
                else
                {
                    if (scanModel.LoReferece.Length != 16) throw new Exception($"Invalid LO Reference (KOFAX).");
                    if (scanModel.BrmBarcode.Length != 8) throw new Exception($"Invalid BrmBarcode (KOFAX).");
                    var file = $@"{scanFolder}\{scanModel.LoReferece}.pdf";
                    if (!File.Exists(file))
                    {
                         throw new Exception($"Expected File {file} does not exist (KOFAX).");
                    }
                    var fileInfo = new FileInfo(file);
                    long sizeInBytes = fileInfo.Length;
                    if (sizeInBytes == 0) throw new Exception($"File empty (KOFAX).");
                    if (sizeInBytes > 2000000) throw new Exception($"File too big (KOFAX).");
                    scanModel.BrmBarcode = scanModel.BrmBarcode.ToUpper();

                    DcFile dcFile = await GetDcFileFromLoAsync(scanModel);
                    string result = Validate(dcFile);
                    if (!string.IsNullOrEmpty(result)) throw new Exception(result);
                    dcFile = await CheckForSocpenRecordAsync(dcFile, scanModel.LoReferece);
                    //File is valid and ready to be processed
                    //Rename the file
                    var newFileName = GetFilename(dcFile, staticService.GetLocalOffice(dcFile.OfficeId));
                    string newFilePath = $@"{scanFolder}\" + newFileName;
                    if (addCover)
                    {
                        //Add cover sheet
                        coverSheetService.AddCoverSheetToFile(dcFile.UnqFileNo, file, newFilePath);
                    }
                    else
                    {
                        //Just move the file
                        File.Move(file, newFilePath);
                    }
                    ////build the csNode index
                    //string csNode = dcFile.ApplicantNo;
                    ////pretend this one for now!
                    //await UploadToContentserver(csNode, newFilePath);
                    //File.Move(newFilePath, $@"{scanFolder}\Processed\{newFileName}");
                }
            }
            catch 
            {
                throw;
            }
        }

        public async Task RetryLoRecord(FasttrackScan scanModel, bool addCover = false)
        {
            try
            {
                if (string.IsNullOrEmpty(scanModel.Result)) throw new Exception("Validation Result not provided for retry.");
                DcFile dcFile = await GetDcFileFromLoAsync(scanModel);
                var file = $@"{scanFolder}\{scanModel.LoReferece}.pdf";
                var newFileName = GetFilename(dcFile, staticService.GetLocalOffice(dcFile.OfficeId));
                string newFilePath = $@"{scanFolder}\" + newFileName;
                if (File.Exists(file)) //Validation has not completed
                {
                    string result = Validate(dcFile);
                    if (!string.IsNullOrEmpty(result)) throw new Exception(result);
                    //Check and update socpenrecord
                    dcFile = await CheckForSocpenRecordAsync(dcFile, scanModel.LoReferece);
                    //File is valid and ready to be processed
                    newFileName = GetFilename(dcFile, staticService.GetLocalOffice(dcFile.OfficeId));
                    newFilePath = $@"{scanFolder}\" + newFileName;
                    if (addCover)
                    {
                        //Add cover sheet
                        coverSheetService.AddCoverSheetToFile(dcFile.UnqFileNo, file, newFilePath);
                    }
                    else
                    {
                            //Just move the file
                        File.Move(file, newFilePath);
                    }
                }
                //else
                //{
                //    if (!File.Exists(newFilePath)) throw new Exception($"File {newFileName} not found for Cs Upload.");
                //}
                ////build the csNode index
                //string csNode = dcFile.ApplicantNo;
                ////Insert Contentserver file
                //await UploadToContentserver(csNode, newFilePath);
                //File.Move(newFilePath, $@"{scanFolder}\Processed\{newFileName}");
            }
            catch
            {
                throw;
            }
        }
        private string GetFilename(DcFile doc, DcLocalOffice office)
        {
            //0110040430081_Child Support Grant_KZNC31572469_UMZIMKHULU_KZN_LO_LC.pdf
            string officeName = office.OfficeName.Replace("(", "_").Replace(")", "").Replace("  ","").Replace(" ", "").Replace("/", "_").ToUpper();
            string lcTxt = doc.Lctype > 0 ? $"_LC" : "";
            return $"{doc.ApplicantNo}_{staticService.GetGrantType(doc.GrantType)}_{doc.UnqFileNo}_{officeName}_{staticService.GetRegionCode(office.RegionId)}_{office.OfficeType}{lcTxt}.pdf";
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
                    throw new Exception("ApplicationDate is invalid or missing.");
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

        private async Task<DcFile> CreateBrmRecordAsync(DcFile file)
        {
            try
            {
                using (var _context = dbContextFactory.CreateDbContext())
                {
                    if(file.UnqFileNo == "")
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

        private async Task<DcFile> CheckForSocpenRecordAsync(DcFile file, string loReference)
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
                        throw new Exception($"No Socpen record found for {file.ApplicantNo} with Grant Type {file.GrantType} and Srd No {file.SrdNo}. (Retry)");
                    }
                    else
                    {
                        //Create BRM record
                        file = await CreateBrmRecordAsync(file);
                        await loService.UpdateLOCover(loReference, file);
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

        private async Task UploadToContentserver(string csNode,string file)
        {
            // Retry strategy for UploadDoc
            const int maxRetries = 3;
            const int delayMs = 2000;
            int attempt = 0;
            Exception? lastException = null;
            while (attempt < maxRetries)
            {
                try
                {
                    await csService.UploadGrantDoc(csNode, file);
                    lastException = null;
                    break; // Success
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    attempt++;
                    if (attempt < maxRetries)
                        await Task.Delay(delayMs);
                }
            }
            if (lastException != null)
                throw new Exception($"Upload to CS failed after {maxRetries} attempts. (Retry)");

        }
    }
}

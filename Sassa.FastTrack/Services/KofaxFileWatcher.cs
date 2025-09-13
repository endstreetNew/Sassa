using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Data.ViewModels;
using Sassa.BRM.Models;
using Sassa.Models;
using Sassa.Services;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Services.Description;

namespace Sassa.BRM.Services
{
    public class KofaxFileWatcher : BackgroundService
    {
        private readonly ILogger<KofaxFileWatcher> _logger;
        private readonly string _watchDirectory;
        private readonly string _processedDirectory;
        private readonly string _rejectDirectory;
        private System.Threading.Timer? _timer;
        private readonly TimeSpan _pollInterval;
        private readonly LoService _loService;
        private readonly CoverSheetService _coverSheetService;  
        private readonly IDbContextFactory<ModelContext> _dbContextFactory;
        private readonly StaticService _staticService;

        // Re-entrancy guard (0 = idle, 1 = running)
        private int _isProcessing;
        private CancellationToken _stoppingToken;
        public KofaxFileWatcher(
            ILogger<KofaxFileWatcher> logger,
            IConfiguration config,LoService loService, CoverSheetService coverSheetService, IDbContextFactory<ModelContext> dbContextFactory, StaticService staticService)
        {
            _logger = logger;
            _watchDirectory = config.GetValue<string>($"Urls:ScanFolderRoot")!;
            _processedDirectory = Path.Combine(_watchDirectory, "Processed");
            _rejectDirectory = Path.Combine(_watchDirectory, "Rejected");
            _pollInterval = TimeSpan.FromSeconds(config.GetValue<int>("Functions:KofaxPollIntervalSeconds"));
            _loService = loService;
            _coverSheetService = coverSheetService;
            _dbContextFactory = dbContextFactory;
            _staticService = staticService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;
            try
            {
                if (!Directory.Exists(_watchDirectory)) Directory.CreateDirectory(_watchDirectory);
                if (!Directory.Exists(_processedDirectory)) Directory.CreateDirectory(_processedDirectory);
                if (!Directory.Exists(_rejectDirectory)) Directory.CreateDirectory(_rejectDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating directories");
                throw;
            }

            _timer = new System.Threading.Timer(ProcessFiles, null, TimeSpan.Zero, _pollInterval);

            return Task.CompletedTask;
        }

        private async void ProcessFiles(object? state)
        {
            // Prevent overlapping executions
            if (Interlocked.Exchange(ref _isProcessing, 1) == 1)
            {
                // Another run is still active
                return;
            }

            // Pause timer while we work (avoid piling callbacks)
            _timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            try
            {
                if (_stoppingToken.IsCancellationRequested) return;
                string[] files;
                try
                {
                    files = Directory.GetFiles(_watchDirectory, "*.pdf");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to enumerate files in {Dir}", _watchDirectory);
                    return;
                }
                if (files.Length == 0)
                {
                    // No files, stop timer until next interval
                    return;
                }

                foreach (var file in files)
                {
                    // Wait for file to be ready
                    for (int i = 0; i < 100; i++)
                    {
                        try
                        {
                            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None))
                            {
                                break;
                            }
                        }
                        catch (IOException)
                        {
                            await Task.Delay(1000);
                        }
                    }

                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var fileParts = fileName.Split('.');
                    fileName = fileName + ".pdf";
                    FasttrackScan scanModel = new FasttrackScan
                    {
                        LoReferece = fileParts[0],
                        BrmBarcode = fileParts[1].ToUpper(),
                    };

                    if (scanModel.LoReferece.Length != 16 || scanModel.BrmBarcode.Length != 8 || !scanModel.LoReferece.IsNumeric())
                    {
                        _logger.LogError("Invalid file name format: {FileName}. Expected format: <16-digit LO Reference>.<8-digit BRM Barcode>", fileName);
                        File.Move(file, Path.Combine(_rejectDirectory, "InvalidFileName." + fileName));
                        continue;
                    }
                    //Check for dupliate Barcode
                    using var brmctx = _dbContextFactory.CreateDbContext();
                    var barcodes = await brmctx.DcFiles.Where(f => f.BrmBarcode == scanModel.BrmBarcode).ToListAsync();

                    if (barcodes.Any())
                    {
                        _logger.LogError("Duplicate BRM Barcode", fileName);
                        File.Move(file, Path.Combine(_rejectDirectory, "DuplicateBarcode." + fileName));
                        continue;
                    }
                    //---------------------------------
                    CustCoversheet coverSheet = await _loService.GetCoversheetAsync(scanModel.LoReferece);
                    if (coverSheet is null)
                    {
                        _logger.LogError("LOReferenceNotFound", fileName);
                        File.Move(file, Path.Combine(_rejectDirectory, "LOReferencNotFound." + fileName));
                        continue;
                    }
                    try
                    {
                        DcFile dcFile = await GetDcFileFromLoAsync(coverSheet,scanModel, file,fileName);
                        string result = Validate(dcFile);
                        if (!string.IsNullOrEmpty(result)) throw new Exception(result);
                        dcFile = await CheckForSocpenRecordAsync(dcFile, scanModel.LoReferece);
                        //File is valid and ready to be processed
                        //Rename the file
                        var newFileName = GetFilename(dcFile, _staticService.GetLocalOffice(dcFile.OfficeId));
                        string newFilePath = $@"{_processedDirectory}\" + newFileName;
                        //Just move the file
                        if (!File.Exists(newFilePath))
                        {
                            File.Move(file, newFilePath);
                        }
                        else
                        {
                            _logger.LogInformation("File {newFilePath} already exists in processed folder, Deleting duplicate", newFilePath);
                            File.Delete(file);
                        }
                        _logger.LogInformation("Processed file {File}", file);
                        await _loService.UpdateValidation(new CustCoversheetValidation { ReferenceNum = scanModel.LoReferece, ValidationDate = DateTime.Now, Validationresult = "ok" });
                    }
                    catch (Exception ex)
                    {
                        await _loService.UpdateValidation(new CustCoversheetValidation { ReferenceNum = scanModel.LoReferece, ValidationDate = DateTime.Now, Validationresult = ex.Message });
                    }
                }
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "Error processing files in directory {Dir}", _watchDirectory);
            }
            finally
            {
                // Resume timer only if not stopping
                if (!_stoppingToken.IsCancellationRequested)
                {
                    _timer?.Change(_pollInterval, _pollInterval);
                    _logger.LogInformation("I am alive..!");
                }
                Interlocked.Exchange(ref _isProcessing, 0);
            }
        }
        private string GetFilename(DcFile doc, DcLocalOffice office)
        {
            //0110040430081_Child Support Grant_KZNC31572469_UMZIMKHULU_KZN_LO_LC.pdf
            string officeName = office.OfficeName.Replace("(", "_").Replace(")", "").Replace("  ", "").Replace(" ", "").Replace("/", "_").ToUpper();
            string lcTxt = doc.Lctype > 0 ? $"_LC" : "";
            return $"{doc.ApplicantNo}_{_staticService.GetGrantType(doc.GrantType)}_{doc.UnqFileNo}_{officeName}_{_staticService.GetRegionCode(office.RegionId)}_{office.OfficeType}{lcTxt}.pdf";
        }
        private async Task<DcFile> GetDcFileFromLoAsync(CustCoversheet coverSheet,FasttrackScan scanModel, string fileN,string fileName)
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
                using (var _context = _dbContextFactory.CreateDbContext())
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
        private async Task<DcFile> CheckForSocpenRecordAsync(DcFile file, string loReference)
        {
            try
            {
                long? srd = null;

                if (file.SrdNo != null && file.SrdNo.IsNumeric())
                {
                    srd = long.Parse(file.SrdNo);
                }
                using (var _context = _dbContextFactory.CreateDbContext())
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
    }
}

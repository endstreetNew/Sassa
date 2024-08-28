using Microsoft.EntityFrameworkCore;
using razor.Components.Models;
using Sassa.Brm.Common;
using Sassa.Brm.Common.Models;
using Sassa.Brm.Common.Services;
using Sassa.Brm.Common.Helpers;
using Sassa.BRM.Models;
using Sassa.BRM.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Sassa.Brm.Reports.Services
{
    public class BRMDbService
    {

        //ModelContext _context;
        StaticService sservice;
        RawSqlService _raw;
        SessionService _sessionService;
        MailMessages _mail;
        UserSession session;


        private readonly IDbContextFactory<ModelContext> _contextFactory;

        public BRMDbService(IDbContextFactory<ModelContext> contextFactory, StaticService staticService, RawSqlService raw, MailMessages mail, SessionService sessionService)
        {
            _contextFactory = contextFactory;
            sservice = staticService;
            _raw = raw;
            _mail = mail;
            _sessionService = sessionService;
            session = _sessionService.session!;
        }
        //public BRMDbService(ModelContext context, StaticService staticService,RawSqlService raw,  MailMessages mail,SessionService sessionService)
        //{
        //    //if (StaticD.Users == null) StaticD.Users = new List<string>();
        //    _context = context;
        //    sservice = staticService;
        //    _raw = raw;
        //    _mail = mail;
        //    session = sessionService.session;

        //    //try
        //    //{
        //    //    GetUserSession((WindowsIdentity)ctx.HttpContext.User.Identity);
        //    //    //if (!StaticD.Users.Contains(session.SamName)) StaticD.Users.Add(session.SamName);
        //    //}
        //    //catch //(Exception ex)
        //    //{
        //    //    session = null;
        //    //    //WriteEvent($"{ctx.HttpContext.User.Identity.Name} : {ex.Message}");
        //    //}
        //}





        #region BRM Records

        public async Task<bool> checkBRMExists(string brmno)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var interim = await _context.DcFiles.Where(f => f.BrmBarcode == brmno).ToListAsync();
                return interim.Any();
            }
        }


        public async Task EditBarCode(Application brm, string barCode)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                DcFile file = await _context.DcFiles.Where(d => d.BrmBarcode == brm.Brm_BarCode).FirstAsync();
                file.BrmBarcode = barCode;
                await _context.SaveChangesAsync();
                CreateActivity("Update" + GetFileArea(file.SrdNo, file.Lctype), "Update BRM Barcode", file.UnqFileNo);
            }

        }

        public async Task<DcFile> CreateBRM(Application application, string reason)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                //Removes all duplicates
                await RemoveBRM(application.Brm_BarCode, reason);
                decimal? batch = null;
                var office = _context.DcLocalOffices.Where(o => o.OfficeId == application.OfficeId).First();
                if (office.ManualBatch == "A")
                {
                    batch = 0;
                }
                else
                {
                    string batchType = application.Id.StartsWith("S") ? "SrdNoId" : application.AppStatus;
                    batch = string.IsNullOrEmpty(application.TDW_BOXNO) ? await CreateBatchForUser(batchType) : 0;
                }

                DcFile file = new DcFile()
                {
                    UnqFileNo = "",
                    ApplicantNo = application.Id,
                    BrmBarcode = application.Brm_BarCode,
                    BatchAddDate = DateTime.Now,
                    TransType = application.TRANS_TYPE,
                    BatchNo = batch,
                    GrantType = application.GrantType,
                    OfficeId = session.Office.OfficeId,
                    RegionId = session.Office.RegionId,
                    FspId = session.Office.FspId,
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
                    UpdatedByAd = session.SamName,
                    TdwBatch = 0
                };
                _context.ChangeTracker.Clear();
                _context.DcFiles.Add(file);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    CreateActivity("Capture" + GetFileArea(file.SrdNo, file.Lctype), "Error:" + ex.Message.Substring(0, 200), file.UnqFileNo);
                    throw;
                }

                //if (_context.DcBrmGrants.Where(g => g.ApplicantNo == file.ApplicantNo && g.GrantType == file.GrantType).Any())
                //{

                //    DcBrmGrant progress = new DcBrmGrant { ApplicantNo = file.ApplicantNo, GrantType = file.GrantType, BrmBarcode = file.BrmBarcode, CaptureDate = DateTime.Now };
                //    _context.DcBrmGrants.Add(progress);
                //    await _context.SaveChangesAsync();
                //    string sql = $"UPDATE SASSA.SOCPEN_PERSONAL_GRANTS S SET BRM_Reference = '{file.BrmBarcode}', File_DATE = TO_DATE('{DateTime.Now.ToString("yyyy-MM-dd")}','YYYY-MM-DD') where S.PENSION_NO = '{file.ApplicantNo}' AND S.GRANT_TYPE = '{file.GrantType}' ";
                //    _raw.ExecuteNonQuery(sql);
                //}

                file = _context.DcFiles.Where(k => k.BrmBarcode == application.Brm_BarCode).FirstOrDefault();
                //await SetBatchCount((decimal)file.BatchNo);
                CreateActivity("Capture" + GetFileArea(file.SrdNo, file.Lctype), "Print Coversheet", file.UnqFileNo);
                DcSocpen dc_socpen;
                //if (application.SocpenIsn > 0)
                //{
                //    dc_socpen = await _context.DcSocpen.FindAsync(application.SocpenIsn);
                //    dc_socpen.CaptureReference = file.UnqFileNo;
                //    dc_socpen.BrmBarcode = file.BrmBarcode;
                //    dc_socpen.CaptureDate = DateTime.Now;
                //    dc_socpen.RegionId = session.Office.RegionId;
                //    dc_socpen.LocalofficeId = session.Office.OfficeId;

                //}
                //else
                //{
                long? srd;
                try
                {
                    srd = long.Parse(application.Srd_No);
                }
                catch
                {
                    srd = null;
                }
                //Remove existing Barcode for this id/grant
                _context.DcSocpen.Where(s => s.BrmBarcode == application.Brm_BarCode).ToList().ForEach(s => s.BrmBarcode = null);
                await _context.SaveChangesAsync();

                var result = await _context.DcSocpen.Where(s => s.BeneficiaryId == application.Id && s.GrantType == application.GrantType && s.SrdNo == srd && s.ChildId == application.ChildId).ToListAsync();
                if (result.Any())
                {
                    dc_socpen = result.First();
                    dc_socpen.CaptureReference = file.UnqFileNo;
                    dc_socpen.BrmBarcode = file.BrmBarcode;
                    dc_socpen.CaptureDate = DateTime.Now;
                    dc_socpen.RegionId = session.Office.RegionId;
                    dc_socpen.LocalofficeId = session.Office.OfficeId;
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
                    dc_socpen.RegionId = session.Office.RegionId;
                    dc_socpen.LocalofficeId = session.Office.OfficeId;
                    dc_socpen.Documents = file.DocsPresent;

                    _context.DcSocpen.Add(dc_socpen);
                }
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    CreateActivity("Capture" + GetFileArea(file.SrdNo, file.Lctype), "Error:" + ex.Message.Substring(0, 200), file.UnqFileNo);
                    throw;
                }

                return file;
            }
        }

        public async Task<DcFile> GetBRMRecord(string barcode)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                return await _context.DcFiles.Where(f => f.BrmBarcode == barcode).FirstAsync();
            }
        }

        public async Task AutoMerge(Application app, List<Application> parents)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                //Is not merged 
                if (string.IsNullOrEmpty(app.Brm_Parent))
                {
                    //Is there a valid parent?
                    if (parents.Where(p => p.TDW_BOXNO == null).Any())
                    {
                        var parent = parents.Where(p => p.TDW_BOXNO == null).First();

                        DcMerge merge = _context.DcMerges.Where(k => k.BrmBarcode == app.Brm_BarCode).FirstOrDefault();

                        if (merge == null)//No existing merge create one
                        {
                            //this record will be the parent
                            DcMerge newMerge = new DcMerge();
                            newMerge.BrmBarcode = app.Brm_BarCode;
                            newMerge.ParentBrmBarcode = parent.Brm_BarCode;
                            app.Brm_Parent = parent.Brm_BarCode;
                            _context.DcMerges.Add(newMerge);
                        }
                        else//Existing merge modify it
                        {
                            merge.ParentBrmBarcode = parent.Brm_BarCode;
                            app.Brm_Parent = parent.Brm_BarCode;
                        }


                    }
                    else
                    {
                        //this record will be a parent
                        DcMerge newMerge = new DcMerge();
                        newMerge.BrmBarcode = app.Brm_BarCode;
                        newMerge.ParentBrmBarcode = app.Brm_BarCode;
                        app.Brm_Parent = app.Brm_BarCode;
                        _context.DcMerges.Add(newMerge);

                    }
                    await _context.SaveChangesAsync();
                }
            }
        }

        #endregion

        #region Boxing and Re-Boxing
        public async Task<PagedResult<ReboxListItem>> GetAllFilesByBoxNo(string boxNo, int page)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                bool repaired = await RepairAltBoxSequence(boxNo);

                PagedResult<ReboxListItem> result = new PagedResult<ReboxListItem>();
                if (StaticDataService.GrantTypes == null)
                {
                    _ = sservice.GetGrantTypes();
                }
                result.count = _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo).Count();

                var interim = _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo).OrderByDescending(f => f.UpdatedDate).ToList();
                result.result = interim.Skip((page - 1) * 20).Take(20).OrderBy(f => f.UnqFileNo)
                            .Select(f => new ReboxListItem
                            {
                                ClmNo = f.UnqFileNo,
                                BrmNo = f.BrmBarcode,
                                IdNo = f.ApplicantNo,
                                FullName = f.FullName,
                                GrantType = StaticDataService.GrantTypes[f.GrantType],
                                BoxNo = boxNo,
                                AltBoxNo = f.AltBoxNo,
                                Scanned = f.ScanDatetime != null,
                                MiniBox = (int?)f.MiniBoxno,
                                RegType = f.ApplicationStatus,
                                TdwBatch = (int)f.TdwBatch
                            }).ToList();
                return result;
            }
        }



        public async Task<PagedResult<ReboxListItem>> SearchBox(string boxNo, int page, string searchText)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                PagedResult<ReboxListItem> result = new PagedResult<ReboxListItem>();
                searchText = searchText.ToUpper();
                if (StaticDataService.GrantTypes == null)
                {
                    _ = sservice.GetGrantTypes();
                }
                result.count = _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo && (bn.ApplicantNo.Contains(searchText) || bn.BrmBarcode.Contains(searchText))).Count();
                if (result.count == 0) throw new Exception("No result!");
                result.result = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo && (bn.ApplicantNo.Contains(searchText) || bn.BrmBarcode.Contains(searchText))).OrderByDescending(f => f.UpdatedDate).Skip((page - 1) * 20).Take(20).OrderBy(f => f.UnqFileNo).AsNoTracking()
                            .Select(f => new ReboxListItem
                            {
                                ClmNo = f.UnqFileNo,
                                BrmNo = f.BrmBarcode,
                                IdNo = f.ApplicantNo,
                                FullName = f.FullName,
                                GrantType = StaticDataService.GrantTypes[f.GrantType],
                                BoxNo = boxNo,
                                AltBoxNo = f.AltBoxNo,
                                Scanned = f.ScanDatetime != null,
                                MiniBox = (int?)f.MiniBoxno,
                                TdwBatch = (int)f.TdwBatch
                            }).ToListAsync();
                return result;
            }
        }

        //public async Task LockBox(string boxNo)
        //{
        //    using (var _context = _contextFactory.CreateDbContext())
        //    {
        //        List<DcFile> boxfiles = await _context.DcFiles.Where(b => b.TdwBoxno == boxNo).ToListAsync();
        //        boxfiles.ToList().ForEach(f => { f.TdwBatch = 0; f.BoxLocked = 0; });
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
        public async Task<bool> IsBoxLocked(string boxNo)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                return await _context.DcFiles.Where(b => b.TdwBoxno == boxNo && b.BoxLocked == 1).AnyAsync();
            }
        }

        public async Task RemoveFileFromBox(string brmBarcode)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var files = _context.DcFiles.Where(b => b.BrmBarcode == brmBarcode);
                foreach (var file in files)
                {
                    file.TdwBoxno = null;
                }
                await _context.SaveChangesAsync();
            }
        }


        public async Task<List<ReboxListItem>> GetAllFilesByBoxNo(string boxNo, bool notScanned = false)
        {

            using (var _context = _contextFactory.CreateDbContext())
            {
                _ = sservice.GetGrantTypes();
                if (notScanned)
                {
                    var interimNs = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo && bn.ScanDatetime == null).OrderBy(f => f.UnqFileNo).AsNoTracking().ToListAsync();
                    return interimNs.Select(f => new ReboxListItem
                    {
                        ClmNo = f.UnqFileNo,
                        BrmNo = f.BrmBarcode,
                        IdNo = f.ApplicantNo,
                        FullName = f.FullName,
                        GrantType = StaticDataService.GrantTypes[f.GrantType],
                        BoxNo = boxNo,
                        AltBoxNo = f.AltBoxNo,
                        Scanned = f.ScanDatetime != null
                    }).ToList();
                }
                var interim = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo).OrderBy(f => f.UnqFileNo).AsNoTracking().ToListAsync();
                return interim.Select(f => new ReboxListItem
                {
                    ClmNo = f.UnqFileNo,
                    BrmNo = f.BrmBarcode,
                    IdNo = f.ApplicantNo,
                    FullName = f.FullName,
                    GrantType = StaticDataService.GrantTypes[f.GrantType],
                    BoxNo = boxNo,
                    AltBoxNo = f.AltBoxNo,
                    Scanned = f.ScanDatetime != null
                }).ToList();
            }
        }

        public async Task SetBulkReturned(string boxNo, bool sendTDWMail)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                List<string> parentlist = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo).AsNoTracking().Select(f => f.BrmBarcode).ToListAsync();
                IQueryable query = _context.DcMerges.AsNoTracking();
                foreach (string parent in parentlist)
                {
                    var item = await _context.DcPicklistItems.Where(i => i.BrmNo == parent).FirstOrDefaultAsync();
                    if (item == null) continue;
                    item.Status = "Returned";
                    List<string> childlist = await _context.DcMerges.AsNoTracking().Where(bn => bn.ParentBrmBarcode == parent).Select(c => c.BrmBarcode).ToListAsync();

                    foreach (string child in childlist)
                    {
                        item = await _context.DcPicklistItems.Where(i => i.BrmNo == child).FirstOrDefaultAsync();
                        if (item == null || item.Status == "Returned") continue;
                        item.Status = "Returned";
                    }
                    await _context.SaveChangesAsync();
                    await SyncPicklistFromItems(item.UnqPicklist, "Returned");
                }

                //Return MisFiles
                List<string> mislist = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo).AsNoTracking().Select(f => f.FileNumber).ToListAsync();
                foreach (string parent in mislist)
                {
                    var item = await _context.DcPicklistItems.Where(i => i.BrmNo == parent).FirstOrDefaultAsync();//TDWData has the misfileno i.s.o.brmno in old records
                    if (item == null) continue;
                    item.Status = "Returned";
                    await _context.SaveChangesAsync();
                    await SyncPicklistFromItems(item.UnqPicklist, "Returned");
                }

                //Send tdw email with csv of returned files for LC boxes.
                if (sendTDWMail)
                {
                    await SendTDWReturnedMail(boxNo);
                }
            }
        }

        public async Task SendTDWReturnedMail(string boxNo)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                List<TDWRequestMain> tpl = new List<TDWRequestMain>();
                List<DcFile> parentlist = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo).AsNoTracking().ToListAsync();
                TDWRequestMain TdwFormat;
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
                        Year = parent.UpdatedDate.Value.ToString("YYYY"),
                        Location = parent.TdwBoxno,
                        Reg = parent.RegType,
                        //Bin  = parent. ,
                        Box = parent.MiniBoxno.ToString(),
                        //Pos  = parent. ,
                        UserPicked = ""
                    };
                    tpl.Add(TdwFormat);

                }
                string FileName = session.Office.RegionCode + "-" + session.SamName.ToUpper() + $"-TDW_ReturnedBox_{boxNo.Trim()}-" + DateTime.Now.ToShortDateString().Replace("/", "-") + "-" + DateTime.Now.ToShortTimeString().Replace(":", "-");
                //attachment list
                List<string> files = new List<string>();
                //write attachments for manual download/add to mail
                File.WriteAllText(StaticDataService.ReportFolder + $@"{FileName}.csv", tpl.CreateCSV());
                files.Add(StaticDataService.ReportFolder + $@"{FileName}.csv");
                //send mail to TDW
                try
                {
                    //if (!Environment.MachineName.ToLower().Contains("prod")) return;
                    _mail.SendTDWIncoming(session, boxNo, files);
                }
                catch
                {
                    //ignore confirmation errors
                }
            }
        }
        public async Task<string> GetNexRegionAltBoxSequence()
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                return session.Office.RegionCode + await _raw.GetNextAltbox(session.Office.RegionCode);
            }
        }
        private async Task<bool> RepairAltBoxSequence(string boxNo)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                string AltBoxNo;
                //Repair null altbox values

                //todo: test this
                _context.ChangeTracker.Clear();
                try
                {
                    //nullaltboxfiles
                    var nullaltboxfiles = _context.DcFiles.Where(b => string.IsNullOrEmpty(b.AltBoxNo) && b.TdwBoxno == boxNo).ToList();
                    if (nullaltboxfiles.Any())
                    {
                        var altboxes = _context.DcFiles.Where(b => !string.IsNullOrEmpty(b.AltBoxNo) && b.TdwBoxno == boxNo);
                        if (altboxes.Any())
                        {
                            AltBoxNo = altboxes.First().AltBoxNo;
                        }
                        else
                        {
                            AltBoxNo = await GetNexRegionAltBoxSequence();
                        }
                        var fix = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo).ToListAsync();
                        foreach (var file in fix)
                        {
                            file.AltBoxNo = AltBoxNo;
                        }
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    //Repair RegionMisMatch values
                    var regionmismatchfiles = _context.DcFiles.Where(b => !b.AltBoxNo.Contains(session.Office.RegionCode) && b.TdwBoxno == boxNo).ToList();
                    if (regionmismatchfiles.Any())
                    {


                        AltBoxNo = await GetNexRegionAltBoxSequence();
                        var fix = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo).ToListAsync();
                        foreach (var file in fix)
                        {
                            file.AltBoxNo = AltBoxNo;
                        }
                        await _context.SaveChangesAsync();
                        return true;
                    }
                }
                catch (Exception ex)
                {

                }
                return false;
            }
        }



        public async Task<DcFile> GetReboxCandidate(Reboxing rebox)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                List<DcFile> candidates = new List<DcFile>();
                //Find Barcode in DCFile
                if (!string.IsNullOrEmpty(rebox.BrmBarcode))
                {
                    candidates = await _context.DcFiles.Where(f => f.BrmBarcode == rebox.BrmBarcode).ToListAsync();
                }
                else
                {
                    //Try the newBarcode in DCFiles
                    //candidates = await _context.DcFiles.Where(f => f.BrmBarcode == rebox.NewBarcode).ToListAsync();
                    if (await checkBRMExists(rebox.NewBarcode)) throw new Exception("The new barcode already exists!");
                    //if (!candidates.Any())
                    //{
                    //Try the MisFile in BRM
                    candidates = await _context.DcFiles.Where(k => k.FileNumber == rebox.MisFileNo).ToListAsync();
                    if (!candidates.Any())
                    {
                        //We need to create a record!
                        var miss = await _context.MisLivelinkTbls.Where(mis => mis.FileNumber == rebox.MisFileNo).Select(
                            mis => new DcFile
                            {
                                UnqFileNo = "",
                                ApplicantNo = mis.IdNumber.GetDigitId(),
                                ApplicationStatus = mis.RegistryType.ApplicationStatusFromMIS(),
                                TransType = 0,
                                BatchAddDate = DateTime.Now,
                                BrmBarcode = rebox.NewBarcode,
                                FileComment = "Added from TDW/MIS on reboxing.",
                                FileNumber = mis.FileNumber,
                                FileStatus = "Completed",
                                GrantType = mis.GrantType.GrantTypeFromMIS(),
                                Isreview = "N",
                                MisBoxno = mis.BoxNumber,
                                OfficeId = session.Office.OfficeId,
                                RegionId = session.Office.RegionId,
                                FspId = session.Office.FspId,
                                TdwBoxno = "",//rebox.BoxNo,
                                TransDate = "2016-05-29".ToDate("yyyy-mm-dd"),
                                UpdatedByAd = session.SamName,
                                UpdatedDate = DateTime.Now,
                                UserFirstname = mis.Name,
                                UserLastname = mis.Surname
                            }).ToListAsync();
                        if (!miss.Any()) throw new Exception("No suitable MIS record found to create BRM record, please recapture.");
                        _context.ChangeTracker.Clear();
                        var misfiledata = miss.First();
                        if (string.IsNullOrEmpty(misfiledata.ApplicantNo)) throw new Exception("No suitable MIS record found to create BRM record, please recapture.");
                        _context.DcFiles.Add(misfiledata);
                        await _context.SaveChangesAsync();
                        candidates = await _context.DcFiles.Where(f => f.BrmBarcode == rebox.NewBarcode).ToListAsync();
                        // }
                    }
                    rebox.BrmBarcode = rebox.NewBarcode;
                    rebox.MisFileNo = string.Empty;
                    rebox.NewBarcode = string.Empty;
                }

                if (!candidates.Any())
                {
                    throw new Exception("BRM record not found.");
                }

                if (candidates.Count() > 1)
                {
                    ////Try Repair
                    //if (candidates.Where(c => string.IsNullOrEmpty(c.ApplicantNo) && c.BrmBarcode == rebox.BrmBarcode).Any())
                    //{
                    //    DcFile corrupt = _context.DcFiles.Where(c => string.IsNullOrEmpty(c.ApplicantNo) && c.BrmBarcode == rebox.BrmBarcode).First();
                    //    _context.DcFiles.Remove(corrupt);
                    //    await _context.SaveChangesAsync();
                    //    candidates = await _context.DcFiles.Where(f => f.BrmBarcode == rebox.BrmBarcode).ToListAsync();
                    //}
                    //if (candidates.Count() > 1)
                    //{
                    throw new Exception($"Duplicate BRM No {rebox.BrmBarcode} please delete/recapture this file.");
                    //}
                }
                if (candidates.Where(f => f.BrmBarcode == rebox.BrmBarcode && f.TdwBoxno == rebox.BoxNo).Any())
                {
                    throw new Exception($"{rebox.BrmBarcode} is already in this box !");
                }
                return candidates.First();
            }
        }
        public async Task Rebox(Reboxing rebox, DcFile file)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                try
                {
                    file.MisReboxDate = DateTime.Now;
                    file.MisReboxStatus = "Completed";
                    file.TdwBoxno = rebox.BoxNo;
                    file.MiniBoxno = rebox.MiniBox;
                    file.TdwBoxTypeId = decimal.Parse(rebox.SelectedType);
                    file.TdwBatch = 0;
                    //file.FileNumber = rebox.MisFileNo;
                    if ("14|15|16|17|18".Contains(rebox.SelectedType))
                    {
                        file.TdwBoxArchiveYear = rebox.ArchiveYear;
                    }
                    else
                    {
                        file.TdwBoxArchiveYear = null;
                    }

                    file.AltBoxNo = rebox.AltBoxNo;
                    file.UpdatedDate = DateTime.Now;
                    file.UpdatedByAd = session.SamName;

                    await _context.SaveChangesAsync();
                    CreateActivity("Reboxing" + GetFileArea(file.SrdNo, file.Lctype), "Rebox file", file.UnqFileNo);
                }
                catch //(Exception ex)
                {
                    throw;
                }
            }
        }

        #endregion

        #region File Requests

        public async Task AddFileRequest(RequestModel fr)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                try
                {

                    if (string.IsNullOrEmpty(fr.GrantType))
                    {
                        foreach (string granttype in StaticDataService.GrantTypes.Keys) //All Grant types
                        {
                            fr.GrantType = granttype;
                            await AddValidRequest(fr);
                        }
                        return;
                    }
                    await AddValidRequest(fr);
                }
                catch //(Exception ex)
                {
                    throw;
                }

            }
        }

        public async Task AddValidRequest(RequestModel fr)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                try
                {
                    var requests = await _context.DcFileRequests.Where(r => r.IdNo == fr.IdNo && r.GrantType == fr.GrantType && r.Status != "Compliant" && r.Status != "NonCompliant").ToListAsync();
                    if (requests.Any())
                    {
                        //throw new Exception("An in progress request exists. Please finalize the existing request first.");
                        return;
                    }
                    List<TdwFileLocation> tdws = await _context.TdwFileLocations.Where(l => l.Description == fr.IdNo).AsNoTracking().ToListAsync();
                    foreach (TdwFileLocation tdw in tdws.Where(t => t.GrantType == fr.GrantType))
                    {
                        await AddRequestFromTDW(tdw, fr);
                    }
                }
                catch //(Exception ex)
                {
                    throw;
                }
            }

        }
        private async Task AddRequestFromTDW(TdwFileLocation tdw, RequestModel fr)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var reqs = await _context.DcFileRequests.Where(r => r.IdNo == tdw.Description && r.GrantType == tdw.GrantType).ToListAsync();
                DcFileRequest req;
                if (!reqs.Any())
                {
                    _context.ChangeTracker.Clear();
                    req = new DcFileRequest();
                    req.IdNo = fr.IdNo;
                    req.GrantType = tdw.GrantType;
                    _context.DcFileRequests.Add(req);
                }
                else
                {
                    req = reqs.First();
                    //if (req.Status  == "TDWPicklist")
                    //{
                    //    //Refresh the date
                    //    req.RequestedByAd = session.SamName;
                    //    req.RequestedOfficeId = session.Office.OfficeId;
                    //    req.RequestedDate = DateTime.Now;
                    //    req.RegionId = session.Office.RegionId;

                    //    await _context.SaveChangesAsync();
                    //}

                }
                //req.AppDate
                req.Stakeholder = decimal.Parse(fr.Category);
                req.ReqCategory = decimal.Parse(fr.Category);
                req.ReqCategoryType = decimal.Parse(fr.CategoryType);
                req.ReqCategoryDetail = fr.Description;
                req.IdNo = tdw.Description;
                if (!string.IsNullOrEmpty(tdw.FilefolderAltcode))
                {
                    req.MisFileNo = tdw.FilefolderAltcode.Length == 12 ? "" : tdw.FilefolderAltcode;
                }
                req.BrmBarcode = tdw.FilefolderCode;
                req.GrantType = tdw.GrantType;
                req.TdwBoxno = tdw.ContainerCode;
                req.RequestedByAd = session.SamName;
                req.RequestedOfficeId = session.Office.OfficeId;
                req.RequestedDate = DateTime.Now;
                req.RegionId = session.Office.RegionId;
                req.Name = tdw.Name;//Could query Socpen for the name and surname
                req.Status = "TDWPicklist";
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string> GetSearchId(SearchModel sm)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                if (!string.IsNullOrEmpty(sm.SrdNo))
                {
                    sm.SrdNo = sm.SrdNo.ToUpper();
                    sm.IdNo = await GetSocpenSearchId(sm.SrdNo);
                }
                else if (!string.IsNullOrEmpty(sm.ClmNo))
                {
                    var src = await _context.DcFiles.Where(f => f.UnqFileNo == sm.ClmNo).FirstOrDefaultAsync();
                    if (src == null)
                    {
                        throw new Exception("CLM No not found");
                    }
                    sm.IdNo = src.ApplicantNo;
                }
                else if (!string.IsNullOrEmpty(sm.BrmNo))
                {
                    var src = await _context.DcFiles.Where(f => f.BrmBarcode == sm.BrmNo).FirstOrDefaultAsync();
                    if (src == null)
                    {
                        throw new Exception("BRM Barcode not found");
                    }
                    sm.IdNo = src.ApplicantNo;
                }
                return sm.IdNo;
            }
        }

        public async Task<Dictionary<string, string>> GetTDWGrants(string IdNo)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                if (StaticDataService.GrantTypes == null)
                {
                    StaticDataService.GrantTypes = await _context.DcGrantTypes.AsNoTracking().ToDictionaryAsync(key => key.TypeId, value => value.TypeName);
                }

                List<TdwFileLocation> results = await _context.TdwFileLocations.Where(t => t.Description == IdNo).AsNoTracking().ToListAsync();
                if (results.Any())
                {
                    result.Add("", "All");
                    foreach (var gt in results.ToList())
                    {
                        if (result.ContainsKey(gt.GrantType)) continue;
                        result.Add(gt.GrantType, StaticDataService.GrantTypes[gt.GrantType]);
                    }

                }
                return result;
            }

        }

        public PagedResult<DcFileRequest> GetFileRequests(bool filterUser, bool filterOffice, int page, string statusFilter = "", string reasonFilter = "")
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                PagedResult<DcFileRequest> result = new PagedResult<DcFileRequest>();
                var query = _context.DcFileRequests.AsQueryable();

                if (filterUser)
                {
                    query = query.Where(r => r.RequestedByAd == session.SamName);
                }
                if (filterOffice)
                {
                    if (session.Office.OfficeType == "RMC")
                    {
                        query = query.Where(r => r.RegionId == session.Office.RegionId);
                    }
                    else
                    {
                        query = query.Where(r => r.RequestedOfficeId == session.Office.OfficeId);
                    }
                }
                if (!string.IsNullOrEmpty(reasonFilter))
                {
                    query = query.Where(r => r.ReqCategoryType.ToString() == reasonFilter);
                }
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    query = query.Where(r => r.Status == statusFilter);
                }

                result.count = query.Count();
                //var reversed = query.AsEnumerable().Reverse();
                result.result = query.AsEnumerable().OrderByDescending(d => d.RequestedDate).Skip((page - 1) * 12).Take(12).ToList();
                foreach (var req in result.result)
                {
                    sservice.GetRequestCategoryTypes();
                    try
                    {
                        req.Reason = StaticDataService.RequestCategoryTypes.Where(r => r.TypeId == req.ReqCategoryType).First().TypeDescr;
                    }
                    catch (Exception ex)
                    {
                        var ss = ex.Message;
                    }
                }
                return result;
            }
        }

        //public async Task<PagedResult<DcFileRequest>> GetFilrR()
        //{
        //    PagedResult<DcFileRequest> result = new PagedResult<DcFileRequest>();
        //    var query = _context.DcFileRequests.AsQueryable();
        //    var requests = _context.DcFileRequests.AsQueryable();
        //    var reasons = _context.DcReqCategoryTypes.AsQueryable();
        //    var innerJoin = requests.Join(// outer sequence 
        //              reasons,  // inner sequence 
        //              request => request.ReqCategoryType,    // outerKeySelector
        //              reason => reason.TypeId,  // innerKeySelector
        //              (request, reason) => new DcFileRequest// result selector
        //              {
        //                  Reason = reason.TypeDescr
        //              });
        //}

        /// <summary>
        /// Deprecated for now Compliant and NonCompliant status to be set by Kofax data
        /// </summary>
        /// <param name="fr"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task ChangeFileRequestStatus(DcFileRequest fr, string status)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                //Todo:
                if (status == "Closed") throw new Exception("Feature in dev. Kofax data will set status");
                if (fr.Status == "Requested") throw new Exception("Request is with TDW, can't change status now.");
                DcFileRequest req = await _context.DcFileRequests.FindAsync(fr.IdNo, fr.GrantType);
                req.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<PagedResult<DcPicklist>> GetPickLists(bool filterRequestUser, bool filterInProgress, int page)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                PagedResult<DcPicklist> result = new PagedResult<DcPicklist>();

                var query = _context.DcPicklists.OrderByDescending(o => o.PicklistDate).AsQueryable();

                if (filterRequestUser)
                {
                    query = query.Where(r => r.RequestedByAd.ToLower() == session.SamName.ToLower());
                }
                else if (filterInProgress)
                {
                    query = query.Where(r => r.Status != "Returned");
                }
                else
                {
                    query = query.Where(r => r.RegionId == session.Office.RegionId);
                }

                result.count = query.Where(r => r.RegionId == session.Office.RegionId).Count();
                result.result = await query.Where(r => r.RegionId == session.Office.RegionId).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
                return result;
            }
        }

        public async Task<PagedResult<DcPicklist>> SearchPickLists(string searchTxt, int page)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                PagedResult<DcPicklist> result = new PagedResult<DcPicklist>();

                var query = _context.DcPicklists.OrderByDescending(o => o.PicklistDate).AsQueryable();


                query = query.Where(r => r.UnqPicklist.ToLower().Contains(searchTxt.ToLower()));


                result.count = query.Where(r => r.RegionId == session.Office.RegionId).Count();
                result.result = await query.Where(r => r.RegionId == session.Office.RegionId).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
                return result;
            }
        }

        public async Task ChangePickListStatus(DcPicklist pi)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                DcPicklist pl = await _context.DcPicklists.FindAsync(pi.UnqPicklist);
                pl.Status = pi.nextStatus;
                await _context.SaveChangesAsync();
            }
        }

        internal async Task<PagedResult<DcPicklistItem>> GetPicklistItems(string unq_picklist, int page)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                PagedResult<DcPicklistItem> result = new PagedResult<DcPicklistItem>();
                result.count = _context.DcPicklistItems.Where(p => p.UnqPicklist == unq_picklist).Count();
                result.result = await _context.DcPicklistItems.Where(p => p.UnqPicklist == unq_picklist).OrderByDescending(p => p.PicklistItemId).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
                return result;
            }
        }

        internal async Task ReceivePickList(string unq_picklist)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var items = _context.DcPicklistItems.Where(p => p.UnqPicklist == unq_picklist).ToList();
                foreach (DcPicklistItem item in items)
                {

                    item.Status = "Received";
                    await SyncFileRequestStatusReceived(item);
                }
                var picklist = _context.DcPicklists.Find(unq_picklist);
                picklist.Status = "Received";
                await _context.SaveChangesAsync();
                _mail.SendTDWReceipt(session, StaticDataService.RegionIDEmails[picklist.RegionId], picklist.UnqPicklist, new List<string>());
            }
        }

        internal async Task SyncFileRequestStatusReceived(DcPicklistItem item)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var GrantId = sservice.GetGrantId(item.GrantType);
                DcFileRequest fileReq = await _context.DcFileRequests.FindAsync(item.IdNumber, GrantId);
                if (fileReq == null) return;
                if (fileReq.Status == "Received") return;//skip nochange...
                fileReq.Status = "Received";
                await _context.SaveChangesAsync();
            }
        }
        public async Task SetStatusPickListItem(decimal ItemId)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var item = _context.DcPicklistItems.Find(ItemId);
                item.Status = item.nextStatus;
                await _context.SaveChangesAsync();

                //set picklist status if all items accounted for

                await SyncPicklistFromItems(item.UnqPicklist, item.Status);
                if (item.Status != "Received")
                {
                    return;
                }
                else
                {
                    var files = _context.DcFiles.Where(f => f.BrmBarcode == item.BrmNo);
                    if (files.Any())
                    {
                        DcFile file = files.First();
                        file.ScanDatetime = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }
                }
                await SyncFileRequestStatusReceived(item);
            }

        }

        //public bool IsAllItemsStatus(string unqPicklist,string status)
        //{
        //    return !_context.DcPicklistItems.Where(i => i.UnqPicklist == unqPicklist && i.Status != status).Any();
        //}

        public async Task SyncPicklistFromItems(string unqPicklist, string status)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                if (_context.DcPicklistItems.Where(i => i.UnqPicklist == unqPicklist && i.Status != status).Any()) return;
                var picklist = _context.DcPicklists.Find(unqPicklist);
                picklist.Status = status;
                await _context.SaveChangesAsync();
                if (status == "Received")
                {
                    _mail.SendTDWReceipt(session, StaticDataService.RegionIDEmails[picklist.RegionId], picklist.UnqPicklist, new List<string>());
                }
            }

        }
        /// <summary>
        /// Set next status on Picklist Item
        /// </summary>
        /// <param name="brmBarcode"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<bool> SetPickListItemStatus(string brmBarcode, string status)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var item = await _context.DcPicklistItems.Where(i => i.BrmNo == brmBarcode).FirstOrDefaultAsync();
                if (item == null) return false;
                item.Status = status;
                await _context.SaveChangesAsync();
                //set picklist status if all items accounted for
                await SyncPicklistFromItems(item.UnqPicklist, status);
                return true;
            }

        }
        public async Task SendTDWRequestsPerRegion(int requestCount)
        {

            using (var _context = _contextFactory.CreateDbContext())
            {
                TDWPicklist tpl = await GetTDWFILE(requestCount);

                if (!tpl.result.Any()) return;
                string FileName = session.Office.RegionCode + "-" + session.SamName.ToUpper() + "-TDW_File_Request-" + DateTime.Now.ToShortDateString().Replace("/", "-") + "-" + DateTime.Now.ToShortTimeString().Replace(":", "-");
                //attachment list
                List<string> files = new List<string>();
                //write attachments for manual download/add to mail
                File.WriteAllText(StaticDataService.ReportFolder + $@"{FileName}.csv", tpl.result.CreateCSV());
                files.Add(StaticDataService.ReportFolder + $@"{FileName}.csv");
                //send mail to TDW
                try
                {
                    _mail.SendTDWRequest(session, StaticDataService.RegionEmails[session.Office.RegionName.ToUpper()], tpl.UnqPickList, files);
                }
                catch
                {
                    //ignore confirmation errors
                }
                //Update the status to sent
                foreach (var pli in _context.DcPicklistItems.Where(p => p.UnqPicklist == tpl.UnqPickList).ToList())
                {
                    SelectedRequest request = new SelectedRequest { IDNo = pli.IdNumber, GrantTypeId = sservice.GetGrantId(pli.GrantType) };
                    await SetStatusTDWSent(request);
                }
                //Get the picklist
                var pl = await _context.DcPicklists.Where(p => p.UnqPicklist == tpl.UnqPickList).FirstAsync();
                ///Send mails
                string tomail = pl.RequestedByAd.GetADEmail();
                if (string.IsNullOrEmpty(tomail)) return; //Skip if no tomail address
                try
                {
                    _mail.SendRequestStatusChange(session, "Requested", pl.UnqPicklist, tomail);
                }
                catch
                {
                    //ignore confirmation errors
                }
            }
        }

        public async Task<TDWPicklist> GetTDWFILE(int maxRecords)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                TDWPicklist tpl = new TDWPicklist();
                try
                {
                    var candidates = await (from tdw in _context.TdwFileLocations
                                            join fr in _context.DcFileRequests
                                            on tdw.Description equals fr.IdNo
                                            where fr.Status == "TDWPicklist" && fr.RegionId == session.Office.RegionId && tdw.GrantType == fr.GrantType
                                            orderby fr.RequestedDate descending
                                            select new TDWRequestMain
                                            {
                                                BRM_No = tdw.FilefolderCode,
                                                CLM_No = tdw.FilefolderAltcode.Length == 12 ? tdw.FilefolderAltcode : null,
                                                Folder_ID = tdw.FilefolderAltcode.Length != 12 ? tdw.FilefolderAltcode : null,
                                                Grant_Type = tdw.GrantType.Trim(),
                                                Firstname = fr.Name,
                                                Surname = fr.Surname,
                                                ID_Number = tdw.Description,
                                                Reg = "U",
                                                Location = tdw.ContainerCode,
                                                UserPicked = ""

                                            }).AsNoTracking().Take(maxRecords).ToListAsync();
                    //Get filerequest Candidates
                    if (!candidates.Any()) throw new Exception("No files to request !");
                    //Create a picklist for every selected fr

                    //Create new PICKList
                    DcPicklist pl = new DcPicklist();
                    pl.UnqPicklist = string.Empty;
                    pl.PicklistDate = DateTime.Now;
                    pl.Status = "Requested";
                    pl.RegionId = session.Office.RegionId;
                    pl.RegistryType = "U";
                    pl.RequestedByAd = session.SamName;
                    pl.UpdatedBy = session.SamName;
                    pl.PicklistStatus = "N";
                    _context.ChangeTracker.Clear();
                    _context.DcPicklists.Add(pl);
                    await _context.SaveChangesAsync();
                    pl = _context.DcPicklists.OrderByDescending(p => p.PicklistDate).First();
                    //Create a new TDWPickList
                    tpl.UnqPickList = pl.UnqPicklist;
                    //Add Picklist items
                    foreach (var item in candidates)
                    {
                        item.Grant_Type = sservice.GetGrantType(item.Grant_Type);
                        tpl.result.Add(item);
                        DcPicklistItem pi = new DcPicklistItem();
                        pi.UnqPicklist = pl.UnqPicklist;
                        pi.IdNumber = item.ID_Number;
                        pi.FolderId = item.Folder_ID;
                        pi.GrantType = item.Grant_Type;
                        pi.Bin = item.Bin;
                        pi.BrmNo = item.BRM_No;
                        pi.ClmNo = item.CLM_No;
                        pi.Firstname = item.Firstname;
                        pi.Surname = item.Surname;
                        pi.Status = "Requested";
                        pi.Year = item.Year;
                        pi.Reg = item.Reg;
                        pi.Position = item.Pos;
                        pi.Minibox = "";
                        pi.LooseCorrespondenceId = "";//Todo:
                        pi.Location = item.Location;
                        pi.LcType = "";//Todo:

                        _context.DcPicklistItems.Add(pi);
                        await _context.SaveChangesAsync();
                    }
                    //Save the changes

                    return tpl;
                }
                catch //(Exception ex)
                {
                    //foreach (var eve in e.EntityValidationErrors)
                    //{
                    //    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    //        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    //    foreach (var ve in eve.ValidationErrors)
                    //    {
                    //        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                    //            ve.PropertyName, ve.ErrorMessage);
                    //    }
                    //}
                    throw;
                }
            }
        }

        public async Task SetStatusTDWSent(SelectedRequest req)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var fileReqs = await _context.DcFileRequests.Where(f => f.IdNo == req.IDNo && f.GrantType == req.GrantTypeId).ToListAsync();
                if (!fileReqs.Any()) return;
                DcFileRequest fileReq = fileReqs.First();
                if (fileReq.Status == "Requested") return;//skip nochange...
                fileReq.Status = "Requested";
                fileReq.SentTdw = session.SamName;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<TreeNode> GetCSFiles(string idNo)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                TreeNode node = new TreeNode();
                var intermediate = await _context.DcDocumentImages.Where(d => d.IdNo == idNo).ToListAsync();
                var files = intermediate.Where(d => (d.Filename.ToLower().EndsWith(".pdf") || !(bool)d.Type));
                foreach (var file in files)
                {
                    TreeNode child = new TreeNode
                    {
                        ParentId = file.Parentnode == null ? 0 : (int)file.Parentnode,
                        Id = (int)file.Csnode,
                        NodeType = (bool)file.Type,
                        NodeName = file.Filename,
                        NodeContent = file.Image
                    };
                    if (file.Parentnode != null)
                    {
                        node.AddOnParent((int)file.Parentnode, child);
                    }
                    else //Rootnode
                    {
                        node.Add((int)file.Csnode, child);
                    }

                }
                return node;
            }
        }
        #endregion

        #region SocpenData
        public async Task<string> GetSocpenSearchId(string srdNo)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                if (!srdNo.IsNumeric()) throw new Exception("SRD is Invalid.");

                long srd = long.Parse(srdNo);
                var result = await _context.DcSocpen.Where(s => s.SrdNo == srd).ToListAsync();

                if (!result.Any()) throw new Exception("SRD not found.");

                return result.First().BeneficiaryId;
            }
        }
        #endregion

        public async Task SaveChanges(string unqFileNo, string docsPresent)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                DcFile original = await _context.DcFiles.FindAsync(unqFileNo);
                original.DocsPresent = docsPresent;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Create standard report file name
        /// </summary>
        /// <param name="reportName"></param>
        /// <returns></returns>
        public string GetFileName(string reportName)
        {
            return $"{session.Office.RegionCode}-{session.SamName.ToUpper()}-{reportName}-{DateTime.Now.ToShortDateString().Replace("/", "-")}-{DateTime.Now.ToString("HH-mm")}";
        }
        /// <summary>
        /// Create Activity Record
        /// </summary>
        /// <param name="Area"></param>
        /// <param name="Activity"></param>
        /// <returns></returns>
        public void CreateActivity(string Area, string Activity, string UniqueFileNo = "")
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                DcActivity activity = new DcActivity { ActivityDate = DateTime.Now, RegionId = session.Office.RegionId, OfficeId = decimal.Parse(session.Office.OfficeId), Userid = 0, Username = session.SamName, Area = Area, Activity = Activity, Result = "OK", UnqFileNo = UniqueFileNo };
                try
                {

                    _context.DcActivities.Add(activity);
                    _context.SaveChanges();

                }
                catch
                {
                    //just ignoring activity post errors for now.
                }
            }
        }
        public string GetFileArea(string srdNo, decimal? lcType)
        {
            if (!string.IsNullOrEmpty(srdNo))
            {
                return "-SRD";
            }
            if (lcType != null)
            {
                return "-LC";
            }
            return "-File";
        }

        /// <summary>
        /// Write System Event log entry
        /// </summary>
        /// <param name="eventLogEntry"></param>
        public void WriteEvent(string eventLogEntry)
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry(eventLogEntry, EventLogEntryType.Error, 101, 1);
            }
        }

        #region SearchData

        /// <summary>
        /// New DC_SOCPEN Dataset
        /// </summary>
        /// <param name="SearchId"></param>
        /// <param name="FullSearch"></param>
        /// <returns></returns>
        public async Task<List<Application>> SearchSocpenId(string SearchId, bool FullSearch)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                List<Application> idquery;
                sservice.GetGrantType("S");//Dummy call to ensure static loaded
                sservice.GetRegionCode("7");//Dummy call to ensure static loaded
                List<Application> oldidquery = new List<Application>();
                idquery = await _context.DcSocpen.Where(d => d.BeneficiaryId == SearchId).Select(d => new Application
                {
                    SocpenIsn = (long)d.Id,
                    Id = d.BeneficiaryId,
                    Srd_No = d.SrdNo > 0 ? d.SrdNo.ToString() : "",
                    Name = d.Name,
                    SurName = d.Surname,
                    GrantType = d.GrantType,
                    GrantName = StaticDataService.GrantTypes[d.GrantType],
                    AppDate = d.ApplicationDate.ToStandardDateString(),
                    OfficeId = session.Office.OfficeId,
                    RegionId = d.RegionId,
                    RegionCode = StaticDataService.RegionCode(d.RegionId),
                    RegionName = StaticDataService.RegionName(d.RegionId),
                    AppStatus = d.StatusCode.ToUpper() == "ACTIVE" ? "MAIN" : "ARCHIVE",
                    ARCHIVE_YEAR = StaticDataService.GetArchiveYear(d.ApplicationDate, d.StatusCode.ToUpper()),
                    ChildId = d.ChildId,
                    LcType = "0",
                    IsRMC = session.Office.OfficeType == "RMC" ? "Y" : "N",
                    DocsPresent = d.Documents,
                    IdHistory = d.IdHistory,
                    Source = "Socpen"
                }).AsNoTracking().ToListAsync();

                if (FullSearch)
                {
                    oldidquery = await _context.DcSocpen.Where(d => d.IdHistory.Contains(SearchId)).Select(d => new Application
                    {
                        SocpenIsn = (long)d.Id,
                        Id = d.BeneficiaryId,
                        Srd_No = d.SrdNo > 0 ? d.SrdNo.ToString() : "",
                        Name = d.Name,
                        SurName = d.Surname,
                        GrantType = d.GrantType,
                        GrantName = StaticDataService.GrantTypes[d.GrantType],
                        AppDate = d.ApplicationDate.ToStandardDateString(),
                        OfficeId = session.Office.OfficeId,
                        RegionId = d.RegionId,
                        RegionCode = StaticDataService.RegionCode(d.RegionId),
                        RegionName = StaticDataService.RegionName(d.RegionId),
                        AppStatus = d.StatusCode.ToUpper() == "ACTIVE" ? "MAIN" : "ARCHIVE",
                        ARCHIVE_YEAR = d.StatusCode.ToUpper() == "ACTIVE" ? null : ((DateTime)d.ApplicationDate).ToString("yyyy"),
                        ChildId = d.ChildId,
                        LcType = "0",
                        IsRMC = session.Office.OfficeType == "RMC" ? "Y" : "N",
                        DocsPresent = d.Documents,
                        IdHistory = d.IdHistory,
                        Source = "Socpen"
                    }).AsNoTracking().ToListAsync();
                }
                var result = idquery.Union(oldidquery).ToList();

                foreach (var item in result)
                {
                    item.IsMergeCandidate = result.Where(s => s.AppDate == item.AppDate).Count() > 1;
                    item.DocsPresent = await GetDocsPresent(item.Id, item.GrantType, item.AppDate);
                }
                return result;
            }
        }


        //public string DcSocpenSql(string SearchId)
        //{
        //    //Select* from DC_Socpen spn
        //    //where spn.BENEFICIARY_ID = '8112226059082'
        //    //order by spn.STATUS_DATE desc


        //    return @$"select 
        //                        spn.BENEFICIARY_ID as Id,
        //                        spn.NAME as Name,
        //                        spn.SURNAME as Surname,
        //                        spn.GRANT_TYPE as GrantType,
        //                        g.TYPE_NAME as GrantName,
        //                        spn.APPLICATION_DATE as AppDate,
        //                        spn.Region_id as RegionId,
        //                        rg.REGION_CODE as REGIONCODE,
        //                        rg.REGION_NAME as REGIONNAME,
        //                        null as DOCSPRESENT,
        //                        spn.PRIM_STATUS as Prim_Status,
        //                        spn.SEC_STATUS as Sec_Status,
        //                        spn.STATUS_DATE as StatusDate,
        //                        spn.APPLICATION_DATE as Child_App_Date,
        //                        spn.STATUS_CODE as Child_Status_Code,
        //                        spn.STATUS_DATE as Child_Status_Date,
        //                        spn.Child_id as ChildId,
        //                        r.DATE_REVIEWED as LASTREVIEWDATE,
        //                        '' AS ARCHIVE_YEAR,
        //                        Status_code As AppStatus,
        //                        '' As Brm_Barcode,
        //                        '' As Brm_Parent,
        //                        '' As Clm_no,
        //                        '' As DateApproved,
        //                        0 As IsCombinationCandidate,
        //                        0 As IsMergeCandidate,
        //                        0 As IsNew,
        //                        'Y' AS IsRmc,
        //                        0 As Batch_No,
        //                        ID As SocpenIsn,
        //                        0 As LcType,
        //                        0 As RowType,
        //                        '' As Srd_No,
        //                        0 As StatusCode,
        //                        0 As Tdw_BoxNo,
        //                        0 As Trans_type,
        //                        '' as IdHistory,
        //                        'Socpen' as Source
        //                from DC_Socpen spn
        //                inner join DC_REGION rg on spn.Region_ID = rg.REGION_ID
        //                inner join DC_Grant_type g on g.TYPE_ID = spn.GRANT_TYPE
        //                left join SASSA.SOCPEN_REVIEW r on r.PENSION_NO = spn.BENEFICIARY_ID
        //                where spn.BENEFICIARY_ID = '{SearchId}'
        //                order by spn.STATUS_DATE desc";
        //}

        //public async Task<List<Application>> SearchOldIds(string SearchId)
        //{
        //    List<Application> idquery;
        //    string sql = $@"select spn.PENSION_NO as Id,
        //                                spn.NAME as Name,
        //                                spn.SURNAME as Surname,
        //                                spn.GRANT_TYPE as GrantType,
        //                                g.TYPE_NAME as GrantName,
        //                                spn.APP_DATE as AppDate,
        //                                spn.PROVINCE as RegionId,
        //                                rg.REGION_CODE as REGIONCODE,
        //                                rg.REGION_NAME as REGIONNAME,
        //                                spn.DOCS_PRESENT as DOCSPRESENT,
        //                                spn.PRIM_STATUS as Prim_Status,
        //                                spn.SEC_STATUS as Sec_Status,
        //                                spn.STATUS_DATE as StatusDate,
        //                                null as Child_App_Date,
        //                                null as Child_Status_Code,
        //                                null as Child_Status_Date,
        //                                null as ChildId,
        //                                null as LASTREVIEWDATE,
        //                                '' AS ARCHIVE_YEAR,
        //                                CASE
        //                                WHEN spn.PRIM_STATUS IN ('B','A','9') AND spn.SEC_STATUS IN ('2') THEN 'MAIN'
        //                                ELSE 'ARCHIVE'
        //                                END AS AppStatus,
        //                                '' As Brm_Barcode,
        //                                '' As Brm_Parent,
        //                                '' As Clm_no,
        //                                '' As DateApproved,
        //                                0 As IsCombinationCandidate,
        //                                0 As IsMergeCandidate,
        //                                0 As IsNew,
        //                                '{(session.Office.OfficeType == "RMC" ? "Y" : "N")}' AS IsRmc,
        //                                0 As Batch_No,
        //                                0 As SocpenIsn,
        //                                0 As LcType,
        //                                0 As RowType,
        //                                '' As Srd_No,
        //                                0 As StatusCode,
        //                                0 As Tdw_BoxNo,
        //                                1 As MiniBox,
        //                                0 As Trans_type,
        //                                spn.OLD_ID1||' '||spn.OLD_ID2||' '|| spn.OLD_ID3||' '|| spn.OLD_ID4||' '|| spn.OLD_ID5||' '|| spn.OLD_ID6||' '|| spn.OLD_ID7||' '|| spn.OLD_ID8||' '|| spn.OLD_ID9||' '|| spn.OLD_ID10 as IdHistory,
        //                                'Socpen' as Source
        //                        from SOCPENGRANTS spn
        //                        inner join DC_REGION rg on spn.PROVINCE = rg.REGION_ID
        //                        inner join DC_Grant_type g on g.TYPE_ID = spn.GRANT_TYPE
        //                        where '{SearchId}' in ( spn.OLD_ID1, spn.OLD_ID2, spn.OLD_ID3, spn.OLD_ID4, spn.OLD_ID5, spn.OLD_ID6, spn.OLD_ID7, spn.OLD_ID8, spn.OLD_ID9, spn.OLD_ID10)
        //                        order by spn.STATUS_DATE desc";
        //    idquery = await _context.Applications.FromSqlRaw(sql).AsNoTracking().ToListAsync();
        //    return idquery;
        //}
        /// <summary>
        /// Deprecated for switch to DC_SOCPEN
        /// </summary>
        /// <param name="SearchId"></param>
        /// <param name="FullSearch"></param>
        /// <returns></returns>

        public async Task<List<Application>> SearchSocpenSrd(long srd)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                List<Application> srdsquery = await _context.DcSocpen.Where(d => d.SrdNo == srd).Select(d => new Application
                {
                    SocpenIsn = (long)d.Id,
                    Id = d.BeneficiaryId,
                    Srd_No = d.SrdNo > 0 ? d.SrdNo.ToString() : "",
                    Name = d.Name,
                    SurName = d.Surname,
                    GrantType = d.GrantType,
                    GrantName = StaticDataService.GrantTypes[d.GrantType],
                    AppDate = d.ApplicationDate.ToStandardDateString(),
                    OfficeId = session.Office.OfficeId,
                    RegionId = d.RegionId,
                    RegionCode = StaticDataService.RegionCode(d.RegionId),
                    RegionName = StaticDataService.RegionName(d.RegionId),
                    AppStatus = d.StatusCode.ToUpper() == "ACTIVE" ? "MAIN" : "ARCHIVE",
                    ARCHIVE_YEAR = d.StatusCode.ToUpper() == "ACTIVE" ? null : ((DateTime)d.ApplicationDate).ToString("yyyy"),
                    ChildId = d.ChildId,
                    LcType = "0",
                    IsRMC = session.Office.OfficeType == "RMC" ? "Y" : "N",
                    DocsPresent = d.Documents,
                    IdHistory = d.IdHistory,
                    Source = "Socpen"
                }).AsNoTracking().ToListAsync();

                return srdsquery;
            }
        }

        public async Task<List<Application>> SearchBRMID(string SearchId)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                string sql = $@"select f.APPLICANT_NO as Id,
                                f.USER_FirstName as Name,
                                f.USER_LASTNAME as Surname,
                                f.GRANT_TYPE as GrantType,
                                g.TYPE_NAME as GrantName,
                                f.TRANS_DATE as AppDate,
                                f.Office_ID as OfficeId,
                                f.REGION_ID as RegionId,
                                rg.REGION_CODE as REGIONCODE,
                                rg.REGION_NAME as REGIONNAME,
                                f.DOCS_PRESENT as DOCSPRESENT,
                                f.APPLICATION_STATUS as APPSTATUS,
                                null as StatusDate,
                                null as Child_App_Date,
                                null as Child_Status_Code,
                                null as Child_Status_Date,
                                f.TDW_BOXNO,
                                NVL(f.Mini_Boxno,1) As MiniBox,
                                f.BATCH_NO,
                                f.SRD_NO as Srd_No,
                                f.CHILD_ID_NO as ChildId,
                                m.PARENT_BRM_Barcode as Brm_Parent,
                                f.BRM_BARCODE as Brm_BarCode,
                                f.UNQ_FILE_NO as Clm_No,
                                f.LCTYPE,
                                f.LASTREVIEWDATE as LASTREVIEWDATE,
                0 As IsCombinationCandidate,
                0 As IsMergeCandidate,
                0 As IsNew,
                    '{(session.Office.OfficeType == "RMC" ? "Y" : "N")}' AS IsRmc,
                0 As SocpenIsn,
                '' as Prim_Status,
                '' as Sec_Status,
                0 As RowType,
                0 As StatusCode,
                                '' AS ARCHIVE_YEAR,
                                null as DateApproved,
                0 As Trans_type,
                '' AS IdHistory,
                'Brm' as Source,
                '' as BrmUserName,
                0 as IsSelected
                        from Dc_File f
                        inner join DC_REGION rg on f.Region_ID = rg.REGION_ID
                        inner join DC_Grant_type g on g.TYPE_ID = f.GRANT_TYPE
                        left join DC_Merge m on f.BRM_BARCODE = m.BRM_BARCODE
                            where f.APPLICANT_NO  = '{SearchId}' and f.BRM_BARCODE is not null
                        order by f.TRANS_DATE desc";

                return await _context.Applications.FromSqlRaw(sql).AsNoTracking().ToListAsync();
            }

        }

        /// <summary>
        /// Get SOCPEN Docs for this Application
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="grantype"></param>
        /// <param name="applicationDate"></param>
        /// <returns></returns>
        private async Task<string> GetDocsPresent(string idNo, string grantype, string applicationDate)
        {

            try
            {
                using (var _context = _contextFactory.CreateDbContext())
                {
                    string sql = "";
                    if (string.IsNullOrEmpty(applicationDate))
                    {
                        sql = $@"select DISTINCT DI.DOC_NO_IN AS IdString
                            from SASSA.SOCPEN_DOW_APPLICATIONS_CHEC01 DAC
                            LEFT JOIN SASSA.SOCPEN_DOCUMENTS_IN DI ON DI.ADABAS_ISN = DAC.ADABAS_ISN
                            LEFT JOIN SASSA.SOCPEN_DOC_REL_IN DRI ON DRI.ADABAS_ISN = DI.ADABAS_ISN AND DRI.DPS_PE_SEQ = DI.DPS_PE_SEQ
                            where ID_NO = '{idNo}'
                            and GRANT_TYPE = '{grantype}'
                            and DPS_MU_SEQ = '001'
                            and DOC_REL_IN = 'Y'";
                    }
                    else
                    {

                        sql = $@"select DISTINCT DI.DOC_NO_IN AS IdString
                            from SASSA.SOCPEN_DOW_APPLICATIONS_CHEC01 DAC
                            LEFT JOIN SASSA.SOCPEN_DOCUMENTS_IN DI ON DI.ADABAS_ISN = DAC.ADABAS_ISN
                            LEFT JOIN SASSA.SOCPEN_DOC_REL_IN DRI ON DRI.ADABAS_ISN = DI.ADABAS_ISN AND DRI.DPS_PE_SEQ = DI.DPS_PE_SEQ
                            where ID_NO = '{idNo}'
                            and GRANT_TYPE = '{grantype}'
                            and DPS_MU_SEQ = '001'
                            and DOC_REL_IN = 'Y' 
                            AND APPLICATION_DATE = '{applicationDate.ToUpper()}'";
                    }
                    var query = await _context.IdResults.FromSqlRaw(sql).AsNoTracking().ToListAsync();

                    if (query.Any())
                    {
                        query = query.Distinct().ToList();
                        StringBuilder result = new StringBuilder();
                        foreach (var doc in query)
                        {
                            result.Append(doc.IdString + ";");
                        }

                        return result.ToString().TrimEnd(';');
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        //public async Task<string> GetSocPenSearchId(string SRDNo)
        //{

        //    List<IdResult> srdquery = await _context.IdResults.FromSqlRaw
        //            ($@"select cast(srdben.ID_NO as nvarchar2(13)) as IdString
        //            from SASSA.SOCPEN_SRD_BEN srdben
        //            left
        //            join SASSA.SOCPEN_SRD_TYPE srdtype on srdtype.SOCIAL_RELIEF_NO = srdben.SRD_NO
        //            inner
        //            join DC_REGION rg on rg.REGION_ID = cast(srdben.PROVINCE as NUMBER)
        //            where cast(srdben.SRD_NO as NUMBER) = cast('{SRDNo}' as NUMBER)").AsNoTracking().ToListAsync();

        //    if (!srdquery.Any())
        //    {
        //        throw new Exception("SRD not found.");
        //    }
        //    foreach (IdResult value in srdquery)
        //    {
        //        if (value.IdString == null) throw new Exception("SRD has no Id Number associated and can't be processed.");
        //        return value.IdString;
        //    }
        //    return null;
        //}

        //private string GetWhereclause(string SearchId, bool FullSearch)
        //{
        //    return FullSearch ? $" where '{SearchId}' in (spn.PENSION_NO, spn.OLD_ID1, spn.OLD_ID2, spn.OLD_ID3, spn.OLD_ID4, spn.OLD_ID5, spn.OLD_ID6, spn.OLD_ID7, spn.OLD_ID8, spn.OLD_ID9, spn.OLD_ID10) " : $" where spn.PENSION_NO  = '{SearchId}' ";
        //}

        //private string GetStatusFromSocpen(Application application)
        //{
        //    string mystatus;

        //    switch (application.Child_Status_Code)
        //    {
        //        case null:
        //            string checkstatus = application.Prim_Status == null ? "" : application.Prim_Status.Trim() + (application.Sec_Status == null ? "" : application.Sec_Status.Trim());
        //            if (checkstatus == "B2" || checkstatus == "A2" || checkstatus == "92")
        //            {
        //                mystatus = "MAIN";
        //            }
        //            else
        //            {
        //                mystatus = "ARCHIVE";
        //            }
        //            break;

        //        case "1":
        //            mystatus = "MAIN";
        //            break;

        //        default:
        //            mystatus = "ARCHIVE";
        //            break;
        //    }
        //    return mystatus;
        //}

        //public async Task<DcFile> RemoveDuplicateBRM(string brmNo)
        //{

        //    var files = _context.DcFiles.Where(k => k.BrmBarcode == brmNo);
        //    if (files.Count() > 1)
        //    {
        //        foreach (var dcfile in files)
        //        {
        //            if (string.IsNullOrEmpty(dcfile.ApplicantNo))
        //            {
        //                await BackupDcFileEntry(dcfile);
        //                _context.DcFiles.Remove(dcfile);
        //                await _context.SaveChangesAsync();
        //            }
        //        }
        //    }
        //    files = _context.DcFiles.Where(k => k.BrmBarcode == brmNo);
        //    if (files.Any())
        //    {
        //        return files.FirstOrDefault();
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        public async Task RemoveBRM(string brmNo, string reason)
        {
            using (var _context = _contextFactory.CreateDbContext())
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
                        CreateActivity("Delete" + GetFileArea(dcfile.SrdNo, dcfile.Lctype), "Delete BRM Record", dcfile.UnqFileNo);

                    }
                }
                var merges = await _context.DcMerges.Where(m => m.BrmBarcode == brmNo || m.ParentBrmBarcode == brmNo).ToListAsync();
                foreach (var merge in merges.ToList())
                {
                    _context.DcMerges.Remove(merge);
                }
                //if (_context.DcBrmGrants.Where(d => d.BrmBarcode == brmNo).Any())
                //{
                //    _context.DcBrmGrants.Remove(_context.DcBrmGrants.Where(d => d.BrmBarcode == brmNo).First());
                //}
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
            using (var _context = _contextFactory.CreateDbContext())
            {
                DcFileDeleted removed = new DcFileDeleted();
                file.UpdatedByAd = session.SamName;
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
        #endregion

        #region Batching

        public async Task<decimal?> CreateBatchForUser(string sRegType)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                DcBatch batch;
                List<DcBatch> batches = new List<DcBatch>();
                //Get open batch for User
                if (session.IsRmc())
                {
                    batches = await _context.DcBatches.Where(b => b.OfficeId == session.Office.OfficeId && b.BatchStatus == "RMCBatch" && b.UpdatedByAd == session.SamName && b.RegType == sRegType).ToListAsync();
                }
                else
                {
                    batches = await _context.DcBatches.Where(b => b.OfficeId == session.Office.OfficeId && b.BatchStatus == "Open" && b.UpdatedByAd == session.SamName && b.RegType == sRegType).ToListAsync();
                }

                if (batches.ToList().Any())
                {
                    batch = batches.First();
                    if (batch.NoOfFiles > 34 && !sRegType.StartsWith("LC"))
                    {
                        throw new Exception($"Batch is full. Please verify and close batch before adding more to batch for {sRegType}");
                    }
                }
                else
                {
                    try
                    {
                        batch = new DcBatch
                        {
                            BatchStatus = session.IsRmc() ? "RMCBatch" : "Open",
                            BatchCurrent = "Y",
                            OfficeId = session.Office.OfficeId,
                            RegType = sRegType,
                            UpdatedDate = DateTime.Now,
                            UpdatedBy = 0,
                            UpdatedByAd = session.SamName
                        };
                        _context.DcBatches.Add(batch);

                        await _context.SaveChangesAsync();
                    }
                    catch
                    {
                        throw;
                    }

                }
                return batch.BatchNo;
            }
        }

        /// <summary>
        /// LO -> Open|Closed|Transport RMC => Received|Delivered
        /// </summary>
        /// <param name="listIsFor"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<PagedResult<DcBatch>> GetBatches(string status, int page)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                PagedResult<DcBatch> result = new PagedResult<DcBatch>();
                if (session.IsRmc())
                {
                    result.count = _context.DcBatches.Where(b => b.BatchStatus == "RMCBatch" && b.NoOfFiles > 0 && b.OfficeId == session.Office.OfficeId).Count();
                    result.result = await _context.DcBatches.Where(b => b.BatchStatus == "RMCBatch" && b.NoOfFiles > 0 && b.OfficeId == session.Office.OfficeId).OrderByDescending(b => b.UpdatedDate).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
                }
                else
                {
                    if (status != "")
                    {
                        result.count = _context.DcBatches.Where(b => b.BatchStatus == status && b.OfficeId == session.Office.OfficeId).Count();
                        result.result = await _context.DcBatches.Where(b => b.BatchStatus == status && b.OfficeId == session.Office.OfficeId).OrderByDescending(b => b.UpdatedDate).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
                    }
                    else
                    {
                        result.count = _context.DcBatches.Where(b => b.OfficeId == session.Office.OfficeId).Count();
                        result.result = await _context.DcBatches.Where(b => b.OfficeId == session.Office.OfficeId).OrderByDescending(b => b.UpdatedDate).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
                    }
                }

                return result;
            }
        }
        //The query uses a row limiting operator ('Skip'/'Take') without an 'OrderBy' operator.
        //This may lead to unpredictable results.
        //If the 'Distinct' operator is used after 'OrderBy', then make sure to use the 'OrderBy' operator after 'Distinct' as the ordering would otherwise get erased.
        public async Task<PagedResult<DcBatch>> FindBatch(decimal searchBatch, int page = 1)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                PagedResult<DcBatch> result = new PagedResult<DcBatch>();
                if (session.IsRmc())
                {
                    result.count = _context.DcBatches.Where(b => b.BatchStatus == "RMCBatch" && b.NoOfFiles > 0 && b.OfficeId == session.Office.OfficeId && b.BatchNo == searchBatch).Count();
                    result.result = await _context.DcBatches.Where(b => b.BatchStatus == "RMCBatch" && b.NoOfFiles > 0 && b.OfficeId == session.Office.OfficeId && b.BatchNo == searchBatch).OrderByDescending(b => b.UpdatedDate).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
                }
                else
                {
                    result.count = _context.DcBatches.Where(b => b.OfficeId == session.Office.OfficeId).Count();
                    result.result = await _context.DcBatches.Where(b => b.OfficeId == session.Office.OfficeId && b.BatchNo == searchBatch).OrderByDescending(b => b.UpdatedDate).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
                }

                return result;
            }
        }

        public async Task<PagedResult<DcBatch>> GetMyBatches(bool myBatches, int page = 1)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                PagedResult<DcBatch> result = new PagedResult<DcBatch>();

                if (myBatches)
                {
                    result.count = _context.DcBatches.Where(b => b.UpdatedByAd == session.SamName).Count();
                    result.result = await _context.DcBatches.Where(b => b.UpdatedByAd == session.SamName).OrderByDescending(b => b.UpdatedDate).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
                }
                else
                {
                    result.count = _context.DcBatches.Where(b => b.OfficeId == session.Office.OfficeId).Count();
                    result.result = await _context.DcBatches.Where(b => b.OfficeId == session.Office.OfficeId).OrderByDescending(b => b.UpdatedDate).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
                }

                return result;
            }
        }
        public async Task SetBatchCount(string batchIds)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                //if (session.IsRmc()) return;
                if (string.IsNullOrEmpty(batchIds)) return;
                decimal batchId = decimal.Parse(batchIds);
                int fileCount = await GetBatchCount(batchId);
                if (fileCount == 0)
                {
                    _context.DcBatches.Remove(_context.DcBatches.Where(b => b.BatchNo == batchId).First());
                    await _context.SaveChangesAsync();
                    return;
                }
                DcBatch batch = await _context.DcBatches.Where(b => b.BatchNo == batchId).FirstAsync();
                if (batch == null) return;
                batch.NoOfFiles = fileCount;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<int> GetBatchCount(decimal batchId)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                return await _context.DcFiles.CountAsync(b => b.BatchNo == batchId);
            }
        }
        public async Task SetBatchStatus(string batchIdS, string newStatus)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                if (string.IsNullOrEmpty(batchIdS)) return;
                decimal batchId = decimal.Parse(batchIdS);
                DcBatch batch = _context.DcBatches.Where(b => b.BatchNo == batchId).First();
                batch.BatchStatus = newStatus;
                if (newStatus == "Closed" && session.Office.OfficeType == "LO")
                {
                    batch.BrmWaybill = await GetNextOpenBrmWaybill(batchId);
                }
                await _context.SaveChangesAsync();

                //Set the batchcount
                if (newStatus == "Closed")
                {
                    await SetBatchCount(batchIdS);

                }
            }
            CreateActivity("Batching", $"Status {newStatus}");
        }
        public async Task<string> GetNextOpenBrmWaybill(decimal? batchId)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                //See if there is an Open Waybill for this office
                var waybillbatches = await _context.DcBatches.Where(b => b.BatchStatus == "Closed" && b.OfficeId == session.Office.OfficeId).ToListAsync();
                if (waybillbatches.Any())
                {
                    string waybillNo;
                    if (string.IsNullOrEmpty(waybillbatches.First().BrmWaybill))
                    {
                        waybillNo = $"{session.Office.OfficeId}-{_raw.GetNextWayBill()}";
                        foreach (var batch in waybillbatches)
                        {
                            batch.BrmWaybill = waybillNo;
                        }
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        waybillNo = waybillbatches.First().BrmWaybill;
                    }

                    return waybillNo;
                }
                else
                {
                    return $"{session.Office.OfficeId}-{_raw.GetNextWayBill()}";
                }
            }
        }

        public async Task RemoveFileFromBatch(string brmBarCode)
        {
            decimal? batchNo;
            DcFile file;
            using (var _context = _contextFactory.CreateDbContext())
            {
                _context.ChangeTracker.Clear();
                file = await _context.DcFiles.Where(f => f.BrmBarcode == brmBarCode).FirstAsync();
                batchNo = file.BatchNo;
                file.BatchNo = 0;
                await _context.SaveChangesAsync();
            }
            CreateActivity("Batching" + GetFileArea(file.SrdNo, file.Lctype), "Remove File", file.UnqFileNo);
            if (batchNo != 0) await SetBatchCount(batchNo.ToString());

        }

        public async Task AddFileToBatch(string brmBarCode, decimal batchNo)
        {
            try
            {
                DcFile file;
                decimal sourceBatch = 0;
                using (var _context = _contextFactory.CreateDbContext())
                {
                    file = await _context.DcFiles.Where(f => f.BrmBarcode == brmBarCode).FirstAsync();

                    if (file.BatchNo != 0)
                    {
                        if (_context.DcBatches.Where(b => b.BatchNo == file.BatchNo && b.BatchStatus != "Open").Any())
                        {
                            throw new Exception($"This file is in closed batch: {file.BatchNo} and cant be added.");
                        }
                        sourceBatch = (decimal)file.BatchNo;
                    }
                    var batch = await _context.DcBatches.Where(b => b.BatchNo == batchNo && b.BatchStatus == "Open").FirstAsync();
                    if (file.RegType != batch.RegType)
                    {
                        throw new Exception($"This file is not of reg Type {batch.RegType} and cant be added.");
                    }

                    file.BatchNo = batchNo;

                    await _context.SaveChangesAsync();
                }
                CreateActivity("Batching" + GetFileArea(file.SrdNo, file.Lctype), "Add File", file.UnqFileNo);
                await SetBatchCount(batchNo.ToString());
                if (sourceBatch != 0) await SetBatchCount(sourceBatch.ToString());
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<DcFile>> GetAllFilesByBatchNo(decimal BatchId)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                return await _context.DcFiles.Where(f => f.BatchNo == BatchId).ToListAsync();
            }
        }
        public async Task<PagedResult<DcFile>> GetAllFilesByBatchNo(decimal batchId, int page)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                //List<DcFile> files = await _context.DcFiles.Where(f => f.BatchNo == batchId).ToListAsync();
                PagedResult<DcFile> result = new PagedResult<DcFile>();
                result.count = _context.DcFiles.Where(f => f.BatchNo == batchId).Count();
                result.result = await _context.DcFiles.OrderByDescending(f => f.UpdatedDate).Where(f => f.BatchNo == batchId).Skip((page - 1) * 12).Take(12).ToListAsync();
                foreach (var file in result.result)
                {
                    var merge = await _context.DcMerges.FirstOrDefaultAsync(m => m.BrmBarcode == file.BrmBarcode);
                    if (merge == null) continue;
                    file.MergeStatus = merge.BrmBarcode == merge.ParentBrmBarcode ? "Parent" : "Merged";
                }
                return result;
            }
        }
        /// <summary>
        /// Remove Merged files from batch
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        public async Task SetParentBatchCount(string batchIds)
        {
            if (session.IsRmc()) return;
            if (string.IsNullOrEmpty(batchIds)) return;
            decimal batchId = decimal.Parse(batchIds);
            bool updated = false;
            using (var _context = _contextFactory.CreateDbContext())
            {
                List<DcFile> files = await _context.DcFiles.Where(f => f.BatchNo == batchId).ToListAsync();
                foreach (var file in files)
                {
                    var merge = await _context.DcMerges.FirstOrDefaultAsync(m => m.BrmBarcode == file.BrmBarcode);
                    if (merge == null) continue;
                    file.MergeStatus = merge.BrmBarcode == merge.ParentBrmBarcode ? "Parent" : "Merged";
                    if (merge.BrmBarcode != merge.ParentBrmBarcode)
                    {
                        file.BatchNo = 0;
                        updated = true;
                    }
                }
                if (updated)
                {
                    await _context.SaveChangesAsync();
                }
            }
            if (updated)
            {
                await SetBatchCount(batchIds);
            }

        }


        public async Task<List<Waybill>> GetBatchWaybills()
        {
            try
            {
                List<Waybill> result = new List<Waybill>();
                List<DcBatch> batches;
                using (var _context = _contextFactory.CreateDbContext())
                {
                    if (session.Office.OfficeType == "RMC")
                    {
                        List<string> offices = sservice.GetOfficeIds(session.Office.RegionId);

                        batches = await _context.DcBatches.Where(b => b.BatchStatus == "Transport" && offices.Contains(b.OfficeId)).ToListAsync();

                    }
                    else
                    {
                        batches = await _context.DcBatches.Where(b => b.OfficeId == session.Office.OfficeId && b.BatchStatus != "Completed").ToListAsync();
                    }

                    string brmWaybill = "";
                    Waybill waybill = null;
                    foreach (var batch in batches.OrderBy(b => b.BrmWaybill))
                    {
                        if (string.IsNullOrEmpty(batch.BrmWaybill)) continue;
                        if (batch.BrmWaybill != brmWaybill)
                        {
                            if (waybill != null)
                            {
                                result.Add(waybill);
                            }
                            brmWaybill = batch.BrmWaybill;
                            waybill = new Waybill();
                            waybill.OfficeId = batch.OfficeId;
                            waybill.Status = batch.BatchStatus;
                            waybill.BrmWaybill = batch.BrmWaybill;
                            waybill.CourierName = batch.CourierName;
                            waybill.WaybillNo = batch.WaybillNo;
                            waybill.UpdatedByAd = session.SamName;
                            waybill.UpdatedDate = batch.UpdatedDate;//DateTime.Now;

                        }
                        waybill.NoOfFiles = waybill.NoOfFiles + (int)batch.NoOfFiles;
                        waybill.NoOfBatches++;
                    }
                    if (waybill != null)
                    {
                        result.Add(waybill);
                    }
                    return result;
                }
            }
            catch
            {
                throw;
            }
        }



        public async Task<List<DcBatch>> GetWaybillBatches(string brmWaybill)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                return await _context.DcBatches.Where(b => b.BrmWaybill == brmWaybill).ToListAsync();
            }
        }

        public async Task<List<DcBatch>> GetWaybillBoxes(string brmWaybill)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                //todo:Create DC_Box
                return await _context.DcBatches.Where(b => b.BrmWaybill == brmWaybill).ToListAsync();
            }
        }

        public async Task<List<string>> GetBatchBarcodes(string BatchNo)
        {
            decimal batch;
            using (var _context = _contextFactory.CreateDbContext())
            {
                if (!decimal.TryParse(BatchNo, out batch))
                {
                    throw new Exception("Invalid batch No.");
                }
                return await (from file in _context.DcFiles.Where(f => f.BatchNo == batch) select file.BrmBarcode).ToListAsync();
            }
        }

        public async Task<List<string>> GetPickListBarcodes(string pickListNo)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                return await (from pli in _context.DcPicklistItems.Where(f => f.UnqPicklist == pickListNo) select pli.BrmNo).ToListAsync();
            }
        }
        public async Task DispatchWaybill(string brmWaybill, string tdwWaybill)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                List<DcBatch> batches = await _context.DcBatches.Where(b => b.BrmWaybill == brmWaybill).ToListAsync();

                foreach (var batch in batches)
                {
                    batch.BatchStatus = "Transport";
                    batch.WaybillNo = tdwWaybill;
                    batch.WaybillDate = DateTime.Now;
                    batch.CourierName = "TDW";
                    batch.BatchCurrent = "N";
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task ReceiveWaybill(string brmWaybill, string tdwWaybill)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                List<DcBatch> batches = await _context.DcBatches.Where(b => b.BrmWaybill == brmWaybill).ToListAsync();
                foreach (var batch in batches)
                {
                    batch.BatchStatus = "Received";
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Reboxing> GetBoxCounts(Reboxing rebox)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var boxFiles = await _context.DcFiles.Where(f => f.TdwBoxno == rebox.BoxNo).ToListAsync();
                if (boxFiles.Any())
                {
                    rebox.BoxCount = boxFiles.Count();
                    rebox.RegType = boxFiles.First().RegType;
                    if (string.IsNullOrEmpty(rebox.SelectedType))
                    {
                        // 1 main 14 archive
                        //13 main lc 18 archive lc
                        switch (rebox.RegType)
                        {
                            case "LC-MAIN":
                                rebox.SelectedType = "13";
                                break;
                            case "LC-ARCHIVE":
                                rebox.SelectedType = "18";
                                break;
                            case "MAIN":
                                rebox.SelectedType = "1";
                                break;
                            case "ARCHIVE":
                                rebox.SelectedType = "14";
                                break;
                            default:
                                rebox.SelectedType = "1";
                                break;
                        }

                    }
                    rebox.MiniBoxCount = boxFiles.Where(b => b.MiniBoxno == rebox.MiniBox).Count();
                }
                else
                {
                    rebox.BoxCount = 0;
                    rebox.RegType = null;
                    rebox.MiniBoxCount = 0;
                }

                //rebox.MiniBoxCount = await _context.DcFiles.CountAsync(b => b.TdwBoxno == rebox.BoxNo && b.MiniBoxno == rebox.MiniBox);

                return rebox;
            }
        }
        #endregion

        #region Enquiry page
        public async Task<Enquiry> GetEnquiry(string brmBarCode)
        {
            Enquiry result = new Enquiry();
            using (var _context = _contextFactory.CreateDbContext())
            {
                var dcfiles = await _context.DcFiles.Where(f => f.BrmBarcode == brmBarCode && !string.IsNullOrEmpty(f.ApplicantNo)).ToListAsync();
                if (!dcfiles.Any()) throw new Exception("Barcode not found");
                if (dcfiles.Count() > 1)
                {
                    throw new Exception("Duplicate Brm Record please delete duplicate first.");
                }
                DcFile file = dcfiles.First();
                CreateActivity("Enquiry" + GetFileArea(file.SrdNo, file.Lctype), "Enquiry", file.UnqFileNo);

                var merged = await _context.DcMerges.Where(m => m.BrmBarcode == brmBarCode).ToListAsync();

                result.AppDate = file.TransDate == null ? "" : ((DateTime)file.TransDate).ToString("dd/MMM/yy");
                result.ApplicantNo = file.ApplicantNo;
                result.AppType = sservice.GetTransactionType((int)file.TransType);
                result.BrmBarCode = brmBarCode;
                result.MisFileNo = file.FileNumber;
                result.BrmRecord = true;
                result.CaptureDate = (DateTime)file.BatchAddDate;
                result.CsgStatus = "";
                result.GrantType = sservice.GetGrantType(file.GrantType);
                result.LastAction = file.UpdatedDate;
                result.MultiGrant = merged.Any();
                if (result.MultiGrant)
                {
                    result.MergeParent = merged.First().ParentBrmBarcode;
                }
                result.Province = sservice.GetRegion(file.RegionId);
                result.RegType = file.RegType;
                result.SocPenActive = false;
                result.SocPenRecord = false;

                result.UnqFileNo = file.UnqFileNo;

                //Tdw
                var tdwresult = await _context.TdwFileLocations.Where(t => t.FilefolderCode == brmBarCode).ToListAsync();
                result.TdwRecord = tdwresult.Any();

                //SocPen
                var socpenresult = await _context.DcSocpen.Where(s => s.BeneficiaryId == file.ApplicantNo && s.GrantType == file.GrantType).ToListAsync();//SearchSocpenId(file.ApplicantNo, false);
                if (socpenresult.Any())
                {
                    result.SocPenRecord = true;
                    result.SocPenActive = socpenresult.Where(s => s.StatusCode == "ACTIVE").Any();
                }
                return result;
            }
        }

        public async Task<List<DcFileDeleted>> GetDeleteHistory(string idNumber)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                return await _context.DcFileDeleteds.Where(d => d.ApplicantNo == idNumber).ToListAsync();
            }
        }

        public async Task<List<Enquiry>> GetEnquiryById(string idNumber)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                List<Enquiry> resultlist = new List<Enquiry>();
                var dcfiles = await _context.DcFiles.Where(f => f.ApplicantNo.Contains(idNumber.Trim())).ToListAsync();
                if (!dcfiles.Any()) throw new Exception("Applicant Id not found");
                CreateActivity("Enquiry" + GetFileArea(dcfiles.First().SrdNo, dcfiles.First().Lctype), "Enquiry", dcfiles.First().UnqFileNo);
                foreach (DcFile file in dcfiles)
                {
                    Enquiry result = new Enquiry();

                    result.AppDate = file.TransDate == null ? "" : ((DateTime)file.TransDate).ToString("dd/MMM/yy");
                    result.ApplicantNo = file.ApplicantNo;
                    result.AppType = sservice.GetTransactionType((int)file.TransType);
                    result.BrmBarCode = file.BrmBarcode;
                    result.MisFileNo = file.FileNumber;
                    result.BrmRecord = true;
                    result.CaptureDate = (DateTime)file.BatchAddDate;
                    result.CsgStatus = "";
                    result.GrantType = sservice.GetGrantType(file.GrantType);
                    result.LastAction = file.UpdatedDate;
                    var merged = _context.DcMerges.Where(m => m.BrmBarcode == file.BrmBarcode).ToList();
                    if (merged.Any())
                    {
                        result.MergeParent = (merged.First()).ParentBrmBarcode;
                    }
                    result.MultiGrant = merged.Any();
                    result.Province = sservice.GetRegion(file.RegionId);
                    result.RegType = file.RegType;
                    result.SocPenActive = false;
                    result.SocPenRecord = false;

                    result.UnqFileNo = file.UnqFileNo;

                    //Tdw
                    //var tdwresult = _context.TdwFileLocations.Where(t => t.FilefolderCode == file.BrmBarcode);
                    result.TdwRecord = _context.DcSocpen.Where(f => f.BeneficiaryId == file.ApplicantNo && f.GrantType == file.GrantType && f.TdwRec != null).ToList().Any();

                    List<Application> spresult = null;
                    //SocPen
                    if (idNumber.Contains("S"))
                    {
                        if (long.TryParse(idNumber.Replace("S", ""), out long lsrd))
                        {
                            spresult = await SearchSocpenSrd(lsrd);
                            result.SocPenRecord = true;
                            result.SocPenActive = spresult.First().AppStatus.Contains("MAIN");
                        }

                    }
                    else
                    {
                        spresult = await SearchSocpenId(idNumber, false);
                        if (spresult.Any())
                        {
                            foreach (Application sr in spresult)
                            {
                                if (string.IsNullOrEmpty(result.AppDate)) continue;
                                if (sr.GrantType == file.GrantType && sr.Id == result.ApplicantNo && sr.AppDate == result.AppDate)
                                {
                                    result.SocPenRecord = true;
                                    result.SocPenActive = sr.AppStatus.Contains("MAIN");
                                    break;
                                }
                            }
                        }
                    }



                    resultlist.Add(result);
                }
                return resultlist;
            }
        }

        public async Task<List<Enquiry>> GetEnquiryBySrd(string idNumber)
        {
            List<Enquiry> resultlist = new List<Enquiry>();
            using (var _context = _contextFactory.CreateDbContext())
            {
                var dcfiles = await _context.DcFiles.Where(f => f.SrdNo.Contains(idNumber.Trim())).ToListAsync();
                if (!dcfiles.Any()) throw new Exception("SRD not found");

                foreach (DcFile file in dcfiles)
                {
                    Enquiry result = new Enquiry();

                    result.AppDate = file.TransDate == null ? "" : ((DateTime)file.TransDate).ToString("dd/MMM/yy");
                    result.ApplicantNo = file.ApplicantNo;
                    result.AppType = sservice.GetTransactionType((int)file.TransType);
                    result.BrmBarCode = file.BrmBarcode;
                    result.MisFileNo = file.FileNumber;
                    result.BrmRecord = true;
                    result.CaptureDate = (DateTime)file.BatchAddDate;
                    result.CsgStatus = "";
                    result.GrantType = sservice.GetGrantType(file.GrantType);
                    result.LastAction = file.UpdatedDate;
                    var merged = _context.DcMerges.Where(m => m.BrmBarcode == file.BrmBarcode);
                    if (merged.Any())
                    {
                        result.MergeParent = (await merged.FirstAsync()).ParentBrmBarcode;
                    }
                    result.MultiGrant = merged.Any();
                    result.Province = sservice.GetRegion(file.RegionId);
                    result.RegType = file.RegType;
                    result.SocPenActive = false;
                    result.SocPenRecord = false;

                    result.UnqFileNo = file.UnqFileNo;

                    //Tdw
                    var tdwresult = _context.TdwFileLocations.Where(t => t.FilefolderCode == file.BrmBarcode);
                    result.TdwRecord = tdwresult.Any();

                    //SocPen
                    if (long.TryParse(idNumber.Replace("S", ""), out long lsrd))
                    {
                        var spresult = await SearchSocpenSrd(lsrd);
                        if (spresult.Any())
                        {
                            result.SocPenRecord = true;
                            result.SocPenActive = spresult.First().AppStatus.Contains("MAIN");
                        }
                    }
                    //var socpenresult = await SearchSocPenID(file.ApplicantNo, false);
                    //if (socpenresult.Any())
                    //{
                    //    foreach (Application sr in socpenresult)
                    //    {
                    //        if (string.IsNullOrEmpty(result.AppDate)) continue;
                    //        if (sr.GrantType == file.GrantType && sr.Id == result.ApplicantNo && sr.AppDate == result.AppDate)
                    //        {
                    //            result.SocPenRecord = true;
                    //            result.SocPenActive = GetStatusFromSocpen(sr).Contains("MAIN");
                    //            break;
                    //        }
                    //    }
                    //}

                    resultlist.Add(result);
                }
                return resultlist;
            }
        }
        public async Task<List<DcActivity>> GetFileActivity(string unqFile)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                return await _context.DcActivities.Where(a => a.UnqFileNo == unqFile).ToListAsync();
            }
        }
        #endregion

        #region Audit
        /// <summary>
        /// Gets PieData on File requests
        /// </summary>
        /// <returns></returns>
        public PieData GetRequestPieData()
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                PieData pd = new PieData();
                pd.ChartName = "File request progress";
                int colorindex = 0;
                double total = _context.DcFileRequests.Count();
                pd.TotalItems = (int)total;
                List<RawSegment> query = _context.DcFileRequests
                    .GroupBy(c => c.Status)
                    .Select(o => new RawSegment
                    {
                        Name = o.Key,
                        Count = o.Where(c => c.Status == o.Key).Count()
                    }).ToList();

                foreach (var rs in query)
                {
                    PieSegment ps = new PieSegment();
                    ps.Name = rs.Name;
                    ps.Percent = rs.Count / (total / 100);
                    ps.Color = StaticDataService.PastelColors[colorindex++];
                    pd.Segments.Add(ps);
                }

                return pd;
            }
        }

        public PieData GetRequestPieData(string regionId)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                PieData pd = new PieData();
                pd.ChartName = sservice.GetRegion(regionId) + " region.";
                int colorindex = 0;
                double total = _context.DcFileRequests.Where(r => r.RegionId == regionId).Count();
                pd.TotalItems = (int)total;
                List<RawSegment> query = _context.DcFileRequests
                    .Where(r => r.RegionId == regionId)
                    .GroupBy(c => c.Status)
                    .Select(o => new RawSegment
                    {
                        Name = o.Key,
                        Count = o.Where(c => c.Status == o.Key).Count()
                    }).ToList();

                foreach (var rs in query)
                {
                    PieSegment ps = new PieSegment();
                    ps.Name = rs.Name;
                    ps.Percent = rs.Count / (total / 100);
                    ps.Color = StaticDataService.PastelColors[colorindex++];
                    pd.Segments.Add(ps);
                }

                return pd;
            }
        }
        #endregion

        #region Destruction 

        string BatchResult = string.Empty;
        string Errors = string.Empty;
        public int ErrorCount = 0;
        public async Task AddExclusionFile(byte[] contentBytes, string exclusionType)
        {
            string content = Encoding.UTF8.GetString(contentBytes, 0, contentBytes.Length);
            var rows = content.Replace("\r\n", "|").Split('|');
            List<DcExclusion> values = rows
                        .Skip(1)
                        .Select(v => Extentions.FromCsv(v.Trim(), exclusionType, session.Office.RegionId, session.SamName))
                        .ToList();

            foreach (DcExclusion exclusion in values)
            {
                try
                {
                    await AddExclusion(exclusion.IdNo, exclusionType);
                }
                catch
                {
                    //Todo: count error
                    ErrorCount++;
                }

            }
        }

        public List<string> UndestroyedYears()
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var existingyears = _context.DcExclusionBatches.Where(ex => ex.RegionId == int.Parse(session.Office.RegionId)).Select(ex => ex.ExclusionYear).ToList();
                return StaticDataService.DestructionYears.Except(existingyears).ToList();
            }
        }

        public async Task UpdateDestructionStatus(string pension_no, string status)
        {
            if (string.IsNullOrEmpty(pension_no))
            {
                BatchResult += "Empty entry Ignored. " + Environment.NewLine;
                return;
            }
            try
            {
                using (var _context = _contextFactory.CreateDbContext())
                {
                    DcDestruction de = _context.DcDestructions.Where(d => d.PensionNo == pension_no).First();
                    if (de == null)
                    {
                        BatchResult += pension_no + " Not Found. " + Environment.NewLine;
                    }
                    else
                    {
                        de.Status = status;
                        de.StatusDate = DateTime.Now.ToString("yyyyMMdd");
                        await _context.SaveChangesAsync();
                    }
                }

            }
            catch
            {
                throw;
            }
        }

        public async Task AddExclusion(string pension_no, string exclusionType)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                if (string.IsNullOrEmpty(pension_no)) throw new System.Exception("Invalid Id");
                string PensionNo = pension_no.GetDigitId();
                if (string.IsNullOrEmpty(exclusionType)) throw new System.Exception("Invalid Exclusion type");
                //check for valid exclusionType
                if (!StaticDataService.ExclusionTypes.Contains(exclusionType))
                {
                    ErrorCount++;
                    throw new System.Exception("Invalid Exclusion type");
                }
                //Check if duplicate
                if (_context.DcExclusions.Where(d => d.IdNo == pension_no).Any())
                {

                    ErrorCount++;
                    throw new System.Exception("Duplicate exclusion");
                }

                //todo: Check if batch exists for this year/region
                DcExclusion exclusion = new DcExclusion
                {
                    ExclusionType = exclusionType,
                    RegionId = decimal.Parse(session.Office.RegionId),
                    IdNo = pension_no,
                    Username = session.SamName,
                    ExclusionBatchId = 0,
                    ExclDate = DateTime.Now
                };
                //Check if Destruction record exists
                if (_context.DcDestructions.Where(d => d.PensionNo == pension_no).Any())
                {
                    await UpdateDestructionStatus(pension_no, "Excluded");
                }

                _context.DcExclusions.Add(exclusion);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveExclusion(decimal id)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var exclusion = await _context.DcExclusions.FindAsync(id);
                if (exclusion == null) return;
                _context.DcExclusions.Remove(exclusion);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<int> GetExclusionBatch(string destructionYear)
        {
            DcExclusionBatch exclusionb;
            using (var _context = _contextFactory.CreateDbContext())
            {
                if (string.IsNullOrEmpty(destructionYear)) throw new System.Exception("Destruction Year is required");
                //if there is a batch for this year , use it.
                exclusionb = await _context.DcExclusionBatches.Where(eb => eb.RegionId == decimal.Parse(session.Office.RegionId) && eb.ExclusionYear == destructionYear.Replace(" ", "")).FirstOrDefaultAsync();
                if (exclusionb == null)
                {

                    exclusionb = new DcExclusionBatch
                    {
                        RegionId = decimal.Parse(session.Office.RegionId),
                        ExclusionYear = destructionYear.Replace(" ", ""),
                        CreatedBy = session.SamName,
                        CreatedDate = DateTime.Now.ToString("yyyyMMdd")
                    };

                    _context.DcExclusionBatches.Add(exclusionb);
                    await _context.SaveChangesAsync();
                }
                return Decimal.ToInt32(exclusionb.BatchId);
            }
        }

        public async Task UpdateExclusionBatch(string destructionYear)
        {
            try
            {
                using (var _context = _contextFactory.CreateDbContext())
                {
                    int batchId = await GetExclusionBatch(destructionYear);

                    var exlusions = _context.DcExclusions.Where(e => e.RegionId == decimal.Parse(session.Office.RegionId) && e.ExclusionBatchId == 0);
                    if (!exlusions.Any()) return;
                    foreach (var excl in exlusions.ToList())
                    {
                        DcDestruction dd = _context.DcDestructions.Where(d => d.PensionNo == excl.IdNo).FirstOrDefault();
                        if (dd != null)
                        {
                            dd.ExclusionbatchId = batchId;
                        }
                        excl.ExclusionBatchId = batchId;

                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<PagedResult<DcExclusionBatch>> GetExclusionBatches(string year, int page)
        {
            PagedResult<DcExclusionBatch> result = new PagedResult<DcExclusionBatch>();
            using (var _context = _contextFactory.CreateDbContext())
            {
                result.count = _context.DcExclusionBatches.Where(p => p.ExclusionYear == year).Count();
                result.result = await _context.DcExclusionBatches.Where(p => p.ExclusionYear == year).OrderByDescending(d => d.CreatedDate).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
                return result;
                //using (Entities context = new Entities())
                //{
                //    var regions =
                //        (from r in context.DC_REGION
                //         select new VIEW_REGION()
                //         {
                //             RegionString = r.REGION_ID,
                //             RegionName = r.REGION_NAME
                //         }).ToList();
                //    return context.DC_EXCLUSION_BATCH
                //     .Where(e => e.APPROVED_BY == null && e.EXCLUSION_YEAR == year)
                //     .Select(x =>
                //     new VIEW_EXCLUSION_BATCH()
                //     {
                //         REGION_ID = (int)x.REGION_ID,
                //         APPROVED_BY = x.APPROVED_BY,
                //         BATCH_ID = (int)x.BATCH_ID,
                //         EXCLUSION_YEAR = x.EXCLUSION_YEAR,
                //         CREATED_BY = x.CREATED_BY,
                //         CREATED_DATE = x.CREATED_DATE
                //     })
                //     .AsEnumerable() // database query ends here, the rest is a query in memory
                //     .Join(regions, f => f.REGION_ID, p => p.RegionId, (f, p) =>
                //     new VIEW_EXCLUSION_BATCH()
                //     {
                //         REGION_NAME = p.RegionName,
                //         APPROVED_BY = f.APPROVED_BY,
                //         BATCH_ID = f.BATCH_ID,
                //         EXCLUSION_YEAR = f.EXCLUSION_YEAR,
                //         CREATED_BY = f.CREATED_BY,
                //         CREATED_DATE = f.CREATED_DATE
                //     })
                //     .ToList();

                //}
            }
        }

        public async Task<PagedResult<DcExclusionBatch>> GetApprovedBatches(string year, int page)
        {
            PagedResult<DcExclusionBatch> result = new PagedResult<DcExclusionBatch>();
            using (var _context = _contextFactory.CreateDbContext())
            {
                result.count = _context.DcExclusionBatches.Where(p => p.ExclusionYear == year && !string.IsNullOrEmpty(p.ApprovedBy)).Count();
                result.result = await _context.DcExclusionBatches.Where(p => p.ExclusionYear == year && !string.IsNullOrEmpty(p.ApprovedBy)).OrderByDescending(e => e.CreatedDate).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
                return result;
            }
        }

        public async Task ApproveBatch(decimal batchId)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                DcExclusionBatch batch = _context.DcExclusionBatches.Find(batchId);
                if (batch == null) throw new Exception("Batch not Found");
                batch.ApprovedBy = session.SamName;
                batch.ApprovedDate = System.DateTime.Now.ToString("yyyyMMdd");
                await _context.SaveChangesAsync();
            }

        }

        public async Task<PagedResult<DcExclusion>> getExclusions(int page)
        {
            PagedResult<DcExclusion> result = new PagedResult<DcExclusion>();
            using (var _context = _contextFactory.CreateDbContext())
            {
                result.count = _context.DcExclusions.Where(p => p.RegionId == decimal.Parse(session.Office.RegionId)).Count();
                result.result = await _context.DcExclusions.Where(p => p.RegionId == decimal.Parse(session.Office.RegionId)).OrderByDescending(e => e.ExclDate).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
                return result;
            }
        }

        public async Task SaveDestructionList()
        {
            foreach (var region in StaticDataService.TdwRegions.Keys)
            {
                await SaveDestructionList(region);
            }
        }

        public async Task SaveDestructionList(string regionId)
        {

            string region = StaticDataService.TdwRegions[regionId];
            if (string.IsNullOrEmpty(regionId)) throw new Exception("Invalid Region");
            string sql = $"SELECT d.PENSION_NO,f.REGION,f.CONTAINER_CODE,CONTAINER_ALTCODE,FILEFOLDER_CODE,FILEFOLDER_ALTCODE,d.DESTRUCTIO_DATE,f.NAME from TDW_FILE_LOCATION f JOIN DC_DESTRUCTION d ON d.PENSION_NO = f.DESCRIPTION WHERE f.REGION ='{region}' AND d.PENSION_NO NOT IN (SELECT ID_NO from Dc_Exclusions)";
            DataTable dt = await _raw.GetTable(sql);
            string FileName = session.Office.RegionCode + "-" + session.SamName!.ToUpper() + "-Destruction-" + DateTime.Now.ToShortDateString().Replace("/", "-") + "-" + DateTime.Now.ToString("HH-mm");
            //File.WriteAllText(FileName, dt.ToCSV());
            File.WriteAllText(StaticDataService.ReportFolder + $@"{FileName}.csv", dt.ToCSV());
        }

        public async Task RefreshFromSocPenData(string year)
        {
            string sql = @"INSERT INTO DC_DESTRUCTION d (PENSION_NO, DESTRUCTIO_DATE, STATUS_DATE, STATUS ) " +
                           "SELECT DISTINCT PENSION_NO, '" + year + "0101" + "', TO_CHAR(SYSDATE,'YYYYMMDD'),'Selected' " +
                            "FROM DC_DESTRUCTION_LIST dl " +
                            "WHERE PENSION_NO NOT IN(SELECT PENSION_NO FROM DC_DESTRUCTION) " +
                            "AND SUBSTR(STATUS_DATE,1,4) = '" + year + "'";
            await _raw.ExecuteNonQuery(sql);
            string updateregionSQL = "Update DC_DESTRUCTION D " +
                            "SET Region_ID = " +
                            "(select r.Region_ID from TDW_FILE_LOCATION F " +
                            "Join DC_REGION R on LOWER(TRIM(R.Region_Name)) = LOWER(TRIM(F.region)) " +
                            "Join DC_DESTRUCTION D ON F.DESCRIPTION = D.PENSION_NO " +
                            "WHERE ROWNUM = 1)";
            await _raw.ExecuteNonQuery(updateregionSQL);

        }

        #endregion
    }
}


using Microsoft.EntityFrameworkCore;
using razor.Components.Models;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Models;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;
using Sassa.BRM.ViewModels;
using System.Data;

namespace Sassa.BRM.Services;

public class BoxingService(IDbContextFactory<ModelContext> _contextFactory, RawSqlService _raw, MailMessages _mail, SessionService _sessionService, BrmApiService brmApiService)
{

    private UserSession _userSession = _sessionService.session!;

    #region Boxing and Re-Boxing
    public async Task<PagedResult<ReboxListItem>> GetAllFilesByBoxNo(string boxNo, int page)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            bool repaired = await RepairAltBoxSequence(boxNo);

            PagedResult<ReboxListItem> result = new PagedResult<ReboxListItem>();
            result.count = _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo).Count();

            var interim = _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo).OrderByDescending(f => f.UpdatedDate).ToList();
            result.result = interim.Skip((page - 1) * 20).Take(20).OrderBy(f => f.UnqFileNo)
                        .Select(f => new ReboxListItem
                        {
                            ClmNo = f.UnqFileNo,
                            BrmNo = f.BrmBarcode,
                            IdNo = f.ApplicantNo,
                            FullName = f.FullName,
                            GrantType = StaticDataService.GrantTypes![f.GrantType],
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
            result.count = _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo && (bn.ApplicantNo.Contains(searchText) || bn.BrmBarcode.Contains(searchText))).Count();
            if (result.count == 0) throw new Exception("No result!");
            result.result = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo && (bn.ApplicantNo.Contains(searchText) || bn.BrmBarcode.Contains(searchText))).OrderByDescending(f => f.UpdatedDate).Skip((page - 1) * 20).Take(20).OrderBy(f => f.UnqFileNo).AsNoTracking()
                        .Select(f => new ReboxListItem
                        {
                            ClmNo = f.UnqFileNo,
                            BrmNo = f.BrmBarcode,
                            IdNo = f.ApplicantNo,
                            FullName = f.FullName,
                            GrantType = StaticDataService.GrantTypes![f.GrantType],
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
            var interim = await _context.DcFiles.Where(b => b.TdwBoxno == boxNo && b.BoxLocked == 1).ToListAsync();
            return interim.Any();
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
            if (notScanned)
            {
                var interimNs = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo && bn.ScanDatetime == null).OrderBy(f => f.UnqFileNo).AsNoTracking().ToListAsync();
                return interimNs.Select(f => new ReboxListItem
                {
                    ClmNo = f.UnqFileNo,
                    BrmNo = f.BrmBarcode,
                    IdNo = f.ApplicantNo,
                    FullName = f.FullName,
                    GrantType = StaticDataService.GrantTypes![f.GrantType],
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
                GrantType = StaticDataService.GrantTypes![f.GrantType],
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
                if (item != null) await SyncPicklistFromItems(item.UnqPicklist, "Returned");
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
                    Year = (parent.UpdatedDate ?? DateTime.Now).ToString("YYYY"),
                    Location = parent.TdwBoxno,
                    Reg = parent.RegType,
                    //Bin  = parent. ,
                    Box = parent.MiniBoxno.ToString(),
                    //Pos  = parent. ,
                    UserPicked = ""
                };
                tpl.Add(TdwFormat);

            }
            string FileName = _userSession.Office.RegionCode + "-" + _userSession.SamName!.ToUpper() + $"-TDW_ReturnedBox_{boxNo.Trim()}-" + DateTime.Now.ToShortDateString().Replace("/", "-") + "-" + DateTime.Now.ToShortTimeString().Replace(":", "-");
            //attachment list
            List<string> files = new List<string>();
            //write attachments for manual download/add to mail
            File.WriteAllText(StaticDataService.ReportFolder + $@"{FileName}.csv", tpl.CreateCSV());
            files.Add(StaticDataService.ReportFolder + $@"{FileName}.csv");
            //send mail to TDW
            try
            {
                //if (!Environment.MachineName.ToLower().Contains("prod")) return;
                _mail.SendTDWIncoming(_userSession, boxNo, files);
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
            return _userSession.Office.RegionCode + await _raw.GetNextAltbox(_userSession.Office.RegionCode);
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
                if (nullaltboxfiles.ToList().Any())
                {
                    var altboxes = _context.DcFiles.Where(b => !string.IsNullOrEmpty(b.AltBoxNo) && b.TdwBoxno == boxNo);
                    if (altboxes.ToList().Any())
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
                var regionmismatchfiles = _context.DcFiles.Where(b => !b.AltBoxNo.Contains(_userSession.Office.RegionCode) && b.TdwBoxno == boxNo).ToList();
                if (regionmismatchfiles.ToList().Any())
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
                var message = ex.Message;
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
                if ((await _context.DcFiles.Where(d => d.BrmBarcode == rebox.NewBarcode).ToListAsync()).Any()) throw new Exception("The new barcode already exists!");
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
                            OfficeId = _userSession.Office.OfficeId,
                            RegionId = _userSession.Office.RegionId,
                            FspId = _userSession.Office.FspId,
                            TdwBoxno = "",//rebox.BoxNo,
                            TransDate = "2016-05-29".ToDate("yyyy-mm-dd"),
                            UpdatedByAd = _userSession.SamName,
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
                var boxedFile = _context.DcFiles.Where(f => f.BrmBarcode == rebox.BrmBarcode).First();

                file.MisReboxDate = DateTime.Now;
                file.MisReboxStatus = "Completed";
                file.TdwBoxno = rebox.BoxNo;
                file.MiniBoxno = rebox.MiniBox;
                file.TdwBoxTypeId = decimal.Parse(rebox.SelectedType);
                file.TdwBatch = 0;

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
                file.UpdatedByAd = _userSession.SamName;

                boxedFile.FromDcFile(file);

                await _context.SaveChangesAsync();
                brmApiService.CreateActivity("Reboxing", file.SrdNo, file.Lctype, "Rebox file", _userSession.Office.RegionId, decimal.Parse(_userSession.Office.OfficeId), _userSession.SamName, file.UnqFileNo);
            }
            catch //(Exception ex)
            {
                throw new Exception($"Error reboxing file - {file.BrmBarcode}");
            }
        }
    }

    public async Task SyncPicklistFromItems(string unqPicklist, string status)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            DcPicklist? picklist = _context.DcPicklists.Find(unqPicklist);
            if (picklist == null) return;
            picklist.Status = status;
            await _context.SaveChangesAsync();
            var items = await _context.DcPicklistItems.Where(i => i.UnqPicklist == unqPicklist && i.Status != status).ToListAsync();
            if (items.Any()) return;
            await _context.SaveChangesAsync();
            if (status == "Received")
            {
                _mail.SendTDWReceipt(_userSession, StaticDataService.RegionIDEmails![picklist.RegionId], picklist.UnqPicklist, new List<string>());
            }
        }

    }
    #endregion

}

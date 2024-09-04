using Microsoft.EntityFrameworkCore;
using razor.Components.Models;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Models;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;
using Sassa.BRM.ViewModels;
using System.Data;
using System.Diagnostics;
using System.Text;


public class BRMDbService(IDbContextFactory<ModelContext> _contextFactory, StaticService _staticService, RawSqlService _raw, MailMessages _mail, SessionService _sessionService, BrmApiService brmApiService)
{
    private UserSession _userSession = _sessionService.session;

    #region Brm Capture
    public async Task<bool> checkBRMExists(string brmno)
    {
        try
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                return await _context.DcFiles.Where(f => f.BrmBarcode == brmno).CountAsync() > 0;
            }
        }
        catch (Exception ex)
        {
            return true;
            //throw new Exception($"Error checking for duplicate: {ex.Message}");
        }
    }
    public async Task EditBarCode(Application brm, string barCode)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            DcFile file = await _context.DcFiles.Where(d => d.BrmBarcode == brm.Brm_BarCode).FirstAsync();
            file.BrmBarcode = barCode;
            await _context.SaveChangesAsync();
            CreateActivity("Update ", file.SrdNo, file.Lctype, "Update BRM Barcode", file.UnqFileNo);
        }

    }
    public async Task<DcFile> CreateBRM(Application application, string reason)
    {
            decimal batch = 0;
            var office = StaticDataService.LocalOffices.Where(o => o.OfficeId == application.OfficeId).First();
            if (office.ManualBatch != "A")
            {
                string batchType = application.Id.StartsWith("S") ? "SrdNoId" : application.AppStatus;
                batch = string.IsNullOrEmpty(application.TDW_BOXNO) ? await CreateBatchForUser(batchType) : 0;
            }
            application.BatchNo = batch;

            DcFile? file = await brmApiService.PostApplication(application);

            if (file?.UnqFileNo == null) throw new Exception("Error creating BRM record");
            return file;
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

                    DcMerge? merge = _context.DcMerges.Where(k => k.BrmBarcode == app.Brm_BarCode).FirstOrDefault();

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
    public async Task SaveDocuments(string unqFileNo, string docsPresent)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            DcFile? original = await _context.DcFiles.FindAsync(unqFileNo);
            if (original == null) throw new Exception($"File {unqFileNo} not found.");
            original.DocsPresent = docsPresent;
            await _context.SaveChangesAsync();
        }
    }
    #endregion

    #region Boxing and Re-Boxing
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
            var interim = await _context.DcFiles.Where(b => b.TdwBoxno == boxNo && b.BoxLocked == 1).Select(f => f.BoxLocked).ToListAsync();
            return interim.Any();
        }
    }

    public async Task RemoveFileFromBox(string brmBarcode)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            await _context.DcFiles.Where(b => b.BrmBarcode == brmBarcode).ForEachAsync(f => { f.TdwBoxno = null; });
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<ReboxListItem>> GetAllFilesByBoxNo(string boxNo, bool notScanned = false)
    {

        using (var _context = _contextFactory.CreateDbContext())
        {
            if (notScanned)
            {
                var interimNs = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo && bn.ScanDatetime == null).AsNoTracking().ToListAsync();
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
            var interim = await _context.DcFiles.Where(bn => bn.TdwBoxno == boxNo).AsNoTracking().ToListAsync();
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
                DcPicklistItem? item = await _context.DcPicklistItems.Where(i => i.BrmNo == parent).FirstOrDefaultAsync();
                if (item == null) continue;
                item.Status = "Returned";
                await _context.SaveChangesAsync();
                List<string> childlist = await _context.DcMerges.AsNoTracking().Where(bn => bn.ParentBrmBarcode == parent).Select(c => c.BrmBarcode).ToListAsync();
                if (!childlist.Any()) continue;
                foreach (string child in childlist)
                {
                    DcPicklistItem? citem = await _context.DcPicklistItems.Where(i => i.BrmNo == child).FirstOrDefaultAsync();
                    if (citem == null || citem.Status == "Returned") continue;
                    citem.Status = "Returned";
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
                    Year = parent.UpdatedDate == null ? "" : parent.UpdatedDate.Value.ToString("YYYY"),
                    Location = parent.TdwBoxno,
                    Reg = parent.RegType,
                    //Bin  = parent. ,
                    Box = parent.MiniBoxno.ToString(),
                    //Pos  = parent. ,
                    UserPicked = ""
                };
                tpl.Add(TdwFormat);

            }
            string FileName = _userSession.Office.RegionCode + "-" + _userSession.SamName.ToUpper() + $"-TDW_ReturnedBox_{boxNo.Trim()}-" + DateTime.Now.ToShortDateString().Replace("/", "-") + "-" + DateTime.Now.ToShortTimeString().Replace(":", "-");
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

    /// <summary>
    /// Todo: move to boxing service
    /// </summary>
    /// <param name="boxNo"></param>
    /// <returns></returns>
    public async Task<bool> RepairAltBoxSequence(string boxNo)
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
                if (await checkBRMExists(rebox.NewBarcode)) throw new Exception("The new barcode already exists!");
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
                CreateActivity("Reboxing", file.SrdNo, file.Lctype, "Rebox file", file.UnqFileNo);
            }
            catch //(Exception ex)
            {
                throw new Exception($"Error reboxing file - {file.BrmBarcode}");
            }
        }
    }

    #endregion

    #region File Requests

    public async Task<int> AddFileRequest(RequestModel fr)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            DcFileRequest req; 
            try
            {
                //Get all the tdw Grants
                List<TdwFileLocation> tdws = await _context.TdwFileLocations.Where(l => l.Description == fr.IdNo).GroupBy(p => new { p.Description, p.GrantType })
                .Select(g => g.First()).AsNoTracking().ToListAsync();
                //Get the existing requests
                var requests = await _context.DcFileRequests.Where(r => r.IdNo == fr.IdNo).ToListAsync();
                //successCount
                int count = 0;
                if (string.IsNullOrEmpty(fr.GrantType))
                {
                    //All Grant types
                    foreach (var tdw in tdws) 
                    {
                        fr.GrantType = tdw.GrantType;
                        if (requests.Where(r => r.GrantType == fr.GrantType).Any())
                        {
                            req = requests.Where(r => r.GrantType == fr.GrantType).First();
                        }
                        else
                        { 
                            req = new DcFileRequest() { IdNo = fr.IdNo, GrantType = fr.GrantType, ReqCategoryDetail = fr.Description, ReqCategory = fr.Category, ReqCategoryType = fr.CategoryType, Stakeholder = fr.StakeHolder };
                            _context.DcFileRequests.Add(req);
                        }
                        await AddRequestFromTDW(_context,tdw, req);
                        count++;
                    }
                    return count;
                }
                if (tdws.Where(t => t.GrantType == fr.GrantType).Any())
                {
                    var tdw = tdws.Where(t => t.GrantType == fr.GrantType).First();
                        if (requests.Where(r => r.GrantType == fr.GrantType).Any())
                        {
                            req = requests.Where(r => r.GrantType == fr.GrantType).First();
                        }
                        else
                        { 
                            req = new DcFileRequest() { IdNo = fr.IdNo, GrantType = fr.GrantType, ReqCategoryDetail = fr.Description, ReqCategory = fr.Category, ReqCategoryType = fr.CategoryType, Stakeholder = fr.StakeHolder };
                            _context.DcFileRequests.Add(req);
                        }
                        await AddRequestFromTDW(_context,tdw, req);
                    count++;
                    
                }
                return count;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
 
    private async Task AddRequestFromTDW(ModelContext context,TdwFileLocation tdw, DcFileRequest req)
    {

            if (!string.IsNullOrEmpty(tdw.FilefolderAltcode))
            {
                req.MisFileNo = tdw.FilefolderAltcode.Length == 12 ? "" : tdw.FilefolderAltcode;
                req.UnqFileNo = tdw.FilefolderAltcode.Length == 12 ? tdw.FilefolderAltcode : "" ;
            }
            req.BrmBarcode = tdw.FilefolderCode;
            req.GrantType = tdw.GrantType;
            req.TdwBoxno = tdw.ContainerCode;
            req.RequestedByAd = _userSession.SamName;
            req.RequestedOfficeId = _userSession.Office.OfficeId;
            req.RequestedDate = DateTime.Now;
            req.RegionId = _userSession.Office.RegionId;
            req.Name = tdw.Name;//Could query Socpen for the name and surname
            req.Status = "TDWPicklist";
            await context.SaveChangesAsync();
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
                foreach (var gt in results)
                {
                    if (result.ContainsKey(gt.GrantType)) continue;
                    result.Add(gt.GrantType, StaticDataService.GrantTypes[gt.GrantType]);
                }

            }
            return result;
        }

    }

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
            DcFileRequest? req = await _context.DcFileRequests.FindAsync(fr.IdNo, fr.GrantType);
            if (req == null) throw new Exception($"Request {fr.IdNo}/{fr.GrantType} not found.");
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
                query = query.Where(r => r.RequestedByAd.ToLower() == _userSession.SamName.ToLower());
            }
            else if (filterInProgress)
            {
                query = query.Where(r => r.Status != "Returned");
            }
            else
            {
                query = query.Where(r => r.RegionId == _userSession.Office.RegionId);
            }
            //The query uses a row limiting operator ('Skip' / 'Take') without an 'OrderBy' operator. This may lead to unpredictable results.
            //If the 'Distinct' operator is used after 'OrderBy', then make sure to use the 'OrderBy' operator after 'Distinct' as the ordering would otherwise get erased.
            //The query uses the 'Distinct' operator after applying an ordering.
            //If there are any row limiting operation used before 'Distinct' and after ordering then ordering will be used for it.
            //Ordering(s) will be erased after 'Distinct' and results afterwards would be unordered.
            //Orderby => Distinct
      result.count = query.Where(r => r.RegionId == _userSession.Office.RegionId).Count();
            result.result = await query.Where(r => r.RegionId == _userSession.Office.RegionId).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
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
            result.count = query.Where(r => r.RegionId == _userSession.Office.RegionId).Count();
            result.result = await query.Where(r => r.RegionId == _userSession.Office.RegionId).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
            return result;
        }
    }

    public async Task ChangePickListStatus(DcPicklist pi)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            DcPicklist? pl = await _context.DcPicklists.FindAsync(pi.UnqPicklist);
            if (pl == null) throw new Exception($"Picklist {pi.UnqPicklist} not found.");
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
            DcPicklist? picklist = _context.DcPicklists.Find(unq_picklist);
            if (picklist == null) return;
            picklist.Status = "Received";
            await _context.SaveChangesAsync();
            var items = _context.DcPicklistItems.Where(p => p.UnqPicklist == unq_picklist).ToList();
            if (!items.Any()) return;
            foreach (DcPicklistItem item in items)
            {
                item.Status = "Received";
                await SyncFileRequestStatusReceived(item);
            }
            await _context.SaveChangesAsync();
            _mail.SendTDWReceipt(_userSession, StaticDataService.RegionIDEmails[picklist.RegionId], picklist.UnqPicklist, new List<string>());
        }
    }
    internal async Task SyncFileRequestStatusReceived(DcPicklistItem item)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            var GrantId = _staticService.GetGrantId(item.GrantType);
            DcFileRequest? fileReq = await _context.DcFileRequests.FindAsync(item.IdNumber, GrantId);
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
            DcPicklistItem? item = _context.DcPicklistItems.Find(ItemId);
            if (item == null) return;
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
            var interim = await _context.DcPicklistItems.Where(i => i.UnqPicklist == unqPicklist && i.Status != status).ToListAsync();
            if (interim.Any()) return;
            DcPicklist? picklist = _context.DcPicklists.Find(unqPicklist);
            if (picklist == null) return;
            picklist.Status = status;
            await _context.SaveChangesAsync();
            if (status == "Received")
            {
                _mail.SendTDWReceipt(_userSession, StaticDataService.RegionIDEmails[picklist.RegionId], picklist.UnqPicklist, new List<string>());
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
            string FileName = _userSession.Office.RegionCode + "-" + _userSession.SamName.ToUpper() + "-TDW_File_Request-" + DateTime.Now.ToShortDateString().Replace("/", "-") + "-" + DateTime.Now.ToShortTimeString().Replace(":", "-");
            //attachment list
            List<string> files = new List<string>();
            //write attachments for manual download/add to mail
            File.WriteAllText(StaticDataService.ReportFolder + $@"{FileName}.csv", tpl.result.CreateCSV());
            files.Add(StaticDataService.ReportFolder + $@"{FileName}.csv");
            //send mail to TDW
            try
            {
                _mail.SendTDWRequest(_userSession, StaticDataService.RegionEmails[_userSession.Office.RegionName.ToUpper()], tpl.UnqPickList, files);
            }
            catch
            {
                //ignore confirmation errors
            }
            //Update the status to sent
            foreach (var pli in _context.DcPicklistItems.Where(p => p.UnqPicklist == tpl.UnqPickList).ToList())
            {
                SelectedRequest request = new SelectedRequest { IDNo = pli.IdNumber, GrantTypeId = _staticService.GetGrantId(pli.GrantType) };
                await SetStatusTDWSent(request);
            }
            //Get the picklist
            var pl = await _context.DcPicklists.Where(p => p.UnqPicklist == tpl.UnqPickList).FirstAsync();
            ///Send mails
            string? tomail = pl.RequestedByAd.GetADEmail();
            if (string.IsNullOrEmpty(tomail)) return; //Skip if no tomail address
            try
            {
                _mail.SendRequestStatusChange(_userSession, "Requested", pl.UnqPicklist, tomail);
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
                var candidates = await _context.TdwFileLocations
                    .Join(_context.DcFileRequests,
                        tdw => tdw.Description,
                        fr => fr.IdNo,
                        (tdw, fr) => new { Tdw = tdw, Fr = fr })
                    .Where(x => x.Fr.Status == "TDWPicklist" && x.Fr.RegionId == _userSession.Office.RegionId && x.Tdw.GrantType == x.Fr.GrantType)
                    .OrderByDescending(x => x.Fr.RequestedDate)
                    .Select(x => new TDWRequestMain
                    {
                        BRM_No = x.Tdw.FilefolderCode,
                        CLM_No = x.Tdw.FilefolderAltcode.Length == 12 ? x.Tdw.FilefolderAltcode : null,
                        Folder_ID = x.Tdw.FilefolderAltcode.Length != 12 ? x.Tdw.FilefolderAltcode : null,
                        Grant_Type = x.Tdw.GrantType.Trim(),
                        Firstname = x.Fr.Name,
                        Surname = x.Fr.Surname,
                        ID_Number = x.Tdw.Description,
                        Reg = "U",
                        Location = x.Tdw.ContainerCode,
                        UserPicked = ""
                    })
                    .AsNoTracking()
                    .Take(maxRecords)
                    .ToListAsync();
                //Get filerequest Candidates
                if (!candidates.Any()) throw new Exception("No files to request !");
                //Create a picklist for every selected fr

                //Create new PICKList
                DcPicklist pl = new DcPicklist
                {
                    UnqPicklist = string.Empty,
                    PicklistDate = DateTime.Now,
                    Status = "Requested",
                    RegionId = _userSession.Office.RegionId,
                    RegistryType = "U",
                    RequestedByAd = _userSession.SamName,
                    UpdatedBy = _userSession.SamName,
                    PicklistStatus = "N"
                };
                _context.ChangeTracker.Clear();
                _context.DcPicklists.Add(pl);
                await _context.SaveChangesAsync();
                pl = _context.DcPicklists.OrderByDescending(p => p.PicklistDate).First();
                //Create a new TDWPickList
                tpl.UnqPickList = pl.UnqPicklist;
                //Add Picklist items
                foreach (var item in candidates)
                {
                    item.Grant_Type = _staticService.GetGrantType(item.Grant_Type);
                    tpl.result.Add(item);
                    DcPicklistItem pi = new DcPicklistItem
                    {
                        UnqPicklist = pl.UnqPicklist,
                        IdNumber = item.ID_Number,
                        FolderId = item.Folder_ID,
                        GrantType = item.Grant_Type,
                        Bin = item.Bin,
                        BrmNo = item.BRM_No,
                        ClmNo = item.CLM_No,
                        Firstname = item.Firstname,
                        Surname = item.Surname,
                        Status = "Requested",
                        Year = item.Year,
                        Reg = item.Reg,
                        Position = item.Pos,
                        Minibox = "",
                        LooseCorrespondenceId = "",
                        Location = item.Location,
                        LcType = ""
                    };

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
            fileReq.SentTdw = _userSession.SamName;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<FileTreeNode> GetCSFiles(string idNo)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            FileTreeNode node = new FileTreeNode();
            var intermediate = await _context.DcDocumentImages.AsNoTracking().Where(d => d.IdNo == idNo).ToListAsync();
            var files = intermediate.Where(d => (d.Filename.ToLower().EndsWith(".pdf") || !(d.Type ?? false)));
            foreach (var file in files)
            {
                FileTreeNode child = new FileTreeNode
                {
                    ParentId = file.Parentnode == null ? 0 : (int)file.Parentnode,
                    Id = ((int)(file.Csnode ?? 0)),
                    NodeType = file.Type ?? false,
                    NodeName = file.Filename,
                    NodeContent = file.Image
                };
                if (file.Parentnode != null)
                {
                    node.AddOnParent((int)file.Parentnode, child);
                }
                else //Rootnode
                {
                    node.Add(((int)(file.Csnode ?? 0)), child);
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
            var result = await _context.DcSocpens.Where(s => s.SrdNo == srd).ToListAsync();

            if (!result.Any()) throw new Exception("SRD not found.");

            return result.First().BeneficiaryId;
        }
    }
    #endregion

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
            List<Application> oldidquery = new List<Application>();
            var pselect = await _context.DcSocpens.Where(d => d.BeneficiaryId == SearchId).AsNoTracking().ToListAsync();
            idquery = pselect.Select(d => new Application
            {
                SocpenIsn = (long)d.Id,
                Id = d.BeneficiaryId,
                Srd_No = d.SrdNo > 0 ? d.SrdNo.ToString() : "",
                Name = d.Name,
                SurName = d.Surname,
                GrantType = d.GrantType,
                GrantName = StaticDataService.GrantTypes![d.GrantType],
                AppDate = d.ApplicationDate == null ? DateTime.Now.ToStandardDateString() : ((DateTime)d.ApplicationDate).ToStandardDateString(),
                OfficeId = _userSession.Office!.OfficeId,
                RegionId = d.RegionId,
                RegionCode = StaticDataService.RegionCode(d.RegionId ?? _userSession.Office.RegionId),
                RegionName = StaticDataService.RegionName(d.RegionId ?? _userSession.Office.RegionId),
                AppStatus = d.StatusCode.ToUpper() == "ACTIVE" ? "MAIN" : "ARCHIVE",
                ARCHIVE_YEAR = StaticDataService.GetArchiveYear(d.ApplicationDate ?? DateTime.Now, d.StatusCode.ToUpper()),
                ChildId = d.ChildId,
                LcType = "0",
                FspId = _userSession.Office.FspId,
                IsRMC = _userSession.IsRmc(),
                DocsPresent = d.Documents,
                IdHistory = d.IdHistory,
                Source = "Socpen"
            }).ToList();

            if (FullSearch)
            {
                var pselect1 = await _context.DcSocpens.Where(d => d.IdHistory.Contains(SearchId)).AsNoTracking().ToListAsync();
                oldidquery = pselect1.Select(d => new Application
                {
                    SocpenIsn = (long)d.Id,
                    Id = d.BeneficiaryId,
                    Srd_No = d.SrdNo > 0 ? d.SrdNo.ToString() : "",
                    Name = d.Name,
                    SurName = d.Surname,
                    GrantType = d.GrantType,
                    GrantName = StaticDataService.GrantTypes[d.GrantType],
                    AppDate = d.ApplicationDate == null ? DateTime.Now.ToStandardDateString() : ((DateTime)d.ApplicationDate).ToStandardDateString(),
                    OfficeId = _userSession.Office.OfficeId,
                    RegionId = d.RegionId,
                    RegionCode = StaticDataService.RegionCode(d.RegionId),
                    RegionName = StaticDataService.RegionName(d.RegionId),
                    AppStatus = d.StatusCode.ToUpper() == "ACTIVE" ? "MAIN" : "ARCHIVE",
                    ARCHIVE_YEAR = StaticDataService.GetArchiveYear(d.ApplicationDate, d.StatusCode.ToUpper()),
                    ChildId = d.ChildId,
                    LcType = "0",
                    FspId = _userSession.Office.FspId,
                    IsRMC = _userSession.IsRmc(),
                    DocsPresent = d.Documents,
                    IdHistory = d.IdHistory,
                    Source = "Socpen"
                }).ToList();
            }
            var result = idquery.Union(oldidquery).ToList();

            foreach (var item in result)
            {
                item.IsMergeCandidate = result.Where(s => s.AppDate == item.AppDate).Count() > 1;
                var docs = await GetDocsPresent(item.Id, item.GrantType, item.AppDate);
                item.DocsPresent = string.IsNullOrEmpty(docs) ? item.DocsPresent : docs;
            }
            return result;
        }
    }

    public async Task<List<Application>> SearchSocpenSrd(long srd)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            var result = await _context.DcSocpens.Where(d => d.SrdNo == srd).AsNoTracking().ToListAsync();
            List<Application> srdsquery =  result.Select(d => new Application
            {
                SocpenIsn = (long)d.Id,
                Id = d.BeneficiaryId,
                Srd_No = d.SrdNo > 0 ? d.SrdNo.ToString() : "",
                Name = d.Name,
                SurName = d.Surname,
                GrantType = d.GrantType,
                GrantName = StaticDataService.GrantTypes![d.GrantType],
                AppDate = d.ApplicationDate == null ? DateTime.Now.ToStandardDateString() : ((DateTime)d.ApplicationDate).ToStandardDateString(),
                OfficeId = _userSession.Office!.OfficeId,
                RegionId = d.RegionId,
                RegionCode = StaticDataService.RegionCode(d.RegionId ?? _userSession.Office.RegionId),
                RegionName = StaticDataService.RegionName(d.RegionId ?? _userSession.Office.RegionId),
                AppStatus = d.StatusCode.ToUpper() == "ACTIVE" ? "MAIN" : "ARCHIVE",
                ARCHIVE_YEAR = d.StatusCode.ToUpper() == "ACTIVE" ? null : ((DateTime)(d.ApplicationDate ?? DateTime.Now)).ToString("yyyy"),
                ChildId = d.ChildId,
                LcType = "0",
                IsRMC = _userSession.IsRmc(),
                DocsPresent = d.Documents,
                IdHistory = d.IdHistory,
                Source = "Socpen"
            }).ToList();

            return srdsquery;
        }
    }

    public async Task<List<Application>> SearchBRMID(string SearchId)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {

            var pselect = await _context.DcFiles
                .Where(f => f.ApplicantNo == SearchId)
                .GroupJoin(_context.DcMerges, fgm => fgm.BrmBarcode, merge => merge.BrmBarcode, (fgm, merge) => new { file = fgm, merge = merge }).AsNoTracking().ToListAsync();

            return pselect.SelectMany(x => x.merge.DefaultIfEmpty(), (f, merge) => new Application
                {
                    Id = f.file.ApplicantNo,
                    Name = f.file.UserFirstname,
                    SurName = f.file.UserLastname,
                    GrantType = f.file.GrantType,
                    GrantName = StaticDataService.GrantTypes![f.file.GrantType],
                    AppDate = f.file.TransDate == null ? DateTime.Now.ToStandardDateString() : ((DateTime)f.file.TransDate).ToStandardDateString(),
                    OfficeId = f.file.OfficeId,
                    RegionId = f.file.RegionId,
                    DocsPresent = f.file.DocsPresent,
                    AppStatus = f.file.ApplicationStatus,
                    StatusDate = null,
                    TDW_BOXNO = f.file.TdwBoxno,
                    MiniBox = (int)(f.file.MiniBoxno ?? 0),
                    BatchNo = (decimal)(f.file.BatchNo ?? 0),
                    Srd_No = f.file.SrdNo,
                    ChildId = f.file.ChildIdNo,
                    Brm_Parent = merge == null ? "" : merge.ParentBrmBarcode,
                    Brm_BarCode = f.file.BrmBarcode,
                    Clm_No = f.file.UnqFileNo,
                    LcType = f.file.Lctype.ToString(),
                    LastReviewDate = f.file.Lastreviewdate == null ? "" : ((DateTime)f.file.Lastreviewdate).ToStandardDateString(),
                    IsCombinationCandidate = false,
                    IsMergeCandidate = false,
                    IsNew = false,
                    IsRMC = _userSession.IsRmc(),
                    SocpenIsn = 0,
                    RowType = "",
                    ARCHIVE_YEAR = f.file.ArchiveYear,
                    DateApproved = null
                }).ToList();
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
                    return "";
                }
            }
        }
        catch
        {
            throw;
        }
    }

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
                    CreateActivity("Delete", dcfile.SrdNo, dcfile.Lctype, "Delete BRM Record", dcfile.UnqFileNo);

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
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
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
            file.UpdatedByAd = _userSession.SamName;
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

    public async Task<decimal> CreateBatchForUser(string sRegType)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            DcBatch batch;
            List<DcBatch> batches = new List<DcBatch>();
            //Get open batch for User
            if (_userSession.IsRmc())
            {
                batches = await _context.DcBatches.Where(b => b.OfficeId == _userSession.Office.OfficeId && b.BatchStatus == "RMCBatch" && b.UpdatedByAd == _userSession.SamName && b.RegType == sRegType).ToListAsync();
            }
            else
            {
                batches = await _context.DcBatches.Where(b => b.OfficeId == _userSession.Office.OfficeId && b.BatchStatus == "Open" && b.UpdatedByAd == _userSession.SamName && b.RegType == sRegType).ToListAsync();
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
                        BatchStatus = _userSession.IsRmc() ? "RMCBatch" : "Open",
                        BatchCurrent = "Y",
                        OfficeId = _userSession.Office.OfficeId,
                        RegType = sRegType,
                        UpdatedDate = DateTime.Now,
                        UpdatedBy = 0,
                        UpdatedByAd = _userSession.SamName
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

    public async Task SetBatchCount(decimal? batchId)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            if (batchId == null || batchId == 0) return;
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
    public async Task<int> GetBatchCount(decimal? batchId)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            return await _context.DcFiles.CountAsync(b => b.BatchNo == batchId);
        }
    }
    public async Task SetBatchStatus(decimal? batchId, string newStatus)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            if (batchId < 1) return;
            DcBatch batch = _context.DcBatches.Where(b => b.BatchNo == batchId).First();
            batch.BatchStatus = newStatus;
            if (newStatus == "Closed" && _userSession.Office.OfficeType == "LO")
            {
                batch.BrmWaybill = await GetNextOpenBrmWaybill(batchId);
            }
            await _context.SaveChangesAsync();

            //Set the batchcount
            if (newStatus == "Closed")
            {
                await SetBatchCount(batchId);

            }
        }
        CreateActivity("Batching", "", 0, $"Status {newStatus}");
    }
    public async Task<string> GetNextOpenBrmWaybill(decimal? batchId)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            //See if there is an Open Waybill for this office
            var waybillbatches = await _context.DcBatches.Where(b => b.BatchStatus == "Closed" && b.OfficeId == _userSession.Office.OfficeId).ToListAsync();
            if (waybillbatches.Any())
            {
                string waybillNo;
                if (string.IsNullOrEmpty(waybillbatches.First().BrmWaybill))
                {
                    waybillNo = $"{_userSession.Office.OfficeId}-{_raw.GetNextWayBill()}";
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
                return $"{_userSession.Office.OfficeId}-{_raw.GetNextWayBill()}";
            }
        }
    }
    public async Task RemoveFileFromBatch(string brmBarCode)
    {
        decimal batchNo;
        DcFile file;
        using (var _context = _contextFactory.CreateDbContext())
        {
            _context.ChangeTracker.Clear();
            file = await _context.DcFiles.Where(f => f.BrmBarcode == brmBarCode).FirstAsync();
            batchNo = ((decimal)(file.BatchNo ?? 0));
            file.BatchNo = 0;
            await _context.SaveChangesAsync();
        }
        CreateActivity("Batching", file.SrdNo, file.Lctype, "Remove File", file.UnqFileNo);
        if (batchNo != 0) await SetBatchCount(batchNo);

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

                if ((decimal)(file.BatchNo ?? 0) != 0)
                {
                    var interim = await _context.DcBatches.Where(b => b.BatchNo == file.BatchNo && b.BatchStatus != "Open").ToListAsync();
                    if (interim.Any())
                    {
                        throw new Exception($"This file is in closed batch: {file.BatchNo} and cant be added.");
                    }
                    sourceBatch = (decimal)(file.BatchNo ?? 0);
                }
                var batch = await _context.DcBatches.Where(b => b.BatchNo == batchNo && b.BatchStatus == "Open").FirstAsync();
                if (file.RegType != batch.RegType)
                {
                    throw new Exception($"This file is not of reg Type {batch.RegType} and cant be added.");
                }

                file.BatchNo = batchNo;

                await _context.SaveChangesAsync();
            }
            CreateActivity("Batching", file.SrdNo, file.Lctype, "Add File", file.UnqFileNo);
            await SetBatchCount(batchNo);
            if (sourceBatch != 0) await SetBatchCount(sourceBatch);
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
            return await _context.DcFiles.OrderByDescending(f => f.UpdatedDate).Where(f => f.BatchNo == BatchId).AsNoTracking().ToListAsync();
        }
    }

    public async Task<PagedResult<DcFile>> GetAllFilesByBatchNo(decimal batchId, int page)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            //List<DcFile> files = await _context.DcFiles.Where(f => f.BatchNo == batchId).ToListAsync();
            PagedResult<DcFile> result = new PagedResult<DcFile>();
            result.count = _context.DcFiles.Where(f => f.BatchNo == batchId).Count();
            result.result = await _context.DcFiles.Where(f => f.BatchNo == batchId).Skip((page - 1) * 12).Take(12).ToListAsync();
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
    public async Task SetParentBatchCount(decimal? batchId)
    {
        if (_userSession.IsRmc()) return;
        if (batchId == 0) return;
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
            await SetBatchCount(batchId);
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
                if (_userSession.IsRmc())
                {
                    List<string> offices = _staticService.GetOfficeIds(_userSession.Office.RegionId);

                    batches = await _context.DcBatches.Where(b => b.BatchStatus == "Transport" && offices.Contains(b.OfficeId)).ToListAsync();

                }
                else
                {
                    batches = await _context.DcBatches.Where(b => b.OfficeId == _userSession.Office.OfficeId && b.BatchStatus != "Completed").ToListAsync();
                }

                string brmWaybill = "";
                Waybill waybill = new();
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
                        waybill.UpdatedByAd = _userSession.SamName;
                        waybill.UpdatedDate = batch.UpdatedDate;//DateTime.Now;

                    }
                    waybill.NoOfFiles = waybill.NoOfFiles + (int)(batch.NoOfFiles ?? 0);
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

    public async Task<List<string>> GetBatchBarcodes(decimal batchNo)
    {

        using (var _context = _contextFactory.CreateDbContext())
        {
            if (batchNo == 0)
            {
                throw new Exception("Invalid batch No.");
            }
            return await (from file in _context.DcFiles.Where(f => f.BatchNo == batchNo) select file.BrmBarcode).ToListAsync();
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
            CreateActivity("Enquiry", file.SrdNo, file.Lctype, "Enquiry", file.UnqFileNo);

            var merged = await _context.DcMerges.Where(m => m.BrmBarcode == brmBarCode).ToListAsync();

            result.AppDate = file.TransDate == null ? "" : ((DateTime)file.TransDate).ToString("dd/MMM/yy");
            result.ApplicantNo = file.ApplicantNo;
            result.AppType = _staticService.GetTransactionType((int)(file.TransType ?? 0));
            result.BrmBarCode = brmBarCode;
            result.MisFileNo = file.FileNumber;
            result.BrmRecord = true;
            result.CaptureDate = (DateTime)(file.BatchAddDate ?? DateTime.Now);
            result.CsgStatus = "";
            result.GrantType = _staticService.GetGrantType(file.GrantType);
            result.LastAction = file.UpdatedDate;
            result.MultiGrant = merged.Any();
            if (result.MultiGrant)
            {
                result.MergeParent = merged.First().ParentBrmBarcode;
            }
            result.Province = _staticService.GetRegion(file.RegionId);
            result.RegType = file.RegType;
            result.SocPenActive = false;
            result.SocPenRecord = false;

            result.UnqFileNo = file.UnqFileNo;

            //Tdw
            var tdwresult = await _context.TdwFileLocations.Where(t => t.FilefolderCode == brmBarCode).ToListAsync();
            result.TdwRecord = tdwresult.Any();

            //SocPen
            var socpenresult = await _context.DcSocpens.Where(s => s.BeneficiaryId == file.ApplicantNo && s.GrantType == file.GrantType).ToListAsync();//SearchSocpenId(file.ApplicantNo, false);
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
        List<Enquiry> resultlist = new List<Enquiry>();
        using (var _context = _contextFactory.CreateDbContext())
        {
            var dcfiles = await _context.DcFiles.Where(f => f.ApplicantNo.Contains(idNumber.Trim())).ToListAsync();
            if (!dcfiles.Any()) throw new Exception("Applicant Id not found");
            var brmBarcodeList = dcfiles.Select(f => f.BrmBarcode).ToList();
            var merges = await _context.DcMerges.Where(m => brmBarcodeList.Contains(m.BrmBarcode)).ToListAsync();
            var socpen = await _context.DcSocpens.Where(f => f.BeneficiaryId == idNumber).ToListAsync();
            foreach (DcFile file in dcfiles)
            {
                Enquiry result = new Enquiry();

                result.AppDate = file.TransDate == null ? "" : ((DateTime)file.TransDate).ToString("dd/MMM/yy");
                result.ApplicantNo = file.ApplicantNo;
                result.AppType = _staticService.GetTransactionType((int)(file.TransType ?? 0));
                result.BrmBarCode = file.BrmBarcode;
                result.MisFileNo = file.FileNumber;
                result.BrmRecord = true;
                result.CaptureDate = (DateTime)(file.BatchAddDate ?? DateTime.Now);
                result.CsgStatus = "";
                result.GrantType = _staticService.GetGrantType(file.GrantType);
                result.LastAction = file.UpdatedDate;
                var merged = merges.Where(m => m.BrmBarcode == file.BrmBarcode);
                if (merged.Any())
                {
                    result.MergeParent = merged.First().ParentBrmBarcode;
                }
                result.MultiGrant = merged.Any();
                result.Province = _staticService.GetRegion(file.RegionId);
                result.RegType = file.RegType;
                result.SocPenActive = socpen.Any(s => s.StatusCode == "ACTIVE" && s.GrantType == file.GrantType);
                result.SocPenRecord = socpen.Any(s => s.GrantType == file.GrantType);

                result.UnqFileNo = file.UnqFileNo;
                //Tdw
                result.TdwRecord = socpen.Where(f => f.BeneficiaryId == file.ApplicantNo && f.GrantType == file.GrantType && f.TdwRec != null).ToList().Any();

                //List<Application> spresult = null;
                ////SocPen
                //if (idNumber.Contains("S"))
                //{
                //    if (long.TryParse(idNumber.Replace("S", ""), out long lsrd))
                //    {
                //        spresult = await SearchSocpenSrd(lsrd);
                //        result.SocPenRecord = true;
                //        result.SocPenActive = spresult.First().AppStatus.Contains("MAIN");
                //    }

                //}
                //else
                //{
                //    spresult = await SearchSocpenId(idNumber, false);
                //    if (spresult.Any())
                //    {
                //        foreach (Application sr in spresult)
                //        {
                //            if (string.IsNullOrEmpty(result.AppDate)) continue;
                //            if (sr.GrantType == file.GrantType && sr.Id == result.ApplicantNo && sr.AppDate == result.AppDate)
                //            {
                //                result.SocPenRecord = true;
                //                result.SocPenActive = sr.AppStatus.Contains("MAIN");
                //                break;
                //            }
                //        }
                //    }
                //}



                resultlist.Add(result);
            }
            CreateActivity("Enquiry", dcfiles.First().SrdNo, dcfiles.First().Lctype, "Enquiry", dcfiles.First().UnqFileNo);
            dcfiles = null;
            brmBarcodeList = null;
            merges = null;
            socpen = null;

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
            var brmBarcodeList = dcfiles.Select(f => f.BrmBarcode).ToList();
            var socpen = await _context.DcSocpens.Where(f => f.BeneficiaryId == idNumber || f.SrdNo.ToString() == idNumber.Trim()).ToListAsync();
            foreach (DcFile file in dcfiles)
            {
                Enquiry result = new Enquiry();

                result.AppDate = file.TransDate == null ? "" : ((DateTime)file.TransDate).ToString("dd/MMM/yy");
                result.ApplicantNo = file.ApplicantNo;
                result.AppType = _staticService.GetTransactionType((int)(file.TransType ?? 0));
                result.BrmBarCode = file.BrmBarcode;
                result.MisFileNo = file.FileNumber;
                result.BrmRecord = true;
                result.CaptureDate = (DateTime)(file.BatchAddDate ?? DateTime.Now);
                result.CsgStatus = "";
                result.GrantType = _staticService.GetGrantType(file.GrantType);
                result.LastAction = file.UpdatedDate;
                result.Province = _staticService.GetRegion(file.RegionId);
                result.RegType = file.RegType;
                result.SocPenActive = false;
                result.SocPenRecord = false;
                result.UnqFileNo = file.UnqFileNo;

                //Tdw
                result.TdwRecord = socpen.Where(f => f.BeneficiaryId == file.ApplicantNo && f.GrantType == file.GrantType && f.TdwRec != null).ToList().Any();

                //SocPen
                if (socpen.Any())
                {
                    result.SocPenRecord = true;
                    result.SocPenActive = socpen.Any(s => s.StatusCode == "ACTIVE");
                }

                resultlist.Add(result);
            }
            CreateActivity("Enquiry", dcfiles.First().SrdNo, dcfiles.First().Lctype, "Enquiry", dcfiles.First().UnqFileNo);
            dcfiles = null;
            brmBarcodeList = null;
            socpen = null;

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
            pd.ChartName = _staticService.GetRegion(regionId) + " region.";
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
                    .Select(v => Extentions.FromCsv(v.Trim(), exclusionType, _userSession.Office.RegionId, _userSession.SamName))
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
            var existingyears = _context.DcExclusionBatches.Where(ex => ex.RegionId == int.Parse(_userSession.Office.RegionId)).Select(ex => ex.ExclusionYear).ToList();
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
            var interim = await _context.DcExclusions.Where(d => d.IdNo == pension_no).ToListAsync();
            if (interim.Any())
            {

                ErrorCount++;
                throw new System.Exception("Duplicate exclusion");
            }

            //todo: Check if batch exists for this year/region
            DcExclusion exclusion = new DcExclusion
            {
                ExclusionType = exclusionType,
                RegionId = decimal.Parse(_userSession.Office.RegionId),
                IdNo = pension_no,
                Username = _userSession.SamName,
                ExclusionBatchId = 0,
                ExclDate = DateTime.Now
            };
            //Check if Destruction record exists
            var result = await _context.DcDestructions.Where(d => d.PensionNo == pension_no).ToListAsync();
            if (result.Any())
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

        using (var _context = _contextFactory.CreateDbContext())
        {
            DcExclusionBatch? exclusionb;
            if (string.IsNullOrEmpty(destructionYear)) throw new System.Exception("Destruction Year is required");
            //if there is a batch for this year , use it.
            exclusionb = await _context.DcExclusionBatches.Where(eb => eb.RegionId == decimal.Parse(_userSession.Office.RegionId) && eb.ExclusionYear == destructionYear.Replace(" ", "")).FirstOrDefaultAsync();
            if (exclusionb == null)
            {

                exclusionb = new DcExclusionBatch
                {
                    RegionId = decimal.Parse(_userSession.Office.RegionId),
                    ExclusionYear = destructionYear.Replace(" ", ""),
                    CreatedBy = _userSession.SamName,
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

                var exlusions = _context.DcExclusions.Where(e => e.RegionId == decimal.Parse(_userSession.Office.RegionId) && e.ExclusionBatchId == 0);
                if (!exlusions.Any()) return;
                foreach (var excl in exlusions.ToList())
                {
                    DcDestruction? dd = _context.DcDestructions.Where(d => d.PensionNo == excl.IdNo).FirstOrDefault();
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
            result.result = await _context.DcExclusionBatches.Where(p => p.ExclusionYear == year).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
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
            result.result = await _context.DcExclusionBatches.Where(p => p.ExclusionYear == year && !string.IsNullOrEmpty(p.ApprovedBy)).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
            return result;
        }
    }

    public async Task ApproveBatch(decimal batchId)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            DcExclusionBatch? batch = _context.DcExclusionBatches.Find(batchId);
            if (batch == null) throw new Exception("Batch not Found");
            batch.ApprovedBy = _userSession.SamName;
            batch.ApprovedDate = System.DateTime.Now.ToString("yyyyMMdd");
            await _context.SaveChangesAsync();
        }

    }

    public async Task<PagedResult<DcExclusion>> getExclusions(int page)
    {
        PagedResult<DcExclusion> result = new PagedResult<DcExclusion>();
        using (var _context = _contextFactory.CreateDbContext())
        {
            result.count = _context.DcExclusions.Where(p => p.RegionId == decimal.Parse(_userSession.Office.RegionId)).Count();
            result.result = await _context.DcExclusions.Where(p => p.RegionId == decimal.Parse(_userSession.Office.RegionId)).OrderByDescending(e => e.ExclDate).Skip((page - 1) * 12).Take(12).AsNoTracking().ToListAsync();
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
        string FileName = _userSession.Office.RegionCode + "-" + _userSession.SamName.ToUpper() + "-Destruction-" + DateTime.Now.ToShortDateString().Replace("/", "-") + "-" + DateTime.Now.ToString("HH-mm");
        //File.WriteAllText(FileName, dt.ToCSV());
        File.WriteAllText(StaticDataService.ReportFolder + $@"{FileName}.csv", dt.ToCSV());
    }

    //public async Task RefreshFromSocPenData(string year)
    //{
    //    string sql = @"INSERT INTO DC_DESTRUCTION d (PENSION_NO, DESTRUCTIO_DATE, STATUS_DATE, STATUS ) " +
    //                   "SELECT DISTINCT PENSION_NO, '" + year + "0101" + "', TO_CHAR(SYSDATE,'YYYYMMDD'),'Selected' " +
    //                    "FROM DC_DESTRUCTION_LIST dl " +
    //                    "WHERE PENSION_NO NOT IN(SELECT PENSION_NO FROM DC_DESTRUCTION) " +
    //                    "AND SUBSTR(STATUS_DATE,1,4) = '" + year + "'";
    //    await _raw.ExecuteNonQuery(sql);
    //    string updateregionSQL = "Update DC_DESTRUCTION D " +
    //                    "SET Region_ID = " +
    //                    "(select r.Region_ID from TDW_FILE_LOCATION F " +
    //                    "Join DC_REGION R on LOWER(TRIM(R.Region_Name)) = LOWER(TRIM(F.region)) " +
    //                    "Join DC_DESTRUCTION D ON F.DESCRIPTION = D.PENSION_NO " +
    //                    "WHERE ROWNUM = 1)";
    //    await _raw.ExecuteNonQuery(updateregionSQL);

    //}

    #endregion

    #region Activity
    /// <summary>
    /// Create Activity Record
    /// </summary>
    /// <param name="Area"></param>
    /// <param name="Activity"></param>
    /// <returns></returns>
    public void CreateActivity(string action, string srdNo, decimal? lcType, string Activity, string UniqueFileNo = "")
    {
        brmApiService.CreateActivity(action, srdNo, lcType, Activity, _userSession.Office!.RegionId, decimal.Parse(_userSession.Office.OfficeId), _userSession!.SamName!, UniqueFileNo);
    }
    #endregion
}


using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Models;
using Sassa.BRM.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sassa.Brm.Common.Services
{
    public class StaticService
    {

        public bool IsInitialized { get; set; }
        private readonly IDbContextFactory<ModelContext> _contextFactory;
        private readonly ILogger<StaticService> _logger;

        public StaticService(IDbContextFactory<ModelContext> contextFactory, IConfiguration config, IWebHostEnvironment env,ILogger<StaticService> logger)
        {
            _logger = logger;
            StaticDataService.SupportUsers = config.GetRequiredSection("SupportUsers").GetChildren().Select(c => c.Value!.ToLower()).ToList()!;
            _contextFactory = contextFactory;
            StaticDataService.ReportFolder = Path.Combine(env.ContentRootPath, @$"wwwroot\{config["Folders:Reports"]!}\");
            StaticDataService.DocumentFolder = $"{env.WebRootPath}\\{config.GetValue<string>("Folders:CS")}\\";
            Folders.CleanFolderHistory(StaticDataService.ReportFolder);
            Folders.CleanFolderHistory(StaticDataService.DocumentFolder);
            Initialize();

        }

        #region Static Data access

        private void Initialize()
        {
            StaticDataService.TransactionTypes = new Dictionary<int, string>
            {
                { 0, "Application" },
                { 1, "Loose Correspondence" },
                { 2, "Review" }
            };
            //First db accass try.
            //Do a db savitry test here
            using (var context = _contextFactory.CreateDbContext())
            {
                StaticDataService.Regions = context.DcRegions.AsNoTracking().ToList();
                StaticDataService.LocalOffices = context.DcLocalOffices.AsNoTracking().ToList();
                StaticDataService.Users = context.DcUsers.AsNoTracking().ToList();
                StaticDataService.GrantTypes = context.DcGrantTypes.AsNoTracking().ToDictionary(key => key.TypeId, value => value.TypeName);
                StaticDataService.LcTypes = context.DcLcTypes.AsNoTracking().ToDictionary(key => key.Pk, value => value.Description);
                StaticDataService.ServicePoints = context.DcFixedServicePoints.AsNoTracking().ToList();
                StaticDataService.RequiredDocs = context.DcGrantDocLinks
                    .Join(context.DcDocumentTypes, reqDocGrant => reqDocGrant.DocumentId, reqDoc => reqDoc.TypeId, (reqDocGrant, reqDoc) => new { reqDocGrant, reqDoc })
                    .Where(joinResult => joinResult.reqDocGrant.CriticalFlag == "Y")
                    .OrderBy(joinResult => joinResult.reqDocGrant.Section)
                    .ThenBy(joinResult => joinResult.reqDoc.TypeId)
                    .Select(joinResult => new RequiredDocsView
                    {
                        GrantType = joinResult.reqDocGrant.GrantId,
                        DOC_ID = joinResult.reqDoc.TypeId,
                        DOC_NAME = joinResult.reqDoc.TypeName,
                        DOC_SECTION = joinResult.reqDocGrant.Section,
                        DOC_CRITICAL = joinResult.reqDocGrant.CriticalFlag
                    }).Distinct().AsNoTracking().ToList();
                StaticDataService.BoxTypes = context.DcBoxTypes.AsNoTracking().ToList();
                StaticDataService.RequestCategoryTypeLinks = context.DcReqCategoryTypeLinks.AsNoTracking().ToList();
                StaticDataService.RequestCategoryTypes = context.DcReqCategoryTypes.AsNoTracking().ToList();
                StaticDataService.RequestCategories = context.DcReqCategories.OrderBy(e => e.CategoryDescr).AsNoTracking().ToList();
                StaticDataService.StakeHolders = context.DcStakeholders.Distinct().AsNoTracking().ToList();
            }
            IsInitialized = true;
        }
        public string GetTransactionType(int key)
        {
            return StaticDataService.TransactionTypes![key];
        }

        /// <summary>
        /// User office change
        /// Used by session service to update user office
        /// </summary>
        /// <param name="samName"></param>
        /// <param name="supervisor"></param>
        /// <returns></returns>
        public UserOffice GetUserLocalOffice(string userName)
        {
            UserOffice? office = null;
            try
            {
                office = StaticDataService.LocalOffices!
                .Join(StaticDataService.Users!.Where(l => l.AdUser == userName),
                lo => lo.OfficeId,
                link => link.DcLocalOfficeId,
                (lo, link) => new UserOffice(lo, link.DcFspId)).FirstOrDefault();
                if (office is null)
                {
                    throw new Exception($"User {userName} does not have a local office assigned.");
                }
            }
            catch //(Exception ex)
            {
                _logger.LogError("User {userName} does not have an office assigned", userName);
                DcLocalOffice defaultOffice = GetOffices("7").FirstOrDefault()!;
                office = new UserOffice(defaultOffice, null);
            }
            office.RegionName = GetRegion(office.RegionId);
            office.RegionCode = GetRegionCode(office.RegionId);
            return office;
        }
        public DcUser CreateDcUser(UserSession session)
        {
            DcUser? user = StaticDataService.Users.Where(u => u.AdUser == session.SamName).FirstOrDefault();
            if (user is null)
            {
                user = new DcUser
                {
                    AdUser = session.SamName,
                    DcLocalOfficeId = session.Office?.OfficeId ?? "712", // Default to Gauteng office if no office is available
                    DcFspId = session.Office?.FspId,
                    Settings = $"{(session.IsInRole("GRP_BRM_Supervisors") ? "Y" : "N")};csv", // Default settings
                    Firstname = session.Name,
                    Lastname = session.Surname
                };
                using (var context = _contextFactory.CreateDbContext())
                {
                    context.DcUsers.Add(user);
                    try
                    {
                        context.SaveChanges();
                        StaticDataService.Users = context.DcUsers.AsNoTracking().ToList();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating user {UserName} Please contact support.", session.SamName);
                        throw new Exception(ex.Message);
                    }
                }
            }
            return user;
        }
        public UserSettings GetUserSettings(string userName)
        {
            return new UserSettings(StaticDataService.Users.Where(u => u.AdUser == userName).FirstOrDefault()!.Settings);
        }
        public DcLocalOffice GetLocalOffice(string officeId)
        {
            return StaticDataService.LocalOffices!.Where(lo => lo.OfficeId == officeId).FirstOrDefault()!;
        }
        public List<DcFixedServicePoint> GetServicePoints(string regionID)
        {
            return StaticDataService.ServicePoints!.Where(sp => StaticDataService.LocalOffices!.Where(lo => lo.RegionId == regionID).Select(l => l.OfficeId).ToList().Contains(sp.OfficeId.ToString())).ToList();
        }
        public List<DcFixedServicePoint> GetOfficeServicePoints(string officeID)
        {
            return StaticDataService.ServicePoints!.Where(sp => sp.OfficeId == officeID).ToList();
        }
        public string GetServicePointName(decimal? fspID)
        {
            var result = StaticDataService.ServicePoints!.Where(sp => sp.Id == fspID);
            if (result.Any())
            {
                return result.First().ServicePointName;
            }
            return "";
        }

        public DcUser GetDcUser(string AdUser)
        {
            DcUser? user;
            if (string.IsNullOrEmpty(AdUser))
            {
                throw new ArgumentException("AdUser cannot be null or empty.", nameof(AdUser));
            }

            user = StaticDataService.Users.Where(u => u.AdUser == AdUser).FirstOrDefault();
            if (user == null)
            {

                throw new KeyNotFoundException($"User with AdUser '{AdUser}' not found.");
            }
            return user;
        }

        public async Task<bool> UpdateUserLocalOffice(DcUser user)
        {
            //Todo: if officeId is invalid throw exception
            if (string.IsNullOrEmpty(user.DcLocalOfficeId) || StaticDataService.LocalOffices.Where(o => o.OfficeId == user.DcLocalOfficeId).Count() == 0)
            {
                throw new ArgumentException("Invalid Office Id", user.DcLocalOfficeId);
            }
            using (var context = _contextFactory.CreateDbContext())
            {
                //Get Officelink for this user
                DcUser? userupdate = await context.DcUsers.Where(okl => okl.AdUser == user.AdUser).FirstOrDefaultAsync();
                if (userupdate is not null)
                {
                    userupdate.DcLocalOfficeId = user.DcLocalOfficeId;
                    userupdate.DcFspId = user.DcFspId;
                    userupdate.Settings = user.Settings;
                    userupdate.Firstname = user.Firstname;
                    userupdate.Lastname = user.Lastname;
                }
                else
                {
                    context.DcUsers.Add(user);
                }
                try
                {
                    await context.SaveChangesAsync();
                    //Update the staticData
                    StaticDataService.Users = await context.DcUsers.AsNoTracking().ToListAsync();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user local office {Office} for user {UserName} Please contact support.", user.DcLocalOfficeId, user.AdUser);
                    throw new Exception(ex.Message);
                }
            }
            return true;
        }
        public async Task<bool> UpdateUserLocalOffice(string officeId, decimal? fspId, UserSession session)
        {
            //Todo: if officeId is invalid throw exception
            if (string.IsNullOrEmpty(officeId) || officeId == "0" || StaticDataService.LocalOffices.Where(o => o.OfficeId == officeId).Count() == 0)
            {
                throw new ArgumentException("Invalid Office Id", nameof(officeId));
            }
            session.Office.FspId = fspId;
            DcUser officeLink;
            using (var context = _contextFactory.CreateDbContext())
            {
                //Get Officelink for this user
                var query = await context.DcUsers.Where(okl => okl.AdUser == session.SamName).ToListAsync();

                if (query.Any())
                {
                    officeLink = query.First();
                    officeLink.DcLocalOfficeId = officeId;
                    officeLink.DcFspId = fspId;
                    officeLink.Firstname = session.Name;
                    officeLink.Lastname = session.Surname;
                    officeLink.Settings = string.IsNullOrEmpty(officeLink.Settings) ? $"{(session.IsInRole("GRP_BRM_Supervisors") ? "Y" : "N")};csv" : officeLink.Settings;
                }
                else
                {
                    officeLink = new DcUser() { DcLocalOfficeId = officeId, DcFspId = session.Office?.FspId, AdUser = session.SamName,Firstname = session.Name,Lastname = session.Surname, Settings = $"{(session.IsInRole("GRP_BRM_Supervisors") ? "Y" : "N")};csv" };
                    context.DcUsers.Add(officeLink);
                }
                try
                {
                    await context.SaveChangesAsync();
                    //Update the staticData
                    StaticDataService.Users = await context.DcUsers.AsNoTracking().ToListAsync();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user local office {Office} for user {UserName} Please contact support.",officeId, session.SamName);
                    throw new Exception(ex.Message);
                }
            }
            return true;
        }
        public string GetRegion(string regionId)
        {
            if (string.IsNullOrEmpty(regionId)) return "Unknown";
            return StaticDataService.Regions!.Where(r => r.RegionId == regionId).First().RegionName;
        }
        public string GetRegionCode(string regionId)
        {
            return StaticDataService.Regions!.Where(r => r.RegionId == regionId).First().RegionCode;
        }
        public Dictionary<string, string> GetRegions()
        {
            return StaticDataService.Regions!.ToDictionary(key => key.RegionId, value => value.RegionName);
        }
        /// <summary>
        /// Include 0 region for audit Report
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetAuditRegions()

        {
            Dictionary<string, string> auditRegions = new Dictionary<string, string> { { "0", "No Region" } };
            var source = StaticDataService.Regions!.ToDictionary(key => key.RegionId, value => value.RegionName);
            foreach (var element in source)
            {
                auditRegions.Add(element.Key, element.Value);
            }
            return auditRegions;
        }
        //Includes All
        public Dictionary<string, string> GetReportOffices(string regionId)
        {
            return new Dictionary<string, string> { { "All", "All" } }.Concat(GetOffices(regionId).ToDictionary(key => key.OfficeId, value => value.OfficeName)).ToDictionary(k => k.Key, v => v.Value);
        }
        public Dictionary<string, string> GetReportGrants()
        {
            return new Dictionary<string, string> { { "All", "All" } }.Concat(StaticDataService.GrantTypes).ToDictionary(k => k.Key, v => v.Value);
        }
        //----------
        public List<DcLocalOffice> GetOffices(string regionId)
        {
            return StaticDataService.LocalOffices!.Where(o => o.RegionId == regionId).ToList();
        }
        public List<RegionOffice> GetRegionOffices(string regionId)
        {
            return StaticDataService.LocalOffices!.Where(o => o.RegionId == regionId).Select(o =>
            new RegionOffice
            {
                OfficeId = int.Parse(o.OfficeId),
                OfficeName = o.OfficeName,
                Status = o.ActiveStatus
            }).ToList();
        }
        public async Task ChangeOfficeStatus(string officeId, string status)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                DcLocalOffice lo = await context.DcLocalOffices.Where(o => o.OfficeId == officeId).FirstAsync();
                lo.ActiveStatus = status;
                await context.SaveChangesAsync();
                StaticDataService.LocalOffices = await context.DcLocalOffices.AsNoTracking().ToListAsync();
            }
        }
        public async Task ChangeOfficeName(string officeId, string name)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                DcLocalOffice lo = await context.DcLocalOffices.Where(o => o.OfficeId == officeId).FirstAsync();
                lo.OfficeName = name;
                await context.SaveChangesAsync();
                StaticDataService.LocalOffices = await context.DcLocalOffices.AsNoTracking().ToListAsync();
            }
        }
        public async Task MoveOffice(string fromOfficeId, string toOfficeId)
        {

            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    //DC_FIle
                    (await context.DcFiles.Where(o => o.OfficeId == fromOfficeId).ToListAsync()).ForEach(o => o.OfficeId = toOfficeId);
                    //DC_FIXED_SERVICE_POINT
                    var oldFsprecs = await context.DcFixedServicePoints.Where(o => o.OfficeId == fromOfficeId).ToListAsync();
                    foreach (var fsp in oldFsprecs)
                    {
                        fsp.OfficeId = toOfficeId.ToString();
                    }
                    //DC_OFFICE_KUAF_LINK
                    var oldKuafrecs = await context.DcUsers.Where(o => o.DcLocalOfficeId == fromOfficeId).ToListAsync();
                    foreach (var kuaf in oldKuafrecs)
                    {
                        kuaf.DcLocalOfficeId = toOfficeId.ToString();
                    }

                    //DC_Batches
                    var oldBatchRecs = await context.DcBatches.Where(o => o.OfficeId == fromOfficeId).ToListAsync();
                    foreach (var batch in oldBatchRecs)
                    {
                        batch.OfficeId = toOfficeId.ToString();
                    }
                    await context.SaveChangesAsync();
                    //Remove the oldoffice
                    context.DcLocalOffices.RemoveRange(await context.DcLocalOffices.Where(o => o.OfficeId == fromOfficeId).AsNoTracking().ToListAsync());
                    await context.SaveChangesAsync();
                    StaticDataService.LocalOffices = await context.DcLocalOffices.AsNoTracking().ToListAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error moving office {fromOfficeId} to {toOfficeId}. Please contact support. {ex.Message}");
                }
            }
            //await DeleteLocalOffice(fromOfficeId);
        }
        public async Task SaveManualBatch(string officeId, string manualBatch)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                DcLocalOffice lo = await context.DcLocalOffices.Where(o => o.OfficeId == officeId).FirstAsync();
                lo.ManualBatch = manualBatch;
                await context.SaveChangesAsync();
                StaticDataService.LocalOffices = await context.DcLocalOffices.AsNoTracking().ToListAsync();
            }
        }
        public async Task DeleteLocalOffice(string officeId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var lol = await context.DcLocalOffices.Where(o => o.OfficeId == officeId).ToListAsync();
                if (lol.Any())
                {
                    context.DcLocalOffices.RemoveRange(lol);
                    await context.SaveChangesAsync();
                    StaticDataService.LocalOffices = await context.DcLocalOffices.AsNoTracking().ToListAsync();
                }
            }
        }
        public async Task UpdateServicePoint(DcFixedServicePoint s)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                DcFixedServicePoint sp = await context.DcFixedServicePoints.Where(o => o.Id == s.Id).FirstAsync();
                sp.ServicePointName = s.ServicePointName;
                sp.OfficeId = s.OfficeId;
                await context.SaveChangesAsync();
                StaticDataService.ServicePoints = await context.DcFixedServicePoints.AsNoTracking().ToListAsync();
            }
        }
        public async Task CreateOffice(RegionOffice office)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                DcLocalOffice lo = new DcLocalOffice();
                lo.OfficeName = office.OfficeName;
                lo.OfficeId = (int.Parse(await context.DcLocalOffices.MaxAsync(o => o.OfficeId)!) + 1).ToString();
                lo.RegionId = office.RegionId;
                lo.ActiveStatus = "A";
                lo.OfficeType = "LO";
                context.DcLocalOffices.Add(lo);
                await context.SaveChangesAsync();
                StaticDataService.LocalOffices = await context.DcLocalOffices.AsNoTracking().ToListAsync();
            }
        }
        public async Task CreateServicePoint(DcFixedServicePoint s)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.DcFixedServicePoints.Add(s);
                await context.SaveChangesAsync();

                StaticDataService.ServicePoints = await context.DcFixedServicePoints.AsNoTracking().ToListAsync();
            }
        }
        public List<string> GetOfficeIds(string regionId)
        {
            List<DcLocalOffice> offices = GetOffices(regionId);
            return (from office in offices select office.OfficeId).ToList();
        }
        public string GetOfficeName(string officeId)
        {
            return StaticDataService.LocalOffices!.Where(o => o.OfficeId == officeId).First().OfficeName;
        }
        public string GetFspName(decimal? fspId)
        {

            if (fspId == null) return "";
            if (StaticDataService.ServicePoints!.Where(o => o.Id == fspId).Any())
            {
                return StaticDataService.ServicePoints!.Where(o => o.Id == fspId).First().ServicePointName;
            }
            return "";
        }
        public string GetOfficeType(string officeId)
        {
            return StaticDataService.LocalOffices!.Where(o => o.OfficeId == officeId).First().OfficeType;
        }
        public string GetGrantType(string grantId)
        {
            return StaticDataService.GrantTypes![grantId];
        }
        public string GetGrantId(string grantType)
        {
            return StaticDataService.GrantTypes!.Where(g => g.Value == grantType).First().Key;
        }
        public Dictionary<string, string> GetGrantTypes()
        {
            return StaticDataService.GrantTypes!;
        }
        public string GetLcType(decimal lcId)
        {
            return StaticDataService.LcTypes![lcId];
        }
        public Dictionary<decimal, string> GetLcTypes()
        {
            return StaticDataService.LcTypes!;
        }
        public List<RequiredDocsView> GetGrantDocuments(string grantType)
        {
            return StaticDataService.RequiredDocs!.Where(r => r.GrantType == grantType).OrderBy(g => g.DOC_SECTION).ThenBy(g => g.DOC_ID).ToList();
        }
        /// <summary>
        /// Transport Y or N
        /// </summary>
        /// <param name="transport"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetBoxTypes(string transport)
        {
            var result = StaticDataService.BoxTypes!.Where(d => d.IsTransport == transport).ToDictionary(i => i.BoxTypeId.ToString(), i => i.BoxType);
            return result;
        }
        public Dictionary<string, string> GetBoxTypes()
        {
            var result = StaticDataService.BoxTypes!.ToDictionary(i => i.BoxTypeId.ToString(), i => i.BoxType);
            return result;
        }
        public Dictionary<string, string> GetYearList()
        {
            Dictionary<string, string> years = new Dictionary<string, string>();
            int start = 2000;
            int end = DateTime.Now.Year;
            for (int i = end; i >= start; i--)
            {
                years.Add(i.ToString(), i.ToString());
            }
            return years;
        }
        public Dictionary<string, string> GetRequestCategories()
        {
            return StaticDataService.RequestCategories!.ToDictionary(i => i.CategoryId.ToString(), i => i.CategoryDescr);
        }
        public Dictionary<string, string> GetRequestCategoryTypes()
        {
            return StaticDataService.RequestCategoryTypes!.ToDictionary(i => i.TypeId.ToString(), i => i.TypeDescr);
        }
        public Dictionary<string, string> GetRequestCategoryTypes(string CategoryId)
        {
            if (string.IsNullOrEmpty(CategoryId)) return new Dictionary<string, string>();
            decimal.TryParse(CategoryId, out decimal catid);
            return (from r in StaticDataService.RequestCategoryTypes!
                    join c in StaticDataService.RequestCategoryTypeLinks!
                           on r.TypeId equals c.TypeId
                    where c.CategoryId == catid
                    select r).ToDictionary(i => i.TypeId.ToString(), i => i.TypeDescr);

        }
        public Dictionary<decimal, string> GetDecimalRequestCategoryTypes(string CategoryId)
        {
            if (string.IsNullOrEmpty(CategoryId)) return new Dictionary<decimal, string>();
            decimal.TryParse(CategoryId, out decimal catid);
            return (from r in StaticDataService.RequestCategoryTypes!
                    join c in StaticDataService.RequestCategoryTypeLinks!
                           on r.TypeId equals c.TypeId
                    where c.CategoryId == catid
                    select r).ToDictionary(i => i.TypeId, i => i.TypeDescr);

        }
        public Dictionary<string, string> GetStakeHolders()
        {
            var result = StaticDataService.StakeHolders!.Distinct().ToDictionary(i => i.StakeholderId.ToString(), i => i.Name + " " + i.Surname);
            result.Add("", "");
            return result;
        }
        public Dictionary<string, string> GetStakeHolders(string DepartmentId)
        {
            decimal did;
            decimal.TryParse(DepartmentId, out did);
            var result = StaticDataService.StakeHolders!.Where(s => s.DepartmentId == did).ToDictionary(i => i.StakeholderId.ToString(), i => i.Name + " " + i.Surname);
            result.Add("", "");
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="officeType">LO or RMC</param>
        /// <returns></returns>
        public Dictionary<string, string> GetBatchStatus(string officeType)
        {
            return officeType switch
            {
                "LO" => new Dictionary<string, string>
                {
                    { "Open", "Open" },
                    { "Closed", "Closed" },
                    { "Transport", "Transport" }
                },
                _ => new Dictionary<string, string>
                {
                    { "Transport", "Transport" },
                    { "Received", "Received" },
                    { "RMCBatch", "RMCBatch" }
                }
            };
        }

        #endregion
    }
}
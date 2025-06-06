using Sassa.Brm.Common.Services;
using Sassa.BRM.Data.ViewModels;
using Sassa.BRM.Models;
using Sassa.BRM.ViewModels;

namespace Sassa.Audit.Services
{
    public class BRMDbService(RawSqlService _raw)
    {
        public IQueryable<MisLivelinkTbl> GetMisFiles(ReportPeriod period, string region, string granttype, string idNumber, ModelContext _context)
        {


            IQueryable<MisLivelinkTbl> query = _context.MisLivelinkTbls.AsQueryable();
            if (period != null)
            {
                //2004-08-25 00:00:00.000
                query = query.Where(x => x.GrantDate <= period.ToDate && x.GrantDate >= period.FromDate);
            }
            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(x => x.RegionId == region);
            }
            if (!string.IsNullOrEmpty(granttype) && granttype != "All")
            {
                query = query.Where(x => x.GrantType == granttype);
            }
            if (!string.IsNullOrEmpty(idNumber))
            {
                query = query.Where(x => x.IdNumber == idNumber);
            }
            return query;
        }

        public IQueryable<MisLivelinkTbl> GetEcFiles(ReportPeriod period, string region, string granttype, string idNumber, ModelContext _context)
        {
            IQueryable<MisLivelinkTbl> query = _context.MisLivelinkTbls.AsQueryable();
            if (period != null)
            {
                //2004-08-25 00:00:00.000
                query = query.Where(x => x.GrantDate <= period.ToDate && x.GrantDate >= period.FromDate);
            }
            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(x => x.RegionId == region);
            }
            if (!string.IsNullOrEmpty(granttype) && granttype != "All")
            {
                query = query.Where(x => x.GrantType == granttype);
            }
            if (!string.IsNullOrEmpty(idNumber))
            {
                query = query.Where(x => x.IdNumber == idNumber);
            }
            return query;
        }

        public IQueryable<DcFileMini> GetBrmFiles(ReportPeriod period, string region, string granttype, string idNumber, ModelContext _context)
        {


            IQueryable<DcFile> query = _context.DcFiles.AsQueryable();
            if (period != null)
            {
                //2004-08-25 00:00:00.000
                query = query.Where(x => x.UpdatedDate <= period.ToDate && x.UpdatedDate >= period.FromDate);
            }
            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(x => x.RegionId == region);
            }
            if (!string.IsNullOrEmpty(granttype) && granttype != "All")
            {
                query = query.Where(x => x.GrantType == granttype);
            }
            if (!string.IsNullOrEmpty(idNumber))
            {
                query = query.Where(x => x.ApplicantNo == idNumber);
            }
            return query.Select(p => new DcFileMini { Id = p.ApplicantNo, Name = p.UserFirstname, Surname = p.UserLastname, GrantType = p.GrantType, Region = p.RegionId, RegType = p.RegType, GrantDate = p.UpdatedDate });
        }

        public List<string> GetTdwRegions(ModelContext _context)
        {

            return _context.TdwFileLocations.Select(x => x.Region).Distinct().ToList();
        }
        public IQueryable<TdwFileLocation> GetTdwFiles(ReportPeriod period, string region, string granttype, string idNumber, ModelContext _context)
        {


            IQueryable<TdwFileLocation> query = _context.TdwFileLocations.AsQueryable();
            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(x => x.Region == region);
            }
            if (!string.IsNullOrEmpty(granttype) && granttype != "All")
            {
                query = query.Where(x => x.GrantType == granttype);
            }
            if (!string.IsNullOrEmpty(idNumber))
            {
                query = query.Where(x => x.Description == idNumber);
            }
            return query;
        }

        public List<AuditSummary> GetSummary(ModelContext _context)
        {
            if (StaticDataService.AuditSummaryList.Count > 0) return StaticDataService.AuditSummaryList;
            StaticDataService.AuditSummaryList.Add(new AuditSummary()
            {
                Datasource = "MIS Records",
                StartDate = _context.MisLivelinkTbls.Min(x => x.GrantDate),
                EndDate = _context.MisLivelinkTbls.Max(x => x.GrantDate),
                Count = _context.MisLivelinkTbls.Count()
            });
            StaticDataService.AuditSummaryList.Add(new AuditSummary()
            {
                Datasource = "EC Records",
                StartDate = _raw.ExecuteScalar<DateTime>("SELECT MIN(Grant_Date) FROM SS_APPLICATION"),
                EndDate = _raw.ExecuteScalar<DateTime>("SELECT MAX(Grant_Date) FROM SS_APPLICATION"),
                Count = (int)_raw.ExecuteScalar<decimal?>("SELECT Count(*) FROM SS_APPLICATION")!,
            });
            StaticDataService.AuditSummaryList.Add(new AuditSummary()
            {
                Datasource = "BRM Records",
                StartDate = (DateTime)_context.DcFiles.Min(x => x.UpdatedDate)!,
                EndDate = (DateTime)_context.DcFiles.Max(x => x.UpdatedDate)!,
                Count = _context.DcFiles.Count()
            });
            StaticDataService.AuditSummaryList.Add(new AuditSummary()
            {
                Datasource = "TDW Records",
                StartDate = StaticDataService.AuditSummaryList.Where(x => x.Datasource == "MIS Records").First().StartDate,
                EndDate = StaticDataService.AuditSummaryList.Where(x => x.Datasource == "BRM Records").First().EndDate,
                Count = _context.TdwFileLocations.Count()
            });

            return StaticDataService.AuditSummaryList;
        }

    }
}



using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using razor.Components.Models;
using Sassa.Brm.Common.Models;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;
using Sassa.BRM.ViewModels;


public class ProgressService(IDbContextFactory<ModelContext> _contextFactory)
{


    #region Missing Files
    public async Task<List<MissingFile>> MissingProgress(ReportPeriod from, ReportPeriod to, string regionId)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            //List<ProcessedGrant> onlineGrants = await _econtext.ProcessedGrants.Where(d => d.ProcessDate >= from.FromDate && d.ProcessDate <= to.ToDate && d.RegionCode == StaticD.RegionCode(regionId)).AsNoTracking().ToListAsync();
            int missingStart = await _context.DcSocpens.Where(s => s.ApplicationDate <= from.FromDate && s.RegionId == regionId && s.StatusCode == "ACTIVE" && s.CaptureDate == null && s.TdwRec == null).AsNoTracking().CountAsync();
            var records = _context.DcSocpens.Where(s => s.ApplicationDate >= from.FromDate && s.ApplicationDate <= to.ToDate && s.RegionId == regionId && s.StatusCode == "ACTIVE" && s.MisFile == null && s.EcmisFile == null).AsNoTracking().AsQueryable();
            List<MissingFile> result = new List<MissingFile>();
            foreach (ReportPeriod period in StaticDataService.QuarterList(from, to).Values.OrderBy(o => o.FromDate))
            {
                List<DcSocpen> periodRecords = records.Where(s => s.ApplicationDate >= period.FromDate && s.ApplicationDate <= period.ToDate).ToList();
                MissingFile entry = new MissingFile
                {
                    Quarter = period,
                    Missing = missingStart,//records.Count(s => s.ApplicationDate <= period.FromDate && s.CaptureDate == null &&  s.TdwRec == null),
                    NewGrants = periodRecords.Count(),
                    Captured = periodRecords.Count(s => s.CaptureDate != null || s.TdwRec != null),
                    //OnlineGrants = onlineGrants.Where(d => d.ProcessDate >= period.FromDate && d.ProcessDate <= period.ToDate && d.RegionCode == StaticD.RegionCode(regionId)).Count(),
                    Scanned = periodRecords.Where(s => s.ScanDate != null).Count(),
                    CsLoaded = periodRecords.Where(s => s.CsDate != null).Count(),
                    TdwSent = periodRecords.Where(s => s.TdwRec != null).Count()
                };
                result.Add(entry);
                missingStart = missingStart + entry.NewGrants - entry.Captured - entry.OnlineGrants;
            }
            return result;
        }
    }

    public async Task<List<DcSocpen>> GetMissingFiles(ReportPeriod period, string regionId)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
                return await _context.DcSocpens.Where(s => s.CaptureReference == null && s.TdwRec == null && s.ApplicationDate >= period.FromDate && s.RegionId == regionId && s.StatusCode == "ACTIVE" && s.MisFile == null).AsNoTracking().ToListAsync();
        }
    }
    #endregion

    #region Progress

    public async Task<List<Brm.Fluent.Components.Report.QuarterDetail>> GetCaptureProgress(ReportPeriod from, ReportPeriod to, UserOffice office)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            //List<DcSocpen> periodRecords = await _context.DcSocpen.Where(s => s.ApplicationDate >= period.FromDate && s.ApplicationDate <= period.ToDate && (s.RegionId == RegionId || s.LocalofficeId == localOfficeId) && s.StatusCode == "ACTIVE").AsNoTracking().ToListAsync();
            //List<DcSocpen> periodRecords = await _context.DcSocpen.Where(s => s.ApplicationDate >= period.FromDate && s.ApplicationDate <= period.ToDate && s.LocalofficeId == localOfficeId && s.StatusCode == "ACTIVE").AsNoTracking().ToListAsync();
            try
            {


                String sql = @$"SELECT * from DC_SOCPEN
                                    WHERE STATUS_CODE ='ACTIVE' 
                                    AND Application_date >= to_date('{from.FromDate.ToString("dd/MMM/yyyy")}')
                                    AND Application_date <= to_date('{to.ToDate.ToString("dd/MMM/yyyy")}')
                                    AND LOCALOFFICE_ID = '{office.OfficeId}'";
                List<DcSocpen> records = await _context.DcSocpens.FromSqlRaw(sql).AsNoTracking().ToListAsync();

                List<Brm.Fluent.Components.Report.QuarterDetail> result = new();
                foreach (ReportPeriod period in StaticDataService.QuarterList(from, to).Values.OrderBy(o => o.FromDate))
                {
                    List<DcSocpen> periodRecords = records.Where(s => s.ApplicationDate >= period.FromDate && s.ApplicationDate <= period.ToDate).ToList();
                    result.Add(new Brm.Fluent.Components.Report.QuarterDetail
                    {
                        Quarter = period,
                        MonthDetail = GetMonthDetail(period, periodRecords, office),
                        RegionId = office.RegionId,
                        OfficeId = office.OfficeId,
                        Total = periodRecords.Count(),
                        Captured = periodRecords.Where(s => s.CaptureDate != null).Count(),//s => s.CaptureDate >= period.FromDate && s.CaptureDate <= period.ToDate).Count(), //+ erecords.Where(d => d.ProcessDate >= period.FromDate && d.ProcessDate <= period.ToDate).Count(),
                                                                                           //OnlineApplications = erecords.Where(d => d.ProcessDate >= period.FromDate && d.ProcessDate <= period.ToDate).Count(),
                        Scanned = periodRecords.Where(s => s.ScanDate != null).Count(),
                        CsLoaded = periodRecords.Where(s => s.CsDate != null).Count(),
                        TdwSent = periodRecords.Where(s => s.TdwRec != null).Count(),
                        Missing = periodRecords.Where(s => s.CaptureReference == null && s.TdwRec == null).Count()
                    });
                }

                return result;
            }
            catch //(Exception ex)
            {
                throw;
            }
        }
    }
    public async Task<List<Brm.Fluent.Components.Report.MonthDetail>> GetMonthDetail(DateTime fromDate, DateTime toDate, UserOffice office)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            List<DcSocpen> records = await _context.DcSocpens.Where(s => s.ApplicationDate >= fromDate && s.ApplicationDate <= toDate && (s.RegionId == office.RegionId || s.LocalofficeId == office.OfficeId) && s.StatusCode == "ACTIVE").AsNoTracking().ToListAsync();
            List<Brm.Fluent.Components.Report.MonthDetail> result = new();
            for (int year = fromDate.Year; year <= toDate.Year; year++)
            {
                for (int month = fromDate.Month; month <= toDate.Month; month++)
                {
                    List<DcSocpen> periodRecords = records.Where(s => s.ApplicationDate >= new DateTime(year, month, 1) && s.ApplicationDate <= new DateTime(year, month, DateTime.DaysInMonth(year, month))).ToList();
                    result.Add(new Brm.Fluent.Components.Report.MonthDetail
                    {
                        Year = year,
                        Month = month,
                        DayDetail = GetDayDetail(year, month, periodRecords),
                        RegionId = periodRecords.First().RegionId,
                        OfficeId = periodRecords.First().LocalofficeId,
                        Total = periodRecords.Count(),
                        Captured = periodRecords.Where(s => s.CaptureDate != null).Count(), //+ erecords.Where(d => d.ProcessDate >= period.FromDate && d.ProcessDate <= period.ToDate).Count(),
                                                                                            //OnlineApplications = erecords.Where(d => d.ProcessDate >= period.FromDate && d.ProcessDate <= period.ToDate).Count(),
                        Scanned = periodRecords.Where(s => s.ScanDate != null).Count(),
                        CsLoaded = periodRecords.Where(s => s.CsDate != null).Count(),
                        TdwSent = periodRecords.Where(s => s.TdwRec != null).Count()
                    });
                }
            }
            return result;
        }
    }
    public List<Brm.Fluent.Components.Report.MonthDetail> GetMonthDetail(ReportPeriod period, List<DcSocpen> records, UserOffice office)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            List<Brm.Fluent.Components.Report.MonthDetail> result = new();
            for (int year = period.FromDate.Year; year <= period.ToDate.Year; year++)
            {
                for (int month = period.FromDate.Month; month <= period.ToDate.Month; month++)
                {
                    List<DcSocpen> periodRecords = records.Where(s => s.ApplicationDate >= new DateTime(year, month, 1) && s.ApplicationDate <= new DateTime(year, month, DateTime.DaysInMonth(year, month))).ToList();
                    result.Add(new Brm.Fluent.Components.Report.MonthDetail
                    {
                        Year = year,
                        Month = month,
                        DayDetail = GetDayDetail(year, month, periodRecords),
                        RegionId = office.RegionId,
                        OfficeId = office.OfficeId,
                        Total = periodRecords.Count(),
                        Captured = periodRecords.Where(s => s.CaptureDate != null).Count(), //+ erecords.Where(d => d.ProcessDate >= period.FromDate && d.ProcessDate <= period.ToDate).Count(),
                                                                                            //OnlineApplications = erecords.Where(d => d.ProcessDate >= period.FromDate && d.ProcessDate <= period.ToDate).Count(),
                        Scanned = periodRecords.Where(s => s.ScanDate != null).Count(),
                        CsLoaded = periodRecords.Where(s => s.CsDate != null).Count(),
                        TdwSent = periodRecords.Where(s => s.TdwRec != null).Count()
                    });
                }
            }
            return result;
        }
    }
    public List<Brm.Fluent.Components.Report.DayDetail> GetDayDetail(int year, int month, List<DcSocpen> records, int page = 1)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            List<Brm.Fluent.Components.Report.DayDetail> result = new();
            string localOfficeId = "";
            for (int day = 1; day <= DateTime.DaysInMonth(year, month); day++)
            {
                List<DcSocpen> periodRecords = records.Where(s => s.ApplicationDate == new DateTime(year, month, day)).ToList();
                if (periodRecords.Any())
                {
                    localOfficeId = periodRecords.First().LocalofficeId;
                }
                result.Add(new Brm.Fluent.Components.Report.DayDetail
                {
                    Year = year,
                    Month = month,
                    Day = day,
                    OfficeId = localOfficeId,
                    OfficeDetail = GetOfficeDetail(periodRecords),
                    Total = periodRecords.Count(),
                    Captured = periodRecords.Where(s => s.CaptureDate != null).Count(), //+ erecords.Where(d => d.ProcessDate >= period.FromDate && d.ProcessDate <= period.ToDate).Count(),
                                                                                        //OnlineApplications = erecords.Where(d => d.ProcessDate >= period.FromDate && d.ProcessDate <= period.ToDate).Count(),
                    Scanned = periodRecords.Where(s => s.ScanDate != null).Count(),
                    CsLoaded = periodRecords.Where(s => s.CsDate != null).Count()
                    //TdwSent = periodRecords.Where(s => s.TdwRec != null).Count()
                }); ;
            }
            return result;
        }
    }
    public List<Brm.Fluent.Components.Report.OfficeDetail> GetOfficeDetail(List<DcSocpen> records, int page = 1)
    {
        using (var _context = _contextFactory.CreateDbContext())
        {
            List<Brm.Fluent.Components.Report.OfficeDetail> result = new();
            string localOfficeId = "";
            foreach (string office in records.DistinctBy(o => o.LocalofficeId).Select(o => o.LocalofficeId).ToList())
            {
                List<DcSocpen> officeRecords = records.Where(s => s.LocalofficeId == office).ToList();
                if (officeRecords.Any())
                {
                    localOfficeId = officeRecords.First().LocalofficeId;
                }
                result.Add(new Brm.Fluent.Components.Report.OfficeDetail
                {

                    OfficeId = localOfficeId,
                    //OfficeDetail = periodRecords.Where(o => o.LocalofficeId == localOfficeId)
                    Total = officeRecords.Count(),
                    Captured = officeRecords.Where(s => s.CaptureDate != null).Count(), //+ erecords.Where(d => d.ProcessDate >= period.FromDate && d.ProcessDate <= period.ToDate).Count(),
                                                                                        //OnlineApplications = erecords.Where(d => d.ProcessDate >= period.FromDate && d.ProcessDate <= period.ToDate).Count(),
                    Scanned = officeRecords.Where(s => s.ScanDate != null).Count(),
                    CsLoaded = officeRecords.Where(s => s.CsDate != null).Count()
                    //TdwSent = periodRecords.Where(s => s.TdwRec != null).Count()
                });
            }
            return result;
        }
    }

    #endregion



}


using Microsoft.EntityFrameworkCore;
using razor.Components.Models;
using Sassa.Brm.Common.Models;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;
using Sassa.BRM.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sassa.Brm.Common.Services
{
    public class ProgressService
    {
        ModelContext _context;
        //eDocumentContext _econtext;
        public ProgressService(ModelContext context)
        {
            _context = context;
            //_econtext = econtext;
        }

        #region Missing Files
        public async Task<List<MissingFile>> MissingProgress(ReportPeriod from, ReportPeriod to, string regionId)
        {
            //List<ProcessedGrant> onlineGrants = await _econtext.ProcessedGrants.Where(d => d.ProcessDate >= from.FromDate && d.ProcessDate <= to.ToDate && d.RegionCode == StaticD.RegionCode(regionId)).AsNoTracking().ToListAsync();
            int missingStart = await _context.DcSocpen.Where(s => s.ApplicationDate <= from.FromDate && s.RegionId == regionId && s.StatusCode == "ACTIVE" && s.CaptureDate == null && s.TdwRec == null).AsNoTracking().CountAsync();
            var records = await _context.DcSocpen.Where(s => s.ApplicationDate >= from.FromDate && s.ApplicationDate <= to.ToDate && s.RegionId == regionId && s.StatusCode == "ACTIVE" && s.MisFile == null && s.EcmisFile == null).AsNoTracking().ToListAsync();
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
            records.Clear();
            //onlineGrants.Clear();
            return result;
        }
        //public async Task<List<MissingSummary>> GetMissingProgress(ReportPeriod from, ReportPeriod to, string regionId)
        //{
        //    var onlineGrants = _econtext.ProcessedGrants.Where(d => d.ProcessDate >= from.FromDate && d.ProcessDate <= to.ToDate && d.RegionCode == StaticD.RegionCode(regionId)).AsNoTracking().AsQueryable();
        //    List<DcSocpen> records = await _context.DcSocpen.Where(s => s.ApplicationDate <= to.ToDate && s.RegionId == regionId && s.StatusCode == "ACTIVE" && s.GrantType != "S").AsNoTracking().ToListAsync();
        //    List<MissingSummary> result = new List<MissingSummary>();
        //    foreach (ReportPeriod period in StaticD.QuarterList(from, to).Values.OrderBy(o => o.FromDate))
        //    {
        //        List<DcSocpen> periodRecords = records.Where(s => s.ApplicationDate >= period.FromDate && s.ApplicationDate <= period.ToDate).ToList();
        //        result.Add(new MissingSummary
        //        {
        //            Quarter = period,
        //            HistoryMissing = records.Count(s => s.CaptureDate == null && s.ApplicationDate <= period.FromDate && s.TdwRec == null ),
        //            HistoryCaptured = records.Count(s => s.ApplicationDate <= period.FromDate && s.CaptureDate >= period.FromDate && s.CaptureDate <= period.ToDate),
        //            NewGrants = periodRecords.Count(),
        //            OnlineGrants = onlineGrants.Where(d => d.ProcessDate >= period.FromDate && d.ProcessDate <= period.ToDate && d.RegionCode == StaticD.RegionCode(regionId)).Count(),
        //            PeriodCaptured = periodRecords.Count(s => s.CaptureDate >= period.FromDate && s.CaptureDate <= period.ToDate),
        //            Scanned = periodRecords.Where(s => s.ScanDate != null).Count(),
        //            CsLoaded = periodRecords.Where(s => s.CsDate != null).Count(),
        //            TdwSent = periodRecords.Where(s => s.TdwRec != null).Count()
        //        }); 

        //    }
        //    records.Clear();
        //    return result;
        //}
        public async Task<PagedResult<DcSocpen>> GetMissingFiles(ReportPeriod period, string regionId, int page = 1)
        {
            PagedResult<DcSocpen> result = new PagedResult<DcSocpen>();
            //&& s.GrantType != "S" && s.MisFiles == null
            result.count = await _context.DcSocpen.Where(s => s.CaptureReference == null && s.TdwRec == null && s.ApplicationDate >= period.FromDate && s.RegionId == regionId && s.StatusCode == "ACTIVE" && s.MisFile == null).AsNoTracking().CountAsync();
            result.result = await _context.DcSocpen.Where(s => s.CaptureReference == null && s.TdwRec == null && s.ApplicationDate >= period.FromDate && s.RegionId == regionId && s.StatusCode == "ACTIVE" && s.MisFile == null).AsNoTracking().OrderBy(s => s.ApplicationDate).Skip((page - 1) * 24).Take(24).ToListAsync();
            //result.count = await _context.DcSocpen.Where(s => s.CaptureReference == null && s.TdwRec == null && s.ApplicationDate >= period.FromDate && s.RegionId == regionId && s.StatusCode == "ACTIVE").AsNoTracking().CountAsync();
            //result.result = await _context.DcSocpen.Where(s => s.CaptureReference == null && s.TdwRec == null && s.ApplicationDate >= period.FromDate && s.RegionId == regionId && s.StatusCode == "ACTIVE").AsNoTracking().OrderBy(s => s.ApplicationDate).Skip((page - 1) * 24).Take(24).ToListAsync();
            return result;
        }
        #endregion

        #region Progress

        //public async Task<ProgressDashBoard> GetProgress(ReportPeriod from, ReportPeriod to, string RegionId)
        //{
        //    try
        //    {


        //    ProgressDashBoard progress = new ProgressDashBoard();
        //    progress.result.result = await CaptureProgressItems(from, to, RegionId);
        //    progress.result.count = progress.result.result.Count();

        //    return progress;
        //    }
        //    catch(Exception ex)
        //    {
        //        throw;
        //    }
        //}

        //public async Task<List<CaptureProgress>> CaptureProgressItems(ReportPeriod from, ReportPeriod to, string RegionId)
        //{
        //    var onlineGrants = _econtext.ProcessedGrants.Where(d => d.ProcessDate >= from.FromDate && d.ProcessDate <= to.ToDate && d.RegionCode == StaticD.RegionCode(RegionId)).AsNoTracking().AsQueryable();
        //    //int onlineGrantsStart = await onlineGrants.Where(d => d.ProcessDate <= from.FromDate && d.RegionCode == StaticD.RegionCode(RegionId)).AsNoTracking().CountAsync();
        //    //int onlineGrantsEnd = await onlineGrants.Where(d => d.ProcessDate <= from.ToDate && d.RegionCode == StaticD.RegionCode(RegionId)).AsNoTracking().CountAsync();

        //    var history = _context.DcSocpen.Where(s => s.CaptureReference == null && s.TdwRec == null && s.ApplicationDate <= to.ToDate && s.RegionId == RegionId && s.StatusCode == "ACTIVE").AsNoTracking().AsQueryable();
        //    //progress.TotalMissingStart = await _context.DcSocpen.Where(s => s.CaptureReference == null && s.TdwRec == null && s.ApplicationDate <= from.FromDate && s.RegionId == RegionId && s.StatusCode == "ACTIVE").AsNoTracking().CountAsync() - onlineGrantsStart;
        //    //progress.TotalMissingEnd = await _context.DcSocpen.Where(s => s.CaptureReference == null && s.TdwRec == null && s.ApplicationDate <= to.ToDate && s.RegionId == RegionId && s.StatusCode == "ACTIVE").AsNoTracking().CountAsync() - onlineGrantsEnd;

        //    List<DcSocpen> records = await _context.DcSocpen.Where(s => s.ApplicationDate >= from.FromDate && s.ApplicationDate <= to.ToDate && s.RegionId == RegionId && s.StatusCode == "ACTIVE").AsNoTracking().ToListAsync();
        //    List<ProcessedGrant> erecords = await _econtext.ProcessedGrants.Where(d => d.ProcessDate >= from.FromDate && d.ProcessDate <= to.ToDate && d.RegionCode == StaticD.RegionCode(RegionId)).AsNoTracking().ToListAsync();
        //    List<CaptureProgress> result = new List<CaptureProgress>();
        //    foreach (ReportPeriod period in StaticD.QuarterList(from, to).Values)
        //    {
        //        List<DcSocpen> periodRecords = records.Where(s => s.ApplicationDate >= period.FromDate && s.ApplicationDate <= period.ToDate).ToList();
        //        int onlineApplications = erecords.Where(d => d.ProcessDate >= period.FromDate && d.ProcessDate <= period.ToDate).Count();
        //        var periodHistory = history.Where(s => s.ApplicationDate <= period.FromDate).ToList();
        //        int periodHistoryCount = history.Where(s => s.ApplicationDate <= period.FromDate).Count() - onlineApplications;
        //        result.Add(new CaptureProgress
        //        {
        //            Quarter = period,
        //            RegionId = RegionId,
        //            History = periodHistoryCount,
        //            Missing = periodRecords.Where(s => s.CaptureReference == null && s.TdwRec == null).Count(),
        //            Total = periodRecords.Count(),
        //            Captured = periodRecords.Where(s => s.CaptureDate >= period.FromDate).Count() + erecords.Where(d => d.ProcessDate >= period.FromDate && d.ProcessDate <= period.ToDate).Count(),
        //            OnlineApplications = onlineApplications,
        //            Scanned = periodRecords.Where(s => s.ScanDate != null).Count(),
        //            CsLoaded = periodRecords.Where(s => s.CsDate != null).Count(),
        //            TdwSent = periodRecords.Where(s => s.TdwRec != null).Count()

        //        }) ;
        //    }
        //    return result;
        //}
        public async Task<List<QuarterDetail>> GetCaptureProgress(ReportPeriod from, ReportPeriod to, UserOffice office)
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
                List<DcSocpen> records = await _context.DcSocpen.FromSqlRaw(sql).AsNoTracking().ToListAsync();

                List<QuarterDetail> result = new List<QuarterDetail>();
                foreach (ReportPeriod period in StaticDataService.QuarterList(from, to).Values.OrderBy(o => o.FromDate))
                {
                    List<DcSocpen> periodRecords = records.Where(s => s.ApplicationDate >= period.FromDate && s.ApplicationDate <= period.ToDate).ToList();
                    result.Add(new QuarterDetail
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
        public async Task<List<MonthDetail>> GetMonthDetail(DateTime fromDate, DateTime toDate, UserOffice office)
        {
            List<DcSocpen> records = await _context.DcSocpen.Where(s => s.ApplicationDate >= fromDate && s.ApplicationDate <= toDate && (s.RegionId == office.RegionId || s.LocalofficeId == office.OfficeId) && s.StatusCode == "ACTIVE").AsNoTracking().ToListAsync();
            List<MonthDetail> result = new List<MonthDetail>();
            for (int year = fromDate.Year; year <= toDate.Year; year++)
            {
                for (int month = fromDate.Month; month <= toDate.Month; month++)
                {
                    List<DcSocpen> periodRecords = records.Where(s => s.ApplicationDate >= new DateTime(year, month, 1) && s.ApplicationDate <= new DateTime(year, month, DateTime.DaysInMonth(year, month))).ToList();
                    result.Add(new MonthDetail
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
        public List<MonthDetail> GetMonthDetail(ReportPeriod period, List<DcSocpen> records, UserOffice office)
        {
            List<MonthDetail> result = new List<MonthDetail>();
            for (int year = period.FromDate.Year; year <= period.ToDate.Year; year++)
            {
                for (int month = period.FromDate.Month; month <= period.ToDate.Month; month++)
                {
                    List<DcSocpen> periodRecords = records.Where(s => s.ApplicationDate >= new DateTime(year, month, 1) && s.ApplicationDate <= new DateTime(year, month, DateTime.DaysInMonth(year, month))).ToList();
                    result.Add(new MonthDetail
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
        public PagedResult<DayDetail> GetDayDetail(int year, int month, List<DcSocpen> records, int page = 1)
        {
            PagedResult<DayDetail> result = new PagedResult<DayDetail>();
            string localOfficeId = "";
            for (int day = 1; day <= DateTime.DaysInMonth(year, month); day++)
            {
                List<DcSocpen> periodRecords = records.Where(s => s.ApplicationDate == new DateTime(year, month, day)).ToList();
                result.count = periodRecords.Count();
                if (periodRecords.Any())
                {
                    localOfficeId = periodRecords.First().LocalofficeId;
                }
                result.result.Add(new DayDetail
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
            result.count = result.result.Count();
            return result;
        }
        public PagedResult<OfficeDetail> GetOfficeDetail(List<DcSocpen> records, int page = 1)
        {
            PagedResult<OfficeDetail> result = new PagedResult<OfficeDetail>();
            string localOfficeId = "";
            foreach (string office in records.DistinctBy(o => o.LocalofficeId).Select(o => o.LocalofficeId).ToList())
            {
                List<DcSocpen> officeRecords = records.Where(s => s.LocalofficeId == office).ToList();
                result.count = officeRecords.Count();
                if (officeRecords.Any())
                {
                    localOfficeId = officeRecords.First().LocalofficeId;
                }
                result.result.Add(new OfficeDetail
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
            result.count = result.result.Count();
            return result;
        }

        #endregion

    }
}

using razor.Components.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sassa.BRM.ViewModels
{
    public class QuarterDetail
    {
        public ReportPeriod Quarter { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public List<MonthDetail> MonthDetail { get; set; }
        public string RegionId { get; set; }
        public string OfficeId { get; set; }
        public int Total { get; set; }
        public int Captured { get; set; }
        public int OnlineApplications { get; set; }
        public int Scanned { get; set; }
        public int CsLoaded { get; set; }
        public int TdwSent { get; set; }
        public int Missing { get; set; }
        public bool IsExpanded { get; set; } = false;

        public int PercentageCaptured
        {
            get
            {
                if (Total > 0)
                {
                    return (int)((Captured + OnlineApplications) * 100 / Total);
                }
                else
                {
                    return 0;
                }
            }
        }
        public int PercentageMissing
        {
            get
            {
                if (Total > 0)
                {
                    return (int)((Total - Captured - OnlineApplications) * 100 / Total);
                }
                else
                {
                    return 0;
                }
            }
        }
    }
    public class MonthDetail
    {
        public ReportPeriod Quarter { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public PagedResult<DayDetail> DayDetail { get; set; }
        public string RegionId { get; set; }
        public string OfficeId { get; set; }
        public int Total { get; set; }
        public int Captured { get; set; }
        public int OnlineApplications { get; set; }
        public int Scanned { get; set; }
        public int CsLoaded { get; set; }
        public int TdwSent { get; set; }
        public int Missing { get; set; }
        public bool IsExpanded { get; set; } = false;

        public int PercentageCaptured
        {
            get
            {
                if (Total > 0)
                {
                    return (int)((Captured + OnlineApplications) * 100 / Total);
                }
                else
                {
                    return 0;
                }
            }
        }
        public int PercentageMissing
        {
            get
            {
                if (Total > 0)
                {
                    return (int)((Total - Captured - OnlineApplications) * 100 / Total);
                }
                else
                {
                    return 0;
                }
            }
        }
    }
    public class DayDetail
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public string RegionId { get; set; }
        public string OfficeId { get; set; }
        public PagedResult<OfficeDetail> OfficeDetail { get; set; }
        public int Total { get; set; }
        public int Captured { get; set; }
        public int OnlineApplications { get; set; }
        public int Scanned { get; set; }
        public int CsLoaded { get; set; }
        public bool RowExpanded { get; set; } = false;

        public string MonthDay
        {
            get
            {
                return $"{Day}-{new DateTime(2010, Month, 1).ToString("MMM", CultureInfo.InvariantCulture)}";
            }
        }

    }
    public class OfficeDetail
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public string RegionId { get; set; }
        public string OfficeId { get; set; }
        public int Total { get; set; }
        public int Captured { get; set; }
        public int OnlineApplications { get; set; }
        public int Scanned { get; set; }
        public int CsLoaded { get; set; }
        public bool RowExpanded { get; set; } = false;


    }
}

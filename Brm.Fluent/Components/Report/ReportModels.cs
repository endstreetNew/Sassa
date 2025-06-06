using Sassa.BRM.ViewModels;
using System.Globalization;

namespace Brm.Fluent.Components.Report
{
    public class QuarterDetail
    {
        public QuarterDetail()
        {
            RegionId = "";
            OfficeId = "";
            MonthDetail = new();
            Quarter = new ReportPeriod();
        }
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
        public MonthDetail()
        {
            RegionId = "";
            OfficeId = "";
            DayDetail = new();
            Quarter = new ReportPeriod();
        }
        public ReportPeriod Quarter { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public List<DayDetail> DayDetail { get; set; }
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
        public DayDetail()
        {
            RegionId = "";
            OfficeId = "";
            OfficeDetail = new();
        }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public string RegionId { get; set; }
        public string OfficeId { get; set; }
        public List<OfficeDetail> OfficeDetail { get; set; }
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
        public OfficeDetail()
        {
            RegionId = "";
            OfficeId = "";
        }
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

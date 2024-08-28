namespace Sassa.BRM.ViewModels
{
    public class MissingFile
    {
        public ReportPeriod Quarter { get; set; }
        public int Missing { get; set; }
        public int NewGrants { get; set; }
        public int Captured { get; set; }
        public int Remaining
        {
            get
            {
                return Missing + NewGrants - Captured - OnlineGrants;
            }
        }
        public int PercentCaptured
        {
            get
            {
                if (Missing > 0)
                {
                    return (int)((Captured) * 100 / Missing);
                }
                else
                {
                    return 0;
                }
            }
        }
        public int PercentMissing
        {
            get
            {
                if (NewGrants > 0)
                {
                    return 100 - (int)((Captured) * 100 / NewGrants);
                }
                else
                {
                    return 0;
                }
            }
        }
        public int OnlineGrants { get; set; }
        public int Scanned { get; set; }
        public int CsLoaded { get; set; }
        public int TdwSent { get; set; }
    }
    public class MissingSummary
    {
        public ReportPeriod Quarter { get; set; }
        public int HistoryMissing { get; set; }
        public int HistoryCaptured { get; set; }
        public int HistoryRemaining
        {
            get
            {
                return HistoryMissing - HistoryCaptured;
            }
        }
        public int PercentHistoryCaptured
        {
            get
            {
                if (HistoryMissing > 0)
                {
                    return (int)((HistoryCaptured) * 100 / HistoryMissing);
                }
                else
                {
                    return 0;
                }
            }
        }
        public int NewGrants { get; set; }
        public int PeriodCaptured { get; set; }
        public int PeriodMissing
        {
            get
            {
                return NewGrants - PeriodCaptured - OnlineGrants;
            }
        }
        public int PercentPeriodCaptured
        {
            get
            {
                if (PeriodMissing > 0)
                {
                    return (int)((PeriodCaptured) * 100 / PeriodMissing);
                }
                else
                {
                    return 0;
                }
            }
        }
        public int PeriodRemaining
        {
            get { return NewGrants - PeriodCaptured - OnlineGrants; }
        }
        public int TotalCaptured
        {
            get
            {
                return HistoryCaptured + PeriodCaptured;
            }
        }
        public int OnlineGrants { get; set; }
        public int Scanned { get; set; }
        public int CsLoaded { get; set; }
        public int TdwSent { get; set; }
        public int TotalRemaining
        {
            get { return HistoryMissing + PeriodMissing - TotalCaptured - OnlineGrants; }
        }
        public int PercentProgress
        {
            get
            {
                if (TotalRemaining > 0)
                {
                    return (int)((TotalCaptured) * 100 / TotalRemaining);
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}

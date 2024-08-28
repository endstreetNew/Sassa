using System;

namespace Sassa.BRM.ViewModels
{

    public class ReportPeriod
    {
        public string FinancialQuarter { get; set; }
        public string QuarterName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public override string ToString()
        {
            return QuarterName;
        }
    }

}

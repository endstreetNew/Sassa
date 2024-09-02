using System;

namespace Sassa.BRM.ViewModels
{

    public class ReportPeriod
    {
        public string FinancialQuarter { get; set; }
        public string QuarterName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        //Workaround for stupid component
        public DateTime? FromNullDate { 
            set
            {
                if (value.HasValue)
                    FromDate = value.Value;
            }
            get { return FromDate; }
        }
        //Workaround for stupid component
        public DateTime? ToNullDate
        {
            set
            {
                if (value.HasValue)
                    ToDate = value.Value;
            }
            get { return ToDate; }
        }
        public override string ToString()
        {
            return QuarterName;
        }
    }

}

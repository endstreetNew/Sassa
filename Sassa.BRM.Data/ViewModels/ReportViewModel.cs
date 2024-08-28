using System;
using System.Data;

namespace Sassa.BRM.ViewModels
{
    public class ReportViewModel
    {
        public string Title { get; set; }
        public DateTime ReportDate { get; set; }
        public string Period { get; set; }
        public DataTable result { get; set; }
    }
}

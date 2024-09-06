using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sassa.BRM.Data.ViewModels
{
    public class ReportList:List<ReportDefinition>
    {
    }

    public class ReportDefinition
    {
        public string ReportIndex { get; set; }
        public string ReportName { get; set; }
        public FilterOptions FilterOptions { get; set; }
    }

    public class FilterOptions : List<string>
    {
        public string FilterOption { get; set; }
    }
}

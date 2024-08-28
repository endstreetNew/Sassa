using System;
using System.Data;

namespace Sassa.BRM.ViewModels
{
    public class PagedReport
    {
        public PagedReport()
        {
            pagedTable = new PagedTable();
        }
        public string Title { get; set; }
        public DateTime ReportDate { get; set; }
        public string Period { get; set; }

        public PagedTable pagedTable { get; set; }
    }

    public class PagedTable
    {
        public int Count { get; set; }
        public DataTable Result { get; set; }
    }
}

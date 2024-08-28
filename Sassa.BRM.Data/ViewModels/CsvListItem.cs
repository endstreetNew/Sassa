using System;
using System.Collections.Generic;
using System.Linq;

namespace Sassa.BRM.ViewModels
{
    public class CsvListItem
    {
        public CsvListItem(string fileName)
        {
            List<string> fields = fileName.Split("-").ToList();
            FileName = fileName;
            RegionCode = fields.First();
            UserName = fields.Skip(1).First();
            ReportName = fields.Skip(2).First();
            ReportDate = new DateTime(int.Parse(fields.Skip(3).First()), int.Parse(fields.Skip(4).First()), int.Parse(fields.Skip(5).First()), int.Parse(fields.Skip(6).First()), int.Parse(fields.Skip(7).First().Substring(0, 2)), 0);
        }
        public string UserName { get; set; }
        public string RegionCode { get; set; }
        public string ReportName { get; set; }
        public string FileName { get; set; }
        public DateTime? ReportDate { get; set; }
    }
}

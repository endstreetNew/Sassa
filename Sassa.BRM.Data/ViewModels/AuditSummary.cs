using System;

namespace Sassa.BRM.ViewModels
{
    public class AuditSummary
    {
        public string Datasource { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Count { get; set; }
    }
}

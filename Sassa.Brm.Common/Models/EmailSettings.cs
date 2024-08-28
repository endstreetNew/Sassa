using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sassa.Brm.Common.Models
{
    public class EmailSettings :IEmailSettings
    {
        public string? ContentRootPath { get; set; }
        public string? WebRootPath { get; set; }
        public string? ReportFolder { get; set; }
        public string? DocumentFolder { get; set; }
        public string? SmtpServer { get; set; }
        public int? SmtpPort { get; set; }
        public string? SmtpUser { get; set; }
        public string? SmtpPassword { get; set; }
        public string? TdwReturnedBox { get; set; }
        public Dictionary<string, string> RegionEmails { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> RegionIDEmails { get; set; } = new Dictionary<string, string>();

    }
}

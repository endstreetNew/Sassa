using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sassa.Brm.Common.Models
{
    public interface IEmailSettings
    {
        string? ContentRootPath { get; set; }
        string? WebRootPath { get; set; }
        string? ReportFolder { get; set; }
        string? DocumentFolder { get; set; }
        string? SmtpServer { get; set; }
        int? SmtpPort { get; set; }
        string? SmtpUser { get; set; }
        string? SmtpPassword { get; set; }
        string? TdwReturnedBox { get; set; }
        Dictionary<string, string> RegionEmails { get; set; }
        Dictionary<string, string> RegionIDEmails { get; set; }
    }
}
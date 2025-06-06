using Sassa.Brm.Common.Models;

namespace Sassa.Brm.Common.Helpers
{
    public class Helper
    {
        /// <summary>
        /// Create standard report file name
        /// </summary>
        /// <param name="reportName"></param>
        /// <returns></returns>
        public string GetFileName(string reportName, UserSession session)
        {
            return $"{session.Office.RegionCode}-{session.SamName!.ToUpper()}-{reportName}-{DateTime.Now.ToShortDateString().Replace("/", "-")}-{DateTime.Now.ToString("HH-mm")}";
        }

        public string GetAuditFileName(string reportName, string regionCode)
        {
            return $"{regionCode}-Audit-{reportName}-{DateTime.Now.ToShortDateString().Replace("/", "-")}-{DateTime.Now.ToString("HH-mm")}";
        }


    }
}

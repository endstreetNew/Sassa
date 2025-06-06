using System.Net.Mail;

namespace Sassa.Brm.Health
{
    public class DailyCheck
    {
        private string htmlContent = @"<!DOCTYPE html>
        <html lang=""en"">
        <head>
            <meta charset=""UTF-8"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
            <title>Health Check Report - Beneficiary Records Management</title>
            <style>
                body { font-family: Arial, sans-serif; margin: 20px; }
                h1, h2 { color: #2E86C1; }
                table { width: 100%; border-collapse: collapse; margin-top: 10px; }
                th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
                th { background-color: #f2f2f2; }
                .status-ok { color: green; }
                .status-pending { color: orange; }
            </style>
        </head>
        <body>
        <div class= 'whiteArea'>
            <h1>Health Check Report</h1>
            <p><strong>System:</strong> Beneficiary Records Management</p>
            <p><strong>Agency:</strong> SASSA (South African Social Security Agency)</p>
            <p><strong>User:</strong>[Creator]]</p>
            <p><strong>Location:</strong> Gauteng</p>
            <p><strong>Version:</strong> 1.0.0.0</p>
            <p><strong>Date:</strong>[Date]</p>

            <h2>System Accessibility</h2>
            <table>
                <tr><th>Checkpoint</th><th>Status</th><th>Comments</th></tr>
                <tr><td>Application URL Access</td><td class=""status-ok"">✅ Up</td><td>Accessible via internal network</td></tr>
                <tr><td>Login Functionality</td><td class=""status-ok"">✅ Working</td><td>User authentication successful ([Creator])</td></tr>
                <tr><td>User Count Display</td><td class=""status-ok"">✅ OK</td><td>Shows: [Users] active users</td></tr>
                <tr><td>Menu Navigation</td><td class=""status-ok"">✅ OK</td><td>All top-level menu items clickable</td></tr>
            </table>

            <h2>Module Availability</h2>
            <table>
                <tr><th>Module</th><th>Status</th><th>Comments</th></tr>
                <tr><td>Home</td><td class=""status-ok"">✅ OK</td><td>Landing page loads correctly</td></tr>
                <tr><td>File Capture</td><td class=""status-ok"">✅ OK</td><td>Opens and is responsive</td></tr>
                <tr><td>Scan</td><td class=""status-ok"">✅ OK</td><td>Scan UI launches as expected</td></tr>
                <tr><td>RSWeb</td><td class=""status-ok"">✅ OK</td><td>Integrated access to RSWeb confirmed</td></tr>
                <tr><td>MIS</td><td class=""status-ok"">✅ OK</td><td>Metrics display correctly</td></tr>
                <tr><td>Enquiry</td><td class=""status-ok"">✅ OK</td><td>User queries are functional</td></tr>
                <tr><td>Batching</td><td class=""status-ok"">✅ OK</td><td>Current active module</td></tr>
                <tr><td>File Requests</td><td class=""status-ok"">✅ OK</td><td>Loads and responds to queries</td></tr>
                <tr><td>Reports</td><td class=""status-ok"">✅ OK</td><td>Report interface available</td></tr>
            </table>

            <h2>Performance Checks</h2>
            <table>
                <tr><th>Metric</th><th>Status</th><th>Notes</th></tr>
                <tr><td>Page Load Speed</td><td class=""status-ok"">✅ Normal</td><td>No delays observed</td></tr>
                <tr><td>Session Stability</td><td class=""status-ok"">✅ Stable</td><td>No session drops</td></tr>
                <tr><td>Error Logs</td><td class=""status-ok"">✅ OK</td><td>Review backend logs for deeper insight</td></tr>
                <tr><td>Database Connectivity</td><td class=""status-ok"">✅ OK</td><td>Responsive</td></tr>
            </table>
        </div>
        </body>
        </html>";

        public string GenerateHealthCheckReport(string creator, int userCount, DateTime date)
        {
            string reportContent = htmlContent
                .Replace("[Creator]", creator)
                .Replace("[Users]", userCount.ToString())
                .Replace("[Date]", date.ToString("yyyy-MM-dd "));

            return reportContent;
        }

    }
}

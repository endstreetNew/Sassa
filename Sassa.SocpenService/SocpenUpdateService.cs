using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Sassa.Services
{
    public class SocpenUpdateService
    {
        string connectionString;
        private readonly ILogger<SocpenUpdateService> _logger;
        public SocpenUpdateService(ILogger<SocpenUpdateService> logger, string connectionstring)
        {
            _logger = logger;
            connectionString = connectionstring;
        }
        public void SyncSocpen()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "UPDATE_DC_SOCPEN";
                        _logger.LogInformation("Starting Socpen Sync");
                        cmd.ExecuteNonQueryAsync();
                        _logger.LogInformation("Socpen Sync Completed");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Socpen Sync [ERR] :" + ex.Message);
                }
            }
        }
    }
}


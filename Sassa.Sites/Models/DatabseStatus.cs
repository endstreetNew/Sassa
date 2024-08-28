using Sassa.Sites.Models;

namespace Sassa.Sites.Models
{
    public class DatabaseStatus
    {
        public DatabaseStatus(string databaseName, string serverName, string ip, string connectionString)
        {
            DatabaseName = databaseName;
            ServerName = serverName;
            Ip = ip;
            ConnectionString = connectionString;
        }

        public string DatabaseName { get; set; }
        public string ServerName { get; set; }
        public string Ip { get; set; }
        public string ConnectionString { get; set; }
        public int CpuUsage { get; set; }
        public int MemoryUsage { get; set; }
        public int DriveUsage { get; set; }
        public StatusEnum ServerStatus { get; set; }
        public StatusEnum DbStatus { get; set; }
        public string? Exception { get; set; }
    }
}

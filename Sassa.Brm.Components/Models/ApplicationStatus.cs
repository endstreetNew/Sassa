using Sassa.Brm.Components.Models;

namespace Sassa.Brm.Components.Models
{
    public class ApplicationStatus
    {
        public ApplicationStatus(string applicationName, string serverName, string ip)
        {
            ApplicationName = applicationName;
            ServerName = serverName;
            Ip = ip;
        }
        public string? ApplicationName { get; set; }
        public string ServerName { get; set; }
        public string Ip { get; set; }
        public StatusEnum ServerStatus {
            get 
            {
                if(MemoryUsage > 90 || CpuUsage > 90 || DriveUsage > 90)
                {
                    return StatusEnum.Critical;
                }
                if(MemoryUsage > 80 || CpuUsage > 80 || DriveUsage > 80)
                {
                    return StatusEnum.Warning;
                }
                return StatusEnum.OK;
            }           
        }
        public Double TotalMemory { get; set; }
        public Double FreeMemory { get; set; }
        public Double UsedMemory 
        { 
            get 
            {
                return Math.Round((TotalMemory - FreeMemory), 1);
                
            }  
        }
        public Double MemoryUsage
        {
            get
            {
                return Math.Round(((UsedMemory / TotalMemory) * 100), 1);
            }
        }
        public Double CpuUsage { get; set; }
        public Double TotalDrive { get; set; }
        public Double FreeDrive { get; set; }
        public Double UsedDrive { 
            get {
                return Math.Round((TotalDrive - FreeDrive), 1);
            }            
        }
        public Double DriveUsage { 
            get {
              return Math.Round(((UsedDrive / TotalDrive) * 100), 1);
            }            }
        public string? Exception { get; set; }
    }
}

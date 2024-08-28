using System.Management;
using System.Security;
using Sassa.Brm.Components.Models;

namespace Sassa.Brm.Components.Services
{
    public class ServerService
    {

        string scopeString = "\\\\serverip\\root\\cimv2";
        private ServerList _servers;

        public ServerService(ServerList servers)
        {
            _servers = servers;
        }
        public void GetServerStatus()
        {
            foreach(var server in _servers)
            {
                //Todo:Assign results
                GetFreeMemory(server);
                GetDriveSpace(server);
                GetCpuUsage(server);
            }
        }
        public void GetFreeMemory(ApplicationStatus server)
        {
            scopeString = scopeString.Replace("serverip", server.Ip);
            //var scope = new ManagementScope(scopeString);
            SecureString securePassword = new SecureString();
            foreach (char c in "23Savitri".ToCharArray())
            {
                securePassword.AppendChar(c);
            }

            var scope = new ManagementScope(scopeString, new ConnectionOptions("MS_409", @"jsmitha", securePassword,
            "ntlmdomain:SASSA",
            System.Management.ImpersonationLevel.Impersonate,
            System.Management.AuthenticationLevel.Default, true,
            null, System.TimeSpan.MaxValue));
            scope.Connect();
            try
            {
                // Get total memory and free memory for the server
                var memoryQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                var memorySearcher = new ManagementObjectSearcher(scope, memoryQuery);
                var memoryCollection = memorySearcher.Get();
                foreach (var memory in memoryCollection)
                {
                    server.TotalMemory = Math.Round(Convert.ToDouble(memory["TotalVisibleMemorySize"]) / 1048576,0);
                    server.FreeMemory = Math.Round(Convert.ToDouble(memory["FreePhysicalMemory"]) / 1048576, 0);
                    // Output the memory usage information
                    //Console.WriteLine($"Memory occupancy on {server} : Used: {usedMemory} GB, Free: {freeMemory} GB, Total: {totalMemory} GB, Usage: {memoryUsage}%");
                    //result = $"Used: {usedMemory} GB GB, Total: {totalMemory} GB, Free: {freeMemory}, Usage: {memoryUsage}%";
                }
            }
            catch (Exception ex)
            {
                // Throw an error if there was an issue with getting memory usage information
                server.Exception = $"Error: Unable to retrieve memory usage information from {server.ServerName}. Exception message: {ex.Message}";
            }
        }
        public void GetDriveSpace(ApplicationStatus server)
        {
            scopeString = scopeString.Replace("serverip", server.Ip);
            SecureString securePassword = new SecureString();
            foreach (char c in "23Savitri".ToCharArray())
            {
                securePassword.AppendChar(c);
            }

            var scope = new ManagementScope(scopeString, new ConnectionOptions("MS_409", @"jsmitha", securePassword,
            "ntlmdomain:SASSA",
            System.Management.ImpersonationLevel.Impersonate,
            System.Management.AuthenticationLevel.Default, true,
            null, System.TimeSpan.MaxValue));
            scope.Connect();
            try
            {
                // Get drive size and free space for the C drive on the server
                var driveQuery = new ObjectQuery("SELECT * FROM CIM_LogicalDisk WHERE DeviceID='C:'");
                var driveSearcher = new ManagementObjectSearcher(scope, driveQuery);
                var driveCollection = driveSearcher.Get();
                foreach (var drive in driveCollection)
                {
                    server.TotalDrive = Math.Round(Convert.ToDouble(drive["Size"]) / 1073741824, 1);
                    server.FreeDrive = Math.Round(Convert.ToDouble(drive["FreeSpace"]) / 1073741824, 1);
                    // Output the C drive usage information
                    //Console.WriteLine($"C drive occupancy on {server} : Used: {usedDrive} GB, Free: {freeDrive} GB, Total: {totalDrive} GB, Usage: {driveUsage}%");
                }
            }
            catch (Exception ex)
            {
                // Throw an error if there was an issue with getting C drive usage information
                server.Exception = $"Error: Unable to retrieve C drive usage information from {server}. Exception message: {ex.Message}";
            }

        }
        public void GetCpuUsage(ApplicationStatus server)
        {
            scopeString = scopeString.Replace("serverIp", server.Ip);
            SecureString securePassword = new SecureString();
            foreach (char c in "23Savitri".ToCharArray())
            {
                securePassword.AppendChar(c);
            }

            var scope = new ManagementScope(scopeString, new ConnectionOptions("MS_409", @"jsmitha", securePassword,
            "ntlmdomain:SASSA",
            System.Management.ImpersonationLevel.Impersonate,
            System.Management.AuthenticationLevel.Default, true,
            null, System.TimeSpan.MaxValue));
            scope.Connect();
            try
            {
                // Get instant CPU usage for the server
                var cpuQuery = new ObjectQuery("SELECT LoadPercentage FROM CIM_Processor");
                var cpuSearcher = new ManagementObjectSearcher(scope, cpuQuery);
                var cpuCollection = cpuSearcher.Get();
                server.CpuUsage = Math.Round((double)cpuCollection.Cast<ManagementObject>().Average(cpu => Convert.ToDouble(cpu["LoadPercentage"])), 0);

                // Output the instant CPU usage information
                //Console.WriteLine($"Instant CPU usage on {server} : {cpuUsage}%");
            }
            catch (Exception ex)
            {
                // Throw an error if there was an issue with getting instant CPU usage information
                server.Exception = $"Error: Unable to retrieve instant CPU usage information from {server}. Exception message: {ex.Message}";
            }

        }
    }
}

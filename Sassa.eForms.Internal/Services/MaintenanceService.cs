using Microsoft.Extensions.Configuration;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Sassa.eForms.Services
{
    public class MaintenanceService
    {
        IConfiguration _settings;
        private DateTime? EndDate { get; }
        public string Message
        {
            get
            {
                return string.Format(_settings["Message"], _settings["Startdate"], _settings["Enddate"]);
            }
        }

        public bool Active
        {
            get
            {
                return EndDate > DateTime.Now;
            }
        }

        public MaintenanceService(IConfiguration config)
        {
            _settings = config.GetSection("Maintenance");
            EndDate = _settings["Enddate"].ToDate();
        }
    }

}

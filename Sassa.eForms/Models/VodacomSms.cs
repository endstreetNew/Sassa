using System;

namespace Sassa.eForms.Models
{
    public class VodacomSms
    {
        public string To { get; set; }
        public string Message { get; set; }
        public string Ems { get { return "0"; } }
        public string UserRef
        {
            get
            {
                Random _rdm = new Random();
                return _rdm.Next(100000, 999999).ToString().Trim();
            }
        }
    }
}

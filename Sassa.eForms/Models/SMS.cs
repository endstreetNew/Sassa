using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sassa.eForms.Models
{
    public class SMSRequest
    {

     public string reference { get; set; }
     public string cellNumber { get; set; }
     public string message { get; set; }
    }

    public class SMSResponse
    {

        public string Action { get; set; }
        public string Key { get; set; }
        public string Result { get; set; }
        public string Number { get; set; }
        public string Error { get; set; }
    }
}

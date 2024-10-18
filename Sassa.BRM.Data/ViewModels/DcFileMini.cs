using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sassa.BRM.Data.ViewModels
{
    public class DcFileMini
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string GrantType { get; set; }
        public string Region { get; set; }
        public DateTime? GrantDate { get; set; }
        public string RegType{ get; set; }
    }
}

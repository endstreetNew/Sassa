using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sassa.Monitor.Shared
{
    public class HealthHistory
    {
        public int Id { get; set; }
        public DateTime StatusDate { get; set; }
        public bool Status { get; set; }

    }
}

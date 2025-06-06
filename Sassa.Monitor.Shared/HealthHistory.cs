using System;

namespace Sassa.Monitor.Shared
{
    public class HealthHistory
    {
        public int Id { get; set; }
        public DateTime StatusDate { get; set; }
        public bool Status { get; set; }

    }
}

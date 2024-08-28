using System;
using System.Collections.Generic;

namespace Sassa.eDocs.Data.Models
{
    public partial class ProcessedGrant
    {
        public string Reference { get; set; }
        public DateTime? ProcessDate { get; set; }
        public string RegionCode { get; set; }
    }
}

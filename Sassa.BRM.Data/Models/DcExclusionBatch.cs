using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcExclusionBatch
{
    public decimal BatchId { get; set; }

    public decimal? RegionId { get; set; }

    public string ExclusionYear { get; set; }

    public string CreatedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ApprovedBy { get; set; }

    public string ApprovedDate { get; set; }
}

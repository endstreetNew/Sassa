using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcExclusion
{
    public string IdNo { get; set; }

    public DateTime? ExclDate { get; set; }

    public decimal? RegionId { get; set; }

    public string Username { get; set; }

    public string ExclusionType { get; set; }

    public decimal? ExclusionBatchId { get; set; }

    public decimal Id { get; set; }
}

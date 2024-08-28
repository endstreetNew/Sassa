using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcDestruction
{
    public string PensionNo { get; set; }

    public string DestructioDate { get; set; }

    public string StatusDate { get; set; }

    public string Status { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public string GrantType { get; set; }

    public decimal RegionId { get; set; }

    public decimal? ExclusionbatchId { get; set; }

    public decimal? DestructionBatchId { get; set; }
}

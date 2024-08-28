using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcPicklist
{
    public string UnqPicklist { get; set; }

    public string RegionId { get; set; }

    public string RegistryType { get; set; }

    public decimal? Userid { get; set; }

    public DateTime? PicklistDate { get; set; }

    public string PicklistStatus { get; set; }

    public string AdUser { get; set; }

    public string Status { get; set; }

    public string RequestedByAd { get; set; }

    public string UpdatedBy { get; set; }
}

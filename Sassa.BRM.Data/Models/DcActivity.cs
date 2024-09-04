using System;

namespace Sassa.BRM.Models;

public partial class DcActivity
{
    public decimal DcActivityId { get; set; }

    public string Username { get; set; }

    public string Area { get; set; }

    public string Activity { get; set; }

    public string Result { get; set; }

    public decimal? Userid { get; set; }

    public DateTime ActivityDate { get; set; }

    public string RegionId { get; set; }

    public decimal OfficeId { get; set; }

    public string UnqFileNo { get; set; }
}

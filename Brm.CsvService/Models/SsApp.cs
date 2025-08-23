using System;
using System.Collections.Generic;

namespace Brm.CsvService.Models;

public partial class SsApp
{
    public decimal Id { get; set; }

    public decimal FormNo { get; set; }

    public string IdNumber { get; set; } = null!;

    public string? Name { get; set; }

    public string? Surname { get; set; }

    public DateTime? ApplicationDate { get; set; }

    public string? GrantType { get; set; }

    public string? FormType { get; set; }

    public string? DisabilityType { get; set; }

    public string? DisabilityDesc { get; set; }

    public string? MedNo { get; set; }

    public string? Gender { get; set; }

    public string? Race { get; set; }

    public string? RegionCode { get; set; }

    public string? DistrictOffice { get; set; }

    public string? ServiceOffice { get; set; }

    public string? Box { get; set; }

    public string? Position { get; set; }

    public decimal? AYear { get; set; }

    public string? BoxType { get; set; }

    public string? ApplStatus { get; set; }

    public DateTime? ActionDate { get; set; }

    public string? ActionResult { get; set; }
}

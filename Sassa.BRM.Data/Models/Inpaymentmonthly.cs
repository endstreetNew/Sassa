using System;

namespace Sassa.BRM.Models;

public partial class Inpaymentmonthly
{
    public string ApplicantNo { get; set; }

    public string GrantType { get; set; }

    public string RegionId { get; set; }

    public string FirstName { get; set; }

    public string Surname { get; set; }

    public decimal? ChildIdNo { get; set; }

    public DateTime? TransDate { get; set; }
    public DateTime? AppDate { get; set; }

    public decimal? Cs { get; set; }

    public decimal? Brm { get; set; }

    public decimal? LoCapture { get; set; }

    public decimal? Lo { get; set; }

    public decimal? Mis { get; set; }

    public decimal? Ecmis { get; set; }

    public decimal? Oga { get; set; }

    public decimal? MisLc { get; set; }

    public decimal? Tdw { get; set; }

    public decimal? Paypoint { get; set; }

    public long Id { get; set; }

    public string FileExists { get; set; }
}

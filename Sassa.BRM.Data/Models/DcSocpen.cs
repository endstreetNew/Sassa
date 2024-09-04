using System;

namespace Sassa.BRM.Models;

public partial class DcSocpen
{
    public string ApplicationNo { get; set; }

    public string BeneficiaryId { get; set; }

    public string ChildId { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public string GrantType { get; set; }

    public string RegionId { get; set; }

    public DateTime? ApplicationDate { get; set; }

    public string StatusCode { get; set; }

    public string CaptureReference { get; set; }

    public DateTime? CaptureDate { get; set; }

    public DateTime? ScanDate { get; set; }

    public DateTime? SocpenDate { get; set; }

    public string AppStatus { get; set; }

    public DateTime? CsDate { get; set; }

    public DateTime? TdwRec { get; set; }

    public string BrmBarcode { get; set; }

    public string LocalofficeId { get; set; }

    public string UniqueId { get; set; }

    public string Paypoint { get; set; }

    public long? SrdNo { get; set; }

    public string AdabasIsnSrd { get; set; }

    public string MisFile { get; set; }

    public string EcmisFile { get; set; }

    public string IdHistory { get; set; }

    public string Documents { get; set; }

    public decimal Id { get; set; }

    public string Exception { get; set; }

    public DateTime? OgaDate { get; set; }
}

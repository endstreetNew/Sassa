using System;
using System.Collections.Generic;

namespace Sassa.Models;

public partial class CustCoversheet
{
    public string ReferenceNum { get; set; } = null!;

    public string? BrmNumber { get; set; }

    public string? TxtSocpenRefNumber { get; set; }

    public string? TxtIdNumber { get; set; }

    public string? TxtName { get; set; }

    public string? TxtSurname { get; set; }

    public string? DrpdwnGrantTypes { get; set; }

    public string? CaptureDate { get; set; }

    public string? ScannedDate { get; set; }

    public string? ScanRegion { get; set; }

    public string? ScanLocaloffice { get; set; }

    public string? Poid { get; set; }

    public string? Clmnumber { get; set; }

    public string? Status { get; set; }

    public string? Docsubmitted { get; set; }

    public string? DrpdwnTransaction { get; set; }

    public string? TxtNameSo { get; set; }

    public string? TxtSurnameSo { get; set; }

    public string? DrpdwnRegionSo { get; set; }

    public string? DrpdwnLocalOfficeSo { get; set; }

    public string? TxtSocpenUseridSo { get; set; }

    public string? TxtSrdRefNumber { get; set; }

    public string? TxtIdNumberChild { get; set; }

    public string? DrpdwnAppStatus { get; set; }

    public string? ArchiveYear { get; set; }

    public string? ApplicationDate { get; set; }

    public string? DocsubmittedBrm { get; set; }

    public string? TxtUsernameSo { get; set; }

    public string? ScannedDocs { get; set; }

    public string? DrpdwnLcType { get; set; }

    public string? TxtIdNumberChild2 { get; set; }

    public string? TxtIdNumberChild3 { get; set; }

    public string? TxtIdNumberChild4 { get; set; }

    public string? TxtIdNumberChild5 { get; set; }

    public string? TxtIdNumberChild6 { get; set; }

    public string? TempGrantTypes { get; set; }

    public string? Granttypelookup { get; set; }

    public string? DrpdwnStatus { get; set; }

    public string? DrpdwnStatusLc { get; set; }

    public DateTime? NewCaptureDate { get; set; }

    public DateTime? NewScannedDate { get; set; }

    public DateTime? NewApplicationDate { get; set; }
}

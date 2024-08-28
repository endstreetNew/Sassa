using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcFileDeleted
{
    public string UnqFileNo { get; set; }

    public decimal? BatchNo { get; set; }

    public string OfficeId { get; set; }

    public string RegionId { get; set; }

    public string ApplicantNo { get; set; }

    public string GrantType { get; set; }

    public decimal? TransType { get; set; }

    public string DocsPresent { get; set; }

    public DateTime? TransDate { get; set; }

    public decimal? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public DateTime? BatchAddDate { get; set; }

    public string FileStatus { get; set; }

    public string FileComment { get; set; }

    public string UserFirstname { get; set; }

    public string UserLastname { get; set; }

    public string ApplicationStatus { get; set; }

    public string DocsScanned { get; set; }

    public DateTime? ScanDatetime { get; set; }

    public string BrmBarcode { get; set; }

    public string TdwBoxno { get; set; }

    public string NonCompliant { get; set; }

    public string QcUserFn { get; set; }

    public string QcUserLn { get; set; }

    public DateTime? QcDate { get; set; }

    public string MisBoxno { get; set; }

    public DateTime? MisBoxDate { get; set; }

    public DateTime? MisReboxDate { get; set; }

    public string MisBoxStatus { get; set; }

    public string MisReboxStatus { get; set; }

    public string FileNumber { get; set; }

    public string ArchiveYear { get; set; }

    public string Exclusions { get; set; }

    public string Missing { get; set; }

    public string Transferred { get; set; }

    public string Compliant { get; set; }

    public string RegionIdFrom { get; set; }

    public decimal? TdwBoxTypeId { get; set; }

    public string TdwBoxArchiveYear { get; set; }

    public string AltBoxNo { get; set; }

    public string SrdNo { get; set; }

    public decimal? TempBoxNo { get; set; }

    public string UpdatedByAd { get; set; }

    public string ChildIdNo { get; set; }

    public decimal? PrintOrder { get; set; }

    public string Isreview { get; set; }

    public DateTime? Lastreviewdate { get; set; }

    public decimal? Lctype { get; set; }

    public decimal? FspId { get; set; }
}

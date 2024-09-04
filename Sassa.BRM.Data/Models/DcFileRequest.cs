using System;

namespace Sassa.BRM.Models;

public partial class DcFileRequest
{
    public string UnqFileNo { get; set; }

    public string IdNo { get; set; }

    public string MisFileNo { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public string RegionId { get; set; }

    public decimal? RequestedBy { get; set; }

    public DateTime? RequestedDate { get; set; }

    public string RequestedOfficeId { get; set; }

    public decimal? ScannedBy { get; set; }

    public DateTime? ScannedDate { get; set; }

    public DateTime? ClosedDate { get; set; }

    public decimal? ClosedBy { get; set; }

    public string RelatedMisFileNo { get; set; }

    public string GrantType { get; set; }

    public DateTime? AppDate { get; set; }

    public string ScannedPhysicalInd { get; set; }

    public decimal? ReqCategory { get; set; }

    public decimal? ReqCategoryType { get; set; }

    public string ReqCategoryDetail { get; set; }

    public string BrmBarcode { get; set; }

    public string BinId { get; set; }

    public string BoxNumber { get; set; }

    public string Position { get; set; }

    public string TdwBoxno { get; set; }

    public string SentTdw { get; set; }

    public string ReceivedTdw { get; set; }

    public string FileRetrieved { get; set; }

    public string SendToRequestor { get; set; }

    public decimal? PicklistNo { get; set; }

    public string PicklistType { get; set; }

    public string PicklistStatus { get; set; }

    public string PickedBy { get; set; }

    public string ClmNumber { get; set; }

    public string RegionIdTo { get; set; }

    public decimal? Stakeholder { get; set; }

    public string ServBy { get; set; }

    public string RequestedByAd { get; set; }

    public string ScannedByAd { get; set; }

    public string ClosedByAd { get; set; }

    public string ApplicationStatus { get; set; }

    public string Status { get; set; }

    public string UnqPicklist { get; set; }

    public virtual DcReqCategoryTypeLink DcReqCategoryTypeLink { get; set; }

    public virtual DcRegion Region { get; set; }

    public virtual DcStakeholder StakeholderNavigation { get; set; }
}

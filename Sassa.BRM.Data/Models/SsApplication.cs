using System;

namespace Sassa.BRM.Models;

public partial class SsApplication
{
    public int? AppId { get; set; }

    public string FormNumber { get; set; }

    public string IdNumber { get; set; }

    public string SocpenIdNumber { get; set; }

    public string AltIdNumber1 { get; set; }

    public string AltIdNumber2 { get; set; }

    public string AltIdNumber3 { get; set; }

    public string Surname { get; set; }

    public string Name { get; set; }

    public string ApplicationDate { get; set; }

    public string GrantType { get; set; }

    public string FormType { get; set; }

    public string DisabilityType { get; set; }

    public string DisabilityDesc { get; set; }

    public string MedForm { get; set; }

    public string DocName { get; set; }

    public string Gender { get; set; }

    public int? Race { get; set; }

    public string Paypoint { get; set; }

    public int? Town { get; set; }

    public string RegionCode { get; set; }

    public int? ServiceId { get; set; }

    public string DistrictOffice { get; set; }

    public string Box { get; set; }

    public string Positn { get; set; }

    public string AYear { get; set; }

    public string BoxType { get; set; }

    public string LetterGenDate { get; set; }

    public string LetterSentDate { get; set; }

    public string OnPersal { get; set; }

    public string OnGepf { get; set; }

    public string OnMuni { get; set; }

    public string PersalNo { get; set; }

    public string ApplStatus { get; set; }

    public string DateIn { get; set; }

    public string ActionDate { get; set; }

    public string ActionResult { get; set; }

    public string Reason { get; set; }

    public string MatchType { get; set; }

    public string RegistryRemoval { get; set; }

    public string ApplicationDoc { get; set; }

    public string IdBook { get; set; }

    public string MedicalCert { get; set; }

    public string BirthCert { get; set; }

    public string CourtOrd { get; set; }

    public string WarPapers { get; set; }

    public string UserIdCreated { get; set; }

    public int? OldPosition { get; set; }

    public long? ApprovedAmount { get; set; }

    public long? OneOffPayment { get; set; }

    public string PaymentDate { get; set; }

    public int? DcPersalNo { get; set; }

    public string DcAction { get; set; }

    public string DcActionDate { get; set; }

    public string DcComplete { get; set; }

    public string Letter { get; set; }

    public string LetterMemo { get; set; }

    public string CopyPrintDate { get; set; }

    public string Bankname { get; set; }

    public string AnnexureA { get; set; }

    public string AnnexureB { get; set; }

    public string HomeAffairsRef1 { get; set; }

    public string HomeAffairsRef2 { get; set; }

    public bool? IdDoc { get; set; }

    public bool? ProofId { get; set; }

    public bool? BirthCertificate { get; set; }

    public bool? ProofBirth { get; set; }

    public string SocpenRef { get; set; }

    public bool? RefugeeGrant { get; set; }

    public string DateExpiryOfTempId { get; set; }

    public int? PaymentMethod { get; set; }

    public string ClientDob { get; set; }

    public bool? FlagAdminReview { get; set; }

    public bool? QaAmendments { get; set; }

    public string Version { get; set; }

    public int? ServicePointId { get; set; }

    public DateTime? GrantDate { get; set; }
}

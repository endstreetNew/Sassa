using System;

namespace Sassa.BRM.ViewModels
{
    public class Enquiry
    {
        public string BrmBarCode { get; set; }
        public string UnqFileNo { get; set; }
        public string MisFileNo { get; set; }
        public string ApplicantNo { get; set; }
        public string Province { get; set; }
        public string AppDate { get; set; }
        public string AppType { get; set; }
        public string RegType { get; set; }
        public bool BrmRecord { get; set; }
        public bool TdwRecord { get; set; }
        public bool SocPenRecord { get; set; }
        public string GrantType { get; set; }
        public string CsgStatus { get; set; }
        public bool MultiGrant { get; set; }
        public DateTime? LastAction { get; set; }
        public bool SocPenActive { get; set; }
        public DateTime CaptureDate { get; set; }
        public string MergeParent { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace Sassa.BRM.Models
{
    public class Application
    {
        public long SocpenIsn { get; set; }
        public string ARCHIVE_YEAR { get; set; }
        public string Id { get; set; }
        //Change to accomodate invalid child ids from socpen
        private string child_id;
        public string ChildId { get; set; }
        //{
        //    get
        //    {
        //        if (!"6C59".Contains(GrantType)) return null;
        //        return child_id;
        //    }
        //    set 
        //    { 
        //        child_id = value == null ? null : value.Trim().PadLeft(13, '0'); 
        //    }
        //}
        //-----------------------------------------------
        public string Name { get; set; }
        public string SurName { get; set; }
        public string OfficeId { get; set; }
        public string RegionId { get; set; }
        public string RegionCode { get; set; }
        public string RegionName { get; set; }
        public string GrantType { get; set; }
        public string GrantName { get; set; }
        public string AppDate { get; set; }
        public string AppStatus { get; set; }
        public string StatusDate { get; set; }
        public string DocsPresent { get; set; }
        public string LastReviewDate { get; set; }
        [JsonIgnore]
        public string DateApproved { get; set; }
        //[JsonIgnore]
        //public string Prim_Status { get; set; }
        //[JsonIgnore]
        //public string Sec_Status { get; set; }
        public string LcType { get; set; }
        public string Srd_No { get; set; }
        public string Brm_Parent { get; set; }
        public string Brm_BarCode { get; set; }
        public string Clm_No { get; set; }
        public string TDW_BOXNO { get; set; }
        public int MiniBox { get; set; }
        public decimal BatchNo { get; set; }
        [JsonIgnore]
        public string IdHistory { get; set; }
        [JsonIgnore]
        public bool IsRMC { get; set; }
        public decimal? TRANS_TYPE { get; set; }
        [JsonIgnore]
        public bool IsNew { get; set; }
        [JsonIgnore]
        public string RowType { get; set; }
        public string Source { get; set; }
        [JsonIgnore]
        public bool IsMergeCandidate { get; set; }
        [JsonIgnore]
        public bool IsCombinationCandidate { get; set; }
        public string BrmUserName { get; set; }
        public decimal? FspId { get; set; }
        [JsonIgnore]
        public bool IsSelected { get; set; }
        [JsonIgnore]
        public string FullName
        {
            get
            {
                return Name + " " + SurName;
            }
        }
        [JsonIgnore]
        public bool IsMergeParent
        {
            get
            {
                return !string.IsNullOrEmpty(Brm_Parent);
            }
            set
            {

            }
        }
        [JsonIgnore]
        public bool ShowChildren { get; set; }
        [JsonIgnore]
        public string Status
        {
            get
            {
                return AppStatus.Contains("MAIN") ? "Active" : "Inactive";
            }
        }
        [JsonIgnore]
        public bool IsPreservedType
        {
            get
            {
                return "MAIN|LC-MAIN|ARCHIVE|LC-ARCHIVE".Contains(AppStatus);
            }
        }
        public string IsValid()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return "Name is reguired";
            }
            if (string.IsNullOrEmpty(SurName))
            {
                return "Surname is required";
            }
            if (string.IsNullOrEmpty(Id))
            {
                return "Id is required";
            }
            if (string.IsNullOrEmpty(GrantType))
            {
                return "GrantType is required";
            }
            if (!"MAIN|ARCHIVE|LC-MAIN|LC-ARCHIVE".Contains(AppStatus))
            {
                return "Regtype is required";
            }
            if (string.IsNullOrEmpty(DocsPresent))
            {
                return "Critical Documents are required";
            }
            if (GrantType == "S" && string.IsNullOrEmpty(Srd_No.Trim()))
            {
                return "Srd is Required for SRD Grants";
            }
            if (!string.IsNullOrEmpty(LcType.Trim('0')))
            {
                if (!AppStatus.StartsWith("LC-"))
                {
                    AppStatus = "LC-" + AppStatus;
                }
            }
            return "Valid";
        }
    }
}

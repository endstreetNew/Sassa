namespace Sassa.BRM.ViewModels
{
    public class RegionOffice
    {
        public decimal ResCode { get; set; }
        public int OfficeId { get; set; }
        public string RegionId { get; set; }
        public string OfficeName { get; set; }
        public string Status { get; set; }
        public bool IsActive { get { return Status == "A"; } }
        public bool IsLinked { get { return ResCode > 0 && OfficeId > 0; } }
        public string ManualBatch { get; set; }
        public decimal LinkOfficeId { get; set; }
        public decimal LinkResCode { get; set; }
    }
}

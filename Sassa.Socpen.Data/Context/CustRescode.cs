namespace Sassa.Socpen.Data
{
    public partial class CustRescode
    {
        public decimal ResCode { get; set; }
        public string LocalOffice { get; set; } = null!;
        public string? Municipality { get; set; }
        public decimal? WardId { get; set; }
        public string? DistrictCode { get; set; }
        public string? DistrictName { get; set; }
        public string? Region { get; set; }
        public decimal? RegionCode { get; set; }
        public decimal? OfficeId { get; set; }
        public string? Status { get; set; }
    }
}

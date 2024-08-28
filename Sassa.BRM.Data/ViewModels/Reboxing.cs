namespace Sassa.BRM.Models
{
    public class Reboxing
    {
        public Reboxing()
        {
            BoxNo = "";
            BatchNo = 0;
            ArchiveYear = "";
            BrmBarcode = "";
            MisFileNo = "";
            NewBarcode = "";
            ToRegionId = "";
            RegType = "";
            PickListNo = "";
            MiniBox = 1;
            BoxCount = 0;
            MiniBoxCount = 0;
            SelectedType = "";
        }
        public string BoxNo { get; set; }
        public decimal BatchNo { get; set; }
        public string RegType { get; set; }
        public string ArchiveYear { get; set; }
        public string ToRegionId { get; set; }
        public string BrmBarcode { get; set; }
        public string MisFileNo { get; set; }
        public string NewBarcode { get; set; }
        public string AltBoxNo { get; set; }
        public string PickListNo { get; set; }
        public int MiniBox { get; set; }
        public int BoxCount { get; set; }
        public int MiniBoxCount { get; set; }
        public string SelectedType { get; set; }

        public bool IsLCSelected
        {
            get
            {
                return SelectedType == "18" || SelectedType == "13";
            }
        }
    }
}

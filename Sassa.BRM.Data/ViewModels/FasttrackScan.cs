using System;

namespace Sassa.BRM.Data.ViewModels
{
    public class FasttrackScan
    {
        public string LoReferece { get; set; }
        public string BrmBarcode { get; set; }
        public DateTime ScanDate { get; set; } = DateTime.Now;
        public string Result { get; set; } = string.Empty;
    }
}

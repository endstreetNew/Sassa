using System;

namespace Sassa.BRM.ViewModels
{
    public class Waybill
    {
        public string OfficeId { get; set; }
        public string BrmWaybill { get; set; }
        public string WaybillNo { get; set; }
        public string CourierName { get; set; }
        public string UpdatedByAd { get; set; }
        public string Status { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int NoOfBatches { get; set; }
        public int NoOfFiles { get; set; }
    }
}

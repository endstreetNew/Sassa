using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcBatch
{
    public decimal BatchNo { get; set; }

    public string OfficeId { get; set; }

    public decimal? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string BatchCurrent { get; set; }

    public string BatchStatus { get; set; }

    public string BatchComment { get; set; }

    public string WaybillNo { get; set; }

    public string CourierName { get; set; }

    public DateTime? WaybillDate { get; set; }

    public string UpdatedByAd { get; set; }

    public string RegType { get; set; }

    public string BrmWaybill { get; set; }

    public decimal? NoOfFiles { get; set; }

    public virtual ICollection<DcFile> DcFiles { get; set; } = new List<DcFile>();
}

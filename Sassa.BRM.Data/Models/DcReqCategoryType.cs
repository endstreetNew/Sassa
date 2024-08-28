using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcReqCategoryType
{
    public decimal TypeId { get; set; }

    public string TypeDescr { get; set; }

    public virtual ICollection<DcReqCategoryTypeLink> DcReqCategoryTypeLinks { get; set; } = new List<DcReqCategoryTypeLink>();
}

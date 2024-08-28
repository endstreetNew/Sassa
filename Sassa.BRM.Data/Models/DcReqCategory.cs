using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcReqCategory
{
    public decimal CategoryId { get; set; }

    public string CategoryDescr { get; set; }

    public virtual ICollection<DcReqCategoryTypeLink> DcReqCategoryTypeLinks { get; set; } = new List<DcReqCategoryTypeLink>();

    public virtual ICollection<DcStakeholder> DcStakeholders { get; set; } = new List<DcStakeholder>();
}

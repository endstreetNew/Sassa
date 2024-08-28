using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcReqCategoryTypeLink
{
    public decimal CategoryId { get; set; }

    public decimal TypeId { get; set; }

    public virtual DcReqCategory Category { get; set; }

    public virtual ICollection<DcFileRequest> DcFileRequests { get; set; } = new List<DcFileRequest>();

    public virtual DcReqCategoryType Type { get; set; }
}

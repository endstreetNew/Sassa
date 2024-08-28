using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcTransactionType
{
    public decimal TypeId { get; set; }

    public string TypeName { get; set; }

    public string ServiceCategory { get; set; }

    public virtual ICollection<DcFile> DcFiles { get; set; } = new List<DcFile>();

    public virtual ICollection<DcGrantDocLink> DcGrantDocLinks { get; set; } = new List<DcGrantDocLink>();
}

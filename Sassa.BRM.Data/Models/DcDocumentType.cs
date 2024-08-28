using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcDocumentType
{
    public decimal TypeId { get; set; }

    public string TypeName { get; set; }

    public virtual ICollection<DcGrantDocLink> DcGrantDocLinks { get; set; } = new List<DcGrantDocLink>();
}

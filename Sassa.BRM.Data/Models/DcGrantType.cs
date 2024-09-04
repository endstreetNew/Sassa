using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcGrantType
{
    public string TypeId { get; set; }

    public string TypeName { get; set; }

    public virtual ICollection<DcFile> DcFiles { get; set; } = new List<DcFile>();

    public virtual ICollection<DcGrantDocLink> DcGrantDocLinks { get; set; } = new List<DcGrantDocLink>();
}

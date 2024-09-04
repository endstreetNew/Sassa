using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcRegion
{
    public string RegionId { get; set; }

    public string RegionName { get; set; }

    public string RegionCode { get; set; }

    public virtual ICollection<DcFileRequest> DcFileRequests { get; set; } = new List<DcFileRequest>();

    public virtual ICollection<DcLocalOffice> DcLocalOffices { get; set; } = new List<DcLocalOffice>();
}

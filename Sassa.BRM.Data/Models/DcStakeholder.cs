using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcStakeholder
{
    public decimal StakeholderId { get; set; }

    public decimal DepartmentId { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public string Email { get; set; }

    public string RegionId { get; set; }

    public virtual ICollection<DcFileRequest> DcFileRequests { get; set; } = new List<DcFileRequest>();

    public virtual DcReqCategory Department { get; set; }
}

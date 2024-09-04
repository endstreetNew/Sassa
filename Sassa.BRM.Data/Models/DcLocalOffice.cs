using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcLocalOffice
{
    public string OfficeId { get; set; }

    public string OfficeName { get; set; }

    public string RegionId { get; set; }

    public string OfficeType { get; set; }

    public string District { get; set; }

    public string ActiveStatus { get; set; }

    public string ManualBatch { get; set; }

    public virtual ICollection<DcFile> DcFiles { get; set; } = new List<DcFile>();

    public virtual ICollection<DcFixedServicePoint> DcFixedServicePoints { get; set; } = new List<DcFixedServicePoint>();

    public virtual ICollection<DcOfficeKuafLink> DcOfficeKuafLinks { get; set; } = new List<DcOfficeKuafLink>();

    public virtual DcRegion Region { get; set; }
}

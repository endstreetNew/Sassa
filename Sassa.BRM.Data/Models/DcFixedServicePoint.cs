using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcFixedServicePoint
{
    public decimal Id { get; set; }

    public string OfficeId { get; set; }

    public string ServicePointName { get; set; }

    public virtual DcLocalOffice Office { get; set; }
}

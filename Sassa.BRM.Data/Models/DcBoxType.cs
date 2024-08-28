using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcBoxType
{
    public decimal BoxTypeId { get; set; }

    public string BoxType { get; set; }

    public string IsTransport { get; set; }
}

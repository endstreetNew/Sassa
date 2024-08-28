using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcMerge
{
    public decimal Pk { get; set; }

    public string BrmBarcode { get; set; }

    public string ParentBrmBarcode { get; set; }
}

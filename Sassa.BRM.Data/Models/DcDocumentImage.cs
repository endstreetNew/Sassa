using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcDocumentImage
{
    public decimal Id { get; set; }

    public string IdNo { get; set; }

    public byte[] Image { get; set; }

    public string Filename { get; set; }

    public string Url { get; set; }

    public bool? Type { get; set; }

    public decimal? Csnode { get; set; }

    public decimal? Parentnode { get; set; }

    public string Csurl { get; set; }
}

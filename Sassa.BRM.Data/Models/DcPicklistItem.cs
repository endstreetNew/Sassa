using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcPicklistItem
{
    public decimal PicklistItemId { get; set; }

    public string BrmNo { get; set; }

    public string ClmNo { get; set; }

    public string FolderId { get; set; }

    public string GrantType { get; set; }

    public string Firstname { get; set; }

    public string Surname { get; set; }

    public string IdNumber { get; set; }

    public string Year { get; set; }

    public string Location { get; set; }

    public string Reg { get; set; }

    public string Bin { get; set; }

    public string Box { get; set; }

    public string Position { get; set; }

    public string LooseCorrespondenceId { get; set; }

    public string BvpLc { get; set; }

    public string LcType { get; set; }

    public string Minibox { get; set; }

    public string Userpicked { get; set; }

    public string UnqPicklist { get; set; }

    public string Status { get; set; }
}

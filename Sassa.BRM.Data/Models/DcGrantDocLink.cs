using System;
using System.Collections.Generic;

namespace Sassa.BRM.Models;

public partial class DcGrantDocLink
{
    public string GrantId { get; set; }

    public decimal TransactionId { get; set; }

    public decimal DocumentId { get; set; }

    public string CriticalFlag { get; set; }

    public string Section { get; set; }

    public virtual DcDocumentType Document { get; set; }

    public virtual DcGrantType Grant { get; set; }

    public virtual DcTransactionType Transaction { get; set; }
}

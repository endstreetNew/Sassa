using System;
using System.Collections.Generic;

namespace Sassa.BRM.Data.tmpModels;

public partial class Inpayment
{
    public string ApplicantNo { get; set; }

    public string GrantType { get; set; }

    public string RegionId { get; set; }

    public string FirstName { get; set; }

    public string Surname { get; set; }

    public string ChildIdNo { get; set; }

    public DateTime? TransDate { get; set; }

    public string RegType { get; set; }

    public string BrmBarcode { get; set; }

    public string ClmNo { get; set; }

    public string MisFileNo { get; set; }

    public string EcMisFile { get; set; }

    public string OgaStatus { get; set; }

    public string Paypoint { get; set; }

    public long Id { get; set; }
}

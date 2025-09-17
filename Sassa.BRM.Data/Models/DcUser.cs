namespace Sassa.BRM.Models;

public partial class DcUser
{
    public string AdUser { get; set; }

    public string DcLocalOfficeId { get; set; }

    public decimal? DcFspId { get; set; }

    public string Firstname { get; set; }

    public string Lastname { get; set; }

    public string Email { get; set; }

    public string Settings { get; set; }

    public virtual DcFixedServicePoint DcFsp { get; set; }

    public virtual DcLocalOffice DcLocalOffice { get; set; }
}

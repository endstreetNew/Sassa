using System.ComponentModel.DataAnnotations;

namespace Sassa.BRM.Models;

public partial class DcFixedServicePoint
{
    public decimal Id { get; set; }
    [Required]
    [MinLength(1)]
    public string OfficeId { get; set; }
    [Required]
    [MinLength(5)]
    public string ServicePointName { get; set; }

    public virtual DcLocalOffice Office { get; set; }
}

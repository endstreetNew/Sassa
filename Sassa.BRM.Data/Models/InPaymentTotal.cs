using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sassa.BRM.Models;

[Table("INPAYMENT_TOTALS", Schema = "CONTENTSERVER")]
[Index(nameof(RegionId))]
public class InpaymentTotal
{
    // REGION_ID NVARCHAR2(2)
    [Required]
    [Unicode(true)]
    [MaxLength(2)]
    [Column("REGION_ID")]
    public string RegionId { get; set; } = string.Empty;

    // FOUND NUMBER (count of rows where flag = 1, or sum of flag values)
    [Column("TOTAL")]
    public decimal Total { get; set; }


}
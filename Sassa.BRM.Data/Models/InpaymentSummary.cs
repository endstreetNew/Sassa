using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sassa.BRM.Models;

[Table("INPAYMENT_SUMMARY", Schema = "CONTENTSERVER")]
[Index(nameof(RegionId))]
public class InpaymentSummary
{
    // REGION_ID NVARCHAR2(2)
    [Required]
    [Unicode(true)]
    [MaxLength(2)]
    [Column("REGION_ID")]
    public string RegionId { get; set; } = string.Empty;

    // FLAG_NAME VARCHAR2(10)
    [Required]
    [MaxLength(10)]
    [Column("FLAG_NAME")]
    public string FlagName { get; set; } = string.Empty;

    // FOUND NUMBER (count of rows where flag = 1, or sum of flag values)
    [Column("FOUND")]
    public decimal? Found { get; set; }

    // MISSING NUMBER (count of rows where flag = 0)
    [Column("MISSING")]
    public decimal? Missing { get; set; }
}
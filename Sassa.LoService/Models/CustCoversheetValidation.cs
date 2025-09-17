namespace Sassa.Models;

public partial class CustCoversheetValidation
{
    public string ReferenceNum { get; set; } = null!;

    public DateTime ValidationDate { get; set; }

    public string? Validationresult { get; set; }
}

using System.ComponentModel.DataAnnotations;


namespace Sassa.eForms.Models
{
    public class ResetModel
    {
        [Required]
        [StringLength(20, ErrorMessage = "Password is invalid.", MinimumLength = 5)]
        public string TempPassword { get; set; }
        [Required]
        [StringLength(20, ErrorMessage = "Password is invalid.", MinimumLength = 5)]
        public string Password { get; set; }
        [Required]
        [StringLength(20, ErrorMessage = "Password is invalid.", MinimumLength = 5)]
        public string VerifyPassword { get; set; }
    }
}

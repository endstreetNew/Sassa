using System.ComponentModel.DataAnnotations;

namespace Sassa.eForms.Models
{
    public class SassaLogin
    {
        [Required]
        [StringLength(100, ErrorMessage = "UserName is required.", MinimumLength = 10)]
        public string Email { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Password is invalid.", MinimumLength = 1)]
        public string Password { get; set; }
        public string VerifyCode { get; set; }

        public string ReturnUrl { get; set; }
    }
}

using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sassa.eForms.Models
{
    public class SassaUser : IdentityUser<int>
    {
        public SassaUser()
        {
            EmailConfirmed = true;
            PhoneNumber = "";
            PhoneNumberConfirmed = false;
            TwoFactorEnabled = false;
            LockoutEnabled = false;
            AccessFailedCount = 0;
            CitizenShip = 1;
            IdType = 1;
            PermitExpiry = DateTime.Now.AddYears(1);
            TemporaryIdExpiry = DateTime.Now.AddYears(1);
            SecurityStamp = Guid.NewGuid().ToString();
        }
        //[Key]
        //public int Id { get; set; }
        //Citizenship	Radio Button Group	Yes
        //If South African or Permanent Resident option is selected, the Refugee ID field is hidden.
        //If Refugee option is selected, the Identification Type = Refugee ID is automatically selected and the ID Book and Temporary ID options are hidden.
        [Required]
        [Range(typeof(int), "1", "4", ErrorMessage = "Please select a Citizenship Type")]
        public int CitizenShip { get; set; }
        //Identification Type	Radio Button Group	Yes	
        //If ID Book, Temporary ID or No ID is selected, the Refugee Permit Number and Refugee Expiry Date fields are hidden.
        //If ID Book, No ID or Refugee ID is selected, the Temporary ID Expiry Date field is hidden.
        //If Refugee ID is selected, the Refugee Permit Number and Refugee Expiry Date fields are shown and the Temporary ID Expiry Date field is hidden.
        //If Temporary ID is selected, the Temporary ID Expiry Date field is shown and Refugee Permit Number and Refugee Expiry Date fields are hidden.
        //If No ID selected, Refugee Permit Number, Refugee Expiry Date, Temporary ID Expiry Date and ID Number fields are hidden.
        [Required]
        [Range(typeof(int), "1", "4", ErrorMessage = "Please select an Id Type")]
        public int IdType { get; set; }
        //Required if ID or Temporary ID selected in Identification Type field.
        //Perform ID number validation using the checksum method.
        public string IdNo { get; set; }
        //Required if Refugee ID selected in Identification Type field.
        public string RefugeePermitNo { get; set; }
        //Required if Refugee ID selected in Identification Type field.
        public DateTime PermitExpiry { get; set; }
        //Required if Temporary ID selected in Identification Type field.
        public DateTime TemporaryIdExpiry { get; set; }
        [Required]
        [Range(typeof(int), "1", "4", ErrorMessage = "Please select a Title.")]
        public int Title { get; set; }
        [Required]
        [StringLength(35, ErrorMessage = "Surname is required.", MinimumLength = 3)]
        public string Surname { get; set; }
        [Required]
        [StringLength(35, ErrorMessage = "Name is required.", MinimumLength = 3)]
        public string FullName { get; set; }
        [Required]
        public override string UserName { get; set; }
        //Either Cell Number or Email Address must be captured.
        [Required]
        [EmailAddress]
        public override string Email { get; set; }
        //Either Cell Number or Email Address must be captured.
        [Required]
        [StringLength(13, ErrorMessage = "Invalid Cell Number.", MinimumLength = 10)]
        public string CellNumber { get; set; }

        public int CellNumberConfirmed { get; set; }
        //public override bool EmailConfirmed { get; set; }
        //public override string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        //public override bool TwoFactorEnabled { get; set; }
        //public override int AccessFailedCount { get; set; }
        //[Column("SOME_BOOLEAN")]
        [NotMapped]
        public bool IsCellConfirmed
        {
            get
            {
                return Convert.ToBoolean(CellNumberConfirmed);
            }
            set
            {
                CellNumberConfirmed = Convert.ToInt32(value);
            }
        }
        [NotMapped]
        public bool IsResetting
        {
            get
            {
                return Convert.ToBoolean(AccessFailedCount);
            }
            set
            {
                AccessFailedCount = Convert.ToInt32(value);
            }
        }

    }
    public enum CitizenType
    {
        South_African,
        Permanent_Resident,
        Refugee
    }

    public enum IdType
    {
        ID_Book,
        Temporary_ID,
        Refugee_ID
    }
    public enum Title
    {
        Mr,
        Mrs,
        Miss
    }
}

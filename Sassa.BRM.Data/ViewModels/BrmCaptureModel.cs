using Sassa.BRM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sassa.BRM.ViewModels
{
    public class BrmCaptureModel
    {
        public BrmCaptureModel()
        {
            ApplicationType = ApplicationType.Application;
        }
        [Required]
        public ApplicationType ApplicationType { get; set; } = ApplicationType.Application;
        [Required] 
        public string Name { get; set; } = "";
        [Required]
        public string Surname { get; set; } = "";
        [Required]
        [Display(Name = "BRM Barcode")]
        [StringLength(8)]
        [MinLength(8)]
        public string BrmBarcode { get; set; } = "";
        [Required]
        [Display(Name = "Applicant ID")]
        [StringLength(13)]
        [MinLength(13)]
        public string ApplicantId { get; set; } = "";
        public string SrdNo { get; set; } = "";
        [Required]
        [MinLength(1)]
        public string Documents { get; set; } = "";
        [Required]
        [Display(Name = "Grant Type")]
        public string GrantType { get; set; } = "";

        [Display(Name = "ChildID")]
        [StringLength(13)]
        [MinLength(13)]
        public string ChildId { get; set; } = null;
        [Display(Name = "App Status")]
        [Required]
        public string AppStatus { get; set; } = null;
        public string AppDate { get; set; } = DateTime.Now.ToString("dd/MMM/yy");
        public string LcType { get; set; }
        public string TdwBoxno { get; set; }
        public Reboxing Reboxing { get; set; } = new Reboxing();
        public bool IsManualCapture { get; set; }
        public decimal? LcTypeInt()
        {
            if (decimal.TryParse(LcType, out decimal result))
            {
                return result;
            }
            return null;
        }
        public bool IsGrantTypeEdit()
        {
            return "06C59".Contains(GrantType);
        }
        public bool IsChildIdEdit()
        {
            return "6C59".Contains(GrantType) && (ChildId == null || ChildId.Trim().Length != 13);
        }
        public bool IsPreservedType()
        {
            return "MAIN|LC-MAIN|ARCHIVE|LC-ARCHIVE".Contains(AppStatus);
        }
    }

}

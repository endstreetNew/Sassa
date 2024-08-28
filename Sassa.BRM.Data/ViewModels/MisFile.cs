using System;

namespace Sassa.BRM.Models
{
    public partial class MisFile
    {
        public Nullable<decimal> BATCH_NO { get; set; }
        public string OFFICE_ID { get; set; }
        public string REGION_ID { get; set; }
        public string APPLICANT_NO { get; set; }
        public string GRANT_TYPE { get; set; }
        public Nullable<decimal> TRANS_TYPE { get; set; }
        public string DOCS_PRESENT { get; set; }
        public Nullable<System.DateTime> TRANS_DATE { get; set; }
        public Nullable<decimal> UPDATED_BY { get; set; }
        public Nullable<System.DateTime> UPDATED_DATE { get; set; }
        public Nullable<System.DateTime> BATCH_ADD_DATE { get; set; }
        public string FILE_STATUS { get; set; }
        public string FILE_COMMENT { get; set; }
        public string USER_FIRSTNAME { get; set; }
        public string USER_LASTNAME { get; set; }
        public string APPLICATION_STATUS { get; set; }
        public string DOCS_SCANNED { get; set; }
        public Nullable<System.DateTime> SCAN_DATETIME { get; set; }
        public string BRM_BARCODE { get; set; }
        public string TDW_BOXNO { get; set; }
        public string NON_COMPLIANT { get; set; }
        public string QC_USER_FN { get; set; }
        public string QC_USER_LN { get; set; }
        public Nullable<System.DateTime> QC_DATE { get; set; }
        public string MIS_BOXNO { get; set; }
        public Nullable<System.DateTime> MIS_BOX_DATE { get; set; }
        public Nullable<System.DateTime> MIS_REBOX_DATE { get; set; }
        public string MIS_BOX_STATUS { get; set; }
        public string MIS_REBOX_STATUS { get; set; }
        public string FILE_NUMBER { get; set; }
        public string ARCHIVE_YEAR { get; set; }
        public string EXCLUSIONS { get; set; }
        public string MISSING { get; set; }
        public string TRANSFERRED { get; set; }
        public string COMPLIANT { get; set; }
        public string REGION_ID_FROM { get; set; }
        public Nullable<decimal> TDW_BOX_TYPE_ID { get; set; }
        public string TDW_BOX_ARCHIVE_YEAR { get; set; }
        public string ALT_BOX_NO { get; set; }
        public string SRD_NO { get; set; }
        public Nullable<decimal> TEMP_BOX_NO { get; set; }
        public string UPDATED_BY_AD { get; set; }
        public string CHILD_ID_NO { get; set; }
        public Nullable<decimal> PRINT_ORDER { get; set; }
        public string ISREVIEW { get; set; }
        public Nullable<System.DateTime> LASTREVIEWDATE { get; set; }
        public Nullable<decimal> LCTYPE { get; set; }

    }
}

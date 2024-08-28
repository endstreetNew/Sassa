using System;

namespace Sassa.BRM.Models
{
    public class FileEntity
    {
        public string UNQ_FILE_NO { get; set; }
        public Nullable<decimal> BATCH_NO { get; set; }
        public string OFFICE_NAME { get; set; }
        public string REGION_NAME { get; set; }
        public string APPLICANT_NO { get; set; }
        public string GRANT_TYPE_NAME { get; set; }
        public decimal? TRANS_TYPE { get; set; }
        public string DOCS_PRESENT { get; set; }
        //public string UPDATED_BY_AD { get; set; }
        public Nullable<System.DateTime> TRANS_DATE { get; set; }
        public Nullable<decimal> UPDATED_BY { get; set; }
        public Nullable<System.DateTime> UPDATED_DATE { get; set; }
        public Nullable<System.DateTime> BATCH_ADD_DATE { get; set; }
        public string FILE_STATUS { get; set; }
        public string FILE_COMMENT { get; set; }
        public string FULL_NAME
        {
            get { return FIRST_NAME + " " + LAST_NAME; }
        }
        public string FIRST_NAME { get; set; }
        public string LAST_NAME { get; set; }
        public string CLM_UNIQUE_CODE { get; set; }
        public bool FILE_STATUS_COMPLETED { get; set; }
        public string BRM_BARCODE { get; set; }
        public string TDW_BOXNO { get; set; }
        public string ALT_BOXNO { get; set; }
        public string NON_COMPLIANT { get; set; }
        public string QC_USER_FN { get; set; }
        public string QC_USER_LN { get; set; }
        public Nullable<System.DateTime> QC_DATE { get; set; }
        public bool FILE_NON_COMPLIANT { get; set; }
        public string APPLICATION_STATUS { get; set; }
        public string GRANT_TYPE { get; set; }
        public string APP_DATE { get; set; }
        public DateTime? APP_DATE_DT { get; set; } //Same as APP_DATE, but nullable DateTime type.
        public string FILE_NUMBER { get; set; }
        public string ARCHIVE_YEAR { get; set; }
        public string EXCLUSIONS { get; set; }
        public string MISSING { get; set; }
        public string MIS_BOXNO { get; set; }
        public DateTime MIS_BOX_DATE { get; set; }
        public DateTime MIS_REBOX_DATE { get; set; }
        public string MIS_BOX_STATUS { get; set; }
        public string MIS_REBOX_STATUS { get; set; }
        public string COMPLIANT { get; set; }
        public string SRD_NO { get; set; }
        public string CHILD_ID_NO { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sassa.BRM.Models
{
    public partial class ModelContext
    {
        public virtual DbSet<Application> Applications { get; set; }

        public virtual DbSet<IdResult> IdResults { get; set; }

        protected void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<IdResult>().HasNoKey();

            modelBuilder.Entity<Application>().HasNoKey();
        }
    }
    public partial class DcFileDeleted
    {
        public void FromDCFile(DcFile file)
        {
            this.AltBoxNo = file.AltBoxNo;
            this.ApplicantNo = file.ApplicantNo;
            this.ApplicationStatus = file.ApplicationStatus;
            this.ArchiveYear = file.ArchiveYear;
            this.BatchAddDate = file.BatchAddDate;
            this.BatchNo = file.BatchNo;
            this.BrmBarcode = file.BrmBarcode;
            this.ChildIdNo = file.ChildIdNo;
            this.Compliant = file.Compliant;
            this.DocsPresent = file.DocsPresent;
            this.DocsScanned = file.DocsScanned;
            this.Exclusions = file.Exclusions;
            this.FileComment = file.FileComment;
            this.FileNumber = file.FileNumber;
            this.FileStatus = file.FileStatus;
            this.GrantType = file.GrantType;
            this.Isreview = file.Isreview;
            this.Lastreviewdate = file.Lastreviewdate;
            this.Lctype = file.Lctype;
            this.Missing = file.Missing;
            this.MisBoxno = file.MisBoxno;
            this.MisBoxDate = file.MisBoxDate;
            this.MisBoxStatus = file.MisBoxStatus;
            this.MisReboxDate = file.MisReboxDate;
            this.MisReboxStatus = file.MisReboxStatus;
            this.NonCompliant = file.NonCompliant;
            this.OfficeId = file.OfficeId;
            this.FspId = file.FspId;
            this.QcUserFn = file.QcUserFn;
            this.QcUserLn = file.QcUserLn;
            this.RegionId = file.RegionId;
            this.RegionIdFrom = file.RegionIdFrom;
            this.ScanDatetime = file.ScanDatetime;
            this.SrdNo = file.SrdNo;
            this.TdwBoxno = file.TdwBoxno;
            this.TdwBoxArchiveYear = file.TdwBoxArchiveYear;
            this.TdwBoxTypeId = file.TdwBoxTypeId;
            this.TempBoxNo = file.TempBoxNo;
            this.Transferred = file.Transferred;
            this.TransDate = file.TransDate;
            this.TransType = file.TransType;
            this.UnqFileNo = file.UnqFileNo;
            this.UpdatedBy = file.UpdatedBy;
            this.UpdatedByAd = file.UpdatedByAd;
            this.UpdatedDate = file.UpdatedDate;
            this.UserFirstname = file.UserFirstname;
            this.UserLastname = file.UserLastname;
        }
    }
    public partial class DcFile
    {
        [NotMapped]
        public string RegType
        {
            get
            {
                if (Lctype == null)
                {
                    return ApplicationStatus;
                }
                else
                {
                    if (!ApplicationStatus.StartsWith("LC-"))
                    {
                        return "LC-" + ApplicationStatus;
                    }
                    else
                    {
                        return ApplicationStatus;
                    }
                }
            }
        }
        [NotMapped]
        public bool IsPreservedType
        {
            get
            {
                return "MAIN|LC-MAIN|ARCHIVE|LC-ARCHIVE".Contains(ApplicationStatus);
            }
        }
        [NotMapped]
        public string FullName
        {
            get
            {
                return this.UserFirstname + " " + this.UserLastname;
            }
        }
        [NotMapped]
        public string MergeStatus { get; set; }

        //[NotMapped]
        //public string TdwBoxNo
        //{
        //    get
        //    {
        //        return TdwBoxno;
        //    }
        //    set
        //    {
        //        if(!IsLocked)
        //        {
        //            TdwBoxno = value;
        //        }
        //    }
        //}

        [NotMapped]
        public bool IsLocked
        {
            get
            {
                return BoxLocked == 1;
            }
        }
    }
    public partial class DcFileRequest
    {
        [NotMapped]
        public bool isSelected { get; set; }
        [NotMapped]
        public string Reason { get; set; }
    }
    public partial class DcPicklist
    {
        public string nextStatus
        {
            get
            {
                switch (Status)
                {
                    case "Requested":
                        return "Received";
                    case "Received":
                        return "Scanned";
                    case "Scanned":
                        return "Returned";
                    case "Returned":
                        return "Compliant";
                    default:
                        return "Returned";
                }
            }
        }
    }
    public partial class DcPicklistItem
    {
        public string nextStatus
        {
            get
            {
                switch (Status)
                {
                    case "Requested":
                        return "Received";
                    case "Received":
                        return "Returned";
                    default:
                        return "Returned";
                }
            }
        }
    }
    public partial class DcBatch
    {
        [NotMapped]
        public bool isSelected { get; set; } = false;
        [NotMapped]
        public string BoxNo { get; set; }
        [NotMapped]
        public int MiniBox { get; set; } = 1;
    }

    public partial class DcSocpen
    {
        [NotMapped]
        public bool isSelected { get; set; } = false;
    }


}

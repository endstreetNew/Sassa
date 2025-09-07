using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Sassa.Models;

public partial class LoModelContext : DbContext
{
    public LoModelContext()
    {
    }

    public LoModelContext(DbContextOptions<LoModelContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CustCoversheet> CustCoversheets { get; set; }

    public virtual DbSet<CustCoversheetValidation> CustCoversheetValidations { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseOracle("Data Source=(DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.117.123.51)(PORT = 1525))(CONNECT_DATA =(service_name = ecsqa)));;Persist Security Info=True;User ID=lo_admin;Password=sassa123;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("LO_ADMIN")
            .UseCollation("USING_NLS_COMP");

        modelBuilder.Entity<CustCoversheet>(entity =>
        {
            entity.HasKey(e => e.ReferenceNum).HasName("CUST_COVERSHEET_PK");

            entity.ToTable("CUST_COVERSHEET");

            entity.HasIndex(e => e.Clmnumber, "COVERSHEET_CSNUMBER");

            entity.HasIndex(e => e.DrpdwnGrantTypes, "COVERSHEET_GRANT_TYPES");

            entity.HasIndex(e => e.TxtIdNumber, "COVERSHEET_ID_NUMBER");

            entity.HasIndex(e => new { e.ScanRegion, e.ScanLocaloffice }, "COVERSHEET_REGION");

            entity.HasIndex(e => e.TxtSocpenRefNumber, "COVERSHEET_SOCPEN_REF_NUM");

            entity.HasIndex(e => new { e.TxtSocpenUseridSo, e.TxtNameSo, e.TxtSurnameSo }, "COVERSHEET_SOCPEN_USERID");

            entity.Property(e => e.ReferenceNum)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("REFERENCE_NUM");
            entity.Property(e => e.ApplicationDate)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("APPLICATION_DATE");
            entity.Property(e => e.ArchiveYear)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ARCHIVE_YEAR");
            entity.Property(e => e.BrmNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("BRM_NUMBER");
            entity.Property(e => e.CaptureDate)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CAPTURE_DATE");
            entity.Property(e => e.Clmnumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .HasColumnName("CLMNUMBER");
            entity.Property(e => e.Docsubmitted)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("DOCSUBMITTED");
            entity.Property(e => e.DocsubmittedBrm)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("DOCSUBMITTED_BRM");
            entity.Property(e => e.DrpdwnAppStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("DRPDWN_APP_STATUS");
            entity.Property(e => e.DrpdwnGrantTypes)
                .HasMaxLength(50)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .HasColumnName("DRPDWN_GRANT_TYPES");
            entity.Property(e => e.DrpdwnLcType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DRPDWN_LC_TYPE");
            entity.Property(e => e.DrpdwnLocalOfficeSo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DRPDWN_LOCAL_OFFICE_SO");
            entity.Property(e => e.DrpdwnRegionSo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .HasColumnName("DRPDWN_REGION_SO");
            entity.Property(e => e.DrpdwnStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("DRPDWN_STATUS");
            entity.Property(e => e.DrpdwnStatusLc)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("DRPDWN_STATUS_LC");
            entity.Property(e => e.DrpdwnTransaction)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("DRPDWN_TRANSACTION");
            entity.Property(e => e.Granttypelookup)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("GRANTTYPELOOKUP");
            //entity.Property(e => e.NewApplicationDate)
            //    .HasColumnType("DATE")
            //    .HasColumnName("NEW_APPLICATION_DATE");
            entity.Property(e => e.NewCaptureDate)
                .HasColumnType("DATE")
                .HasColumnName("NEW_CAPTURE_DATE");
            //entity.Property(e => e.NewScannedDate)
            //    .HasColumnType("DATE")
            //    .HasColumnName("NEW_SCANNED_DATE");
            entity.Property(e => e.Poid)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("POID");
            entity.Property(e => e.ScanLocaloffice)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SCAN_LOCALOFFICE");
            entity.Property(e => e.ScanRegion)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("SCAN_REGION");
            entity.Property(e => e.ScannedDate)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SCANNED_DATE");
            entity.Property(e => e.ScannedDocs)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SCANNED_DOCS");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("STATUS");
            entity.Property(e => e.TempGrantTypes)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TEMP_GRANT_TYPES");
            entity.Property(e => e.TxtIdNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TXT_ID_NUMBER");
            entity.Property(e => e.TxtIdNumberChild)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TXT_ID_NUMBER_CHILD");
            entity.Property(e => e.TxtIdNumberChild2)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TXT_ID_NUMBER_CHILD_2");
            entity.Property(e => e.TxtIdNumberChild3)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TXT_ID_NUMBER_CHILD_3");
            entity.Property(e => e.TxtIdNumberChild4)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TXT_ID_NUMBER_CHILD_4");
            entity.Property(e => e.TxtIdNumberChild5)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TXT_ID_NUMBER_CHILD_5");
            entity.Property(e => e.TxtIdNumberChild6)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TXT_ID_NUMBER_CHILD_6");
            entity.Property(e => e.TxtName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TXT_NAME");
            entity.Property(e => e.TxtNameSo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TXT_NAME_SO");
            entity.Property(e => e.TxtSocpenRefNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TXT_SOCPEN_REF_NUMBER");
            entity.Property(e => e.TxtSocpenUseridSo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TXT_SOCPEN_USERID_SO");
            entity.Property(e => e.TxtSrdRefNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TXT_SRD_REF_NUMBER");
            entity.Property(e => e.TxtSurname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TXT_SURNAME");
            entity.Property(e => e.TxtSurnameSo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TXT_SURNAME_SO");
            entity.Property(e => e.TxtUsernameSo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TXT_USERNAME_SO");
        });

        modelBuilder.Entity<CustCoversheetValidation>(entity =>
        {
            entity.HasKey(e => e.ReferenceNum).HasName("COVERSHEET_VALIDATION_RESULT_PK");

            entity.ToTable("CUST_COVERSHEET_VALIDATION");

            entity.Property(e => e.ReferenceNum)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("REFERENCE_NUM");
            entity.Property(e => e.ValidationDate)
                .HasColumnType("DATE")
                .HasColumnName("VALIDATION_DATE");
            entity.Property(e => e.Validationresult)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("VALIDATIONRESULT");
        });
        modelBuilder.HasSequence("CONTAINERSSEQ");
        modelBuilder.HasSequence("CUST_CLMNO_SEQ");
        modelBuilder.HasSequence("CUST_COVERSHEET_AUDIT_SEQ");
        modelBuilder.HasSequence("CUST_CSG_ASSETS_SEQ");
        modelBuilder.HasSequence("CUST_CSG_CHILD_SEQ");
        modelBuilder.HasSequence("CUST_CSG_DEDUCTION_SEQ");
        modelBuilder.HasSequence("CUST_CSG_INCOME_SEQ");
        modelBuilder.HasSequence("CUST_DOCUMENT_TYPES_SEQ");
        modelBuilder.HasSequence("CUST_EXPORT_SEQ");
        modelBuilder.HasSequence("CUST_FCG_FOSTERCHILD_SEQ");
        modelBuilder.HasSequence("CUST_OAG_ASSETS_SEQ");
        modelBuilder.HasSequence("CUST_OAG_DEDUCTION_SEQ");
        modelBuilder.HasSequence("CUST_OAG_INCOME_SEQ");
        modelBuilder.HasSequence("CUST_OGA_OAG_SEQ");
        modelBuilder.HasSequence("EXPORTSETSEQ");
        modelBuilder.HasSequence("FOLDERSSEQ");
        modelBuilder.HasSequence("GROUPSSEQ");
        modelBuilder.HasSequence("KPISEQ");
        modelBuilder.HasSequence("LDAPSERVERSSEQ");
        modelBuilder.HasSequence("ROUTEATTACHSEQ");
        modelBuilder.HasSequence("SEQUENCE");
        modelBuilder.HasSequence("SOCPEN_SEQUENCE");
        modelBuilder.HasSequence("USERQUERIESSEQ");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

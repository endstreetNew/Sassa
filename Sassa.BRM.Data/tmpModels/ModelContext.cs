using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Sassa.BRM.Data.tmpModels;

public partial class ModelContext : DbContext
{
    public ModelContext()
    {
    }

    public ModelContext(DbContextOptions<ModelContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Inpayment> Inpayments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseOracle("DATA SOURCE=10.124.159.31:1527/ecsbrm;PERSIST SECURITY INFO=True;USER ID=CONTENTSERVER;Password=Password123;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("CONTENTSERVER")
            .UseCollation("USING_NLS_COMP");

        modelBuilder.Entity<Inpayment>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("INPAYMENT");

            entity.Property(e => e.ApplicantNo)
                .IsRequired()
                .HasMaxLength(13)
                .HasColumnName("APPLICANT_NO");
            entity.Property(e => e.BrmBarcode)
                .HasMaxLength(8)
                .HasColumnName("BRM_BARCODE");
            entity.Property(e => e.ChildIdNo)
                .HasMaxLength(13)
                .HasColumnName("CHILD_ID_NO");
            entity.Property(e => e.ClmNo)
                .HasMaxLength(20)
                .HasColumnName("CLM_NO");
            entity.Property(e => e.EcMisFile)
                .HasMaxLength(20)
                .HasColumnName("EC_MIS_FILE");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("FIRST_NAME");
            entity.Property(e => e.GrantType)
                .IsRequired()
                .HasMaxLength(2)
                .HasColumnName("GRANT_TYPE");
            entity.Property(e => e.Id)
                .HasPrecision(19)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.MisFileNo)
                .HasMaxLength(20)
                .HasColumnName("MIS_FILE_NO");
            entity.Property(e => e.OgaStatus)
                .HasMaxLength(30)
                .HasColumnName("OGA_STATUS");
            entity.Property(e => e.Paypoint)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("PAYPOINT");
            entity.Property(e => e.RegType)
                .HasMaxLength(10)
                .HasColumnName("REG_TYPE");
            entity.Property(e => e.RegionId)
                .IsRequired()
                .HasMaxLength(2)
                .HasColumnName("REGION_ID");
            entity.Property(e => e.Surname)
                .HasMaxLength(50)
                .HasColumnName("SURNAME");
            entity.Property(e => e.TransDate)
                .HasColumnType("DATE")
                .HasColumnName("TRANS_DATE");
        });
        modelBuilder.HasSequence("ACTIVEVIEWOVERRIDESSEQUENCE");
        modelBuilder.HasSequence("AGENTSEQUENCE");
        modelBuilder.HasSequence("AUDITCOLLECTIONSITEMSSEQ");
        modelBuilder.HasSequence("BRMWAYBIL");
        modelBuilder.HasSequence("CUST_ACTIVE_GRANTS_SEQ");
        modelBuilder.HasSequence("CUST_DISTRICTSEQ");
        modelBuilder.HasSequence("DAUDITNEWSEQUENCE");
        modelBuilder.HasSequence("DDOCUMENTCLASSSEQUENCE");
        modelBuilder.HasSequence("DFAVORITESTABSSEQUENCE");
        modelBuilder.HasSequence("DPSINSRTPROPSSEQ");
        modelBuilder.HasSequence("DPSTASKSSEQUENCE");
        modelBuilder.HasSequence("DSOCIALFEEDEVENTSSEQ");
        modelBuilder.HasSequence("DSOCIALFOLLOWERSSEQ");
        modelBuilder.HasSequence("DSTAGINGIMPORTSEQUENCE");
        modelBuilder.HasSequence("DSUGGESTWORDSPENDINGSEQUENCE");
        modelBuilder.HasSequence("DSUGGESTWORDSSEQUENCE");
        modelBuilder.HasSequence("DTREECOREEXTSOURCESEQUENCE");
        modelBuilder.HasSequence("DTREENOTIFYSEQUENCE");
        modelBuilder.HasSequence("ELINKMESSAGESEQUENCE").IsCyclic();
        modelBuilder.HasSequence("FILECACHESEQUENCE");
        modelBuilder.HasSequence("KUAFIDENTITYSEQUENCE");
        modelBuilder.HasSequence("KUAFIDENTITYTYPESEQUENCE");
        modelBuilder.HasSequence("LLEVENTSSEQUENCE");
        modelBuilder.HasSequence("NOTIFYSEQUENCE");
        modelBuilder.HasSequence("OI_STATUS_SEQ");
        modelBuilder.HasSequence("PROVIDERRETRYSEQUENCE");
        modelBuilder.HasSequence("RECD_HOTSEQ");
        modelBuilder.HasSequence("RECD_OPERATIONTRACKINGSEQ");
        modelBuilder.HasSequence("RENDITIONFOLDERSSEQ");
        modelBuilder.HasSequence("RENDITIONMIMETYPERULESSEQ");
        modelBuilder.HasSequence("RENDITIONNODERULESSEQ");
        modelBuilder.HasSequence("RENDITIONQUEUESEQ");
        modelBuilder.HasSequence("RESULTIDSEQUENCE");
        modelBuilder.HasSequence("RETENTIONUPDATEFAILEDSEQNID");
        modelBuilder.HasSequence("RETENTIONUPDATELOGSEQNID");
        modelBuilder.HasSequence("RETENTIONUPDATEORDERSEQNID");
        modelBuilder.HasSequence("RM_HOLDQUERYHISTORYSEQUENCE");
        modelBuilder.HasSequence("RMSEC_DEFINEDRULESEQUENCE");
        modelBuilder.HasSequence("SEARCHSTATSSEQUENCE");
        modelBuilder.HasSequence("SEQ_CUST_REGION_REGNUM");
        modelBuilder.HasSequence("SEQ_DC_ALT_BOX_NO_ECA");
        modelBuilder.HasSequence("SEQ_DC_ALT_BOX_NO_FST");
        modelBuilder.HasSequence("SEQ_DC_ALT_BOX_NO_GAU");
        modelBuilder.HasSequence("SEQ_DC_ALT_BOX_NO_KZN");
        modelBuilder.HasSequence("SEQ_DC_ALT_BOX_NO_LIM");
        modelBuilder.HasSequence("SEQ_DC_ALT_BOX_NO_MPU");
        modelBuilder.HasSequence("SEQ_DC_ALT_BOX_NO_NCA");
        modelBuilder.HasSequence("SEQ_DC_ALT_BOX_NO_NWP");
        modelBuilder.HasSequence("SEQ_DC_ALT_BOX_NO_WCA");
        modelBuilder.HasSequence("SEQ_DC_BATCH");
        modelBuilder.HasSequence("SEQ_DC_BOXPICKED");
        modelBuilder.HasSequence("SEQ_DC_FILE");
        modelBuilder.HasSequence("SEQ_DC_FILE_REQUEST");
        modelBuilder.HasSequence("SEQ_DC_PICKLIST");
        modelBuilder.HasSequence("SEQ_TDW_BATCH");
        modelBuilder.HasSequence("WORKERQUEUESEQUENCE");
        modelBuilder.HasSequence("WWORKAUDITSEQ");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

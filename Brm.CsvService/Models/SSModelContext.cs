using Microsoft.EntityFrameworkCore;

namespace Brm.CsvService.Models;

public partial class SSModelContext : DbContext
{
    public SSModelContext()
    {
    }

    public SSModelContext(DbContextOptions<SSModelContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SsApp> SsApps { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseOracle("DATA SOURCE=ssvsdrdbshc01.sassa.local:1527/ecsbrm;PERSIST SECURITY INFO=True;USER ID=CONTENTSERVER;Password=Password123;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("CONTENTSERVER")
            .UseCollation("USING_NLS_COMP");

        modelBuilder.Entity<SsApp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C0037356");

            entity.ToTable("SS_APP");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.AYear)
                .HasColumnType("NUMBER")
                .HasColumnName("A_YEAR");
            entity.Property(e => e.ActionDate)
                .HasColumnType("DATE")
                .HasColumnName("ACTION_DATE");
            entity.Property(e => e.ActionResult)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ACTION_RESULT");
            entity.Property(e => e.ApplStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("APPL_STATUS");
            entity.Property(e => e.ApplicationDate)
                .HasColumnType("DATE")
                .HasColumnName("APPLICATION_DATE");
            entity.Property(e => e.Box)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("BOX");
            entity.Property(e => e.BoxType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("BOX_TYPE");
            entity.Property(e => e.DisabilityDesc)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("DISABILITY_DESC");
            entity.Property(e => e.DisabilityType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DISABILITY_TYPE");
            entity.Property(e => e.DistrictOffice)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DISTRICT_OFFICE");
            entity.Property(e => e.FormNo)
                .HasColumnType("NUMBER")
                .HasColumnName("FORM_NO");
            entity.Property(e => e.FormType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("FORM_TYPE");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("GENDER");
            entity.Property(e => e.GrantType)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("GRANT_TYPE");
            entity.Property(e => e.IdNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ID_NUMBER");
            entity.Property(e => e.MedNo)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("MED_NO");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NAME");
            entity.Property(e => e.Position)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("POSITION");
            entity.Property(e => e.Race)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("RACE");
            entity.Property(e => e.RegionCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("REGION_CODE");
            entity.Property(e => e.ServiceOffice)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SERVICE_OFFICE");
            entity.Property(e => e.Surname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SURNAME");
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

using Microsoft.EntityFrameworkCore;

namespace Sassa.Socpen.Data
{
    public partial class SocpenContext : DbContext
    {
        public SocpenContext()
        {
        }

        public SocpenContext(DbContextOptions<SocpenContext> options) : base(options)
        {
        }

        public virtual DbSet<CustRescode> CustRescodes { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                throw new Exception("Please configure the database connection string in the appsettings.json file");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("SASSA")
                .UseCollation("USING_NLS_COMP");

            modelBuilder.Entity<CustRescode>(entity =>
            {
                entity.HasKey(e => e.ResCode)
                    .HasName("CUST_RESCODES_PK");

                entity.ToTable("CUST_RESCODES");

                entity.HasIndex(e => e.OfficeId, "SUCT_RESCODE_OFFICE_ID");

                entity.Property(e => e.ResCode)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("RES_CODE");

                entity.Property(e => e.DistrictCode)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("DISTRICT_CODE");

                entity.Property(e => e.DistrictName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("DISTRICT_NAME");

                entity.Property(e => e.LocalOffice)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("LOCAL_OFFICE");

                entity.Property(e => e.Municipality)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("MUNICIPALITY");

                entity.Property(e => e.OfficeId)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("OFFICE_ID");

                entity.Property(e => e.Region)
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .HasColumnName("REGION");

                entity.Property(e => e.RegionCode)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("REGION_CODE");

                entity.Property(e => e.Status)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasColumnName("STATUS");

                entity.Property(e => e.WardId)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("WARD_ID");
            });

            modelBuilder.HasSequence("CUST_REGIONS_LOOKUP_RID_SEQ");

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

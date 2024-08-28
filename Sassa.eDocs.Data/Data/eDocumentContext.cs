using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sassa.eDocs.Data.Models;

namespace Sassa.eDocs
{
    public partial class eDocumentContext : DbContext
    {
        //Scaffold-DbContext "DATA SOURCE=SSVSCSODBPHC01.SASSA.LOCAL:1521/ecsprod;USER ID=edocs;Password=Password123;" Oracle.EntityFrameworkCore -OutputDir Context -Tables TDW_FILE_LOCATION -f
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<LoDocumentType> LoDocumentTypes { get; set; }
        public DbSet<ApplicationType> ApplicationTypes { get; set; }
        public DbSet<ApplicationDocument> ApplicationDocuments { get; set; }
        public DbSet<RejectReason> RejectReasons { get; set; }
        public DbSet<DocImage> DocImage { get; set; }
        public DbSet<RejectHistory> RejectHistory { get; set; }
        public virtual DbSet<ProcessedGrant> ProcessedGrants { get; set; }

        public eDocumentContext()
        {

        }
        public eDocumentContext(DbContextOptions<eDocumentContext> options): base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseOracle(@"DATA SOURCE=SSVSCSODBPHC01.SASSA.LOCAL:1521/ecsprod;USER ID=edocs;Password=Password123;");
                //optionsBuilder.UseOracle(_config.GetConnectionString("edocsConnectionstring"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>().HasIndex(p => new { p.Reference });
            modelBuilder.Entity<Document>().HasIndex(p => new { p.IdNo });
            modelBuilder.Entity<Document>().HasIndex(p => new { p.ChildIdNo });
            modelBuilder.Entity<ProcessedGrant>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("PROCESSED_GRANTS");

                entity.Property(e => e.ProcessDate)
                    .HasColumnType("DATE")
                    .HasColumnName("PROCESS_DATE");

                entity.Property(e => e.Reference)
                    .HasMaxLength(20)
                    .HasColumnName("REFERENCE");

                entity.Property(e => e.RegionCode)
                    .HasMaxLength(13)
                    .HasColumnName("REGION_CODE");
            });
        }
    }
}

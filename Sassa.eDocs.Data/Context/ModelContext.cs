using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Sassa.eDocs.Data.Context
{
    public partial class ModelContext : DbContext
    {
        public ModelContext()
        {
        }

        public ModelContext(DbContextOptions<ModelContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ProcessedGrant> ProcessedGrants { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseOracle("DATA SOURCE=SSVSCSODBPHC01.SASSA.LOCAL:1521/ecsprod;USER ID=edocs;Password=Password123;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("EDOCS")
                .UseCollation("USING_NLS_COMP");

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

            modelBuilder.HasSequence("KOFAXDOCUMENTS_SEQ");

            modelBuilder.HasSequence("LODOCTYPES_SEQ");

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

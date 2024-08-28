using Microsoft.EntityFrameworkCore;
using Sassa.eForms.Models;

namespace Sassa.eForms
{
    public partial class SassaServicesContext : DbContext
    {

        public DbSet<SassaUser> Users { get; set; }
        public SassaServicesContext()
        {

        }

        public SassaServicesContext(DbContextOptions<SassaServicesContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseOracle(@"DATA SOURCE=SSVSCSODBPHC01.SASSA.LOCAL:1521/ecsprod;USER ID=ESERVICES;Password=Password123;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SassaUser>()
               .HasIndex(p => new { p.IdNo }).IsUnique();
            modelBuilder.Entity<SassaUser>()
               .HasIndex(p => new { p.UserName }).IsUnique();
            modelBuilder.Entity<SassaUser>()
               .HasIndex(p => new { p.Email }).IsUnique();
            //          modelBuilder.Properties()
            //.Where(p => p.PropertyType == typeof(Boolean))
            //.Configure(p => p.HasColumnType("NumberBool"));
            //          OnModelCreatingPartial(modelBuilder);

        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

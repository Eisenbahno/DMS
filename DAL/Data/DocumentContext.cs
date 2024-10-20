using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL.Data
{
    public sealed class DocumentContext(DbContextOptions<DocumentContext> options) : DbContext(options)
    {
        public DbSet<DocumentItem>? DocumentItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Manuelle Konfiguration der Tabelle
            modelBuilder.Entity<DocumentItem>(entity =>
            {
                entity.ToTable("DocumentItems");  // Setzt den Tabellennamen

                entity.HasKey(e => e.Id);  // Setzt den Primärschlüssel

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);  // Konfiguriert den "Name"-Spalten

            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TP1.Models;

namespace TP1.Data
{
    public class ApplicationDbContext : IdentityDbContext<Utilisateur>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Terrain> Terrains { get; set; }
        public DbSet<Creneau> Creneaux { get; set; }
        public DbSet<PanierItem> PanierItems { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Paiement> Paiements { get; set; }
        public DbSet<Facture> Factures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration des relations
            modelBuilder.Entity<Creneau>()
                .HasOne(c => c.Terrain)
                .WithMany(t => t.Creneaux)
                .HasForeignKey(c => c.TerrainId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PanierItem>()
                .HasOne(p => p.Utilisateur)
                .WithMany(u => u.PanierItems)
                .HasForeignKey(p => p.UtilisateurId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PanierItem>()
                .HasOne(p => p.Creneau)
                .WithMany(c => c.PanierItems)
                .HasForeignKey(p => p.CreneauId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Utilisateur)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UtilisateurId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Creneau)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.CreneauId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Paiement>()
                .HasOne(p => p.Reservation)
                .WithOne(r => r.Paiement)
                .HasForeignKey<Paiement>(p => p.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Facture>()
                .HasOne(f => f.Reservation)
                .WithOne(r => r.Facture)
                .HasForeignKey<Facture>(f => f.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index pour améliorer les performances
            modelBuilder.Entity<Creneau>()
                .HasIndex(c => c.Date);

            modelBuilder.Entity<Creneau>()
                .HasIndex(c => c.TerrainId);

            modelBuilder.Entity<Reservation>()
                .HasIndex(r => r.UtilisateurId);

            modelBuilder.Entity<PanierItem>()
                .HasIndex(p => p.UtilisateurId);
        }
    }
}



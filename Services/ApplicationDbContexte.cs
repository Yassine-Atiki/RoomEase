using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RoomEase.Models;

namespace RoomEase.Services
{
    // Correction : Héritez de IdentityDbContext<AppUser> qui hérite déjà de DbContext
    public class ApplicationDbContexte : IdentityDbContext<AppUser>
    {
        // Correction : Utilisez DbContextOptions au lieu de DbContextOptions<ApplicationDbContexte>
        public ApplicationDbContexte(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<RoomEquipment> RoomEquipments { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Correction : override au lieu de new pour OnModelCreating
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IMPORTANT : Toujours appeler la base pour l'Identity Framework
            base.OnModelCreating(modelBuilder);

            // Configuration de la table de jointure Many-to-Many
            modelBuilder.Entity<RoomEquipment>(entity =>
            {
                // DÉFINITION DE LA CLÉ COMPOSITE (Solution de l'erreur)
                entity.HasKey(re => new { re.RoomId, re.EquipmentId });

                // Configuration de la relation avec Room
                entity.HasOne(re => re.Room)
                    .WithMany(r => r.RoomEquipments)
                    .HasForeignKey(re => re.RoomId);

                // Configuration de la relation avec Equipment
                entity.HasOne(re => re.Equipment)
                    .WithMany(e => e.RoomEquipments)
                    .HasForeignKey(re => re.EquipmentId);
            });

            // Ajoutez ici la configuration pour les autres modèles (si nécessaire, 
            // comme la conversion de l'Enum ReservationStatus)

            modelBuilder.Entity<Reservation>()
                .Property(r => r.Status)
                .HasConversion<string>();
        }
    }
}
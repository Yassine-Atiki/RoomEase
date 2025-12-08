namespace RoomEase.Models
{
    using Microsoft.AspNetCore.Identity;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    // 1. Utilisateur (Identité étendue)
    public class AppUser : IdentityUser
    {
        // IdentityUser gère déjà : Id, UserName, Email, PasswordHash, etc.

        [Required]
        [Display(Name = "Nom complet")]
        public string FullName { get; set; }

        [Display(Name = "Département / Service")]
        public string? Department { get; set; }

        // Relation : Un utilisateur peut avoir plusieurs réservations
        public virtual ICollection<Reservation> Reservations { get; set; }

        // Relation : Un utilisateur peut recevoir plusieurs notifications
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
namespace RoomEase.Models
{
    using Microsoft.AspNetCore.Identity;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Heure de début")]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "Heure de fin")]
        public DateTime EndTime { get; set; }

        [Display(Name = "Motif / Sujet")]
        public string? Purpose { get; set; }

        // Statut de la réservation
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        // Clés étrangères
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } // Réservé par

        [Required]
        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; } // Salle réservée
    }
}
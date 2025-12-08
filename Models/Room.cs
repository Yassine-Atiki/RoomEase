namespace RoomEase.Models
{
    using Microsoft.AspNetCore.Identity;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Room
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Nom de la salle")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Capacité")]
        public int Capacity { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Est disponible")]
        public bool IsAvailable { get; set; } = true;

        // Navigation pour la relation Many-to-Many avec Equipment
        public virtual ICollection<RoomEquipment> RoomEquipments { get; set; }

        // Navigation pour la relation One-to-Many avec Reservation
        public virtual ICollection<Reservation> Reservations { get; set; }
    }
}
namespace RoomEase.Models
{
    using Microsoft.AspNetCore.Identity;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Equipment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Nom de l'équipement")]
        public string Name { get; set; }

        // Navigation pour la relation Many-to-Many avec Room
        public virtual ICollection<RoomEquipment> RoomEquipments { get; set; }
    }
}
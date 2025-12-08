using System.ComponentModel.DataAnnotations;

namespace RoomEase.ViewModels
{
    public class ReservationViewModel
    {
        [Required(ErrorMessage = "La salle est requise")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "La date de début est requise")]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "L'heure de début est requise")]
        [Display(Name = "Heure de début")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "L'heure de fin est requise")]
        [Display(Name = "Heure de fin")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Motif / Sujet")]
        [StringLength(500, ErrorMessage = "Le motif ne peut pas dépasser 500 caractères")]
        public string? Purpose { get; set; }

        // For display purposes
        public string? RoomName { get; set; }
        public int? RoomCapacity { get; set; }
    }
}

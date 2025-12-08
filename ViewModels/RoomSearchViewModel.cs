using System.ComponentModel.DataAnnotations;

namespace RoomEase.ViewModels
{
    public class RoomSearchViewModel
    {
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        [Display(Name = "Capacité minimale")]
        [Range(1, 1000, ErrorMessage = "La capacité doit être entre 1 et 1000")]
        public int? MinCapacity { get; set; }

        [Display(Name = "Équipements requis")]
        public List<int>? EquipmentIds { get; set; }

        [Display(Name = "Heure de début")]
        [DataType(DataType.Time)]
        public TimeSpan? StartTime { get; set; }

        [Display(Name = "Heure de fin")]
        [DataType(DataType.Time)]
        public TimeSpan? EndTime { get; set; }
    }
}

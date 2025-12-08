using RoomEase.Models;

namespace RoomEase.ViewModels
{
    public class ReservationDetailViewModel
    {
        public int Id { get; set; }
        public string RoomName { get; set; }
        public int RoomCapacity { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Purpose { get; set; }
        public ReservationStatus Status { get; set; }
        public string StatusText => Status switch
        {
            ReservationStatus.Pending => "En attente",
            ReservationStatus.Approved => "Approuvée",
            ReservationStatus.Rejected => "Refusée",
            ReservationStatus.Cancelled => "Annulée",
            _ => "Inconnu"
        };
        public string StatusClass => Status switch
        {
            ReservationStatus.Pending => "warning",
            ReservationStatus.Approved => "success",
            ReservationStatus.Rejected => "danger",
            ReservationStatus.Cancelled => "secondary",
            _ => "secondary"
        };
    }
}

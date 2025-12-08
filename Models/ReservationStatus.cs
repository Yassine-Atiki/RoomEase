namespace RoomEase.Models
{
    using Microsoft.AspNetCore.Identity;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public enum ReservationStatus
    {
        Pending,   // En attente
        Approved,  // Validée
        Rejected,  // Refusée
        Cancelled  // Annulée par l'utilisateur
    }
}

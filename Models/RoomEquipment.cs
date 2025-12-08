namespace RoomEase.Models
{
    using Microsoft.AspNetCore.Identity;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class RoomEquipment
    {
        public int RoomId { get; set; }
        public Room Room { get; set; }

        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; }
    }
}
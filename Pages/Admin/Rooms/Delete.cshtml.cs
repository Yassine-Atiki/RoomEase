using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RoomEase.Models;
using RoomEase.Services;

namespace RoomEase.Pages.Admin.Rooms
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContexte _context;

        public DeleteModel(ApplicationDbContexte context)
        {
            _context = context;
        }

        [BindProperty]
        public Room Room { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Room = await _context.Rooms
                .Include(r => r.RoomEquipments)
                .ThenInclude(re => re.Equipment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Room == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Room = await _context.Rooms
                .Include(r => r.RoomEquipments)
                .Include(r => r.Reservations)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Room != null)
            {
                // Check if there are active reservations
                var hasActiveReservations = Room.Reservations != null && 
                    Room.Reservations.Any(r => r.Status == ReservationStatus.Approved || 
                                              r.Status == ReservationStatus.Pending);

                if (hasActiveReservations)
                {
                    ErrorMessage = "Impossible de supprimer cette salle car elle a des réservations actives.";
                    return Page();
                }

                // Remove equipment relationships first
                if (Room.RoomEquipments != null)
                {
                    _context.RoomEquipments.RemoveRange(Room.RoomEquipments);
                }

                _context.Rooms.Remove(Room);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}

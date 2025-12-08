using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RoomEase.Models;
using RoomEase.Services;

namespace RoomEase.Pages.Admin.Rooms
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContexte _context;

        public CreateModel(ApplicationDbContexte context)
        {
            _context = context;
        }

        [BindProperty]
        public Room Room { get; set; }

        [BindProperty]
        public List<int> SelectedEquipmentIds { get; set; } = new List<int>();

        public List<Models.Equipment> AvailableEquipments { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            AvailableEquipments = await _context.Equipments.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                AvailableEquipments = await _context.Equipments.ToListAsync();
                return Page();
            }

            _context.Rooms.Add(Room);
            await _context.SaveChangesAsync();

            // Add selected equipment
            if (SelectedEquipmentIds != null && SelectedEquipmentIds.Any())
            {
                foreach (var equipmentId in SelectedEquipmentIds)
                {
                    _context.RoomEquipments.Add(new RoomEquipment
                    {
                        RoomId = Room.Id,
                        EquipmentId = equipmentId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}

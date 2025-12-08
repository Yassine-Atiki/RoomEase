using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RoomEase.Models;
using RoomEase.Services;

namespace RoomEase.Pages.Admin.Rooms
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContexte _context;

        public EditModel(ApplicationDbContexte context)
        {
            _context = context;
        }

        [BindProperty]
        public Room Room { get; set; }

        [BindProperty]
        public List<int> SelectedEquipmentIds { get; set; } = new List<int>();

        public List<Models.Equipment> AvailableEquipments { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Room = await _context.Rooms
                .Include(r => r.RoomEquipments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Room == null)
            {
                return NotFound();
            }

            AvailableEquipments = await _context.Equipments.ToListAsync();
            SelectedEquipmentIds = Room.RoomEquipments.Select(re => re.EquipmentId).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                AvailableEquipments = await _context.Equipments.ToListAsync();
                return Page();
            }

            _context.Attach(Room).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Update equipment relationships
                var existingEquipments = await _context.RoomEquipments
                    .Where(re => re.RoomId == Room.Id)
                    .ToListAsync();

                _context.RoomEquipments.RemoveRange(existingEquipments);

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
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(Room.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RoomEase.Services;

namespace RoomEase.Pages.Admin.Equipment
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
        public Models.Equipment Equipment { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Equipment = await _context.Equipments
                .Include(e => e.RoomEquipments)
                .ThenInclude(re => re.Room)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Equipment == null)
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

            Equipment = await _context.Equipments
                .Include(e => e.RoomEquipments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Equipment != null)
            {
                // Check if equipment is used in any rooms
                if (Equipment.RoomEquipments != null && Equipment.RoomEquipments.Any())
                {
                    ErrorMessage = "Impossible de supprimer cet équipement car il est utilisé dans des salles.";
                    Equipment = await _context.Equipments
                        .Include(e => e.RoomEquipments)
                        .ThenInclude(re => re.Room)
                        .FirstOrDefaultAsync(m => m.Id == id);
                    return Page();
                }

                _context.Equipments.Remove(Equipment);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}

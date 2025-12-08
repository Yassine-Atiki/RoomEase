using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RoomEase.Services;

namespace RoomEase.Pages.Admin.Equipment
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContexte _context;
        private readonly ILogger<EditModel> _logger;

        public EditModel(ApplicationDbContexte context, ILogger<EditModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public string Name { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _context.Equipments.FirstOrDefaultAsync(m => m.Id == id);

            if (equipment == null)
            {
                return NotFound();
            }

            Id = equipment.Id;
            Name = equipment.Name;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Remove navigation property errors from ModelState
            ModelState.Remove("Equipment.RoomEquipments");

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogWarning("Validation Error: {Error}", error.ErrorMessage);
                    }
                }
                return Page();
            }

            var equipment = await _context.Equipments.FindAsync(Id);
            if (equipment == null)
            {
                return NotFound();
            }

            equipment.Name = Name;

            try
            {
                _context.Equipments.Update(equipment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Equipment updated successfully: {EquipmentName}", equipment.Name);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EquipmentExists(Id))
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

        private bool EquipmentExists(int id)
        {
            return _context.Equipments.Any(e => e.Id == id);
        }
    }
}

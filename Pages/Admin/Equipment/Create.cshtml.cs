using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoomEase.Services;

namespace RoomEase.Pages.Admin.Equipment
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContexte _context;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ApplicationDbContexte context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public string Name { get; set; }

        public IActionResult OnGet()
        {
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

            var equipment = new Models.Equipment
            {
                Name = Name
            };

            _context.Equipments.Add(equipment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Equipment created successfully: {EquipmentName}", equipment.Name);
            return RedirectToPage("./Index");
        }
    }
}

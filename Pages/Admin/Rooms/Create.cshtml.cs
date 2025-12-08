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
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ApplicationDbContexte context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public int Capacity { get; set; }

        [BindProperty]
        public string? Description { get; set; }

        [BindProperty]
        public bool IsAvailable { get; set; } = true;

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
            // Remove navigation property errors from ModelState
            ModelState.Remove("Room.RoomEquipments");
            ModelState.Remove("Room.Reservations");

            // Log validation errors for debugging
            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogWarning("Validation Error: {Error}", error.ErrorMessage);
                    }
                }
                AvailableEquipments = await _context.Equipments.ToListAsync();
                return Page();
            }

            // Create room entity from bound properties
            var room = new Room
            {
                Name = Name,
                Capacity = Capacity,
                Description = Description,
                IsAvailable = IsAvailable
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            // Add selected equipment
            if (SelectedEquipmentIds != null && SelectedEquipmentIds.Any())
            {
                foreach (var equipmentId in SelectedEquipmentIds)
                {
                    _context.RoomEquipments.Add(new RoomEquipment
                    {
                        RoomId = room.Id,
                        EquipmentId = equipmentId
                    });
                }
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Room created successfully: {RoomName}", room.Name);
            return RedirectToPage("./Index");
        }
    }
}

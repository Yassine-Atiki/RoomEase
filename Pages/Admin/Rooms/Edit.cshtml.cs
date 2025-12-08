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

        [BindProperty]
        public int Capacity { get; set; }

        [BindProperty]
        public string? Description { get; set; }

        [BindProperty]
        public bool IsAvailable { get; set; }

        [BindProperty]
        public List<int> SelectedEquipmentIds { get; set; } = new List<int>();

        public List<Models.Equipment> AvailableEquipments { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.RoomEquipments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            // Populate bound properties
            Id = room.Id;
            Name = room.Name;
            Capacity = room.Capacity;
            Description = room.Description;
            IsAvailable = room.IsAvailable;

            AvailableEquipments = await _context.Equipments.ToListAsync();
            SelectedEquipmentIds = room.RoomEquipments?.Select(re => re.EquipmentId).ToList() ?? new List<int>();

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

            // Find the existing room
            var room = await _context.Rooms.FindAsync(Id);
            if (room == null)
            {
                return NotFound();
            }

            // Update room properties
            room.Name = Name;
            room.Capacity = Capacity;
            room.Description = Description;
            room.IsAvailable = IsAvailable;

            try
            {
                _context.Rooms.Update(room);
                await _context.SaveChangesAsync();

                // Update equipment relationships
                var existingEquipments = await _context.RoomEquipments
                    .Where(re => re.RoomId == Id)
                    .ToListAsync();

                _context.RoomEquipments.RemoveRange(existingEquipments);

                if (SelectedEquipmentIds != null && SelectedEquipmentIds.Any())
                {
                    foreach (var equipmentId in SelectedEquipmentIds)
                    {
                        _context.RoomEquipments.Add(new RoomEquipment
                        {
                            RoomId = Id,
                            EquipmentId = equipmentId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Room updated successfully: {RoomName}", room.Name);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(Id))
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

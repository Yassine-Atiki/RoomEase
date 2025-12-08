using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RoomEase.Models;
using RoomEase.Services;
using RoomEase.ViewModels;

namespace RoomEase.Pages.Rooms
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContexte _context;

        public IndexModel(ApplicationDbContexte context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public RoomSearchViewModel SearchCriteria { get; set; } = new RoomSearchViewModel();

        public List<Room> AvailableRooms { get; set; } = new List<Room>();
        public List<Equipment> AllEquipments { get; set; } = new List<Equipment>();

        public async Task OnGetAsync()
        {
            AllEquipments = await _context.Equipments.ToListAsync();

            var query = _context.Rooms
                .Include(r => r.RoomEquipments)
                .ThenInclude(re => re.Equipment)
                .Where(r => r.IsAvailable);

            // Filter by minimum capacity
            if (SearchCriteria.MinCapacity.HasValue)
            {
                query = query.Where(r => r.Capacity >= SearchCriteria.MinCapacity.Value);
            }

            // Filter by equipment
            if (SearchCriteria.EquipmentIds != null && SearchCriteria.EquipmentIds.Any())
            {
                foreach (var equipmentId in SearchCriteria.EquipmentIds)
                {
                    query = query.Where(r => r.RoomEquipments.Any(re => re.EquipmentId == equipmentId));
                }
            }

            var rooms = await query.ToListAsync();

            // Filter by availability (check for conflicts)
            if (SearchCriteria.Date.HasValue && SearchCriteria.StartTime.HasValue && SearchCriteria.EndTime.HasValue)
            {
                var searchDate = SearchCriteria.Date.Value.Date;
                var requestedStart = searchDate.Add(SearchCriteria.StartTime.Value);
                var requestedEnd = searchDate.Add(SearchCriteria.EndTime.Value);

                var availableRoomIds = new List<int>();
                foreach (var room in rooms)
                {
                    var hasConflict = await _context.Reservations
                        .AnyAsync(r => r.RoomId == room.Id
                            && r.Status != ReservationStatus.Rejected
                            && r.Status != ReservationStatus.Cancelled
                            && r.StartTime < requestedEnd
                            && r.EndTime > requestedStart);

                    if (!hasConflict)
                    {
                        availableRoomIds.Add(room.Id);
                    }
                }

                AvailableRooms = rooms.Where(r => availableRoomIds.Contains(r.Id)).ToList();
            }
            else
            {
                AvailableRooms = rooms;
            }
        }
    }
}

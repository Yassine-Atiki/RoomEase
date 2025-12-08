using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RoomEase.Models;
using RoomEase.Services;

namespace RoomEase.Pages.Admin.Rooms
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContexte _context;

        public IndexModel(ApplicationDbContexte context)
        {
            _context = context;
        }

        public IList<Room> Rooms { get; set; }

        public async Task OnGetAsync()
        {
            Rooms = await _context.Rooms
                .Include(r => r.RoomEquipments)
                .ThenInclude(re => re.Equipment)
                .ToListAsync();
        }
    }
}

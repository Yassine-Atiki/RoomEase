using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RoomEase.Models;
using RoomEase.Services;

namespace RoomEase.Pages.Admin.Equipment
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContexte _context;

        public IndexModel(ApplicationDbContexte context)
        {
            _context = context;
        }

        public IList<Models.Equipment> Equipments { get; set; }

        public async Task OnGetAsync()
        {
            Equipments = await _context.Equipments
                .Include(e => e.RoomEquipments)
                .ThenInclude(re => re.Room)
                .ToListAsync();
        }
    }
}

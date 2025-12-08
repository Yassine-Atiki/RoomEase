using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RoomEase.Models;
using RoomEase.Services;

namespace RoomEase.Pages.Notifications
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<AppUser> _userManager;

        public IndexModel(INotificationService notificationService, UserManager<AppUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public List<Notification> Notifications { get; set; } = new List<Notification>();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Notifications = await _notificationService.GetUserNotificationsAsync(user.Id);
        }

        public async Task<IActionResult> OnPostMarkAsReadAsync(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return RedirectToPage();
        }
    }
}

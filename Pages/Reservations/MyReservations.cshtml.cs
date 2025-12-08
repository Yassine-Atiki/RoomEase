using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RoomEase.Models;
using RoomEase.Services;
using RoomEase.ViewModels;

namespace RoomEase.Pages.Reservations
{
    [Authorize]
    public class MyReservationsModel : PageModel
    {
        private readonly ApplicationDbContexte _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;

        public MyReservationsModel(ApplicationDbContexte context, UserManager<AppUser> userManager, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public List<ReservationDetailViewModel> Reservations { get; set; } = new List<ReservationDetailViewModel>();
        public string SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (TempData["SuccessMessage"] != null)
            {
                SuccessMessage = TempData["SuccessMessage"].ToString();
            }

            var reservations = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.StartTime)
                .ToListAsync();

            Reservations = reservations.Select(r => new ReservationDetailViewModel
            {
                Id = r.Id,
                RoomName = r.Room.Name,
                RoomCapacity = r.Room.Capacity,
                UserName = r.User.FullName,
                UserEmail = r.User.Email,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Purpose = r.Purpose,
                Status = r.Status
            }).ToList();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

            if (reservation == null)
            {
                return NotFound();
            }

            if (reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Rejected)
            {
                TempData["ErrorMessage"] = "Cette réservation ne peut pas être annulée.";
                return RedirectToPage();
            }

            reservation.Status = ReservationStatus.Cancelled;
            await _context.SaveChangesAsync();

            // Send notification to user
            await _notificationService.CreateNotificationAsync(
                user.Id,
                $"Vous avez annulé votre réservation de la salle '{reservation.Room.Name}' prévue le {reservation.StartTime:dd/MM/yyyy} à {reservation.StartTime:HH:mm}."
            );

            TempData["SuccessMessage"] = "Votre réservation a été annulée avec succès.";
            return RedirectToPage();
        }
    }
}

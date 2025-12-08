using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RoomEase.Models;
using RoomEase.Services;
using RoomEase.ViewModels;

namespace RoomEase.Pages.Admin.Reservations
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContexte _context;
        private readonly INotificationService _notificationService;

        public IndexModel(ApplicationDbContexte context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public List<ReservationDetailViewModel> AllReservations { get; set; } = new List<ReservationDetailViewModel>();
        public string SuccessMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }

        public async Task OnGetAsync()
        {
            if (TempData["SuccessMessage"] != null)
            {
                SuccessMessage = TempData["SuccessMessage"].ToString();
            }

            var query = _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .AsQueryable();

            // Filter by status if specified
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                if (Enum.TryParse<ReservationStatus>(StatusFilter, out var status))
                {
                    query = query.Where(r => r.Status == status);
                }
            }

            var reservations = await query
                .OrderByDescending(r => r.StartTime)
                .ToListAsync();

            AllReservations = reservations.Select(r => new ReservationDetailViewModel
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

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            if (reservation.Status != ReservationStatus.Pending)
            {
                TempData["ErrorMessage"] = "Seules les réservations en attente peuvent être approuvées.";
                return RedirectToPage();
            }

            // Check for conflicts one more time before approving
            var hasConflict = await _context.Reservations
                .AnyAsync(r => r.Id != id
                    && r.RoomId == reservation.RoomId
                    && r.Status == ReservationStatus.Approved
                    && r.StartTime < reservation.EndTime
                    && r.EndTime > reservation.StartTime);

            if (hasConflict)
            {
                TempData["ErrorMessage"] = "Impossible d'approuver: conflit avec une autre réservation approuvée.";
                return RedirectToPage();
            }

            reservation.Status = ReservationStatus.Approved;
            await _context.SaveChangesAsync();

            // Send notification to user
            await _notificationService.CreateNotificationAsync(
                reservation.UserId,
                $"Bonne nouvelle! Votre réservation de la salle '{reservation.Room.Name}' pour le {reservation.StartTime:dd/MM/yyyy} à {reservation.StartTime:HH:mm} a été approuvée."
            );

            TempData["SuccessMessage"] = "La réservation a été approuvée avec succès.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            if (reservation.Status != ReservationStatus.Pending)
            {
                TempData["ErrorMessage"] = "Seules les réservations en attente peuvent être refusées.";
                return RedirectToPage();
            }

            reservation.Status = ReservationStatus.Rejected;
            await _context.SaveChangesAsync();

            // Send notification to user
            await _notificationService.CreateNotificationAsync(
                reservation.UserId,
                $"Votre demande de réservation de la salle '{reservation.Room.Name}' pour le {reservation.StartTime:dd/MM/yyyy} à {reservation.StartTime:HH:mm} a été refusée."
            );

            TempData["SuccessMessage"] = "La réservation a été refusée.";
            return RedirectToPage();
        }
    }
}

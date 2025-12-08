using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RoomEase.Models;
using RoomEase.Services;
using RoomEase.ViewModels;

namespace RoomEase.Pages.Rooms
{
    [Authorize]
    public class ReserveModel : PageModel
    {
        private readonly ApplicationDbContexte _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;

        public ReserveModel(ApplicationDbContexte context, UserManager<AppUser> userManager, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        [BindProperty]
        public ReservationViewModel Reservation { get; set; }

        public Room Room { get; set; }
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int roomId, string date = null, string startTime = null, string endTime = null)
        {
            Room = await _context.Rooms
                .Include(r => r.RoomEquipments)
                .ThenInclude(re => re.Equipment)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (Room == null)
            {
                return NotFound();
            }

            Reservation = new ReservationViewModel
            {
                RoomId = roomId,
                RoomName = Room.Name,
                RoomCapacity = Room.Capacity
            };

            // Pre-fill from query string if provided
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsedDate))
            {
                Reservation.Date = parsedDate;
            }
            else
            {
                Reservation.Date = DateTime.Today;
            }

            if (!string.IsNullOrEmpty(startTime) && TimeSpan.TryParse(startTime, out var parsedStartTime))
            {
                Reservation.StartTime = parsedStartTime;
            }

            if (!string.IsNullOrEmpty(endTime) && TimeSpan.TryParse(endTime, out var parsedEndTime))
            {
                Reservation.EndTime = parsedEndTime;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Room = await _context.Rooms
                .Include(r => r.RoomEquipments)
                .ThenInclude(re => re.Equipment)
                .FirstOrDefaultAsync(r => r.Id == Reservation.RoomId);

            if (Room == null)
            {
                return NotFound();
            }

            // Validation: End time must be after start time
            if (Reservation.EndTime <= Reservation.StartTime)
            {
                ModelState.AddModelError("Reservation.EndTime", "L'heure de fin doit être après l'heure de début.");
            }

            // Validation: Cannot book in the past
            var requestedStart = Reservation.Date.Date.Add(Reservation.StartTime);
            var requestedEnd = Reservation.Date.Date.Add(Reservation.EndTime);

            if (requestedStart < DateTime.Now)
            {
                ModelState.AddModelError("Reservation.Date", "Vous ne pouvez pas réserver dans le passé.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check for conflicts
            var hasConflict = await _context.Reservations
                .AnyAsync(r => r.RoomId == Reservation.RoomId
                    && r.Status != ReservationStatus.Rejected
                    && r.Status != ReservationStatus.Cancelled
                    && r.StartTime < requestedEnd
                    && r.EndTime > requestedStart);

            if (hasConflict)
            {
                ErrorMessage = "Cette salle est déjà réservée pour ce créneau horaire.";
                return Page();
            }

            // Create the reservation
            var user = await _userManager.GetUserAsync(User);
            var reservation = new Reservation
            {
                RoomId = Reservation.RoomId,
                UserId = user.Id,
                StartTime = requestedStart,
                EndTime = requestedEnd,
                Purpose = Reservation.Purpose,
                Status = ReservationStatus.Pending
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Send notification to user
            await _notificationService.CreateNotificationAsync(
                user.Id,
                $"Votre demande de réservation pour la salle '{Room.Name}' le {requestedStart:dd/MM/yyyy} à {requestedStart:HH:mm} a été enregistrée et est en attente de validation."
            );

            TempData["SuccessMessage"] = "Votre réservation a été enregistrée avec succès. Elle est en attente de validation.";
            return RedirectToPage("/Reservations/MyReservations");
        }
    }
}

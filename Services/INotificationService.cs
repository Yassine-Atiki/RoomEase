using RoomEase.Models;

namespace RoomEase.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string message);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
        Task MarkAsReadAsync(int notificationId);
        Task<int> GetUnreadCountAsync(string userId);
    }
}

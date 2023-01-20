using Backlogs.Models;

namespace Backlogs.Services
{
    /// <summary>
    /// Handles sending toast notifications to the user
    /// </summary>
    public interface IToastNotificationService
    {
        /// <summary>
        /// Creates and registers a toast notification reminding the user about a backlog
        /// </summary>
        /// <param name="backlog"></param>
        void CreateToastNotification(Backlog backlog);
    }
}

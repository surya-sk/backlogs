using Backlogs.Models;

namespace Backlogs.Services
{
    public interface IToastNotificationService
    {
        void CreateToastNotification(Backlog backlog);
    }
}

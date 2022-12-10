using Backlogs.Models;
using System.Threading.Tasks;

namespace Backlogs.Services
{
    /// <summary>
    /// Handles the system share dialog to share content from the app
    /// </summary>
    public interface IShareDialogService
    {
        /// <summary>
        /// Launches the system share dialog to share the link to the app
        /// </summary>
        /// <param name="link"></param>
        void ShareAppLink(string link);

        /// <summary>
        /// Opens the system share dialog to share the backlog
        /// </summary>
        /// <param name="backlog"></param>
        /// <returns></returns>
        Task ShowShareBacklogDialogAsync(Backlog backlog);
    }
}

using Backlogs.Models;
using System.Threading.Tasks;

namespace Backlogs.Services
{
    public interface IShareDialogService
    {
        Task ShowSearchDialog(Backlog backlog);
        void ShareAppLink(string link);
    }
}

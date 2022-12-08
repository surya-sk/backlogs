using Backlogs.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Backlogs.Services
{
    public interface IDialogHandler
    {
        Task OpenTrailerDialogAsync(string video);
        void CloseTrailerDialog();

        Task OpenBacklogPopupAsync();
        Task CloseBacklogPopupAsycn();

        Task ShowErrorDialogAsync(string title, string message, string closeText);
        Task<int> ShowRandomBacklogDialogAsync(Backlog backlog);

        Task ReadMoreDialogAsync(Backlog backlog);
        Task<bool> ShowDeleteConfirmationDialogAsync();

        Task ShowInvalidDateTimeDialogAsycn(string title, string message);

        Task<SearchResult> ShowSearchResultsDialogAsync(string name, ObservableCollection<SearchResult> searchResults);

        Task<bool> ShowCrashLogDialogAsync(string log);

        Task<bool> ShowSignOutDialogAsync();

        Task ShowLogsDialogAsyncAsync(List<string> logs);
    }
}

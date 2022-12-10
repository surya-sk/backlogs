using Backlogs.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Backlogs.Services
{
    /// <summary>
    /// Handles creating ContentDialogs for various purposes, such as errors and popups
    /// </summary>
    public interface IDialogHandler
    {
        /// <summary>
        /// Opens a WebView hosted in a dialog to play the trailer
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        Task OpenTrailerDialogAsync(string video);

        /// <summary>
        /// Closes the trailer dialog
        /// </summary>
        void CloseTrailerDialog();

        /// <summary>
        /// Displays a dialog notifying the user of an error
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="closeText"></param>
        /// <returns></returns>
        Task ShowErrorDialogAsync(string title, string message, string closeText);

        /// <summary>
        /// Shows a popup with a randomly picked Backlog
        /// </summary>
        /// <param name="backlog"></param>
        /// <returns></returns>
        Task<int> ShowRandomBacklogDialogAsync(Backlog backlog);

        /// <summary>
        /// Shows the full description of a Backlog
        /// </summary>
        /// <param name="backlog"></param>
        /// <returns></returns>
        Task ReadMoreDialogAsync(Backlog backlog);

        /// <summary>
        /// Shows a delete confirmation dialog when deleting a Backlog
        /// </summary>
        /// <returns>Whether the user has confirmed deletion</returns>
        Task<bool> ShowDeleteConfirmationDialogAsync();

        /// <summary>
        /// Display search results from various sources 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="searchResults"></param>
        /// <returns>the selected search result</returns>
        Task<SearchResult> ShowSearchResultsDialogAsync(string name, ObservableCollection<SearchResult> searchResults);

        /// <summary>
        /// Shows the log detailing why the application crashed the previous time
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        Task<bool> ShowCrashLogDialogAsync(string log);

        /// <summary>
        /// A confirmation dialog for signing the user out
        /// </summary>
        /// <returns></returns>
        Task<bool> ShowSignOutDialogAsync();

        /// <summary>
        /// Displays the logs written to the Logs folder
        /// </summary>
        /// <param name="logs"></param>
        /// <returns></returns>
        Task ShowLogsDialogAsyncAsync(List<string> logs);
    }
}

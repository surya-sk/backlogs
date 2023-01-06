using Backlogs.Constants;
using Backlogs.Models;
using Backlogs.Services;
using Backlogs.Utils;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Graph;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;


namespace Backlogs.ViewModels
{
    public class BacklogViewModel : INotifyPropertyChanged
    {
        private bool m_inProgress;
        private bool m_showEditControls;
        private bool m_hideEditControls = true;
        private bool m_isLoading;
        private bool m_showProgressSwitch;
        private bool m_enableNotificationToggle;
        private bool m_showNotificationToggle;
        private bool m_showNotificationOptions;
        private DateTimeOffset m_calendarDate;
        private TimeSpan m_notifTime;
        private string m_sourceName;
        private Uri m_sourceLink;
        private int m_backlogIndex;
        private bool m_showTrailerButton = true;
        private bool m_showRatingContent;
        private bool m_hideRatingContent = true;
        private readonly INavigation m_navigationService;
        private readonly IDialogHandler m_dialogHandler;
        private readonly IToastNotificationService m_notificationService;
        private readonly IShareDialogService m_shareDialogService;
        private readonly IUserSettings m_settings;
        private readonly IFileHandler m_fileHandler;
        private readonly ISystemLauncher m_systemLauncher;

        public ObservableCollection<Backlog> Backlogs;
        public Backlog Backlog;

        public ICommand LaunchBingSearchResults { get; }
        public ICommand OpenWebViewTrailer { get; }
        public ICommand ShareBacklog { get; }
        public ICommand StopEditing { get; }
        public ICommand StartEditing { get; }
        public ICommand SaveChanges { get; }
        public ICommand CloseBacklog { get; }
        public ICommand DeleteBacklog { get; }
        public ICommand CompleteBacklog { get; }
        public ICommand ReadMore { get; }
        public ICommand GoBack { get; }
        public ICommand OpenSettings { get; }
        public ICommand MarkAsCompleted { get; }
        public ICommand HideRatingControl { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public DateTime Today { get; } = DateTime.Today;

        #region Properties
        public bool InProgress
        {
            get => m_inProgress;
            set
            {
                m_inProgress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InProgress)));
                if (m_inProgress)
                {
                    Backlog.Progress = Backlog.Length = 1;
                }
                else
                {
                    Backlog.Progress = Backlog.Length = 0;
                }
            }
        }

        public bool ShowProgressSwitch
        {
            get => m_showProgressSwitch;
            set
            {
                m_showProgressSwitch = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowProgressSwitch)));
            }
        }

        public bool ShowEditControls
        {
            get => m_showEditControls;
            set
            {
                m_showEditControls = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowEditControls)));
                //HideEditControls = !m_showEditControls;
            }
        }

        public bool HideEditControls
        {
            get => m_hideEditControls;
            set
            {
               if(m_hideEditControls != value)
                {
                    m_hideEditControls = value;
                    Debug.WriteLine(value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HideEditControls)));
                    //HideRatingContent = true;
                }
            }
        }

        public bool EnableNotificationToggle
        {
            get => m_enableNotificationToggle;
            set
            {
                m_enableNotificationToggle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnableNotificationToggle)));
            }
        }

        public bool ShowNotificationToggle
        {
            get => m_showNotificationToggle;
            set
            {
                m_showNotificationToggle = value;
                if (m_showNotificationToggle)
                {
                    ShowNotificationOptions = true;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowNotificationToggle)));
            }
        }

        public bool ShowNotificationOptions
        {
            get => m_showNotificationOptions;
            set
            {
                m_showNotificationOptions = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowNotificationOptions)));
            }
        }

        public DateTimeOffset CalendarDate
        {
            get => m_calendarDate;
            set
            {
                m_calendarDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CalendarDate)));
                EnableNotificationToggle = m_calendarDate != DateTimeOffset.MinValue;
            }
        }

        public TimeSpan NotifTime
        {
            get => m_notifTime;
            set
            {
                if (m_notifTime != value)
                {
                    m_notifTime = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NotifTime)));
                }
            }
        }

        public bool IsLoading
        {
            get => m_isLoading;
            set
            {
                m_isLoading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
            }
        }

        public string SourceName
        {
            get => m_sourceName;
            set
            {
                m_sourceName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceName)));
            }
        }

        public Uri SourceLink
        {
            get => m_sourceLink;
            set
            {
                m_sourceLink = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceLink)));
            }
        }

        public bool ShowTrailerButton
        {
            get => m_showTrailerButton;
            set
            {
                m_showTrailerButton = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowTrailerButton)));
            }
        }

        public bool ShowRatingContent
        {
            get => m_showRatingContent;
            set
            {
                m_showRatingContent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowRatingContent)));
            }
        }

        public bool HideRatingContent
        {
            get => m_hideRatingContent;
            set
            {
                m_hideRatingContent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HideRatingContent)));
            }
        }
        #endregion

        public BacklogViewModel(Guid id, INavigation navigationService, IDialogHandler dialogHandler, 
            IToastNotificationService notificationService, IShareDialogService shareDialogService,
            IUserSettings settings, IFileHandler fileHandler, ISystemLauncher systemLauncher)
        {
            LaunchBingSearchResults = new AsyncCommand(LaunchBingSearchResultsAsync);
            OpenWebViewTrailer = new AsyncCommand(PlayTrailerAsync);
            ShareBacklog = new AsyncCommand(ShareBacklogAsync);
            StopEditing = new Command(FinishEditing);
            StartEditing = new Command(EnableEditing);
            SaveChanges = new AsyncCommand(SaveChangesAsync);
            CloseBacklog = new AsyncCommand(CloseBacklogAsync);
            DeleteBacklog = new AsyncCommand(DeleteBacklogAsync);
            MarkAsCompleted = new Command(MarkBacklogAsComplete);
            HideRatingControl = new Command(HideRatingGrid);
            CompleteBacklog = new AsyncCommand(CompleteBacklogAsync);
            ReadMore = new AsyncCommand(ReadMoreAsync);
            GoBack = new Command(NavigateToPreviousPage);
            OpenSettings = new Command(OpenSettingsPage);

            m_navigationService = navigationService;
            m_dialogHandler = dialogHandler;
            m_shareDialogService = shareDialogService;
            m_notificationService = notificationService;
            m_settings = settings;
            m_fileHandler = fileHandler;
            m_systemLauncher = systemLauncher;

            CalendarDate = DateTimeOffset.MinValue;
            NotifTime = TimeSpan.Zero;

            Backlogs = BacklogsManager.GetInstance().GetBacklogs();

            foreach (Backlog b in Backlogs)
            {
                if (id == b.id)
                {
                    Backlog = b;
                    switch (Backlog.Type)
                    {
                        case "Film":
                            SourceName = "IMdB";
                            SourceLink = new Uri("https://www.imdb.com/");
                            break;
                        case "Album":
                            SourceName = "LastFM";
                            SourceLink = new Uri("https://www.last.fm/");
                            ShowTrailerButton = false;
                            break;
                        case "TV":
                            SourceName = "IMdB";
                            SourceLink = new Uri("https://www.imbd.com");
                            break;
                        case "Game":
                            SourceName = "IGDB";
                            SourceLink = new Uri("https://www.igdb.com/discover");
                            break;
                        case "Book":
                            SourceName = "Google Books";
                            SourceLink = new Uri("https://books.google.com/");
                            ShowTrailerButton = false;
                            break;
                    }
                    m_backlogIndex = Backlogs.IndexOf(b);
                }
            }
            ShowProgressSwitch = !Backlog.ShowProgress;
            if (ShowProgressSwitch)
            {
                InProgress = Backlog.Progress > 0;
            }
        }

        private async Task ReadMoreAsync()
        {
            await m_dialogHandler.ReadMoreDialogAsync(Backlog);
        }


        /// <summary>
        /// Deletes the backlog
        /// </summary>
        /// <returns></returns>
        private async Task DeleteBacklogAsync()
        {
            try
            {
                await m_fileHandler.WriteLogsAsync("Deleting backlog.....");
            }
            catch { }
            var result = await m_dialogHandler.ShowDeleteConfirmationDialogAsync();
            if (result)
            {
                await DeleteConfirmation_Click();
            }
            try
            {
                await m_fileHandler.WriteLogsAsync("Deleted backlog");
            }
            catch { }
        }


        /// <summary>
        /// Delete a backlog after confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task DeleteConfirmation_Click()
        {
            IsLoading = true;
            Backlogs.Remove(Backlog);
            BacklogsManager.GetInstance().SaveSettings(Backlogs);
            await BacklogsManager.GetInstance().WriteDataAsync(m_settings.Get<bool>(SettingsConstants.IsSignedIn));
            NavigateToPreviousPage();
        }

        /// <summary>
        /// Close the backlog
        /// </summary>
        /// <returns></returns>
        private async Task CloseBacklogAsync()
        {
            if(ShowRatingContent)
            {
                await CompleteBacklogAsync();
            }
            else
            {

                await SaveBacklogAsync();
            }
            NavigateToPreviousPage();
        }

        /// <summary>
        /// Go back
        /// </summary>
        private void NavigateToPreviousPage()
        {
            m_navigationService.GoBack<BacklogViewModel>();
        }

        private void OpenSettingsPage()
        {
             m_navigationService.NavigateTo<SettingsViewModel>();
        }

        /// <summary>
        /// Write backlog to the json file locally and on OneDrive if signed-in
        /// </summary>
        /// <returns></returns>
        private async Task SaveBacklogAsync()
        {
            try
            {
                await m_fileHandler.WriteLogsAsync("Saving backlog....");
            }
            catch { }
            Backlogs[m_backlogIndex] = Backlog;
            BacklogsManager.GetInstance().SaveSettings(Backlogs);
            await BacklogsManager.GetInstance().WriteDataAsync(m_settings.Get<bool>(SettingsConstants.IsSignedIn));
        }

        /// <summary>
        /// Marks backlog as complete
        /// </summary>
        /// <returns></returns>
        private async Task CompleteBacklogAsync()
        {
            try
            {
                await m_fileHandler.WriteLogsAsync("Marking backlog as complete");
            }
            catch { }
            Backlog.IsComplete = true;
            Backlog.CompletedDate = DateTimeOffset.Now.Date.ToString("d", CultureInfo.InvariantCulture);
            await SaveBacklogAsync();
        }

        /// <summary>
        /// Show rating dialog
        /// </summary>
        private void MarkBacklogAsComplete()
        {
            ShowRatingContent = true;
            ShowEditControls = false;
            HideRatingContent = false;
            HideEditControls = true;
        }

        /// <summary>
        /// Hide the rating dialog
        /// </summary>
        private void HideRatingGrid()
        {
            ShowRatingContent = false;
            HideRatingContent = true;
            ShowEditControls = false;
            HideEditControls = true;
        }

        /// <summary>
        /// Enable editing
        /// </summary>
        private void EnableEditing()
        {
            ShowEditControls = true;
            HideEditControls = false;
            ShowRatingContent = false;
            HideRatingContent = true;
        }

        /// <summary>
        /// Validate and save changes made to the backlog
        /// </summary>
        /// <returns></returns>
        public async Task SaveChangesAsync()
        {
            string date = CalendarDate.DateTime.ToString("D", CultureInfo.InvariantCulture);
            if (ShowNotificationOptions)
            {
                if (NotifTime == TimeSpan.Zero)
                {
                    await m_dialogHandler.ShowErrorDialogAsync("Invalid date and time", "Please pick a time", "Ok");
                    return;
                }
                DateTimeOffset dateTime = DateTimeOffset.Parse(date, CultureInfo.InvariantCulture).Add(NotifTime);
                int diff = DateTimeOffset.Compare(dateTime, DateTimeOffset.Now);
                if (diff < 0)
                {
                    await m_dialogHandler.ShowErrorDialogAsync("Invalid time", "The date and time you've chosen are in the past!", "Ok");
                    return;
                }
            }
            else
            {
                int diff = DateTime.Compare(DateTime.Today, CalendarDate.DateTime);
                if (diff > 0)
                {
                    await m_dialogHandler.ShowErrorDialogAsync("Invalid date and time", "The date and time you've chosen are in the past!", "Ok");
                    return;
                }
            }
            IsLoading = true;
            Backlog.TargetDate = CalendarDate.ToString("D", CultureInfo.InvariantCulture);
            Backlog.NotifTime = NotifTime;
            ScheduleNotification();
            FinishEditing();
            IsLoading = false;
        }

        /// <summary>
        /// Create notification and send it to Windows
        /// </summary>
        private void ScheduleNotification()
        {
            if (Backlog.NotifTime != TimeSpan.Zero)
            {
                m_notificationService.CreateToastNotification(Backlog);
            }
        }

        /// <summary>
        /// Stop editing
        /// </summary>
        private void FinishEditing()
        {
            HideRatingContent = true;
            HideEditControls = true;
            ShowEditControls = false;
            ShowRatingContent = false;
        }

        /// <summary>
        /// Open Windows share window to share backlog
        /// </summary>
        /// <returns></returns>
        private async Task ShareBacklogAsync()
        {
            IsLoading = true;
            await m_shareDialogService.ShowShareBacklogDialogAsync(Backlog);
            IsLoading = false;
        }

        /// <summary>
        /// Plays the first Youtube result found for "*Name* Offical Trailer"
        /// </summary>
        private async Task PlayTrailerAsync()
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = Keys.YOUTUBE_KEY,
                ApplicationName = "Backlogs"
            });
            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = Backlog.Name + " offical trailer";
            searchListRequest.MaxResults = 1;

            var searchListResponse = await searchListRequest.ExecuteAsync();

            List<string> videos = new List<string>();

            foreach (var searchResult in searchListResponse.Items)
            {
                videos.Add(searchResult.Id.VideoId);
            }
            await LaunchTrailerWebViewAsync(videos[0]);
        }

        /// <summary>
        /// Launch default browser to show Bing results
        /// </summary>
        private async Task LaunchBingSearchResultsAsync()
        {
            string searchTerm = Backlog.Name;
            if (Backlog.Type == "Album" || Backlog.Type == "Book")
            {
                searchTerm += $" {Backlog.Director}";
            }
            var searchQuery = searchTerm.Replace(" ", "+");
            var searchUri = new Uri($"https://www.bing.com/search?q={searchQuery}");
            await m_systemLauncher.LaunchUriAsync(searchUri);
        }

        /// <summary>
        /// Launch the trailer in a webview
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        private async Task LaunchTrailerWebViewAsync(string video)
        {
            await m_dialogHandler.OpenTrailerDialogAsync(video);
        }
    }
}

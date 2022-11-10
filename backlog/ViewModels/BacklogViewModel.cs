using backlog.Logging;
using backlog.Models;
using backlog.Saving;
using backlog.Utils;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.Notifications;
using MvvmHelpers.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace backlog.ViewModels
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
        private double m_userRating;
        private string m_sourceName;
        private Uri m_sourceLink;
        private int m_backlogIndex;
        private bool m_showTrailerButton = true;
        StorageFolder m_tempFolder = ApplicationData.Current.TemporaryFolder;
        private ContentDialog m_ratingDialog;
        private WebView m_webView;
        private ContentDialog m_trailerDialog;
        private readonly INavigationService m_navigationService;

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
        public ICommand LaunchRatingDialog { get; }
        public ICommand HideRatingDialog { get; }
        public ICommand CompleteBacklog { get; }
        public ICommand ReadMore { get; }
        public ICommand CloseTrailer { get; }
        public ICommand GoBack { get; }
        public ICommand OpenSettings { get; }

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
                HideEditControls = !m_showEditControls;
            }
        }

        public bool HideEditControls
        {
            get => m_hideEditControls;
            set
            {
                m_hideEditControls = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HideEditControls)));
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

        public double UserRating
        {
            get => m_userRating;
            set
            {
                m_userRating = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserRating)));
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
        #endregion

        public BacklogViewModel(Guid id, ContentDialog ratingDialog, ContentDialog trailerDialog, WebView webView, INavigationService navigationService)
        {
            LaunchBingSearchResults = new AsyncCommand(LaunchBingSearchResultsAsync);
            OpenWebViewTrailer = new AsyncCommand(PlayTrailerAsync);
            ShareBacklog = new AsyncCommand(ShareBacklogAsync);
            StopEditing = new Command(FinishEditing);
            StartEditing = new Command(EnableEditing);
            SaveChanges = new AsyncCommand(SaveChangesAsync);
            CloseBacklog = new AsyncCommand(CloseBacklogAsync);
            DeleteBacklog = new AsyncCommand(DeleteBacklogAsync);
            LaunchRatingDialog = new AsyncCommand(OpenRatingDialogAsync);
            HideRatingDialog = new Command(CloseRatingDialog);
            CompleteBacklog = new AsyncCommand(CompleteBacklogAsync);
            ReadMore = new AsyncCommand(ReadMoreAsync);
            CloseTrailer = new Command(CloseWebView);
            GoBack = new Command(NavigateToPreviousPage);
            OpenSettings = new Command(OpenSettingsPage);

            m_ratingDialog = ratingDialog;
            m_trailerDialog = trailerDialog;
            m_webView = webView;
            m_navigationService = navigationService;

            CalendarDate = DateTimeOffset.MinValue;
            NotifTime = TimeSpan.Zero;

            Backlogs = SaveData.GetInstance().GetBacklogs();

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
            ContentDialog contentDialog = new ContentDialog
            {
                Title = Backlog.Name,
                Content = Backlog.Description,
                CloseButtonText = "Close"
            };
            await contentDialog.ShowAsync();
        }


        /// <summary>
        /// Deletes the backlog
        /// </summary>
        /// <returns></returns>
        private async Task DeleteBacklogAsync()
        {
            try
            {
                await Logger.Info("Deleting backlog.....");
            }
            catch { }
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Delete backlog?",
                Content = "Deletion is permanent. This backlog cannot be recovered, and will be gone forever.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };
            ContentDialogResult result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await DeleteConfirmation_Click();
            }
            try
            {
                await Logger.Info("Deleted backlog");
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
            SaveData.GetInstance().SaveSettings(Backlogs);
            await SaveData.GetInstance().WriteDataAsync(Settings.IsSignedIn);
            NavigateToPreviousPage();
        }

        /// <summary>
        /// Close the backlog
        /// </summary>
        /// <returns></returns>
        private async Task CloseBacklogAsync()
        {
            await SaveBacklogAsync();
            NavigateToPreviousPage();
        }

        /// <summary>
        /// Go back
        /// </summary>
        private void NavigateToPreviousPage()
        {
            m_navigationService.GoBack();
        }

        private void OpenSettingsPage()
        {
            try
            {
                m_navigationService.NavigateTo<SettingsViewModel>(null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            catch
            {
                m_navigationService.NavigateTo<SettingsViewModel>();
            }
        }

        private async Task OpenRatingDialogAsync()
        {
            await m_ratingDialog.ShowAsync();
        }

        /// <summary>
        /// Write backlog to the json file locally and on OneDrive if signed-in
        /// </summary>
        /// <returns></returns>
        private async Task SaveBacklogAsync()
        {
            try
            {
                await Logger.Info("Saving backlog....");
            }
            catch { }
            Backlogs[m_backlogIndex] = Backlog;
            SaveData.GetInstance().SaveSettings(Backlogs);
            await SaveData.GetInstance().WriteDataAsync(Settings.IsSignedIn);
        }

        /// <summary>
        /// Marks backlog as complete
        /// </summary>
        /// <returns></returns>
        private async Task CompleteBacklogAsync()
        {
            try
            {
                await Logger.Info("Marking backlog as complete");
            }
            catch { }
            Backlog.IsComplete = true;
            Backlog.UserRating = UserRating;
            Backlog.CompletedDate = DateTimeOffset.Now.Date.ToString("d", CultureInfo.InvariantCulture);
            await SaveBacklogAsync();
            CloseRatingDialog();
            NavigateToPreviousPage();
        }

        private void CloseRatingDialog()
        {
            m_ratingDialog.Hide();
        }

        /// <summary>
        /// Enable editing
        /// </summary>
        private void EnableEditing()
        {
            ShowEditControls = true;
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
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "Invalid date and time",
                        Content = "Please pick a time!",
                        CloseButtonText = "Ok"
                    };
                    await contentDialog.ShowAsync();
                    return;
                }
                DateTimeOffset dateTime = DateTimeOffset.Parse(date, CultureInfo.InvariantCulture).Add(NotifTime);
                int diff = DateTimeOffset.Compare(dateTime, DateTimeOffset.Now);
                if (diff < 0)
                {
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "Invalid time",
                        Content = "The date and time you've chosen are in the past!",
                        CloseButtonText = "Ok"
                    };
                    await contentDialog.ShowAsync();
                    return;
                }
            }
            else
            {
                int diff = DateTime.Compare(DateTime.Today, CalendarDate.DateTime);
                if (diff > 0)
                {
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "Invalid date and time",
                        Content = "The date and time you've chosen are in the past!",
                        CloseButtonText = "Ok"
                    };
                    await contentDialog.ShowAsync();
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
                var notifTime = DateTimeOffset.Parse(Backlog.TargetDate, CultureInfo.InvariantCulture).Add(Backlog.NotifTime);
                var builder = new ToastContentBuilder()
                    .AddText($"It's {Backlog.Name} time!")
                    .AddText($"You wanted to check out {Backlog.Name} by {Backlog.Director} today. Get to it!")
                    .AddHeroImage(new Uri(Backlog.ImageURL));
                ScheduledToastNotification toastNotification = new ScheduledToastNotification(builder.GetXml(), notifTime);
                ToastNotificationManager.CreateToastNotifier().AddToSchedule(toastNotification);
            }
        }

        /// <summary>
        /// Stop editing
        /// </summary>
        private void FinishEditing()
        {
            ShowEditControls = false;
        }

        /// <summary>
        /// Open Windows share window to share backlog
        /// </summary>
        /// <returns></returns>
        private async Task ShareBacklogAsync()
        {
            IsLoading = true;
            StorageFile backlogFile = await m_tempFolder.CreateFileAsync($"{Backlog.Name}.bklg", CreationCollisionOption.ReplaceExisting);
            string json = JsonConvert.SerializeObject(Backlog);
            await FileIO.WriteTextAsync(backlogFile, json);
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
            IsLoading = false;
        }

        private async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest dataRequest = args.Request;
            dataRequest.Data.Properties.Title = $"Share {Backlog.Name} backlog";
            dataRequest.Data.Properties.Description = "Your contacts with the Backlogs app installed can open this file and add it to their backlog";
            var fileToShare = await m_tempFolder.GetFileAsync($"{Backlog.Name}.bklg");
            List<IStorageItem> list = new List<IStorageItem>();
            list.Add(fileToShare);
            dataRequest.Data.SetStorageItems(list);
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
            await Windows.System.Launcher.LaunchUriAsync(searchUri);
        }

        /// <summary>
        /// Launch the trailer in a webview
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        private async Task LaunchTrailerWebViewAsync(string video)
        {
            await m_trailerDialog.ShowAsync();
            try
            {
                m_trailerDialog.CornerRadius = new CornerRadius(0); // Without this, for some fucking reason, buttons inside the WebView do not work
            }
            catch
            {

            }
            string width = AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile" ? "600" : "500";
            string height = AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile" ? "100%" : "400";
            m_webView.NavigateToString($"<iframe width=\"{width}\" height=\"{height}\" src=\"https://www.youtube.com/embed/{video}?autoplay={Settings.AutoplayVideos}\" title=\"YouTube video player\"  allow=\"accelerometer; autoplay; encrypted-media; gyroscope;\"></iframe>");
        }

        /// <summary>
        /// Navigate to a blank page because audio keeps playing after closing the WebView for some reason
        /// </summary>
        private void CloseWebView()
        {
            m_webView.Navigate(new Uri("about:blank"));
        }
    }
}

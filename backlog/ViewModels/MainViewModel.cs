using Backlogs.Auth;
using Backlogs.Constants;
using Backlogs.Logging;
using Backlogs.Models;
using Backlogs.Saving;
using Backlogs.Services;
using Backlogs.Utils;
using Backlogs.Views;
using Microsoft.Toolkit.Uwp.Notifications;
using MvvmHelpers.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Backlogs.ViewModels
{
    public class MainViewModel: INotifyPropertyChanged
    {
        private bool m_isBusy;
        private string m_crashLog;
        private bool m_networkAvailable;
        private Guid m_randomBacklogId = new Guid();
        private Backlog m_randomBacklog;
        private bool m_recentlyCreatedEmpty;
        private bool m_recentlyCompletedEmtpy;
        private bool m_inProgressEmpty;
        private bool m_upcomingEmpty;
        private bool m_showTopTeachingTip;
        private bool m_showBottomTeachingTip;
        private bool m_showProfileButtons;
        private bool m_showSignInButton = true;
        private string m_accountPic;
        private string m_randomBacklogType = "Any";
        private int m_backlogsCount = 0;
        private int m_completedBacklogsCount = 0;
        private int m_incompleteBacklogsCount = 0;
        private double m_completedPercent = 0;
        private readonly INavigationService m_navigationService;
        private readonly IDialogHandler m_dialogHandler;
        private readonly IShareDialogService m_shareService;
        private readonly IUserSettings m_settings;
        private readonly IFileHandler m_fileHandler;
        private readonly ILiveTileService m_liveTileService;
        private readonly IFilePicker m_filePicker;
        private readonly IEmailService m_emailService;

        public ObservableCollection<Backlog> RecentlyAdded { get; set; }
        public ObservableCollection<Backlog> RecentlyCompleted { get; set; }
        public ObservableCollection<Backlog> InProgress { get; set; }
        public ObservableCollection<Backlog> Upcoming { get; set; }
        
        public bool IsFirstRun { get; } = Settings.IsFirstRun;
        public bool ShowWhatsNew { get; } = Settings.ShowWhatsNew;
        public string WelcomeText = Settings.IsSignedIn ? $"Welcome to Backlogs, {Settings.UserName}!" : "Welcome to Backlogs, stranger!";
        public string UserName { get; } = Settings.UserName;
        public bool SignedIn { get; } = Settings.IsSignedIn;
        public bool Sync { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SignIn { get; }
        public ICommand RateAppOnMSStore { get; }
        public ICommand ShareApp { get; }
        public ICommand OpenPaypal { get; }
        public ICommand SyncAllBacklogs { get; }
        public ICommand GenerateRandomBacklog { get; }
        public ICommand ImportBacklog { get; }
        public ICommand OpenWebApp { get; }
        public ICommand SignOut { get; }
        public ICommand SendCrashLog { get; }
        public ICommand OpenCreatePage { get; }
        public ICommand OpenSettingsPage { get; }
        public ICommand OpenCompletedPage { get; }
        public ICommand OpenBacklogsPage { get; }


        #region Properties
        public bool IsBusy
        {
            get => m_isBusy;
            set
            {
                m_isBusy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
            }
        }

        public Guid RandomBacklogId
        {
            get => m_randomBacklogId;
            set
            {
                m_randomBacklogId = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RandomBacklogId)));
            }
        }

        public Backlog RandomBacklog
        {
            get => m_randomBacklog;
            set
            {
                m_randomBacklog = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RandomBacklog)));
            }
        }

        public bool RecentlyCreatedEmpty
        {
            get => m_recentlyCreatedEmpty;
            set
            {
                m_recentlyCreatedEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecentlyCreatedEmpty)));
            }
        }

        public bool RecentlyCompletedEmpty
        {
            get => m_recentlyCompletedEmtpy;
            set
            {
                m_recentlyCompletedEmtpy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecentlyCompletedEmpty)));
            }
        }

        public bool InProgressEmpty
        {
            get => m_inProgressEmpty;
            set
            {
                m_inProgressEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InProgressEmpty)));
            }
        }

        public bool UpcomingEmpty
        {
            get => m_upcomingEmpty;
            set
            {
                m_upcomingEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpcomingEmpty)));
            }
        }

        public bool ShowTopTeachingTip
        {
            get => m_showTopTeachingTip;
            set
            {
                m_showTopTeachingTip = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowTopTeachingTip)));
            }
        }

        public bool ShowBottomTeachingTip
        {
            get => m_showBottomTeachingTip;
            set
            {
                m_showBottomTeachingTip = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowBottomTeachingTip)));

            }
        }

        public string AccountPic
        {
            get => m_accountPic;
            set
            {
                m_accountPic = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccountPic)));
            }
        }

        public bool ShowProfileButton
        {
            get => m_showProfileButtons;
            set
            {
                m_showProfileButtons = value;
                ShowSignInButton = !m_showProfileButtons;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowProfileButton)));
            }
        }

        public bool ShowSignInButton
        {
            get => m_showSignInButton;
            set
            {
                m_showSignInButton = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowSignInButton)));
            }
        }

        public string RandomBacklogType
        {
            get => m_randomBacklogType;
            set
            {
                if(m_randomBacklogType != value)
                {
                    m_randomBacklogType = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(RandomBacklogType)));
                }
            }
        }

        public int BacklogsCount
        {
            get => m_backlogsCount;
            set
            {
                m_backlogsCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BacklogsCount)));
            }
        }

        public int CompletedBacklogsCount
        {
            get => m_completedBacklogsCount;
            set
            {
                m_completedBacklogsCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompletedBacklogsCount)));
            }
        }
        
        public int IncompleteBacklogsCount
        {
            get => m_incompleteBacklogsCount;
            set
            {
                m_incompleteBacklogsCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IncompleteBacklogsCount)));
            }
        }

        public double CompletedBacklogsPercent
        {
            get => m_completedPercent;
            set
            {
                m_completedPercent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompletedBacklogsPercent)));
            }
        }
        #endregion

        public MainViewModel(INavigationService navigationService, 
            IDialogHandler dialogHandler, IShareDialogService shareService, 
            IUserSettings settings, IFileHandler fileHandler, ILiveTileService liveTileService,
            IFilePicker filePicker, IEmailService emailService)
        {
            m_networkAvailable = NetworkInterface.GetIsNetworkAvailable();

            SignIn = new AsyncCommand(SignInAsync);
            RateAppOnMSStore = new AsyncCommand(RateAppOnMSStoreAsync);
            ShareApp = new Command(ShareAppLink);
            OpenPaypal = new AsyncCommand(OpenPaypalAsync);
            SyncAllBacklogs = new Command(SyncBacklogs);
            GenerateRandomBacklog = new AsyncCommand(GenerateRandomBacklogAsync);
            ImportBacklog = new AsyncCommand(ImportBacklogAsync);
            OpenWebApp = new AsyncCommand(OpenWebAppAsync);
            SignOut = new AsyncCommand(SignOutAsync);
            SendCrashLog = new AsyncCommand(SendCrashLogAsync);
            OpenCreatePage = new Command(NavigateToCreatePage);
            OpenSettingsPage = new Command(NavigateToSettingsPage);
            OpenCompletedPage = new Command(NavigateToCompletedPage);
            OpenBacklogsPage = new Command(NavigateToBacklogsPage);

            RecentlyAdded = new ObservableCollection<Backlog>();
            RecentlyCompleted = new ObservableCollection<Backlog>();
            InProgress = new ObservableCollection<Backlog>();
            Upcoming = new ObservableCollection<Backlog>();

            LoadBacklogs();
            m_navigationService = navigationService;
            m_dialogHandler = dialogHandler;
            m_shareService = shareService;
            m_settings = settings;
            m_fileHandler = fileHandler;
            m_liveTileService = liveTileService;
            m_filePicker = filePicker;
            m_emailService = emailService;

            m_liveTileService.EnableLiveTileQueue();
        }

        public async Task SetupProfile()
        {
            IsBusy = true;
            await ShowCrashLog();
            if(m_networkAvailable && Settings.IsSignedIn)
            {
                try
                {
                    await Logger.Info("Internet access");
                    await Logger.Info("Signing in user....");
                }
                catch { }
                await SetUserPhotoAsync();
                ShowProfileButton = true;
                if (Sync)
                {
                    await SaveData.GetInstance().ReadDataAsync(Sync);
                    SaveData.GetInstance().ResetHelperBacklogs();
                    LoadBacklogs();
                }
            }
            IsBusy = false;
            ShowTeachingTips();
            ShowLiveTiles();
        }

        /// <summary>
        /// Show a content dialog with the reason for crash
        /// </summary>
        /// <returns></returns>
        private async Task ShowCrashLog()
        {
            m_crashLog = m_settings.Get<string>(SettingsConstants.LastCrashLog);
            if(await m_dialogHandler.ShowCrashLogDialogAsync(m_crashLog))
            {
                await SendCrashLogAsync();
            }
            m_settings.Set<string>(SettingsConstants.LastCrashLog, null);
        }

        /// <summary>
        /// Loads up backlogs for the homepage
        /// </summary>
        private async void LoadBacklogs()
        {
            var _recentlyAdded = SaveData.GetInstance().GetRecentlyAddedBacklogs();
            RecentlyAdded.Clear();
            foreach(var b in _recentlyAdded)
            {
                RecentlyAdded.Add(b);
            }
            RecentlyCreatedEmpty = RecentlyAdded.Count <= 0;
            
            var _recentlyCompleted = SaveData.GetInstance().GetRecentlyCompletedBacklogs();
            RecentlyCompleted.Clear();
            foreach (var b in _recentlyCompleted)
            {
                RecentlyCompleted.Add(b);
            }
            RecentlyCompletedEmpty = RecentlyCompleted.Count <= 0;

            var _inProgress = SaveData.GetInstance().GetInProgressBacklogs();
            InProgress.Clear();
            foreach(var b in _inProgress)
            {
                InProgress.Add(b);
            }    
            InProgressEmpty = InProgress.Count <= 0;

            var _upcoming = SaveData.GetInstance().GetUpcomingBacklogs();
            Upcoming.Clear();
            foreach(var b in _upcoming)
            {
                Upcoming.Add(b);
            }
            UpcomingEmpty = Upcoming.Count <= 0;

            CompletedBacklogsCount = SaveData.GetInstance().GetCompletedBacklogs().Count;
            IncompleteBacklogsCount = SaveData.GetInstance().GetIncompleteBacklogs().Count;
            BacklogsCount = SaveData.GetInstance().GetBacklogs().Count();
            if(BacklogsCount > 0)
            {
                CompletedBacklogsPercent = (Convert.ToDouble(CompletedBacklogsCount) / BacklogsCount) * 100.0;
            }

            await GenerateRandomBacklogAsync();
        }

        /// <summary>
        /// Set the user photo in the command bar
        /// </summary>
        /// <returns></returns>
        private async Task SetUserPhotoAsync()
        {
            try
            {
                AccountPic = await m_fileHandler.ReadImageAsync("profile.png");
            }
            catch
            {
                // No image set
            }
        }

        /// <summary>
        /// Show Teaching tips
        /// </summary>
        private void ShowTeachingTips()
        {
            if (IsFirstRun)
            {
                Settings.IsFirstRun = false;
            }
            if (!Settings.IsSignedIn)
            {
                //if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                //{
                //    ShowTopTeachingTip = true;
                //}
                //else
                //{
                //    ShowBottomTeachingTip = true;
                //}
                ShowBottomTeachingTip = true;
            }
            if (Settings.ShowWhatsNew)
            {
                Settings.ShowWhatsNew = false;
            }
        }

        /// <summary>
        /// Signs the user in
        /// </summary>
        /// <returns></returns>
        private async Task SignInAsync()
        {
            IsBusy = true;
            ShowTopTeachingTip = false;
            ShowBottomTeachingTip = false;
            if (m_networkAvailable)
            {
                if (!Settings.IsSignedIn)
                {
                    try
                    {
                        await Logger.Info("Signing in...");
                    }
                    catch { }
                    await SaveData.GetInstance().DeleteLocalFileAsync();
                    Settings.IsSignedIn = true;
                    await SaveData.GetInstance().ReadDataAsync(true);
                    SyncBacklogs();
                }
            }
            else
            {
                await m_dialogHandler.ShowErrorDialogAsync("No internet", "You need to be connected to the internet for this!", "Ok");
            }
        }

        /// <summary>
        /// Launches the Store rating page for the app
        /// </summary>
        private async Task RateAppOnMSStoreAsync()
        {
            var _ratingUri = new Uri(@"ms-windows-store://review/?ProductId=9N2H8CM2KWVZ");
            await Windows.System.Launcher.LaunchUriAsync(_ratingUri);
        }

        /// <summary>
        /// Build the live tile queue
        /// </summary>
        private void ShowLiveTiles()
        {
            var tileContent = m_settings.Get<string>(SettingsConstants.TileContent);
            var tileStyle = m_settings.Get<string>(SettingsConstants.TileStyle);
            switch (tileContent)
            {
                case "Recently Created":
                    {
                        if (RecentlyAdded != null)
                        {
                            m_liveTileService.ShowLiveTiles(tileContent, tileStyle, RecentlyAdded.ToList());
                        }
                    }
                    break;
                case "Recently Completed":
                    {
                        if (RecentlyCompleted != null)
                        {
                            m_liveTileService.ShowLiveTiles(tileContent, tileStyle, RecentlyCompleted.ToList());
                        }
                    }
                    break;
                case "In Progress":
                    {
                        if (InProgress != null)
                        {
                            m_liveTileService.ShowLiveTiles(tileContent, tileStyle, InProgress.ToList());
                        }
                    }
                    break;
                case "Upcoming":
                    {
                        if (Upcoming != null)
                        {
                            m_liveTileService.ShowLiveTiles(tileContent, tileStyle, Upcoming.ToList());
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Launch Windows Share UI to show share options
        /// </summary>
        /// <returns></returns>
        private void ShareAppLink()
        {
            m_shareService.ShareAppLink("https://www.microsoft.com/store/apps/9N2H8CM2KWVZ");
        }

        /// <summary>
        /// Open paypal link so users can donate
        /// </summary>
        /// <returns></returns>
        private async Task OpenPaypalAsync()
        {
            try
            {
                await Logger.Info("Opening Paypal link...");
            }
            catch { }
            var paypalUri = new Uri(@"https://paypal.me/surya4822?locale.x=en_US");
            await Windows.System.Launcher.LaunchUriAsync(paypalUri);
        }

        private void SyncBacklogs()
        {
            m_navigationService.NavigateTo<MainViewModel>("sync");
        }

        /// <summary>
        /// Generate a random backlog
        /// </summary>
        /// <returns></returns>
        private async Task GenerateRandomBacklogAsync()
        {
            try
            {
                await Logger.Info("Generating random backlog...");
            }
            catch { }
            Random _random = new Random();
            bool _error = false;
            var _incompleteBacklogs = SaveData.GetInstance().GetIncompleteBacklogs();
            switch (RandomBacklogType.ToLower())
            {
                case "any":
                    if (_incompleteBacklogs.Count <= 0)
                    {
                        await ShowErrorMessage("Add more backlogs to see suggestions");
                        _error = true;
                        break;
                    }
                    RandomBacklog = _incompleteBacklogs[_random.Next(0, IncompleteBacklogsCount)];
                    break;
                case "film":
                    var filmBacklogs = new ObservableCollection<Backlog>(_incompleteBacklogs.Where(b => b.Type == "Film"));
                    if (filmBacklogs.Count <= 0)
                    {
                        await ShowErrorMessage("Add more films to see suggestions");
                        _error = true;
                        break;
                    }
                    RandomBacklog = filmBacklogs[_random.Next(0, filmBacklogs.Count)];
                    break;
                case "album":
                    var musicBacklogs = new ObservableCollection<Backlog>(_incompleteBacklogs.Where(b => b.Type == "Album"));
                    if (musicBacklogs.Count <= 0)
                    {
                        await ShowErrorMessage("Add more albums to see suggestions");
                        _error = true;
                        break;
                    }
                    RandomBacklog = musicBacklogs[_random.Next(0, musicBacklogs.Count)];
                    break;
                case "game":
                    var gameBacklogs = new ObservableCollection<Backlog>(_incompleteBacklogs.Where(b => b.Type == "Game"));
                    if (gameBacklogs.Count <= 0)
                    {
                        await ShowErrorMessage("Add more games to see suggestions");
                        _error = true;
                        break;
                    }
                    RandomBacklog = gameBacklogs[_random.Next(0, gameBacklogs.Count)];
                    break;
                case "book":
                    var bookBacklogs = new ObservableCollection<Backlog>(_incompleteBacklogs.Where(b => b.Type == "Book"));
                    if (bookBacklogs.Count <= 0)
                    {
                        await ShowErrorMessage("Add more books to see suggestions");
                        _error = true;
                        break;
                    }
                    RandomBacklog = bookBacklogs[_random.Next(0, bookBacklogs.Count)];
                    break;
                case "tv":
                    var tvBacklogs = new ObservableCollection<Backlog>(_incompleteBacklogs.Where(b => b.Type == "TV"));
                    if (tvBacklogs.Count <= 0)
                    {
                        await ShowErrorMessage("Add more series to see suggestions");
                        _error = true;
                        break;
                    }
                    RandomBacklog = tvBacklogs[_random.Next(0, tvBacklogs.Count)];
                    break;
            }
            if (!_error)
            {
                RandomBacklogId = RandomBacklog.id;
            }
        }

        private async Task ShowErrorMessage(string message)
        {
            await m_dialogHandler.ShowErrorDialogAsync("Not enought backlogs", message, "OK");
        }

        /// <summary>
        /// Import backog from file
        /// </summary>
        /// <returns></returns>
        private async Task ImportBacklogAsync()
        {
            var name = await m_filePicker.LaunchFilePickerAsync();
            m_navigationService.NavigateTo<ImportBacklogViewModel>(name);
        }

        /// <summary>
        /// Opens the web app
        /// </summary>
        /// <returns></returns>
        private async Task OpenWebAppAsync()
        {
            var _webAppUri = new Uri(@"https://backlogs.azurewebsites.net/");
            await Windows.System.Launcher.LaunchUriAsync(_webAppUri);
        }

        /// <summary>
        /// Signs the user out
        /// </summary>
        /// <returns></returns>
        private async Task SignOutAsync()
        {
            if (await m_dialogHandler.ShowSignOutDialogAsync())
            {
                await MSAL.SignOut();
                Settings.IsSignedIn = false;
                SyncBacklogs();
            }
        }

        /// <summary>
        /// Send crash log via email
        /// </summary>
        /// <returns></returns>
        private async Task SendCrashLogAsync()
        {
            IsBusy = true;
            var subject = "Crash Dump from Backlogs";
            StringBuilder body = new StringBuilder();
            body.AppendLine("*Enter additional info such as what may have caused the crash*");
            body.AppendLine("\n\n\n");
            body.AppendLine(m_crashLog);
            await m_emailService.SendEmailAsync(subject, body.ToString());
            IsBusy = false;
        }

        private void NavigateToCreatePage()
        {
            m_navigationService.NavigateTo<CreateBacklogViewModel>();
        }

        private void NavigateToSettingsPage(object args = null)
        {
            m_navigationService.NavigateTo<SettingsViewModel>(args);
        }

        private void NavigateToCompletedPage()
        {
            m_navigationService.NavigateTo<CompletedBacklogsViewModel>();
        }

        private void NavigateToBacklogsPage()
        {
            m_navigationService.NavigateTo<BacklogsViewModel>();
        }
    }
}

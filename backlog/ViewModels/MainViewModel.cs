using Backlogs.Auth;
using Backlogs.Logging;
using Backlogs.Models;
using Backlogs.Saving;
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
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

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
        private BitmapImage m_accountPic;
        private string m_randomBacklogType = "Any";
        private int m_backlogsCount = 0;
        private int m_completedBacklogsCount = 0;
        private int m_incompleteBacklogsCount = 0;
        private double m_completedPercent = 0;
        private ContentDialog m_crashLogPopup;
        private readonly INavigationService m_navigationService;

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

        public BitmapImage AccountPic
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

        public MainViewModel(ContentDialog crashLogPopup, INavigationService navigationService)
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

            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
            LoadBacklogs();
            m_crashLogPopup = crashLogPopup;
            m_navigationService = navigationService;
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
            var localSettings = ApplicationData.Current.LocalSettings;
            m_crashLog = localSettings.Values["LastCrashLog"] as string;
            if (m_crashLog != null)
            {
                m_crashLogPopup.Content = $"It seems the application crashed the last time, with the following error: {m_crashLog}";
                await m_crashLogPopup.ShowAsync();
            }
            localSettings.Values["LastCrashLog"] = null;
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
            var cacheFolder = ApplicationData.Current.LocalCacheFolder;
            try
            {
                var accountPicFile = await cacheFolder.GetFileAsync("profile.png");
                using (IRandomAccessStream stream = await accountPicFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage image = new BitmapImage();
                    stream.Seek(0);
                    await image.SetSourceAsync(stream);
                    AccountPic = image;
                }
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
                if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                {
                    ShowTopTeachingTip = true;
                }
                else
                {
                    ShowBottomTeachingTip = true;
                }
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
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "No Internet",
                    Content = "You need to be connected to sign-in",
                    CloseButtonText = "Ok"
                };
                _ = await contentDialog.ShowAsync();
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

        #region LiveTile
        /// <summary>
        /// Build the live tile queue
        /// </summary>
        private void ShowLiveTiles()
        {
            switch (Settings.TileContent)
            {
                case "Recently Created":
                    {
                        if (RecentlyAdded != null)
                        {
                            foreach (var b in RecentlyAdded.Take(5))
                            {
                                GenerateRecentlyAddedLiveTile(b);
                            }
                        }
                    }
                    break;
                case "Recently Completed":
                    {
                        if (RecentlyCompleted != null)
                        {
                            foreach (var b in RecentlyCompleted.Take(5))
                            {
                                GenerateRecentlyCompletedLiveTile(b);
                            }
                        }
                    }
                    break;
                case "In Progress":
                    {
                        if (InProgress != null)
                        {
                            foreach (var b in InProgress.Take(5))
                            {
                                GenerateInProgressLiveTile(b);
                            }
                        }
                    }
                    break;
                case "Upcoming":
                    {
                        if (Upcoming != null)
                        {
                            foreach (var b in Upcoming.Take(5))
                            {
                                GenerateUpcomingLiveTile(b);
                            }
                        }
                    }
                    break;
            }
        }

        private void GenerateRecentlyAddedLiveTile(Backlog b)
        {
            TileContent tileContent = null;
            if (Settings.TileStyle == "Peeking")
            {
                tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = "Backlogs",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL,
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Type,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.TargetDate,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL
                                },
                                Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = b.Name
                                },
                                new AdaptiveText()
                                {
                                    Text = $"{b.Type} - {b.Director}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = b.Description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = b.TargetDate,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = false
                                }
                            }
                            }
                        }
                    }
                };
            }
            else
            {
                tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = "Backlogs",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Type,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.TargetDate,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                    HintOverlay = 50
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                    HintOverlay = 50
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.TargetDate,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = false
                                    }
                                }
                            }
                        }
                    }
                };
            }


            // Create the tile notification
            var tileNotif = new TileNotification(tileContent.GetXml());

            // And send the notification to the primary tile
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
        }

        private void GenerateRecentlyCompletedLiveTile(Backlog b)
        {
            TileContent tileContent = null;
            if (Settings.TileStyle == "Peeking")
            {
                tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = "Backlogs",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL,
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Type,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.UserRating} / 5",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"Rating: {b.UserRating}/5",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL
                                },
                                Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = b.Name
                                },
                                new AdaptiveText()
                                {
                                    Text = $"{b.Type} - {b.Director}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = b.Description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = $"Rating: {b.UserRating} / 5",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = false
                                }
                            }
                            }
                        }
                    }
                };
            }
            else
            {
                tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = "Backlogs",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Type,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.UserRating} / 5",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                    HintOverlay = 50
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"Rating: {b.UserRating}/5",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                    HintOverlay = 50
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"Rating: {b.UserRating} / 5",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = false
                                    }
                                }
                            }
                        }
                    }
                };
            }


            // Create the tile notification
            var tileNotif = new TileNotification(tileContent.GetXml());

            // And send the notification to the primary tile
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
        }

        private void GenerateInProgressLiveTile(Backlog b)
        {
            TileContent tileContent = null;
            if (Settings.TileStyle == "Peeking")
            {
                tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = "Backlogs",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL,
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Type,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Progress} {b.Units}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Progress} of {b.Length} {b.Units}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL
                                },
                                Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = b.Name
                                },
                                new AdaptiveText()
                                {
                                    Text = $"{b.Type} - {b.Director}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = b.Description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = $"{b.Progress} of {b.Length} {b.Units}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                }
                            }
                            }
                        }
                    }
                };
            }
            else
            {
                tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = "Backlogs",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Type,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Progress} {b.Units}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                    HintOverlay = 50
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Progress} of {b.Length} {b.Units}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                    HintOverlay = 50
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Progress} of {b.Length} {b.Units}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        }
                    }
                };
            }


            // Create the tile notification
            var tileNotif = new TileNotification(tileContent.GetXml());

            // And send the notification to the primary tile
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
        }

        private void GenerateUpcomingLiveTile(Backlog b)
        {
            TileContent tileContent = null;
            if (Settings.TileStyle == "Peeking")
            {
                tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = "Backlogs",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL,
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Type,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.TargetDate,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"Target Date: {b.TargetDate}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL
                                },
                                Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = b.Name
                                },
                                new AdaptiveText()
                                {
                                    Text = $"{b.Type} - {b.Director}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = b.Description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = $"Target Date: {b.TargetDate}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                }
                            }
                            }
                        }
                    }
                };
            }
            else
            {
                tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = "Backlogs",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Type,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.TargetDate,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                    HintOverlay = 50
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"Target Date: {b.TargetDate}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                    HintOverlay = 50
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"Target Date: {b.TargetDate}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        }
                    }
                };
            }


            // Create the tile notification
            var tileNotif = new TileNotification(tileContent.GetXml());

            // And send the notification to the primary tile
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
        }
        #endregion

        /// <summary>
        /// Launch Windows Share UI to show share options
        /// </summary>
        /// <returns></returns>
        private void ShareAppLink()
        {
            DataTransferManager _dataTransferManager = DataTransferManager.GetForCurrentView();
            _dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest _request = args.Request;
            _request.Data.SetText("https://www.microsoft.com/store/apps/9N2H8CM2KWVZ");
            _request.Data.Properties.Title = "https://www.microsoft.com/store/apps/9N2H8CM2KWVZ";
            _request.Data.Properties.Description = "Share this app with your contacts";
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
            ContentDialog contentDialog = new ContentDialog()
            {
                Title = "Not enough Backlogs",
                Content = message,
                CloseButtonText = "Ok"
            };
            await contentDialog.ShowAsync();
        }

        /// <summary>
        /// Import backog from file
        /// </summary>
        /// <returns></returns>
        private async Task ImportBacklogAsync()
        {
            var _picker = new Windows.Storage.Pickers.FileOpenPicker();
            _picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            _picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            _picker.FileTypeFilter.Add(".bklg");

            StorageFile _file = await _picker.PickSingleFileAsync();
            if (_file != null)
            {
                StorageFolder _tempFolder = ApplicationData.Current.TemporaryFolder;
                await _tempFolder.CreateFileAsync(_file.Name, CreationCollisionOption.ReplaceExisting);
                string json = await FileIO.ReadTextAsync(_file);
                var _stFile = await _tempFolder.GetFileAsync(_file.Name);
                await FileIO.WriteTextAsync(_stFile, json);
                m_navigationService.NavigateTo<ImportBacklogViewModel>(_stFile.Name);
            }
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
            ContentDialog contentDialog = new ContentDialog
            {
                Title = "Sign out?",
                Content = "You will no longer have access to your backlogs, and new ones will no longer be synced",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };
            ContentDialogResult result = await contentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
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
            EmailMessage emailMessage = new EmailMessage();
            emailMessage.To.Add(new EmailRecipient("surya.sk05@outlook.com"));
            emailMessage.Subject = "Crash Dump from Backlogs";
            StringBuilder body = new StringBuilder();
            body.AppendLine("*Enter additional info such as what may have caused the crash*");
            body.AppendLine("\n\n\n");
            body.AppendLine(m_crashLog);
            emailMessage.Body = body.ToString();
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
            IsBusy = false;
        }

        private void NavigateToCreatePage()
        {
            try
            {
                m_navigationService.NavigateTo<CreateBacklogViewModel>(null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
            }
            catch
            {
                m_navigationService.NavigateTo<CreateBacklogViewModel>();
            }
        }

        private void NavigateToSettingsPage(object args = null)
        {
            try
            {
                m_navigationService.NavigateTo<SettingsViewModel>(args, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            catch
            {
                m_navigationService.NavigateTo<SettingsViewModel>(args);
            }
        }

        private void NavigateToCompletedPage()
        {
            try
            {
                m_navigationService.NavigateTo<CompletedBacklogsViewModel>(null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
            catch
            {
                m_navigationService.NavigateTo<CompletedBacklogsViewModel>();
            }
        }

        private void NavigateToBacklogsPage()
        {
            m_navigationService.NavigateTo<BacklogsViewModel>();
        }
    }
}

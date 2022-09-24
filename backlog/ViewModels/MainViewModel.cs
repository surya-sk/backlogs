using backlog.Auth;
using backlog.Logging;
using backlog.Models;
using backlog.Saving;
using backlog.Utils;
using backlog.Views;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.Notifications;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace backlog.ViewModels
{
    public class MainViewModel: INotifyPropertyChanged
    {
        private bool _isBusy;
        private string _crashLog;
        private bool _networkAvailable;
        private Guid _randomBacklogId = new Guid();
        private Backlog _randomBacklog;
        private bool _recentlyCreatedEmpty;
        private bool _recentlyCompletedEmtpy;
        private bool _inProgressEmpty;
        private bool _upcomingEmpty;
        private bool _showTopTeachingTip;
        private bool _showBottomTeachingTip;
        private bool _showProfileButtons;
        private bool _showSignInButton = true;
        private BitmapImage _accountPic;
        private string _randomBacklogType;

        public ObservableCollection<Backlog> Backlogs { get; set; }
        public ObservableCollection<Backlog> RecentlyAdded { get; set; }
        public ObservableCollection<Backlog> RecentlyCompleted { get; set; }
        public ObservableCollection<Backlog> InProgress { get; set; }
        public ObservableCollection<Backlog> Upcoming { get; set; }
        
        public int BacklogsCount { get; set; }
        public int CompletedBacklogsCount { get; set; }
        public int IncompleteBacklogsCount { get; set; }
        public bool IsFirstRun { get; } = Settings.IsFirstRun;
        public bool ShowWhatsNew { get; } = Settings.ShowWhatsNew;
        public double CompletedBacklogsPercent { get; set; }
        public string WelcomeText = Settings.IsSignedIn ? $"Welcome to Backlogs, {Settings.UserName}!" : "Welcome to Backlogs, stranger!";
        public string UserName { get; } = Settings.UserName;
        public bool SignedIn { get; } = Settings.IsSignedIn;
        public bool Sync { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public delegate Task ShowLastCrashLog(string log);
        public delegate void ReloadAndSync();
        public delegate void OpenImportPage(string fileName);

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

        public ShowLastCrashLog ShowLastCrashLogFunc;
        public ReloadAndSync ReloadAndSyncFunc;
        public OpenImportPage OpenImportPageFunc;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
            }
        }

        public Guid RandomBacklogId
        {
            get => _randomBacklogId;
            set
            {
                _randomBacklogId = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RandomBacklogId)));
            }
        }

        public Backlog RandomBacklog
        {
            get => _randomBacklog;
            set
            {
                _randomBacklog = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RandomBacklog)));
            }
        }

        public bool RecentlyCreatedEmpty
        {
            get => _recentlyCreatedEmpty;
            set
            {
                _recentlyCreatedEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecentlyCreatedEmpty)));
            }
        }

        public bool RecentlyCompletedEmpty
        {
            get => _recentlyCompletedEmtpy;
            set
            {
                _recentlyCompletedEmtpy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecentlyCompletedEmpty)));
            }
        }

        public bool InProgressEmpty
        {
            get => _inProgressEmpty;
            set
            {
                _inProgressEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InProgressEmpty)));
            }
        }

        public bool UpcomingEmpty
        {
            get => _upcomingEmpty;
            set
            {
                _upcomingEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpcomingEmpty)));
            }
        }

        public bool ShowTopTeachingTip
        {
            get => _showTopTeachingTip;
            set
            {
                _showTopTeachingTip = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowTopTeachingTip)));
            }
        }

        public bool ShowBottomTeachingTip
        {
            get => _showBottomTeachingTip;
            set
            {
                _showBottomTeachingTip = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowBottomTeachingTip)));

            }
        }

        public BitmapImage AccountPic
        {
            get => _accountPic;
            set
            {
                _accountPic = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccountPic)));
            }
        }

        public bool ShowProfileButton
        {
            get => _showProfileButtons;
            set
            {
                _showProfileButtons = value;
                ShowSignInButton = !_showProfileButtons;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowProfileButton)));
            }
        }

        public bool ShowSignInButton
        {
            get => _showSignInButton;
            set
            {
                _showSignInButton = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowSignInButton)));
            }
        }

        public string RandomBacklogType
        {
            get => _randomBacklogType;
            set
            {
                _randomBacklogType = value;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(RandomBacklogType)));
            }
        }

        public MainViewModel()
        {
            _networkAvailable = NetworkInterface.GetIsNetworkAvailable();

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

            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
            LoadBacklogs();
        }

        public async Task SetupProfile()
        {
            IsBusy = true;
            await ShowCrashLog();
            if(_networkAvailable && Settings.IsSignedIn)
            {
                try
                {
                    await Logger.Info("Internet access");
                    await Logger.Info("Signing in user....");
                }
                catch { }
                await SetUserPhotoAsync();
                ShowProfileButton = true;
                await SaveData.GetInstance().ReadDataAsync(Sync);
                SaveData.GetInstance().ResetHelperBacklogs();
                LoadBacklogs();
                ShowTeachingTips();
                ShowLiveTiles();
                IsBusy = false;
            }
        }

        /// <summary>
        /// Show a content dialog with the reason for crash
        /// </summary>
        /// <returns></returns>
        private async Task ShowCrashLog()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            _crashLog = localSettings.Values["LastCrashLog"] as string;
            if (_crashLog != null)
            {
                await ShowLastCrashLogFunc(_crashLog);
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
            RecentlyCreatedEmpty = RecentlyAdded.Count > 0;
            
            var _recentlyCompleted = SaveData.GetInstance().GetRecentlyCompletedBacklogs();
            RecentlyCompleted.Clear();
            foreach (var b in _recentlyCompleted)
            {
                RecentlyCompleted.Add(b);
            }
            RecentlyCompletedEmpty = RecentlyCompleted.Count > 0;

            var _inProgress = SaveData.GetInstance().GetInProgressBacklogs();
            InProgress.Clear();
            foreach(var b in _inProgress)
            {
                InProgress.Add(b);
            }    
            InProgressEmpty = InProgress.Count > 0;

            var _upcoming = SaveData.GetInstance().GetUpcomingBacklogs();
            Upcoming.Clear();
            foreach(var b in _upcoming)
            {
                Upcoming.Add(b);
            }
            UpcomingEmpty = Upcoming.Count > 0;

            CompletedBacklogsCount = SaveData.GetInstance().GetCompletedBacklogs().Count;
            IncompleteBacklogsCount = SaveData.GetInstance().GetIncompleteBacklogs().Count;
            BacklogsCount = SaveData.GetInstance().GetBacklogs().Count();
            CompletedBacklogsPercent = (Convert.ToDouble(CompletedBacklogsCount) / BacklogsCount) * 100;

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
            if (_networkAvailable)
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
                    ReloadAndSyncFunc();
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
            ReloadAndSyncFunc();
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
                OpenImportPageFunc(_stFile.Name);
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
                ReloadAndSyncFunc();
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
            body.AppendLine(_crashLog);
            emailMessage.Body = body.ToString();
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
            IsBusy = false;
        }
    }
}

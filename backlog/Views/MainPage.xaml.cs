using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using backlog.Models;
using backlog.Saving;
using backlog.Utils;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Core;
using System.Net.NetworkInformation;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using System.Globalization;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Animation;
using backlog.Logging;
using Windows.Storage;
using backlog.Auth;
using Windows.ApplicationModel.Email;
using System.Text;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Backlog> allBacklogs { get; set; }
        private ObservableCollection<Backlog> backlogs { get; set; }

        private ObservableCollection<Backlog> recentlyAdded { get; set; }
        private ObservableCollection <Backlog> recentlyCompleted { get; set; }
        private ObservableCollection<Backlog> inProgress { get; set; }
        private ObservableCollection<Backlog> comingUp { get; set; }

        ObservableCollection<Backlog> completedBacklogs;
        ObservableCollection<Backlog> incompleteBacklogs;
        ObservableCollection<Backlog> inProgressBacklogs;
        ObservableCollection<Backlog> comingUpBacklogs;

        private int backlogCount;
        private int completedBacklogsCount;
        private int incompleteBacklogsCount;
        private double completedPercent;

        GraphServiceClient graphServiceClient;

        bool isNetworkAvailable = false;
        bool signedIn;
        int backlogIndex = -1;
        bool sync = false;
        string crashLog;

        Guid randomBacklogId = new Guid();

        public MainPage()
        {
            this.InitializeComponent();
            isNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
            WelcomeText.Text = Settings.IsSignedIn ? $"Welcome to Backlogs, {Settings.UserName}!" : "Welcome to Backlogs, stranger!";
            
            recentlyAdded = new ObservableCollection<Backlog>();
            recentlyCompleted = new ObservableCollection<Backlog>();
            inProgress = new ObservableCollection<Backlog>();
            comingUp = new ObservableCollection<Backlog>();
            completedBacklogs = new ObservableCollection<Backlog>();
            incompleteBacklogs = new ObservableCollection<Backlog>();
            inProgressBacklogs = new ObservableCollection<Backlog>();
            comingUpBacklogs = new ObservableCollection<Backlog>();
            LoadBacklogs();
            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Disabled;
        }

        /// <summary>
        /// Show a content dialog with the reason for crash
        /// </summary>
        /// <returns></returns>
        private async Task ShowCrashLog()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            var lastCrashLog = localSettings.Values["LastCrashLog"] as string;

            if(lastCrashLog != null)
            {
                CrashDialog.Content = $"It seems the application crashed the last time, with the following error: {lastCrashLog}";
                await CrashDialog.ShowAsync();
                crashLog = lastCrashLog;
                localSettings.Values["LastCrashLog"] = null;
            }
        }

        /// <summary>
        /// Shows the teaching tips on fresh install
        /// </summary>
        private void ShowTeachingTips()
        {
            if(Settings.IsFirstRun)
            {
                Settings.IsFirstRun = false;
            }
            if(!Settings.IsSignedIn)
            {
                if(TopAppBar.Visibility == Visibility.Visible)
                {
                    TopSigninTeachingTip.IsOpen=true;
                }
                else
                {
                    BottomSigninTeachingTip.IsOpen = true;
                }
            }
            if(Settings.ShowWhatsNew)
            {
                WhatsNewTip.IsOpen = true;
                Settings.ShowWhatsNew = false;
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null && e.Parameter.ToString() != "")
            {
                if (e.Parameter.ToString() == "sync")
                {
                    sync = true;
                    try
                    {
                        await Logger.Info("Syncing backlogs");
                    }
                    catch { }
                }
                else
                {
                    // for backward connected animation
                    backlogIndex = int.Parse(e.Parameter.ToString());
                }
            }
            ProgBar.Visibility = Visibility.Visible;
            await ShowCrashLog();
            signedIn = Settings.IsSignedIn;
            if (isNetworkAvailable && signedIn)
            {
                try
                {
                    await Logger.Info("Internet access");
                    await Logger.Info("Signing in user....");
                }
                catch { }
                graphServiceClient = await MSAL.GetGraphServiceClient();
                await SetUserPhotoAsync();
                TopSigninButton.Visibility = Visibility.Collapsed;
                BottomSigninButton.Visibility = Visibility.Collapsed;
                TopProfileButton.Visibility = Visibility.Visible;
                BottomProfileButton.Visibility = Visibility.Visible;
                if(sync)
                {
                    await SaveData.GetInstance().ReadDataAsync(true);
                }
                LoadBacklogs();
            }
            ShowLiveTiles();
            ShowTeachingTips();
            ProgBar.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Update backlog list according to the latest copy
        /// </summary>
        private async void LoadBacklogs()
        {
            recentlyAdded.Clear();
            recentlyCompleted.Clear();
            completedBacklogs.Clear();
            comingUpBacklogs.Clear();
            incompleteBacklogs.Clear();
            inProgressBacklogs.Clear();
            comingUp.Clear();
            inProgress.Clear();
            completedBacklogsCount = 0;
            incompleteBacklogsCount = 0;
            completedPercent = 0.0f;
            backlogs = SaveData.GetInstance().GetBacklogs();
            if(backlogs != null && backlogs.Count > 0)
            {
                foreach (var backlog in backlogs)
                {
                    if (!backlog.IsComplete)
                    {
                        if (backlog.CreatedDate == "None" || backlog.CreatedDate == null)
                        {
                            backlog.CreatedDate = DateTimeOffset.MinValue.ToString("d", CultureInfo.InvariantCulture);
                        }
                        incompleteBacklogs.Add(backlog);
                        if (backlog.progress > 0)
                        {
                            inProgressBacklogs.Add(backlog);
                        }
                        try
                        {
                            if (DateTime.Parse(backlog.TargetDate) >= DateTime.Today)
                            {
                                comingUpBacklogs.Add(backlog);
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (backlog.CompletedDate == null)
                        {
                            backlog.CompletedDate = DateTimeOffset.MinValue.ToString("d", CultureInfo.InvariantCulture);
                        }
                        completedBacklogs.Add(backlog);
                    }
                }
                foreach (var backlog in incompleteBacklogs.OrderByDescending(b => DateTimeOffset.Parse(b.CreatedDate, CultureInfo.InvariantCulture)).Skip(0).Take(6))
                {
                    recentlyAdded.Add(backlog);
                }
                foreach (var backlog in completedBacklogs.OrderByDescending(b => DateTimeOffset.Parse(b.CompletedDate, CultureInfo.InvariantCulture)).Skip(0).Take(6))
                {
                    recentlyCompleted.Add(backlog);
                }
                foreach(var backlog in inProgressBacklogs.OrderByDescending(b => b.progress).Skip(0).Take(6))
                {
                    inProgress.Add(backlog);
                }
                foreach (var backlog in comingUpBacklogs.OrderBy(b => DateTimeOffset.Parse(b.TargetDate, CultureInfo.InvariantCulture)).Skip(0).Take(6))
                {
                    comingUp.Add(backlog);
                }
                if (completedBacklogs.Count <= 0)
                {
                    EmptyCompletedText.Visibility = Visibility.Visible;
                    CompletedBacklogsGrid.Visibility = Visibility.Collapsed;
                }
                if(inProgress.Count <= 0)
                {
                    EmptyProgressBackogsText.Visibility = Visibility.Visible;
                    InProgressBacklogsGrid.Visibility = Visibility.Collapsed;
                }
                if(comingUp.Count <= 0)
                {
                    EmptyUpcomingText.Visibility = Visibility.Visible;
                    UpcomingBacklogsGrid.Visibility = Visibility.Collapsed;
                }
                completedBacklogsCount = completedBacklogs.Count;
                incompleteBacklogsCount = incompleteBacklogs.Count;
                backlogCount = backlogs.Count;
                try
                {
                    await Logger.Info($"{backlogCount} backlog(s) found");
                }
                catch { }
                completedPercent = (Convert.ToDouble(completedBacklogsCount) / backlogCount) * 100;
                PercentBar.Value = completedPercent;
                GenerateRandomBacklog();
            }

            else
            {
                EmptyBackogsText.Visibility = Visibility.Visible;
                EmptySuggestionsText.Visibility = Visibility.Visible;
                EmptyCompletedText.Visibility = Visibility.Visible;
                EmptyProgressBackogsText.Visibility = Visibility.Visible;
                EmptyUpcomingText.Visibility = Visibility.Visible;
                InProgressBacklogsGrid.Visibility = Visibility.Collapsed;
                AddedBacklogsGrid.Visibility = Visibility.Collapsed;
                CompletedBacklogsGrid.Visibility = Visibility.Collapsed;
                suggestionsGrid.Visibility = Visibility.Collapsed;
                InputPanel.Visibility = Visibility.Collapsed;
                UpcomingBacklogsGrid.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Set the user photo in the command bar
        /// </summary>
        /// <returns></returns>
        private async Task SetUserPhotoAsync()
        {
            try
            {
                await Logger.Info("Setting user photo....");
            }
            catch { }
            string userName = Settings.UserName;
            TopProfileButton.Label = userName;
            BottomProfileButton.Label = userName;
            var cacheFolder = ApplicationData.Current.LocalCacheFolder;
            try
            {
                var accountPicFile = await cacheFolder.GetFileAsync("profile.png");
                using (IRandomAccessStream stream = await accountPicFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage image = new BitmapImage();
                    stream.Seek(0);
                    await image.SetSourceAsync(stream);
                    TopAccountPic.ProfilePicture = image;
                    BottomAccountPic.ProfilePicture = image;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    await Logger.Error("Error settings", ex);
                }
                catch { }
            }
            
        }


        /// <summary>
        /// Signs the user in if connected to the internet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SigninButton_Click(object sender, RoutedEventArgs e)
        {
            ProgBar.Visibility = Visibility.Visible;
            TopSigninTeachingTip.IsOpen = false;
            BottomSigninTeachingTip.IsOpen = false;
            signedIn = Settings.IsSignedIn;
            if (isNetworkAvailable)
            {
                if (!signedIn)
                {
                    try
                    {
                        await Logger.Info("Signing in...");
                    }
                    catch { }
                    await SaveData.GetInstance().DeleteLocalFileAsync();
                    graphServiceClient = await MSAL.GetGraphServiceClient();
                    Settings.IsSignedIn = true;
                    await SaveData.GetInstance().ReadDataAsync(true);
                    Frame.Navigate(typeof(MainPage), "sync");
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
        /// Opens the Create page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(CreatePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom});
            }
            catch
            {
                Frame.Navigate(typeof(CreatePage));
            }
        }

        /// <summary>
        /// Opens the Setting page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        /// <summary>
        /// Launches the Store rating page for the app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RateButton_Click(object sender, RoutedEventArgs e)
        {
            var ratingUri = new Uri(@"ms-windows-store://review/?ProductId=9N2H8CM2KWVZ");
            await Windows.System.Launcher.LaunchUriAsync(ratingUri);
        }

        /// <summary>
        /// Build the live tile queue
        /// </summary>
        private void ShowLiveTiles()
        {
            switch(Settings.TileContent)
            {
                case "Recently Created":
                    {
                        if (recentlyAdded != null)
                        {
                            foreach (var b in recentlyAdded.Take(5))
                            {
                                GenerateRecentlyAddedLiveTile(b);
                            }
                        }
                    }
                    break;
                case "Recently Completed":
                    {
                        if (recentlyCompleted != null)
                        {
                            foreach (var b in recentlyCompleted.Take(5))
                            {
                                GenerateRecentlyCompletedLiveTile(b);
                            }
                        }
                    }
                    break;
                case "In Progress":
                    {
                        if (inProgress != null)
                        {
                            foreach (var b in inProgress.Take(5))
                            {
                                GenerateInProgressLiveTile(b);
                            }
                        }
                    }
                    break;
                case "Upcoming":
                    {
                        if(comingUp != null)
                        {
                            foreach(var b in comingUp.Take(5))
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
            if(Settings.TileStyle == "Peeking")
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
                                BackgroundImage= new TileBackgroundImage()
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

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.SetText("https://www.microsoft.com/store/apps/9N2H8CM2KWVZ");
            request.Data.Properties.Title = "https://www.microsoft.com/store/apps/9N2H8CM2KWVZ";
            request.Data.Properties.Description = "Share this app with your contacts";
        }


        private async void SupportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Logger.Info("Opening Paypal link...");
            }
            catch { }
            var paypalUri = new Uri(@"https://paypal.me/surya4822?locale.x=en_US");
            await Windows.System.Launcher.LaunchUriAsync(paypalUri);
        }

        /// <summary>
        /// Sync backlogs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage), "sync");
        }

        private void CompletedBacklogsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(CompletedBacklogsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight});
            }
            catch
            {
                Frame.Navigate(typeof(CompletedBacklogsPage));
            }
        }

        private void BacklogsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BacklogsPage));
        }

        private void AddedBacklogsGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedBacklog = (Backlog)e.ClickedItem;
            AddedBacklogsGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
            Frame.Navigate(typeof(BacklogPage), selectedBacklog.id, new SuppressNavigationTransitionInfo());
        }

        private async void AddedBacklogsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (backlogIndex != -1)
            {
                ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("backAnimation");
                try
                {
                    await AddedBacklogsGrid.TryStartConnectedAnimationAsync(animation, backlogs[backlogIndex], "coverImage");
                }
                catch
                {
                    // : )
                }
            }
        }

        private void UpcomingBacklogsGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedBacklog = (Backlog)e.ClickedItem;
            UpcomingBacklogsGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
            Frame.Navigate(typeof(BacklogPage), selectedBacklog.id, new SuppressNavigationTransitionInfo());
        }

        private async void UpcomingBacklogsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if(backlogIndex != -1)
            {
                ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("backAnimation");
                try
                {
                    await UpcomingBacklogsGrid.TryStartConnectedAnimationAsync(animation, backlogs[backlogIndex], "coveerImage");
                }
                catch
                {
                    // : )
                }
            }
        }

        private void InProgressBacklogsGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedBacklog = (Backlog)e.ClickedItem;
            InProgressBacklogsGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
            Frame.Navigate(typeof(BacklogPage), selectedBacklog.id, new SuppressNavigationTransitionInfo());
        }

        private async void InProgressBacklogsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (backlogIndex != -1)
            {
                ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("backAnimation");
                try
                {
                    await InProgressBacklogsGrid.TryStartConnectedAnimationAsync(animation, backlogs[backlogIndex], "coverImage");
                }
                catch
                {
                    // : )
                }
            }
        }

        private void AllAddedButton_Click(object sender, RoutedEventArgs e)
        {
            BacklogsButton_Click(sender, e);
        }

        private void AllCompletedButton_Click(object sender, RoutedEventArgs e)
        {
            CompletedBacklogsButton_Click(sender, e);
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateRandomBacklog();
        }

        private async void GenerateRandomBacklog()
        {
            try
            {
                await Logger.Info("Generating random backlog...");
            }
            catch { }
            var type = TypeComoBox.SelectedItem.ToString();
            Random random = new Random();
            Backlog randomBacklog = new Backlog();
            bool error = false;
            switch(type.ToLower())
            {
                case "any":
                    randomBacklog = incompleteBacklogs[random.Next(0, incompleteBacklogsCount)];
                    break;
                case "film":
                    var filmBacklogs = new ObservableCollection<Backlog>(incompleteBacklogs.Where(b => b.Type == "Film"));
                    if(filmBacklogs.Count <= 0)
                    {
                        await ShowErrorMessage("Add more films to see suggestions");
                        error = true;
                        break;
                    }
                    randomBacklog = filmBacklogs[random.Next(0, filmBacklogs.Count)];
                    break;
                case "album":
                    var musicBacklogs = new ObservableCollection<Backlog>(incompleteBacklogs.Where(b => b.Type == "Album"));
                    if(musicBacklogs.Count <= 0)
                    {
                        await ShowErrorMessage("Add more albums to see suggestions");
                        error = true;
                        break;
                    }
                    randomBacklog = musicBacklogs[random.Next(0, musicBacklogs.Count)];
                    break;
                case "game":
                    var gameBacklogs = new ObservableCollection<Backlog>(incompleteBacklogs.Where(b => b.Type == "Game"));
                    if(gameBacklogs.Count <= 0)
                    {
                        await ShowErrorMessage("Add more games to see suggestions");
                        error = true;
                        break;
                    }    
                    randomBacklog = gameBacklogs[random.Next(0, gameBacklogs.Count)];
                    break;
                case "book":
                    var bookBacklogs = new ObservableCollection<Backlog>(incompleteBacklogs.Where(b => b.Type == "Book"));
                    if(bookBacklogs.Count <= 0)
                    {
                        await ShowErrorMessage("Add more books to see suggestions");
                        error = true;
                        break;
                    }
                    randomBacklog = bookBacklogs[random.Next(0, bookBacklogs.Count)];
                    break;
                case "tv":
                    var tvBacklogs = new ObservableCollection<Backlog>(incompleteBacklogs.Where(b => b.Type == "TV"));
                    if(tvBacklogs.Count <= 0)
                    {
                        await ShowErrorMessage("Add more series to see suggestions");
                        error = true;
                        break;
                    }
                    randomBacklog = tvBacklogs[random.Next(0, tvBacklogs.Count)];
                    break;
            }
            if (!error)
            {
                RunName.Text = randomBacklog.Name;
                suggestionCover.Source = new BitmapImage(new Uri(randomBacklog.ImageURL));
                randomBacklogId = randomBacklog.id;
            }
        }

        private void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            Frame.Navigate(typeof(BacklogPage), randomBacklogId, null);
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

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            picker.FileTypeFilter.Add(".bklg");

            StorageFile file = await picker.PickSingleFileAsync();
            if(file != null)
            {
                StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
                await tempFolder.CreateFileAsync(file.Name, CreationCollisionOption.ReplaceExisting);
                string json = await FileIO.ReadTextAsync(file);
                var stFile = await tempFolder.GetFileAsync(file.Name);
                await FileIO.WriteTextAsync(stFile, json);
                Frame.Navigate(typeof(ImportBacklog), stFile.Name, null);
            }
        }

        private async void WebAppButton_Click(object sender, RoutedEventArgs e)
        {
            var webAppUri = new Uri(@"https://backlogs.azurewebsites.net/");
            await Windows.System.Launcher.LaunchUriAsync(webAppUri);
        }

        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
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
                Frame.Navigate(typeof(MainPage));
            }
        }

        private void WhatsNewTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            Frame.Navigate(typeof(SettingsPage), 1);
        }

        private async void CrashDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ProgBar.Visibility = Visibility.Visible;
            EmailMessage emailMessage = new EmailMessage();
            emailMessage.To.Add(new EmailRecipient("surya.sk05@outlook.com"));
            emailMessage.Subject = "Crash Dump from Backlogs";
            StringBuilder body = new StringBuilder();
            body.AppendLine("*Enter additional info such as what may have caused the crash*");
            body.AppendLine("\n\n\n");
            body.AppendLine(crashLog);
            emailMessage.Body = body.ToString();
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
            ProgBar.Visibility = Visibility.Collapsed;
        }
    }
}

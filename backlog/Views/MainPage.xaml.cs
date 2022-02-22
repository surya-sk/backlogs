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
using System.Collections.Generic;
using System.Diagnostics;

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

        private int backlogCount;
        private int completedBacklogsCount;
        private int incompleteBacklogsCount;
        private double completedPercent;

        GraphServiceClient graphServiceClient;

        bool isNetworkAvailable = false;
        bool signedIn;
        int backlogIndex = -1;
        bool sync = false;

        public MainPage()
        {
            this.InitializeComponent();
            isNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
            WelcomeText.Text = $"Welcome to Backlogs, {Settings.UserName}!";
            Task.Run(async () => { await SaveData.GetInstance().ReadDataAsync(); }).Wait();
            recentlyAdded = new ObservableCollection<Backlog>();
            recentlyCompleted = new ObservableCollection<Backlog>();
            LoadBacklogs();
            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Disabled;
        }

        /// <summary>
        /// Shows the teaching tips on fresh install
        /// </summary>
        private void ShowTeachingTips()
        {
            if(Settings.IsFirstRun)
            {
                NavigationTeachingTip.IsOpen = true;
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
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.Parameter != null && e.Parameter.ToString() != "")
            {
                if(e.Parameter.ToString() == "sync")
                {
                    sync = true;
                }
                else
                {
                    // for backward connected animation
                    backlogIndex = int.Parse(e.Parameter.ToString());
                }
            }
            ProgBar.Visibility = Visibility.Visible;
            signedIn = Settings.IsSignedIn;
            if (isNetworkAvailable && signedIn)
            {
                await Logger.Info("Signing in user....");
                graphServiceClient = await MSAL.GetGraphServiceClient();
                await SetUserPhotoAsync();
                TopSigninButton.Visibility = Visibility.Collapsed;
                BottomSigninButton.Visibility = Visibility.Collapsed;
                TopProfileButton.Visibility = Visibility.Visible;
                BottomProfileButton.Visibility = Visibility.Visible;
                await SaveData.GetInstance().ReadDataAsync(sync);
                LoadBacklogs();
                if (sync)
                {
                }
                BuildNotifactionQueue();
            }
            ShowTeachingTips();
            ProgBar.Visibility = Visibility.Collapsed;
        }

        private void LoadBacklogs()
        {
            recentlyAdded.Clear();
            recentlyCompleted.Clear();
            completedBacklogsCount = 0;
            incompleteBacklogsCount = 0;
            completedPercent = 0.0f;
            backlogs = SaveData.GetInstance().GetBacklogs();
            backlogCount = backlogs.Count;
            foreach (var backlog in backlogs.Where(b => !b.IsComplete).OrderByDescending(b => b.CreatedDate).Skip(1).Take(5))
            {
                recentlyAdded.Add(backlog);
            }
            foreach (var backlog in backlogs.Where(b => b.IsComplete).OrderByDescending(b => b.CompletedDate).Skip(1).Take(5))
            {
                recentlyCompleted.Add(backlog);
            }
            completedBacklogsCount = backlogs.Where(b => b.IsComplete).Count();
            incompleteBacklogsCount = backlogs.Where(b => !b.IsComplete).Count();
            completedPercent = (Convert.ToDouble(completedBacklogsCount) / backlogCount) * 100;
        }

        /// <summary>
        /// Set the user photo in the command bar
        /// </summary>
        /// <returns></returns>
        private async Task SetUserPhotoAsync()
        {
            await Logger.Info("Setting user photo....");
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
                await Logger.Error("Error settings", ex);
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
                     await SaveData.GetInstance().DeleteLocalFileAsync();
                    graphServiceClient = await MSAL.GetGraphServiceClient();
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
        /// Build the notif queue based on whether backlogs have notif time
        /// </summary>
        private void BuildNotifactionQueue()
        {
            foreach (var b in new ObservableCollection<Backlog>(backlogs.OrderByDescending(b => b.TargetDate)))
            {
                if(b.TargetDate != "None")
                {
                    var savedNotifTime = Settings.GetNotifTime(b.id.ToString());
                    if(savedNotifTime == "" || savedNotifTime != b.NotifTime.ToString())
                    {
                        DateTimeOffset date = DateTimeOffset.Parse(b.TargetDate, CultureInfo.InvariantCulture).Add(b.NotifTime);
                        int result = DateTimeOffset.Compare(date, DateTimeOffset.Now);
                        if (result > 0)
                        {
                            var builder = new ToastContentBuilder()
                            .AddText($"Hey there!", hintMaxLines: 1)
                            .AddText($"You wanted to check out {b.Name} by {b.Director} today. Here's your reminder!", hintMaxLines: 2)
                            .AddHeroImage(new Uri(b.ImageURL));
                            ScheduledToastNotification toastNotification = new ScheduledToastNotification(builder.GetXml(), date);
                            ToastNotificationManager.CreateToastNotifier().AddToSchedule(toastNotification);
                        }
                        Settings.SetNotifTime(b.id.ToString(), b.NotifTime.ToString());
                    }
                }
                bool showLiveTile = Settings.ShowLiveTile;
                if (showLiveTile)
                    GenerateLiveTiles(b);
            }
        }

        private void GenerateLiveTiles(Backlog b)
        {
            var tileContent = new TileContent()
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
                }
            };

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
            var ratingUri = new Uri(@"https://paypal.me/surya4822?locale.x=en_US");
            await Windows.System.Launcher.LaunchUriAsync(ratingUri);
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
            Frame.Navigate(typeof(BacklogsPage), "sync");
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

        private void AllAddedButton_Click(object sender, RoutedEventArgs e)
        {
            BacklogsButton_Click(sender, e);
        }

        private void AllCompletedButton_Click(object sender, RoutedEventArgs e)
        {
            CompletedBacklogsButton_Click(sender, e);
        }
    }
}

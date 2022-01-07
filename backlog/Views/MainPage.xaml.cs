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
        private ObservableCollection<Backlog> filmBacklogs { get; set; }
        private ObservableCollection<Backlog> tvBacklogs { get; set; }
        private ObservableCollection<Backlog> gameBacklogs { get; set; }
        private ObservableCollection<Backlog> musicBacklogs { get; set; }
        private ObservableCollection<Backlog> bookBacklogs { get; set; }
        GraphServiceClient graphServiceClient;

        bool isNetworkAvailable = false;
        bool signedIn;
        int backlogIndex = -1;
        bool sync = false;

        public MainPage()
        {
            this.InitializeComponent();
            isNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
            Task.Run(async () => { await SaveData.GetInstance().ReadDataAsync(); }).Wait();
            InitBacklogs();
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
        }

        /// <summary>
        /// Initalize backlogs
        /// </summary>
        private void InitBacklogs()
        {
            allBacklogs = SaveData.GetInstance().GetBacklogs();
            var readBacklogs = new ObservableCollection<Backlog>(allBacklogs.Where(b => b.IsComplete == false));
            backlogs = new ObservableCollection<Backlog>(readBacklogs.OrderBy(b => b.TargetDate));
            filmBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Film.ToString()));
            tvBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.TV.ToString()));
            gameBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Game.ToString()));
            musicBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Album.ToString()));
            bookBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Book.ToString()));
            ShowEmptyMessage();
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
                graphServiceClient = await SaveData.GetInstance().GetGraphServiceClient();
                await SetUserPhotoAsync();
                TopSigninButton.Visibility = Visibility.Collapsed;
                BottomSigninButton.Visibility = Visibility.Collapsed;
                TopProfileButton.Visibility = Visibility.Visible;
                BottomProfileButton.Visibility = Visibility.Visible;
                if(sync)
                {
                    await Logger.Info("Syncing backlogs....");
                    await SaveData.GetInstance().ReadDataAsync(true);
                    PopulateBacklogs();
                }
                BuildNotifactionQueue();
            }
            ShowTeachingTips();
            ProgBar.Visibility = Visibility.Collapsed;
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
            var accountPicFile = await cacheFolder.GetFileAsync("profile.png");
            using(IRandomAccessStream stream = await accountPicFile.OpenAsync(FileAccessMode.Read))
            {
                BitmapImage image = new BitmapImage();
                stream.Seek(0);
                await image.SetSourceAsync(stream);
                TopAccountPic.ProfilePicture = image;
                BottomAccountPic.ProfilePicture = image;
            }
        }

        /// <summary>
        /// Populate the backlogs list with up-to-date backlogs
        /// </summary>
        private void PopulateBacklogs()
        {
            var readBacklogs = SaveData.GetInstance().GetBacklogs().Where(b => b.IsComplete == false);
            var _backlogs = new ObservableCollection<Backlog>(readBacklogs.OrderBy(b => b.TargetDate)); // sort by last created
            var _filmBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Film.ToString()));
            var _tvBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.TV.ToString()));
            var _gameBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Game.ToString()));
            var _musicBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Album.ToString()));
            var _bookBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Book.ToString()));
            backlogs.Clear();
            filmBacklogs.Clear();
            tvBacklogs.Clear();
            gameBacklogs.Clear();
            musicBacklogs.Clear();
            bookBacklogs.Clear();
            EmptyListText.Visibility = Visibility.Collapsed;
            foreach (var b in _backlogs)
            {
                backlogs.Add(b);
            }
            foreach (var b in _bookBacklogs)
            {
                bookBacklogs.Add(b);
            }
            foreach (var b in _filmBacklogs)
            {
                filmBacklogs.Add(b);
            }
            foreach (var b in _gameBacklogs)
            {
                gameBacklogs.Add(b);
            }
            foreach (var b in _tvBacklogs)
            {
                tvBacklogs.Add(b);
            }
            foreach (var b in _musicBacklogs)
            {
                musicBacklogs.Add(b);
            }
            ShowEmptyMessage();
        }

        private void ShowEmptyMessage()
        {
            ObservableCollection<Backlog>[] _backlogs = { backlogs, filmBacklogs, tvBacklogs, gameBacklogs, musicBacklogs, bookBacklogs };
            TextBlock[] textBlocks = { EmptyListText, EmptyFilmsText, EmptyTVText, EmptyGamesText, EmptyMusicText, EmptyBooksText };
            for (int i = 0; i < _backlogs.Length; i++)
            {
                if(_backlogs[i].Count <=0)
                {
                    textBlocks[i].Visibility = Visibility.Visible;
                    if(i > 0)
                    {
                        textBlocks[i].Text = $"Nothing to see here. Add some!";
                    }
                }
                else
                {
                    textBlocks[i].Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Opens the Backlog details page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BacklogView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedBacklog = (Backlog)e.ClickedItem;
            PivotItem pivotItem = (PivotItem)mainPivot.SelectedItem;
            // Prepare connected animation based on which section the user is on
            switch(pivotItem.Header.ToString())
            {
                default:
                    BacklogsGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
                case "films":
                    FilmsGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break ;
                case "tv":
                    TVGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
                case "books":
                    BooksGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
                case "games":
                    GamesGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
                case "albums":
                    AlbumsGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
            }
            Frame.Navigate(typeof(BacklogPage), selectedBacklog.id, new SuppressNavigationTransitionInfo());
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
                    graphServiceClient = await SaveData.GetInstance().GetGraphServiceClient();
                    Settings.IsSignedIn = true;
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
                    ApplicationDataContainer notifSettings = ApplicationData.Current.LocalSettings;
                    int? notifSent = (int?)notifSettings.Values[b.id.ToString()];
                    if (notifSent != 1)
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
                        notifSettings.Values[b.id.ToString()] = 1;
                    }
                }
                string showLiveTile = ApplicationData.Current.LocalSettings.Values["LiveTileOn"]?.ToString();
                if (showLiveTile == null || showLiveTile == "True")
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
                        DisplayName = "Backlogs",
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

        /// <summary>
        /// Finish connected animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BacklogsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if(backlogIndex != -1)
            {
                ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("backAnimation");
                try
                {
                    await BacklogsGrid.TryStartConnectedAnimationAsync(animation, allBacklogs[backlogIndex], "coverImage");
                }
                catch
                {
                    // : )
                }
            }
        }
    }
}

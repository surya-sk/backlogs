using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using backlog.Models;
using backlog.Saving;
using backlog.Utils;
using System.Collections.ObjectModel;
using Windows.Storage;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Windows.UI.Core;
using System.Net.NetworkInformation;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.ApplicationModel.Background;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using System.Globalization;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Backlog> backlogs { get; set; }
        private ObservableCollection<Backlog> filmBacklogs { get; set; }
        private ObservableCollection<Backlog> tvBacklogs { get; set; }
        private ObservableCollection<Backlog> gameBacklogs { get; set; }
        private ObservableCollection<Backlog> musicBacklogs { get; set; }
        private ObservableCollection<Backlog> bookBacklogs { get; set; }
        GraphServiceClient graphServiceClient;

        readonly string toastTaskName = "ToastTask";

        bool isNetworkAvailable = false;
        string signedIn;
        public MainPage()
        {
            this.InitializeComponent();
            isNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
            Task.Run(async () => { await SaveData.GetInstance().ReadDataAsync(); }).Wait();
            InitBacklogs();
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
        }

        private void InitBacklogs()
        {
            var readBacklogs = SaveData.GetInstance().GetBacklogs();
            backlogs = new ObservableCollection<Backlog>(readBacklogs.OrderByDescending(b => b.TargetDate));
            filmBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Film.ToString()));
            tvBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.TV.ToString()));
            gameBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Game.ToString()));
            musicBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Music.ToString()));
            bookBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Book.ToString()));
            ShowEmptyMessage();
            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Disabled;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            ProgBar.Visibility = Visibility.Visible;
            signedIn = ApplicationData.Current.LocalSettings.Values["SignedIn"]?.ToString();
            if (isNetworkAvailable && signedIn == "Yes")
            {
                await Logger.WriteLogAsync("Signed in");
                graphServiceClient = await SaveData.GetInstance().GetGraphServiceClient();
                TopSigninButton.Visibility = Visibility.Collapsed;
                BottomSigninButton.Visibility = Visibility.Collapsed;
                TopProfileButton.Visibility = Visibility.Visible;
                BottomProfileButton.Visibility = Visibility.Visible;
                await SaveData.GetInstance().ReadDataAsync(true);
                await PopulateBacklogs();
                await SetUserPhotoAsync();
            }
            foreach(Backlog b in backlogs)
            {
                await BuildNotifactionQueue(b);
            }
            ProgBar.Visibility = Visibility.Collapsed;
            base.OnNavigatedTo(e);
        }

        public async Task SetUserPhotoAsync()
        {
            try
            {
                await Logger.WriteLogAsync("Getting user photo");
                var user = await graphServiceClient.Me.Request().GetAsync();
                Stream photoresponse = await graphServiceClient.Me.Photo.Content.Request().GetAsync();

                if (photoresponse != null)
                {
                    using (var randomAccessStream = photoresponse.AsRandomAccessStream())
                    {
                        BitmapImage image = new BitmapImage();
                        randomAccessStream.Seek(0);
                        await image.SetSourceAsync(randomAccessStream);
                        TopAccountPic.ProfilePicture = image;
                        BottomAccountPic.ProfilePicture = image;
                    }
                }
                TopProfileButton.Label = user.GivenName;
                BottomProfileButton.Label = user.GivenName;
            }
            catch(Exception e)
            {
                await Logger.WriteLogAsync($"Unable to get user info\n{e.ToString()}");
            }
        }

        private async Task PopulateBacklogs()
        {
            ObservableCollection<Backlog> readBacklogs = SaveData.GetInstance().GetBacklogs();
            await Logger.WriteLogAsync($"Number of backlogs found: {readBacklogs.Count}");
            var _backlogs = new ObservableCollection<Backlog>(readBacklogs.OrderByDescending(b => b.TargetDate)); // sort by last created
            var _filmBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Film.ToString()));
            var _tvBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.TV.ToString()));
            var _gameBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Game.ToString()));
            var _musicBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Music.ToString()));
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
            }
        }

        private void BacklogView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedBacklog = (Backlog)e.ClickedItem;
            Frame.Navigate(typeof(BacklogPage), selectedBacklog.id);
        }

        private async void SigninButton_Click(object sender, RoutedEventArgs e)
        {
            signedIn = ApplicationData.Current.LocalSettings.Values["SignedIn"]?.ToString();
            if (isNetworkAvailable)
            {
                if (signedIn != "Yes")
                {
                    graphServiceClient = await SaveData.GetInstance().GetGraphServiceClient();
                    ApplicationData.Current.LocalSettings.Values["SignedIn"] = "Yes";
                    Frame.Navigate(typeof(MainPage));
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
                ContentDialogResult result = await contentDialog.ShowAsync();
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreatePage));
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private bool IsBackgroundTaskRegistered(string taskName)
        {
            if (BackgroundTaskHelper.IsBackgroundTaskRegistered(taskName))
            {
                // Background task already registered.
                return true;
            }

            return false;
        }

        private async void RateButton_Click(object sender, RoutedEventArgs e)
        {
            var ratingUri = new Uri(@"ms-windows-store://review/?ProductId=9N2H8CM2KWVZ");
            await Windows.System.Launcher.LaunchUriAsync(ratingUri);
        }

        private async Task BuildNotifactionQueue(Backlog b)
        {
            DateTimeOffset date = DateTimeOffset.Parse(b.TargetDate, CultureInfo.InvariantCulture).Add(b.NotifTime);
            int result = DateTimeOffset.Compare(date, DateTimeOffset.Now);
            ApplicationDataContainer notifSettings = ApplicationData.Current.LocalSettings;
            int? notifSent = (int?)notifSettings.Values[b.id.ToString()];
            if (notifSent != 1)
            {
                if(result > 0)
                {
                    Debug.WriteLine(result);
                    var builder = new ToastContentBuilder()
                    .AddText($"Hey there!", hintMaxLines: 1)
                    .AddText($"You wanted to check out {b.Name} by {b.Director} today. Here's your reminder!", hintMaxLines: 2)
                    .AddHeroImage(new Uri(b.ImageURL));
                    ScheduledToastNotification toastNotification = new ScheduledToastNotification(builder.GetXml(), date);
                    ToastNotificationManager.CreateToastNotifier().AddToSchedule(toastNotification);
                    if (b.RemindEveryday)
                    {
                        if (IsBackgroundTaskRegistered(toastTaskName))
                        {
                            return;
                        }

                        await BackgroundExecutionManager.RequestAccessAsync();
                        BackgroundTaskHelper.Register(toastTaskName, new TimeTrigger(1440, false));
                    }
                }
                notifSettings.Values[b.id.ToString()] = 1;
            }
            string showLiveTile = ApplicationData.Current.LocalSettings.Values["LiveTileOn"]?.ToString();
            if (showLiveTile == null || showLiveTile == "True")
                GenerateLiveTiles(b);

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
                                    Text = $"You have this set for {b.TargetDate}",
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

        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
